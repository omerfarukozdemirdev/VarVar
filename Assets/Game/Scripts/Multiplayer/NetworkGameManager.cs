using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [UnitySerializeField]
    [Networked]
    [Capacity(150)]
    public NetworkLinkedList<NetworkCard> NetworkThrowedCards => default;

    [Networked(OnChanged = nameof(OnNetworkCardDealerIndChanged))] public int NetworkCardDealerInd { get; set; }

    [Networked(OnChanged = nameof(OnNetworkDesicionIndChanged))] public int DesicionInd { get; set; }

    [Networked(OnChanged = nameof(OnNetworkPlayingIndChanged))] public int NetworkPlayingInd { get; set; }


    public bool Host;

    public GameObject NetworkLastThrowedCard;

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

    private static void OnNetworkDesicionIndChanged(Changed<NetworkGameManager> changed)
    {
        changed.Behaviour.GameControl.desicionInd = changed.Behaviour.DesicionInd;
        //changed.Behaviour.GameControl.ActorDecisiton();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ChangeDesicionInd(int desicitionInd)
    {
        DesicionInd = desicitionInd;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_DecidePlayer()
    {
        GameControl.desicionActors[GameControl.desicionInd].DecidePlayer();
        // GameControl.orderOfPlayActors[GameControl.desicionInd].DecidePlayer();

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateDeck()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            if (GameControl.deck.Count == 0)
            {
                CardListFromNetworkCardList(GameControl.deck, NetworkDeck);
            }
        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RemoveCardFromDeck(int i)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            GameControl.deck.Remove(GameControl.deck[i]);
            NetworkDeck.Remove(NetworkDeck[i]);
        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PlayerTakeCardAnimations()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {

            var throwedCard = FindObjectOfType<TableAnimationControl>().cardCloses.Last();
            iTween.MoveTo(throwedCard.gameObject, iTween.Hash("position", GameControl.actorControls[GameControl.playingInd].actorTransform.GetChild(0).position, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
            iTween.RotateTo(throwedCard.gameObject, iTween.Hash("y", UnityEngine.Random.Range(500, 900), "time", .3f));
            iTween.ScaleTo(throwedCard.gameObject, iTween.Hash("scale", Vector3.one * 1.1f, "time", .3f, "easetype", iTween.EaseType.easeOutBounce, "onComplete", "OpenDrinkButton", "onCompleteTarget", gameObject));
            StartCoroutine(SetCardInvisible(throwedCard.gameObject));
        }


    }

    IEnumerator SetCardInvisible(GameObject card)
    {
        yield return new WaitForSeconds(0.3f);
        card.SetActive(false);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PlayerThrowCardAnimations()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            //var throwedCard = GameObject.Find("ThrowedCardsPos").transform.GetChild(GameControl.throwedCards.Count-1).gameObject;
            //NetworkLastThrowedCard = GameControl.lastThrowedCard;

            NetworkLastThrowedCard = GameControl.throwedCardObjs[GameControl.throwedCards.Count];
            NetworkLastThrowedCard.SetActive(true);

            var throwedCard = NetworkLastThrowedCard;
            throwedCard.transform.SetParent(GameControl.actorControls[GameControl.playingInd].actorTransform);
            throwedCard.transform.localPosition = new Vector3(0, .1f, 0);
            throwedCard.transform.SetParent(GameControl.tableAnimationControl.throwedCardsPos);


            iTween.MoveTo(throwedCard, iTween.Hash("position", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
            iTween.RotateTo(throwedCard, iTween.Hash("y", UnityEngine.Random.Range(500, 900), "time", .3f));
            iTween.ScaleTo(throwedCard, iTween.Hash("scale", Vector3.one * 1.1f, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));

            GameControl.NextActor();
            //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd();

        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_AddSpriteRendererToThrowedCard(NetworkCard networkCard)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkLastThrowedCard = GameControl.throwedCardObjs[GameControl.throwedCards.Count];
            NetworkLastThrowedCard.SetActive(true);

            SpriteRenderer spriteRenderer = NetworkLastThrowedCard.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(NetworkCardToCard(networkCard),GameControl.gameConfig.deckStyles[GameControl.gameConfig.deckStyleInd]);
            spriteRenderer.sortingOrder = NetworkThrowedCards.Count;
            spriteRenderer.size = new Vector2(2.56f, 3.5f);
        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_AddThrowedCards(NetworkCard networkCard)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            //NetworkCardListFromCardList(NetworkThrowedCards, GameControl.throwedCards);
            NetworkThrowedCards.Add(networkCard);
            GameControl.throwedCards.Add(NetworkCardToCard(networkCard));

            //CardListFromNetworkCardList(GameControl.throwedCards, NetworkThrowedCards);
        }


    }

    private static void OnNetworkPlayingIndChanged(Changed<NetworkGameManager> changed)
    {
        changed.Behaviour.GameControl.playingInd = changed.Behaviour.NetworkPlayingInd;
        Debug.Log("testt");
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_UpdateNetworkPlayingInd()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            Debug.Log(NetworkPlayingInd);
            NetworkPlayingInd = NetworkPlayingInd + 1;
            if (NetworkPlayingInd > GameControl.playingActors.Count - 1)
                NetworkPlayingInd = 0;
        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateNetworkPlayingInd(int i)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            Debug.Log(NetworkPlayingInd);
            NetworkPlayingInd =i;

        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PlayCard()
    {
        GameControl.playingActors[GameControl.playingInd].PlayCard();

    }

}
