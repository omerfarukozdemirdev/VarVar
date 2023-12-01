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

    [Networked] public int cardDealerInd { get; set; }

    private GameControl gameControl;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();
        gameControl.networkGlobals = this;

        // Oturma düzenini belirle
        if (gameControl.networkHandler.isHost)
        {
            gameControl.SetMultiplayerActors();


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

            // Decki ayarla

            gameControl.CreateDeck();
            gameControl.ShuffleDeck();

            deckCards.Clear();
            for (int i = 0; i < gameControl.deck.Count; i++)
            {
                deckCards.Add(NetworkCardConverter.CardToNetworkCard(gameControl.deck[i]));
            }

            //------------

            // Oyuncuların kartlarını belirle

            gameControl.DealCardsToActors();

            for(int i = 1; i < orderedNetworkPlayers.Count; i++)
            {
                orderedNetworkPlayers[i].RPC_PlayInd(i);

                for (int j = 0; j < gameControl.actorControls[i].cardsInHand.Count; j++)
                {
                    orderedNetworkPlayers[i].RPC_CardsInHand(NetworkCardConverter.CardToNetworkCard(gameControl.actorControls[i].cardsInHand[j]));
                }
            }

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

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_PrepeareStartGame()
    {
        gameControl.PrepeareStartGame();
        gameControl.lobbyUIManager.gameObject.SetActive(false);
    }

}
