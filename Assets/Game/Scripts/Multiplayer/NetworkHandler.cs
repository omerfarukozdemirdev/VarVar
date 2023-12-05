using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
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

    private string sessionName;

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
        if (!Connect())
        {
            Disconnect();
            FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Disconnected");
            FindObjectOfType<GameControl>().lobbyUIManager.playersWaitingText.SetActive(false);
            FindObjectOfType<GameControl>().lobbyUIManager.roomMaxPlayerText.gameObject.SetActive(false);
            return;
        }

        FindObjectOfType<GameControl>().lobbyUIManager.roomMaxPlayerText.gameObject.SetActive(true);
        FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Connecting...");

        if (FindObjectOfType<GameControl>().TestMode)
            sessionName = "VarVarTest";
        else
            sessionName = "VarVar";

        networkRunner.ProvideInput = true;
        networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            CustomLobbyName = sessionName,
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

    public void SpawnNetworkGlobals()
    {
        networkRunner.Spawn(networkGlobalPrefab, Vector3.zero, Quaternion.identity);
    }

    #region CALLBACKS
    public void OnConnectedToServer(NetworkRunner runner)
    {
        FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Connected");
        //networkRunner.Spawn(networkPlayerPrefab, Vector3.zero, Quaternion.identity, runner.LocalPlayer);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        FindObjectOfType<GameControl>().lobbyUIManager.SetMesssage("Connection Failed");
        Disconnect();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        isHost = networkRunner.IsSharedModeMasterClient;

        if (player == runner.LocalPlayer)
        {
            NetworkObject networkPlayer = runner.Spawn(networkPlayerPrefab, Vector3.zero, Quaternion.identity, player);
            runner.SetPlayerObject(player, networkPlayer);
        }

        //FindObjectOfType<GameControl>().UpdateLobbyPlayerNames();

    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        switch (GameManager.Instance.gameStat)
        {
            case GameManager.GameStat.menu:
                break;
            case GameManager.GameStat.lobby:

                FindObjectOfType<GameControl>().UpdateLobbyPlayerNames();

                break;
            case GameManager.GameStat.game:

                FindObjectOfType<GameControl>().OpenDisconnetPopup();

                break;
            case GameManager.GameStat.handCompleted:

                FindObjectOfType<GameControl>().OpenDisconnetPopup();

                break;
        }
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (networkRunner != null && networkRunner.gameObject)
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
