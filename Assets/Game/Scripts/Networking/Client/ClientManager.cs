using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
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
    private JoinAllocation _joinAllocation;
    private NetworkClient _networkClient;
    private string _joinCode;
    private Lobby _lobby;

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

    public async UniTask StartClientAsync(string joinCode)
    {
        try
        {
            _joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
            return;
        }

        _joinCode = joinCode;

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(_joinAllocation, "dtls"));

        UserData userData = new UserData
        {
            UserName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, "Noname"),
            UserAvatarIndex = (byte) PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey, 0),
            UserAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes; 

        NetworkManager.Singleton.StartClient();
    }
    public string GetJoinCode()
    {
        return _joinCode;
    }

    public Lobby GetLobby()
    {
        return _lobby;
    }

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
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
