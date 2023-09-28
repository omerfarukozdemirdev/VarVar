using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public GameObject textPrefab;
    public Transform parent;
    public Button readyUp;

    private static readonly Dictionary<NetworkPlayer, LobbyItemUI> ListItems = new Dictionary<NetworkPlayer, LobbyItemUI>();
    private static bool IsSubscribed;

    private void Awake()
    {
        Setup();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (IsAllReady())
        {
            NetworkUIManager.Instance.CloseLobbyPanel();
            NetworkUIManager.Instance.SetTable();
        }
    }

    public void Setup()
    {
        if (IsSubscribed) return;

        NetworkPlayer.PlayerJoined += AddPlayer;
        //RoomPlayer.PlayerLeft += RemovePlayer;

        NetworkPlayer.PlayerChanged += EnsureAllPlayersReady;

        readyUp.onClick.AddListener(ReadyUpListener);

        IsSubscribed = true;
    }

    private void AddPlayer(NetworkPlayer player)
    {
        if (ListItems.ContainsKey(player))
        {
            var toRemove = ListItems[player];
            Destroy(toRemove.gameObject);

            ListItems.Remove(player);
            Debug.Log("var var");
        }

        var obj = Instantiate(textPrefab, parent).GetComponent<LobbyItemUI>();
        obj.SetPlayer(player);

        ListItems.Add(player, obj);

        //UpdateDetails(GameManager.Instance);
        Debug.Log("eklendi");
    }

    private void ReadyUpListener()
    {
        var local = NetworkPlayer.Local;
        if (local && local.Object && local.Object.IsValid)
        {
            local.RPC_ChangeReadyState(!local.IsReady);
        }
    }

    private void EnsureAllPlayersReady(NetworkPlayer lobbyPlayer)
    {
        if (!NetworkPlayer.Local.IsLeader)
            return;

        if (IsAllReady())
        {
            //int scene = ResourceManager.Instance.tracks[GameManager.Instance.TrackId].buildIndex;
            //LevelManager.LoadTrack(scene);
            Debug.Log("Oyuncular hazır");
        }
    }

    private static bool IsAllReady() => NetworkPlayer.Players.Count > 0 && NetworkPlayer.Players.All(player => player.IsReady);


}
