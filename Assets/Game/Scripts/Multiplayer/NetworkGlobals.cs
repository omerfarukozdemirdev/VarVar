using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[System.Serializable]
public struct NetworkCard : INetworkStruct
{
    public CardSuit suit;
    public int value;
}

public class NetworkGlobals : NetworkBehaviour
{
    [UnitySerializeField][Networked][Capacity(7)]
    public NetworkLinkedList<NetworkPlayer> orderedNetworkPlayers => default;

    [UnitySerializeField][Networked][Capacity(104)]
    public NetworkLinkedList<NetworkCard> deckCards => default;

    [UnitySerializeField][Networked][Capacity(11)]
    public NetworkLinkedList<NetworkCard> completedHandCards => default;

    [Networked] public int cardDealerInd { get; set; }

    private GameControl gameControl;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();
        gameControl.networkGlobals = this;

        // Oturma düzenini belirle
        if (gameControl.networkHandler.isHost)
        {
            // Aktorlerin oturma düzeni
            NetworkPlayer[] networkPlayers = FindObjectsOfType<NetworkPlayer>();

            for (int i = 0; i < networkPlayers.Length; i++)
            {
                if (networkPlayers[i].localPlayer)
                    orderedNetworkPlayers.Add(networkPlayers[i]);
            }
            for (int i = 0; i < networkPlayers.Length; i++)
            {
                if (!networkPlayers[i].localPlayer)
                    orderedNetworkPlayers.Add(networkPlayers[i]);
            }
            //--------

            gameControl.SetMultiplayerActors();

            // Decki ayarla

            gameControl.CreateDeck();
            gameControl.ShuffleDeck();

            //------------

            // Oyuncuların kartlarını belirle

            gameControl.DealCardsToActors();

            gameControl.actorControls[0].player = true;
            for (int i = 1; i < orderedNetworkPlayers.Count; i++)
            {
                orderedNetworkPlayers[i].RPC_PlayInd(i);

                for (int j = 0; j < gameControl.actorControls[i].cardsInHand.Count; j++)
                {
                    orderedNetworkPlayers[i].RPC_CardsInHand(NetworkCardConverter.CardToNetworkCard(gameControl.actorControls[i].cardsInHand[j]));
                }
            }

            UpdateNetworkDeckCards();

            //


            // Dağıtıcıyı belirle

            gameControl.ChooseRandomCardDealer();
            cardDealerInd = gameControl.cardDealerInd;

            gameControl.SortOrderOfPlayActors();
            //

            //
            RPC_PrepeareStartGame();
            //
        }
    }

    public void UpdateNetworkDeckCards()
    {
        deckCards.Clear();
        for (int i = 0; i < gameControl.deck.Count; i++)
        {
            deckCards.Add(NetworkCardConverter.CardToNetworkCard(gameControl.deck[i]));
        }
    }

    public void UpdateNetworkCompletedHandCards()
    {
        List<Card> cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());

        completedHandCards.Clear();
        for (int i = 0; i < cards.Count; i++)
        {
            completedHandCards.Add(NetworkCardConverter.CardToNetworkCard(cards[i]));
        }

        gameControl.myNetworkPlayer.RPC_OpenCompletedHandPanel(gameControl.myNetworkPlayer.playInd);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_PrepeareStartGame()
    {
        if (!gameControl.networkHandler.isHost)
        {
            gameControl.deck.Clear();
            for (int i = 0; i < deckCards.Count; i++)
                gameControl.deck.Add(NetworkCardConverter.NetworkCardToCard(deckCards[i]));


            gameControl.cardDealerInd = cardDealerInd;

            gameControl.SetMultiplayerActors();


            gameControl.SortOrderOfPlayActors();
        }


        gameControl.PrepeareStartGame();
        gameControl.lobbyUIManager.gameObject.SetActive(false);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_DeckFromThrowed()
    {
        if (!gameControl.networkHandler.isHost)
        {
            gameControl.deck.Clear();
            for (int i = 0; i < deckCards.Count; i++)
                gameControl.deck.Add(NetworkCardConverter.NetworkCardToCard(deckCards[i]));
        }

        gameControl.DeckFromThrowed();
    }


}
