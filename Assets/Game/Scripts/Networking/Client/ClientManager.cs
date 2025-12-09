using System;
using System.Collections.Generic;
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
        Time.timeScale = 1f;
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
            // 1. Önce girmeyi dene
            try
            {
                joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            }
            catch (LobbyServiceException e)
            {
                // 2. Eðer "Zaten üyesin" (Conflict/409) hatasý alýrsak
                if (e.Reason == LobbyExceptionReason.LobbyConflict)
                {
                    Debug.LogWarning("MIGRATION: Zaten bu lobideyiz. Veriler güncelleniyor...");
                    // Tekrar girmek yerine mevcut bilgiyi çek
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
                }
                else
                {
                    // Baþka bir hata ise (örn: oda dolu, oda yok) dýþarý fýrlat
                    throw e;
                }
            }

            // --- BURADAN SONRASI AYNI ---

            // Relay kodunu al
            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;

            Debug.Log($"MIGRATION: Relay Kodu Alýndý: {relayJoinCode}");

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
            Debug.LogError($"Lobby Baðlantý Hatasý: {e}");
            // Burada throw diyerek hatayý ReconnectToMigrationAsync'e bildirebilirsin
            // ki orada loop devam etsin veya menu'ye dönsün.

            GoToMenu();

            throw;
        }
        catch (Exception ex)
        {
            Debug.Log($"Genel Hata: {ex}");

            GoToMenu();

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

    //

    public async UniTask ReconnectToMigrationAsync()
    {
        Debug.Log("MIGRATION: Yeni odaya baðlanýlmaya çalýþýlýyor...");

        // 10 deneme yapacaðýz (veya sonsuza kadar döngü de olabilir)
        for (int i = 0; i < 20; i++)
        {
            Debug.Log($"MIGRATION: Deneme {i + 1}/20");
            try
            {
                // Eski odanýn ismine sahip bir lobi arýyoruz
                var queryOptions = new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, MigrationBackup.LobbyName, QueryFilter.OpOptions.EQ)
                }
                };

                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

                // Eðer uygun bir oda bulunduysa
                if (queryResponse.Results.Count > 0)
                {
                    Debug.Log("MIGRATION: Oda bulundu! Baðlanýlýyor...");
                    
                    var foundLobby = queryResponse.Results[0]; // Ýlk bulunaný al

                    await JoinWithId(foundLobby.Id); // Baðlan
                    
                    return; // Baþarýlý, çýk
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Arama hatasý: {e.Message}");
            }

            Debug.Log("MIGRATION: Oda henüz hazýr deðil, bekleniyor...");
            await UniTask.Delay(2000, ignoreTimeScale: true); // 2 saniye bekle tekrar dene
        }

        Debug.LogError("MIGRATION: Yeni host bulunamadý. Menüye dönülüyor.");
        GoToMenu();
    }
}
