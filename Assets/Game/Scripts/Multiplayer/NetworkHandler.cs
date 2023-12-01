using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class NetworkHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkRunner networkRunnerPrefab;
    [SerializeField] NetworkPrefabRef networkPlayerPrefab;
    [SerializeField] NetworkPrefabRef networkGlobalPrefab;

    NetworkRunner networkRunner;
    NetworkSceneManagerBase loader;

    public bool isHost;
    public int maxPlayer = 2;

    void Awake()
    {
        loader ??= gameObject.AddComponent<NetworkSceneManagerDefault>();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void StartQuickGame()
    {
        if (!Connect()) return;

        FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Connecting...");

        networkRunner.ProvideInput = true;
        networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            CustomLobbyName = "VarVar",
            PlayerCount = maxPlayer,
            SceneManager = loader
        });
    }

    public void StartRoomGame(string sessionName)
    {
        if (string.IsNullOrEmpty(sessionName))
            return;

        if (!Connect()) return;

        networkRunner.ProvideInput = true;
        networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            CustomLobbyName = sessionName,
            SessionName = sessionName,
            PlayerCount = maxPlayer,
            SceneManager = loader
        });
    }

    bool Connect()
    {
        if (networkRunner != null) return false;

        networkRunner = Instantiate(networkRunnerPrefab, transform);
        networkRunner.AddCallbacks(this);

        return true;
    }

    public void Disconnect()
    {
        if (networkRunner is null) return;

        networkRunner.Shutdown();
    }

    public void CloseConnection()
    {
        Disconnect();
    }

    public void SpawnNetworkGlobals()
    {
        networkRunner.Spawn(networkGlobalPrefab, Vector3.zero, Quaternion.identity);
    }

    #region CALLBACKS
    public void OnConnectedToServer(NetworkRunner runner)
    {
        FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Connected");
        networkRunner.Spawn(networkPlayerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("Disconnected");
        Disconnect();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Connection Failed");
        Disconnect();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        FindObjectOfType<GameControl>().lobbyUIManager.SetPlayerCountText(0,maxPlayer);
        isHost = networkRunner.IsSharedModeMasterClient;
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("PlayerLeft");
        FindObjectOfType<GameControl>().UpdateLobbyPlayerNames();
        //Disconnect();
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (networkRunner!= null  && networkRunner.gameObject)
            Destroy(networkRunner.gameObject);

        networkRunner = null;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    #endregion
}
