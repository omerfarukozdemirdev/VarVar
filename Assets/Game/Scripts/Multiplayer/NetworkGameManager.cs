using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[System.Serializable]
public struct NetworkCard : INetworkStruct
{
    public CardSuit suit;
    public int value;
}

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    public NetworkUIManager NetworkUIManager;
    public GameControl GameControl;


    [UnitySerializeField]
    [Networked]
    [Capacity(150)]
    public NetworkLinkedList<NetworkCard> NetworkDeck => default;

    [Networked(OnChanged = nameof(OnNetworkCardDealerIndChanged))] public int NetworkCardDealerInd { get; set; }


    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);



    }

    public override void Spawned()
    {
        base.Spawned();

        NetworkUIManager = NetworkUIManager.Instance;
        GameControl = FindObjectOfType<GameControl>();

        NetworkUIManager.Setup();


    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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

    public void NetworkCardListFromCardList(NetworkLinkedList<NetworkCard> networkCardList, List<Card> cardList)
    {
        networkCardList.Clear();
        //int counter = 0;
        foreach (Card card in cardList)
        {
            NetworkCard networkCard = new NetworkCard() { suit = (CardSuit)card.suit, value = card.value };
            //networkCardList.Set(counter, networkCard);
            //counter++;
            networkCardList.Add(networkCard);
        }
    }

    public void CardListFromNetworkCardList(List<Card> cardList, NetworkLinkedList<NetworkCard> networkCardList)
    {
        cardList.Clear();
        foreach (NetworkCard networkCard in networkCardList)
        {
            Card newCard = new Card() { suit = (CardSuit)networkCard.suit, value = networkCard.value };
            cardList.Add(newCard);
        }
    }

    public void UpdateNetworkDeck()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkCardListFromCardList(NetworkDeck, GameControl.deck);
        }
    }


    public void UpdateAllCards(int actor)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkCardListFromCardList(NetworkDeck, GameControl.deck);

            NetworkCardListFromCardList(NetworkPlayer.Players[actor].CardsInHand, GameControl.actorControls[actor].cardsInHand);
            NetworkCardListFromCardList(NetworkPlayer.Players[actor].MissingCards, GameControl.actorControls[actor].missingCards);
            NetworkCardListFromCardList(NetworkPlayer.Players[actor].RemainingCards, GameControl.actorControls[actor].remainingCards);

            NetworkPlayer.Players[actor].HandCompleted = GameControl.actorControls[actor].handCompleted;
            NetworkPlayer.Players[actor].HandCompleteStep = GameControl.actorControls[actor].handCompleteStep;

        }
    }

    private static void OnNetworkCardDealerIndChanged(Changed<NetworkGameManager> changed)
    {
        //changed.Behaviour.GameControl.cardDealerInd = changed.Behaviour.NetworkCardDealerInd;
    }
}
