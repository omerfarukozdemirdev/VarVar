using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    [Networked] public NetworkString<_32> nickName { get; set; }

    //[UnitySerializeField][Networked][Capacity(11)]
    //public NetworkLinkedList<NetworkCard> completedHandCards => default;

    //[Networked][Capacity(11)]
    //public NetworkLinkedList<byte> completedHandCardsByte => default;

    public int playInd;
    public bool localPlayer;
    public bool gotPlayInd;
    public bool gotPlayerObject;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();

        localPlayer = Object.HasInputAuthority;

        if (localPlayer)
        {
            nickName = PlayerPrefs.GetString("PlayerName");
            gameControl.myNetworkPlayer = this;
        }

        //gameControl.UpdateLobbyPlayerNames();

        if (gameControl.networkHandler.maxPlayer == gameControl.GetNetworkPlayerCount() && gameControl.networkHandler.isHost)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
            gameControl.networkHandler.SpawnNetworkGlobals();
        }
    }

    private void Update()
    {
        CheckGotPlayerObject();
    }


    void CheckGotPlayerObject()
    {
        if (gotPlayerObject)
            return;

        if (Runner.TryGetPlayerObject(Runner.LocalPlayer, out var plObject))
        {
            gotPlayerObject = true;
            FindObjectOfType<GameControl>().UpdateLobbyPlayerNames();
            SetPlayInd();

        }

    }

    void SetPlayInd()
    {
        var index = 0;
        foreach (PlayerRef playerRef in Runner.ActivePlayers)
        {
            if (playerRef == Runner.LocalPlayer)
            {
                playInd = index;
                gameControl.playerControl.actorControl = gameControl.actorControls[playInd];
                gameControl.playerControl.actorControl.player = true;

                gotPlayInd = true;
            }
            index++;
        }
    }

    //[Rpc(sources: RpcSources.Proxies, targets: RpcTargets.InputAuthority)]
    //public void RPC_PlayInd(byte ind)
    //{
    //    playInd = (int)ind;
    //    gameControl.playerControl.actorControl = gameControl.actorControls[playInd];
    //    gameControl.playerControl.actorControl.player = true;

    //    gotPlayInd = true;
    //}


    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_DecidePlayer(byte pass, byte betUp, byte ind)
    {
        gameControl.actorControls[(int)ind].pass = (int)pass == 1;
        gameControl.actorControls[(int)ind].betUp = (int)betUp == 1;

        gameControl.actorControls[(int)ind].DecidePlayer();
        gameControl.desicionInd++;
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_TakeCard(byte _fromDeck, byte ind)
    {
        if (gameControl.actorControls[ind].player)
            return;

        gameControl.PickCard(gameControl.actorControls[(int)ind], (int)_fromDeck == 1);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_ThrowCard(byte _card, byte ind)//(NetworkCard _card, int ind)
    {
        gameControl.ThrowingCard(NetworkCardConverter.IntToCard(_card), gameControl.actorControls[(int)ind]);
    }

    public void UpdateNetworkCompletedHandCards()
    {
        //List<Card> cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());

        //completedHandCardsByte.Clear();
        //for (int i = 0; i < cards.Count; i++)
        //{
        //    completedHandCardsByte.Add((byte)NetworkCardConverter.CardToInt(cards[i]));
        //}

        RPC_OpenCompletedHandPanel((byte)playInd, NetworkCardConverter.CardsToString(gameControl.playerControl.GetUIOrderedCards()));
    }


    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_OpenCompletedHandPanel(byte playerInd, string cardsInHandString)
    {
        gameControl.actorControls[(int)playerInd].cardsInHand = NetworkCardConverter.StringToCards(cardsInHandString);


        gameControl.OpenHandCompletedPanel(gameControl.actorControls[(int)playerInd]);

        if (!gameControl.networkHandler.isHost)
        {
            gameControl.networkGlobals.RPC_HandCompletedCheck((byte)gameControl.myNetworkPlayer.playInd);
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_NextTour()
    {
        gameControl.NextTouring();
        gameControl.networkGlobals.Setup();
    }

}
