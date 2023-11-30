using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public GameObject textPrefab;
    public Transform[] parent;
    //public Button readyUp;

    private static readonly Dictionary<NetworkPlayer, LobbyItemUI> ListItems = new Dictionary<NetworkPlayer, LobbyItemUI>();
    [SerializeField] private bool IsSubscribed;

    public bool nextStepIsAllReady;
    //[SerializeField] private int playerCounter;
    //[SerializeField] private Text waitingPlayersMessage;
    [SerializeField] private Text playerCounterText;
    [SerializeField] private GameObject loadingPlayersRoot;
    [SerializeField] private Text roomName;

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
        // if (IsAllReady() && !nextStepIsAllReady)
        // {
        //     NetworkUIManager.Instance.CloseLobbyPanel();
        //     //NetworkUIManager.Instance.SetTable();
        //     //Invoke("StartGame", 1f);
        //     FindObjectOfType<GameControl>().Invoke("FriendsModeStartGame", 1f);
        //     //FindObjectOfType<GameControl>().FriendsModeStartGame();
        //     nextStepIsAllReady = true;
        // }
    }

    public void Setup()
    {
        if (IsSubscribed) return;

        NetworkPlayer.PlayerJoined += AddPlayer;
        NetworkPlayer.PlayerLeft += RemovePlayer;

        NetworkPlayer.PlayerChanged += EnsureAllPlayersReady;

        //readyUp.onClick.AddListener(ReadyUpListener);

        IsSubscribed = true;


    }

    private void OnDestroy()
    {
        if (!IsSubscribed) return;

        NetworkPlayer.PlayerJoined -= AddPlayer;
        NetworkPlayer.PlayerLeft -= RemovePlayer;

        //readyUp.onClick.RemoveListener(ReadyUpListener);

        IsSubscribed = false;
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

        var obj = Instantiate(textPrefab, parent[NetworkPlayer.Players.Count - 1]).GetComponent<LobbyItemUI>();
        obj.SetPlayer(player);
        obj.transform.eulerAngles = Vector3.zero;

        ListItems.Add(player, obj);

        //UpdateDetails(GameManager.Instance);
        Debug.Log(player.name + "lobby ui a eklendi");

        //playerCounter++;
        playerCounterText.text = NetworkPlayer.Players.Count + " / 4";
        roomName.text = "Room ID : " + GameObject.FindObjectOfType<NetworkRunner>().SessionInfo.Name;

        //if (playerCounter == 4)
        //{
        //    readyUp.gameObject.SetActive(true);
        //    loadingPlayersRoot.SetActive(false);
        //}
    }

    private void RemovePlayer(NetworkPlayer player)
    {
        if (!ListItems.ContainsKey(player))
            return;

        var obj = ListItems[player];
        if (obj != null)
        {
            Destroy(obj.gameObject);
            ListItems.Remove(player);
        }
        playerCounterText.text = NetworkPlayer.Players.Count + " / 4";

    }

    private void ReadyUpListener()
    {
        var local = NetworkPlayer.Local;
        if (local && local.Object && local.Object.IsValid && !local.IsReady)
        {
            local.RPC_ChangeReadyState(true);
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
            NetworkGameManager.Instance.Rpc_CheckLobbyStart();

        }
    }

    public bool IsAllReady() => NetworkPlayer.Players.Count == 4;


}
