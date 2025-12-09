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

    private async void OnClientDisconnect(ulong clientId)
    {
        if (clientId != 0 && clientId != _networkManager.LocalClientId) { return; }

        if (SceneManager.GetActiveScene().name == Constants.SceneNames.Game)
        {
            if (GameControl.Instance.isIntentionalDisconnect)
            {
                Disconnect();
                GameControl.Instance.isIntentionalDisconnect = false;
                return;
            }

            if (clientId == 0 || clientId == _networkManager.LocalClientId)
            {

                GameControl.Instance.HandleMigrationStateChanged(MigrationState.MigrateStart);

                MigrationBackup.Clear();

                var playerList = MultiplayerGameManager.Instance.GetPlayerDataList();
                MigrationBackup.Players.AddRange(playerList);

                var currentLobby = MultiplayerGameManager.Instance.GetLobby();
                if (currentLobby != null)
                {
                    MigrationBackup.LobbyName = currentLobby.Name;
                    MigrationBackup.MaxPlayers = currentLobby.MaxPlayers;
                    MigrationBackup.IsPrivate = currentLobby.IsPrivate;
                }

                bool iAmHeir = MultiplayerGameManager.Instance.AmITheNextHost();

                _networkManager.Shutdown();

                await UniTask.Delay(1000, ignoreTimeScale: true);

                if (iAmHeir)
                {
                    await HostSingleton.Instance.HostManager.StartHostMigrationAsync();
                }
                else
                {
                    await ClientSingleton.Instance.ClientManager.ReconnectToMigrationAsync();
                }
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
