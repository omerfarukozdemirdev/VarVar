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

            // Decki ayarla

            gameControl.CreateDeck();
            gameControl.ShuffleDeck();

            deckCards.Clear();
            for (int i = 0; i < gameControl.deck.Count; i++)
            {
                deckCards.Add(CardToNetworkCard(gameControl.deck[i]));
            }
                
            //------------


            RPC_StartMultiplayerGame();
        }
    }

    public NetworkCard CardToNetworkCard(Card card)
    {
        NetworkCard newNetworkCard = new NetworkCard() { suit = (CardSuit)card.suit, value = card.value };
        return newNetworkCard;
    }

    public Card NetworkCardToCard(NetworkCard networkCard)
    {
        Card card = new Card() { suit = (CardSuit)networkCard.suit, value = networkCard.value };
        return card;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_StartMultiplayerGame()
    {
        gameControl.StartMultiplayerGame();
    }
}
