using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MultiplayerGameManager : NetworkBehaviour
{
    public static MultiplayerGameManager Instance { get; private set; }

    public event Action OnPlayerDataNetworkListChanged;

    private NetworkList<PlayerDataSerializable> _playerDataNetworkList = new NetworkList<PlayerDataSerializable>();

    private bool isShuttingDown = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playerDataNetworkList.Clear();

            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectedCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
        }
    }

    private void NetworkManager_Server_OnClientDisconnectedCallback(ulong clientId)
    {
        if (isShuttingDown) return;
        
        for (int i = 0; i < _playerDataNetworkList.Count; ++i)
        {
            if (_playerDataNetworkList[i].ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
                break;
            }
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == Constants.SceneNames.Game)
        {
            if (GameManager.Instance.GetGameState() == GameState.PrepareGame)
            {
                KickAllPlayersAndShutdown();
                return;
            }

            if (_playerDataNetworkList.Count <= 1)
            {
                KickAllPlayersAndShutdown();
            }
        }
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
        if (isShuttingDown) return;

        for (int i = 0; i < _playerDataNetworkList.Count; ++i)
        {
            if (_playerDataNetworkList[i].ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }

        _playerDataNetworkList.Add(new PlayerDataSerializable
        {
            ClientId = clientId,
        });
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerDataSerializable> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < _playerDataNetworkList.Count;
    }

    public PlayerDataSerializable GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerDataSerializable playerData in _playerDataNetworkList)
        {
            if (playerData.ClientId == clientId)
            {
                return playerData;
            }
        }

        return default;
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for (int i = 0; i < _playerDataNetworkList.Count; ++i)
        {
            if (_playerDataNetworkList[i].ClientId == clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public PlayerDataSerializable GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerDataSerializable GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return _playerDataNetworkList[playerIndex];
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectedCallback(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Server_OnClientDisconnectedCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Server_OnClientConnectedCallback;
        }
    }

    public Lobby GetLobby()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return HostSingleton.Instance.HostManager.GetLobby();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            return ClientSingleton.Instance.ClientManager.GetLobby();
        }
        else
            return HostSingleton.Instance.HostManager.GetLobby();
    }

    public bool AmITheNextHost()
    {
        if (_playerDataNetworkList.Count == 0) return false;

        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        ulong lowestClientId = ulong.MaxValue;

        foreach (var playerData in _playerDataNetworkList)
        {
            if (playerData.ClientId < lowestClientId && playerData.ClientId != 0)
            {
                lowestClientId = playerData.ClientId;
            }
        }
        return myClientId == lowestClientId;
    }


    public IEnumerable<PlayerDataSerializable> GetPlayerDataList()
    {
        List<PlayerDataSerializable> list = new List<PlayerDataSerializable>();
        foreach (var player in _playerDataNetworkList)
        {
            list.Add(player);
        }
        return list;
    }

    private async void KickAllPlayersAndShutdown()
    {
        if (isShuttingDown) return;

        isShuttingDown = true;

        if (NetworkManager.Singleton == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneNames.Menu);
            Time.timeScale = 1;
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            ForceDisconnectClientRpc();

            float timeOut = 3.0f;
            while (NetworkManager.Singleton.ConnectedClientsIds.Count > 1 && timeOut > 0)
            {
                timeOut -= Time.unscaledDeltaTime;
                await UniTask.Yield();
            }

            await UniTask.Delay(100, ignoreTimeScale: true);
        }

        if (HostSingleton.Instance?.HostManager != null)
        {
            await HostSingleton.Instance.HostManager.Shutdown();
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        isShuttingDown = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneNames.Menu);
        Time.timeScale = 1;
    }

    [ClientRpc]
    private void ForceDisconnectClientRpc()
    {
        if (NetworkManager.Singleton.IsHost) return;

        if (ClientSingleton.Instance?.ClientManager != null)
        {
            ClientSingleton.Instance.ClientManager.Disconnect();
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneNames.Menu);
            Time.timeScale = 1;
        }
    }
}
