using System;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : IDisposable
{
    private NetworkClient _networkClient;
    private Lobby joinedLobby;

    public async UniTask<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        _networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthenticationState authenticationState = await AuthenticationHandler.DoAuth();

        if(authenticationState == AuthenticationState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(Constants.SceneNames.Menu);
    }

    public async UniTask JoinWithCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            UserData userData = new UserData
            {
                UserName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, "Noname"),
                UserAvatarIndex = (byte)PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey, 0),
                UserAuthId = AuthenticationService.Instance.PlayerId
            };

            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async UniTask JoinWithId(string lobbyId)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            UserData userData = new UserData
            {
                UserName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, "Noname"),
                UserAvatarIndex = (byte)PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey, 0),
                UserAuthId = AuthenticationService.Instance.PlayerId
            };

            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async UniTask QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            UserData userData = new UserData
            {
                UserName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, "Noname"),
                UserAvatarIndex = (byte)PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey, 0),
                UserAuthId = AuthenticationService.Instance.PlayerId
            };

            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    public void Disconnect()
    {
        _networkClient.Disconnect();
    }

    public void Dispose()
    {
        _networkClient?.Dispose();
    }
}
