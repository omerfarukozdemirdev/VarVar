using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    NetworkGameManager networkGameManager;
    [SerializeField] private ActorControl actorControl;
    public ActorControl ActorControl
    {
        get => actorControl;
        set
        {
            actorControl = value;
        }
    }
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
    [Networked(OnChanged = nameof(OnNetworkPlayerHostChanged))] public NetworkBool Host { get; set; }


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
        networkGameManager = NetworkGameManager.Instance;
        networkGameManager.NetworkPlayerList.Add(this);
        SetPlayer();

        if (Object.HasInputAuthority)
        {
            Local = this;

            PlayerChanged?.Invoke(this);
            RPC_SetPlayerStats(PlayerPrefs.GetString("PlayerName"));
            //ActorControl = FindObjectOfType<GameControl>().actorControls[Players.IndexOf(NetworkPlayer.Local)];
        }

        if (Object.HasStateAuthority && Players.Count == 1)
        {
            networkGameManager.Host = true;
            FindObjectOfType<GameControl>().Host = true;
            RPC_SetHostState(true);
            actorControl.Host = true;
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

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority, InvokeResim = true)]
    private void RPC_SetHostState(NetworkBool networkBool)
    {
        Host = networkBool;
    }

    private static void OnNetworkPlayerHostChanged(Changed<NetworkPlayer> changed)
    {
        changed.Behaviour.networkGameManager.Rpc_UpdateAllPlayerHost();
    }

    public static void RemovePlayer(NetworkRunner runner, PlayerRef p)
    {
        var networkPlayer = Players.FirstOrDefault(x => x.Object.InputAuthority == p);
        //var networkPlayer = NetworkGameManager.Instance.NetworkPlayerList.FirstOrDefault(x => x.Object.InputAuthority == p);

        if (networkPlayer != null)
        {
            NetworkGameManager.Instance?.NetworkPlayerList.Remove(networkPlayer);

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
                    changed.Behaviour.networkGameManager.GameControl.playerControl.actorControl = changed.Behaviour.networkGameManager.GameControl.actorControls[i];
                    changed.Behaviour.networkGameManager.GameControl.playerControl.actorControl.player = true;
                }
                //Debug.Log(NetworkPlayer.Players[i].CardsInHand.Count);
                changed.Behaviour.networkGameManager.CardListFromNetworkCardList(changed.Behaviour.networkGameManager.GameControl.actorControls[i].cardsInHand, NetworkPlayer.Players[i].CardsInHand);
                changed.Behaviour.networkGameManager.CardListFromNetworkCardList(changed.Behaviour.networkGameManager.GameControl.actorControls[i].missingCards, NetworkPlayer.Players[i].MissingCards);
                changed.Behaviour.networkGameManager.CardListFromNetworkCardList(changed.Behaviour.networkGameManager.GameControl.actorControls[i].remainingCards, NetworkPlayer.Players[i].RemainingCards);

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
        changed.Behaviour.networkGameManager.Rpc_UpdateAllPlayerPass();
    }


    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_ChangeBetUpState(NetworkBool state)
    {
        BetUp = state;
    }

    private static void OnNetworkPlayerBetUpChanged(Changed<NetworkPlayer> changed)
    {
        for (int i = 0; i < NetworkPlayer.Players.Count; i++)
        {
            if (NetworkPlayer.Players[i].BetUp)
            {
                changed.Behaviour.networkGameManager.GameControl.actorControls[i].betUp = changed.Behaviour.BetUp;
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_SetHandCompleted(NetworkBool state)
    {
        HandCompleted = state;
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_AddCard(NetworkCard networkCard)
    {
        CardsInHand.Add(networkCard);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_RemoveCard(NetworkCard networkCard)
    {
        CardsInHand.Remove(networkCard);
    }

    void SetPlayer()
    {
        actorControl = networkGameManager.GameControl.actorControls[networkGameManager.NetworkPlayerList.IndexOf(this)];
        actorControl.SetNetworkPlayer(this);
    }

}
