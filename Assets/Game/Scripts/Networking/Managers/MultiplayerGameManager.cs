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
        for (int i = 0; i < _playerDataNetworkList.Count; ++i)
        {
            PlayerDataSerializable playerData = _playerDataNetworkList[i];
            if (playerData.ClientId == clientId)
            {
                _playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
    {
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

    // dosyasýnýn içine eklenecek

    public bool AmITheNextHost()
    {
        // Eðer listede kimse yoksa veya hata varsa false dön
        if (_playerDataNetworkList.Count == 0) return false;

        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        ulong lowestClientId = ulong.MaxValue;

        // Listeyi dön ve Host (0) hariç en küçük ID'yi bul
        foreach (var playerData in _playerDataNetworkList)
        {
            // Host zaten düþtü varsayýyoruz, o yüzden 0'ý veya düþen host ID'sini dikkate alma
            // Ancak bu fonksiyon host düþmeden hemen önce çalýþacaðý için
            // Kendimiz dýþýndaki en düþük ID'ye bakmalýyýz.

            // Basit mantýk: Listeyi ClientId'ye göre sýrala.
            // Eðer ben listenin baþýndaysam (veya Host'tan sonraki ilk kiþi isem) Host benim.

            if (playerData.ClientId < lowestClientId && playerData.ClientId != 0)
            {
                lowestClientId = playerData.ClientId;
            }
        }

        // Eðer Host (0) gittiyse, en küçük ID bensem, yeni Host benim.
        // Not: Gerçek senaryoda Host ID'si 0 olmayabilir ama genelde 0'dýr.
        // Biz burada basitçe: "Benim ID'm, kalanlar arasýndaki en küçük mü?" diye bakýyoruz.
        return myClientId == lowestClientId;
    }

    // dosyasýnýn içine uygun bir yere ekle:

    public IEnumerable<PlayerDataSerializable> GetPlayerDataList()
    {
        // NetworkList'i normal bir listeye çevirip döndürüyoruz
        List<PlayerDataSerializable> list = new List<PlayerDataSerializable>();
        foreach (var player in _playerDataNetworkList)
        {
            list.Add(player);
        }
        return list;
    }
}
