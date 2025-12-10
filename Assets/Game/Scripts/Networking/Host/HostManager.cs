using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostManager : IDisposable
{    
    public NetworkServer NetworkServer { get; private set; }
    
    private Allocation allocation;
    private Lobby joinedLobby;

    public async UniTask StartHostAsync(string lobbyName, int lobbyMaxPlayer, bool isPrivate, bool loadScene = true)
    {
        try
        {

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName, lobbyMaxPlayer, new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                });

            allocation = await RelayService.Instance.CreateAllocationAsync(lobbyMaxPlayer);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                }
            });

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            NetworkServer = new NetworkServer(NetworkManager.Singleton);

            UserData userData = new UserData
            {
                UserName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, "Noname"),
                UserAvatarIndex = (byte)PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey, 0),
                UserAuthId = AuthenticationService.Instance.PlayerId
            };
            string payload = JsonUtility.ToJson(userData);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartHost();

            NetworkServer.OnClientLeft += HandleClientLeft;

            if (loadScene)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Room, LoadSceneMode.Single);
            }
        }
        catch(LobbyServiceException lobbyServiceException)
        {
            Debug.LogError(lobbyServiceException);
            return;
        }
    }

    private async void HandleClientLeft(string authId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, authId);
        }
        catch(LobbyServiceException lobbyServiceException)
        {
            Debug.Log(lobbyServiceException);
        }
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while(true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            yield return delay;
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    public void Dispose()
    {
        Shutdown();
    }

    public async UniTask Shutdown()
    {
        if (joinedLobby != null && !string.IsNullOrEmpty(joinedLobby.Id))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            }
            catch(LobbyServiceException lobbyServiceException)
            {
                Debug.Log(lobbyServiceException);
            }

            joinedLobby = null;
        }

        if (NetworkServer != null)
        {
            NetworkServer.OnClientLeft -= HandleClientLeft;
            NetworkServer.Dispose();
        }
    }


    public async UniTask StartHostMigrationAsync()
    {
        await StartHostAsync(
            MigrationBackup.LobbyName,
            MigrationBackup.MaxPlayers,
            MigrationBackup.IsPrivate,
            loadScene: false
        );

    }
}
