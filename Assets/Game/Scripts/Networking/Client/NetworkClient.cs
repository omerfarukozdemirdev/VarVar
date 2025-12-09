using Cysharp.Threading.Tasks;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager _networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    //

    private async void OnClientDisconnect(ulong clientId)
    {
        if (clientId != 0 && clientId != _networkManager.LocalClientId) { return; }

        if (SceneManager.GetActiveScene().name == Constants.SceneNames.Game)
        {
            GameControl.Instance.HandleMigrationStateChanged(MigrationState.MigrateStart);

            Debug.Log("Host ayrýldý. Migration baþlýyor...");

            // --- YEDEKLEME BAÞLANGICI ---

            // 1. Önceki verileri temizle
            MigrationBackup.Clear();

            // 2. Oyuncu Listesini Kopyala (MultiplayerGameManager yok olmadan önce!)
            var playerList = MultiplayerGameManager.Instance.GetPlayerDataList();
            // NOT: GetPlayerDataList diye bir metodun yoksa aþaðýda ekleyeceðiz.
            MigrationBackup.Players.AddRange(playerList);

            // 3. Lobby Ýsmini Yedekle
            // Þu anki Lobby verisine eriþmemiz lazým.
            var currentLobby = MultiplayerGameManager.Instance.GetLobby();
            if (currentLobby != null)
            {
                MigrationBackup.LobbyName = currentLobby.Name;
                MigrationBackup.MaxPlayers = currentLobby.MaxPlayers; // EKLENDÝ
                MigrationBackup.IsPrivate = currentLobby.IsPrivate;   // EKLENDÝ
            }

            // --- YEDEKLEME BÝTÝÞÝ ---

            bool iAmHeir = MultiplayerGameManager.Instance.AmITheNextHost();

            //Time.timeScale = 0f;
            _networkManager.Shutdown();

            await UniTask.Delay(1000, ignoreTimeScale: true);

            if (iAmHeir)
            {
                Debug.Log("HOST MIGRATION: Yeni Host ben oluyorum!");

                // Singleton yapýna göre HostSingleton'a eriþiyoruz
                // Eðer senin projende HostSingleton ismi farklýysa (örn: GameManager) onu kullan.
                await HostSingleton.Instance.HostManager.StartHostMigrationAsync();
            }
            else
            {
                Debug.Log("HOST MIGRATION: Yeni Host aranýyor...");

                // Client Singleton üzerinden yeniden baðlanma döngüsünü baþlat
                await ClientSingleton.Instance.ClientManager.ReconnectToMigrationAsync();
            }
        }
        else
        {
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if(SceneManager.GetActiveScene().name == Constants.SceneNames.Room)
        {
            SceneManager.LoadScene(Constants.SceneNames.Menu);
        }
        else if(SceneManager.GetActiveScene().name == Constants.SceneNames.Game)
        {
            var gamecontrol = GameControl.Instance;
            gamecontrol.ClientDisconnectServerRpc((byte)gamecontrol.actorControls.IndexOf(gamecontrol.playerControl.actorControl));
            Time.timeScale = 1;
        }


        if (_networkManager.IsConnectedClient)
        {
            _networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if(_networkManager == null) { return; }

        _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;

        if(_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }
}
