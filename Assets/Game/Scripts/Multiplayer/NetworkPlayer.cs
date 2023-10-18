using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class NetworkPlayer : NetworkBehaviour
{
    public static readonly List<NetworkPlayer> Players = new List<NetworkPlayer>();

    public static Action<NetworkPlayer> PlayerJoined;
    public static Action<NetworkPlayer> PlayerLeft;
    public static Action<NetworkPlayer> PlayerChanged;
    public static NetworkPlayer Local;

    [Networked(OnChanged = nameof(OnStateChanged))] public NetworkBool IsReady { get; set; }
    [Networked(OnChanged = nameof(OnStateChanged))] public NetworkString<_32> Username { get; set; }

    public bool IsLeader => Object != null && Object.IsValid && Object.HasStateAuthority;


    [UnitySerializeField]
    [Capacity(150)]
    [Networked(OnChanged = nameof(OnNetworkCardsInHandChanged))] public NetworkLinkedList<NetworkCard> CardsInHand => default;


    [UnitySerializeField]
    [Networked]
    [Capacity(150)]
    public NetworkLinkedList<NetworkCard> MissingCards => default;



    [UnitySerializeField]
    [Networked]
    [Capacity(150)]
    public NetworkLinkedList<NetworkCard> RemainingCards => default;


    private HandAranger handAranger;

    [SerializeField][Networked] public int HandCompleteStep { get; set; }
    [Networked] public NetworkBool HandCompleted { get; set; }

    [Networked(OnChanged = nameof(OnNetworkPlayerPassChanged))] public NetworkBool Pass { get; set; }
    [Networked(OnChanged = nameof(OnNetworkPlayerBetUpChanged))] public NetworkBool BetUp { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        handAranger = FindObjectOfType<HandAranger>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Spawned()
    {
        base.Spawned();


        Players.Add(this);

        PlayerJoined?.Invoke(this);

        if (Object.HasInputAuthority)
        {
            Local = this;

            PlayerChanged?.Invoke(this);
            RPC_SetPlayerStats("Player_" + (Players.IndexOf(Local) + 1));
            //ActorControl = FindObjectOfType<GameControl>().actorControls[Players.IndexOf(NetworkPlayer.Local)];
        }

        if (IsLeader)
        {
            FindObjectOfType<NetworkGameManager>().Host = true;
            FindObjectOfType<GameControl>().Host = true;

        }

    }

    private void OnDisable()
    {
        // OnDestroy does not get called for pooled objects
        PlayerLeft?.Invoke(this);
        Players.Remove(this);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority, InvokeResim = true)]
    private void RPC_SetPlayerStats(NetworkString<_32> username)
    {
        Username = username;
    }

    public static void RemovePlayer(NetworkRunner runner, PlayerRef p)
    {
        var networkPlayer = Players.FirstOrDefault(x => x.Object.InputAuthority == p);
        if (networkPlayer != null)
        {
            Players.Remove(networkPlayer);
            runner.Despawn(networkPlayer.Object);
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ChangeReadyState(NetworkBool state)
    {
        Debug.Log($"Setting {Object.Name} ready state to {state}");
        IsReady = state;
    }

    private static void OnStateChanged(Changed<NetworkPlayer> changed) => PlayerChanged?.Invoke(changed.Behaviour);

    private static void OnNetworkCardsInHandChanged(Changed<NetworkPlayer> changed)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            for (int i = 0; i < NetworkPlayer.Players.Count; i++)
            {
                if (NetworkPlayer.Players[i].HasInputAuthority)
                {
                    NetworkGameManager.Instance.GameControl.playerControl.actorControl = NetworkGameManager.Instance.GameControl.actorControls[i];
                    NetworkGameManager.Instance.GameControl.playerControl.actorControl.player = true;
                }
                //Debug.Log(NetworkPlayer.Players[i].CardsInHand.Count);
                NetworkGameManager.Instance.CardListFromNetworkCardList(NetworkGameManager.Instance.GameControl.actorControls[i].cardsInHand, NetworkPlayer.Players[i].CardsInHand);
                NetworkGameManager.Instance.CardListFromNetworkCardList(NetworkGameManager.Instance.GameControl.actorControls[i].missingCards, NetworkPlayer.Players[i].MissingCards);
                NetworkGameManager.Instance.CardListFromNetworkCardList(NetworkGameManager.Instance.GameControl.actorControls[i].remainingCards, NetworkPlayer.Players[i].RemainingCards);

            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_ChangePassState(NetworkBool state)
    {
        Pass = state;
    }

    private static void OnNetworkPlayerPassChanged(Changed<NetworkPlayer> changed)
    {


        NetworkGameManager.Instance.GameControl.desicionActors[NetworkGameManager.Instance.DesicionInd - 1].pass = changed.Behaviour.Pass;
        Debug.Log(NetworkGameManager.Instance.DesicionInd - 1);
        Debug.Log(changed.Behaviour.Pass);
        Debug.Log(NetworkGameManager.Instance.GameControl.desicionActors[NetworkGameManager.Instance.DesicionInd - 1].pass);
        Debug.Log(NetworkGameManager.Instance.GameControl.desicionActors[NetworkGameManager.Instance.DesicionInd - 1]);

    }


    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_ChangeBetUpState(NetworkBool state)
    {
        BetUp = state;
    }

    private static void OnNetworkPlayerBetUpChanged(Changed<NetworkPlayer> changed)
    {

        NetworkGameManager.Instance.GameControl.actorControls[NetworkPlayer.Players.IndexOf(NetworkPlayer.Local)].betUp = changed.Behaviour.BetUp;

    }


}
