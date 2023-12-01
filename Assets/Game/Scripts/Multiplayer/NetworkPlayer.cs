using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    [Networked] public NetworkString<_32> nickName { get; set; }

    [UnitySerializeField][Networked][Capacity(11)]
    public NetworkLinkedList<NetworkCard> completedHandCards => default;

    public int playInd;
    public bool localPlayer;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();

        localPlayer = Object.HasInputAuthority;

        if (localPlayer)
        {
            nickName = PlayerPrefs.GetString("PlayerName");
            gameControl.myNetworkPlayer = this;
        }

        gameControl.UpdateLobbyPlayerNames();

        if(gameControl.networkHandler.maxPlayer == gameControl.GetNetworkPlayerCount() && gameControl.networkHandler.isHost)
        {
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
            gameControl.networkHandler.SpawnNetworkGlobals();
        }
    }

    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.InputAuthority)]
    public void RPC_PlayInd(int ind)
    {
        playInd = ind;
        gameControl.playerControl.actorControl = gameControl.actorControls[playInd];
        gameControl.playerControl.actorControl.player = true;
    }

    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.InputAuthority)]
    public void RPC_CardsInHand(NetworkCard _card)
    {
        gameControl.actorControls[playInd].cardsInHand.Add(NetworkCardConverter.NetworkCardToCard(_card));   
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_DecidePlayer(bool pass, bool betUp, int ind)
    {
        gameControl.actorControls[ind].pass = pass;
        gameControl.actorControls[ind].betUp = betUp;

        gameControl.actorControls[ind].DecidePlayer();
        gameControl.desicionInd++;
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_TakeCard(bool _fromDeck, int ind)
    {
        if (gameControl.actorControls[ind].player)
            return;

        gameControl.PickCard(gameControl.actorControls[ind], _fromDeck);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_ThrowCard(NetworkCard _card, int ind)
    {
        gameControl.ThrowingCard(NetworkCardConverter.NetworkCardToCard(_card), gameControl.actorControls[ind]);
    }

    public void UpdateNetworkCompletedHandCards()
    {
        List<Card> cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());

        completedHandCards.Clear();
        for (int i = 0; i < cards.Count; i++)
        {
            completedHandCards.Add(NetworkCardConverter.CardToNetworkCard(cards[i]));
        }

        RPC_OpenCompletedHandPanel(playInd);
    }


    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_OpenCompletedHandPanel(int playerInd)
    {
        gameControl.OpenHandCompletedPanel(gameControl.actorControls[playerInd]);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_NextTour()
    {
        gameControl.NextTouring();
        gameControl.networkGlobals.Setup();
    }
}
