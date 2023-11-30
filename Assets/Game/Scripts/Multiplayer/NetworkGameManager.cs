using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [UnitySerializeField]
    [Networked]
    [Capacity(150)]
    public NetworkLinkedList<NetworkCard> NetworkCompletedHand => default;

    //[Networked(OnChanged = nameof(OnNetworkCardDealerIndChanged))] 
    [Networked] public int NetworkCardDealerInd { get; set; }

    //[Networked(OnChanged = nameof(OnNetworkDesicionIndChanged))] 
    [Networked] public int DesicionInd { get; set; }

    [Networked(OnChanged = nameof(OnNetworkPlayingIndChanged))] public int NetworkPlayingInd { get; set; }

    //[Networked(OnChanged = nameof(OnNetworkGameCounterChanged))]
    [Networked] public int NetworkGameCounter { get; set; }

    //[Networked(OnChanged = nameof(OnNetworkGameLimitChanged))]
    [Networked] public int NetworkGameLimit { get; set; }
    [Networked(OnChanged = nameof(OnNetworkFirstCardChanged))] public NetworkCard NetworkFirstCard { get; set; }
    //[Networked(OnChanged = nameof(OnNetworkDeckCountChanged))] public int NetworkDeckCount { get; set; }


    [UnitySerializeField]
    [Networked]
    [Capacity(10)]
    public NetworkLinkedList<NetworkPlayer> NetworkPlayerList => default;



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
        // DontDestroyOnLoad(gameObject);



    }

    public override void Spawned()
    {
        base.Spawned();

        //NetworkUIManager = NetworkUIManager.Instance;
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
        foreach (Card card in cardList)
        {
            NetworkCard networkCard = new NetworkCard() { suit = (CardSuit)card.suit, value = card.value };

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
            //NetworkDeckCount = NetworkDeck.Count();
            if (NetworkDeck.Count() == 68)
            {
                Rpc_ResetDeck();
            }
        }
    }


    public void UpdateAllCards(int actor)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkCardListFromCardList(NetworkDeck, GameControl.deck);
            //NetworkDeckCount = NetworkDeck.Count();
            if (NetworkDeck.Count() == 68)
            {
                Rpc_ResetDeck();
            }
            NetworkCardListFromCardList(NetworkPlayer.Players[actor].CardsInHand, GameControl.actorControls[actor].cardsInHand);
            NetworkCardListFromCardList(NetworkPlayer.Players[actor].MissingCards, GameControl.actorControls[actor].missingCards);
            NetworkCardListFromCardList(NetworkPlayer.Players[actor].RemainingCards, GameControl.actorControls[actor].remainingCards);

            NetworkPlayer.Players[actor].HandCompleted = GameControl.actorControls[actor].handCompleted;
            NetworkPlayer.Players[actor].HandCompleteStep = GameControl.actorControls[actor].handCompleteStep;

        }
    }

    //private static void OnNetworkCardDealerIndChanged(Changed<NetworkGameManager> changed)
    //{
    //    changed.Behaviour.GameControl.cardDealerInd = changed.Behaviour.NetworkCardDealerInd;
    //}

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangeCardDealerInd()
    {
        GameControl.cardDealerInd = NetworkCardDealerInd;
    }

    //private static void OnNetworkDesicionIndChanged(Changed<NetworkGameManager> changed)
    //{
    //    changed.Behaviour.GameControl.desicionInd = changed.Behaviour.DesicionInd;
    //}

    //[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    //public void RPC_ChangeDesicionInd(int desicionInd)
    //{
    //    GameControl.desicionInd = desicionInd;
    //}

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    //public void RPC_ChangeDesicionIndAll()
    //{
    //    GameControl.desicionInd = DesicionInd;
    //}

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_DecidePlayer()
    {
        GameControl.desicionActors[GameControl.desicionInd].DecidePlayer();

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
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
    public void Rpc_ResetDeck()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            CardListFromNetworkCardList(GameControl.deck, NetworkDeck);
        }
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RemoveCardFromDeck(int i)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            GameControl.deck.Remove(GameControl.deck[i]);
            NetworkDeck.Remove(NetworkDeck[i]);
            //NetworkDeckCount = NetworkDeck.Count();
            if (NetworkDeck.Count() == 68)
            {
                Rpc_ResetDeck();
            }
        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PlayerTakeCardAnimations()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {



            FindObjectOfType<TableAnimationControl>().cardCloses.Remove(FindObjectOfType<TableAnimationControl>().cardCloses.Last());
            var throwedCard = FindObjectOfType<TableAnimationControl>().cardCloses.Last();
            if (GameControl.playingActors[GameControl.playingInd] != GameControl.playerControl.actorControl)
            {
                iTween.MoveTo(throwedCard.gameObject, iTween.Hash("position", GameControl.playingActors[GameControl.playingInd].actorTransform.GetChild(0).position, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
                iTween.RotateTo(throwedCard.gameObject, iTween.Hash("y", UnityEngine.Random.Range(500, 900), "time", .3f));
                iTween.ScaleTo(throwedCard.gameObject, iTween.Hash("scale", Vector3.one * 1.1f, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));

            }
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
            var tempPlayingID = GameControl.playingInd - 1;
            if (tempPlayingID < 0)
            {
                tempPlayingID = GameControl.playingActors.Count - 1;
            }

            NetworkLastThrowedCard = GameControl.throwedCardObjs[GameControl.throwedCards.Count];
            NetworkLastThrowedCard.SetActive(true);

            var throwedCard = NetworkLastThrowedCard;
            throwedCard.transform.SetParent(GameControl.playingActors[tempPlayingID].actorTransform);
            throwedCard.transform.localPosition = new Vector3(0, .1f, 0);
            throwedCard.transform.SetParent(GameControl.tableAnimationControl.throwedCardsPos);


            iTween.MoveTo(throwedCard, iTween.Hash("position", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
            iTween.RotateTo(throwedCard, iTween.Hash("y", UnityEngine.Random.Range(500, 900), "time", .3f));
            iTween.ScaleTo(throwedCard, iTween.Hash("scale", Vector3.one * 1.1f, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));


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
            spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(NetworkCardToCard(networkCard), GameControl.gameConfig.deckStyles[GameControl.gameConfig.deckStyleInd]);
            // spriteRenderer.sortingOrder = NetworkThrowedCards.Count;
            spriteRenderer.sortingOrder = GameControl.throwedCards.Count;
            spriteRenderer.size = new Vector2(2.56f, 3.5f);
        }


    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_AddThrowedCards(NetworkCard networkCard)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkThrowedCards.Add(networkCard);
            GameControl.throwedCards.Add(NetworkCardToCard(networkCard));

        }


    }

    private static void OnNetworkPlayingIndChanged(Changed<NetworkGameManager> changed)
    {
        changed.Behaviour.GameControl.playingInd = changed.Behaviour.NetworkPlayingInd;
        if (changed.Behaviour.GameControl.throwedCards.Count > 0)
        {
            NetworkGameManager.Instance?.Rpc_PlayerThrowCardAnimations();

        }

        if (changed.Behaviour.GameControl.playingActors.Count > 0)
        {
            changed.Behaviour.GameControl.NextActor();

        }


    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_UpdateNetworkPlayingInd()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            //Debug.Log(NetworkPlayingInd);
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
            //Debug.Log(NetworkPlayingInd);
            NetworkPlayingInd = i;

        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PlayCard()
    {
        GameControl.playingActors[GameControl.playingInd].PlayCard();

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ShowHandCompletedPanel(int winID)
    {
        Debug.Log("3");

        foreach (NetworkPlayer networkPlayer in NetworkPlayer.Players)
        {
            Debug.Log("4");

            if (!networkPlayer.HandCompleted)
            {
                Debug.Log("5");

                FindObjectOfType<HandCompletedPanel>(true).OpenPanel(GameControl.actorControls[winID]);
            }
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PickedCardFromThrowed()
    {
        StartCoroutine(PickCardFromThrowedAnimation());
    }

    IEnumerator PickCardFromThrowedAnimation()
    {
        // GameControl.lastThrowedCard = GameControl.throwedCardObjs[GameControl.throwedCards.Count];
        Debug.Log("picked card from throwed");
        if (GameControl.playingActors[GameControl.playingInd] != GameControl.playerControl.actorControl)
        {
            iTween.MoveTo(GameControl.lastThrowedCard, iTween.Hash("position", GameControl.playingActors[GameControl.playingInd].actorTransform.GetChild(0).position, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));
            iTween.RotateTo(GameControl.lastThrowedCard, iTween.Hash("y", 0, "time", .2f));
            iTween.ScaleTo(GameControl.lastThrowedCard, iTween.Hash("scale", Vector3.one * .5f, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));
            yield return new WaitForSeconds(.2f);
            GameControl.lastThrowedCard.SetActive(false);
        }
        GameControl.lastThrowedCard.SetActive(false);




    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RemoveThrowedCards(NetworkCard networkCard)
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            //GameControl.throwedCards.Remove(GameControl.throwedCards.Last());
            // GameControl.throwedCards.Remove(NetworkCardToCard(networkCard));
            NetworkThrowedCards.Remove(networkCard);
            NetworkGameManager.Instance?.CardListFromNetworkCardList(GameControl.throwedCards, NetworkGameManager.Instance.NetworkThrowedCards);

            // CardListFromNetworkCardList(GameControl.throwedCards, NetworkThrowedCards);
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateNetworkLastThrowedCard()
    {
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkLastThrowedCard = GameControl.throwedCardObjs[GameControl.throwedCards.Count];
            GameControl.lastThrowedCard = NetworkLastThrowedCard;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NextTour()
    {

        GameControl.NextTour();

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetAllPlayer()
    {
        foreach (NetworkPlayer networkPlayer in NetworkPlayer.Players)
        {
            networkPlayer.Pass = false;

        }
        GameControl.lastThrowedCard = null;
    }

    //[Rpc(RpcSources.All, RpcTargets.All)]
    //public void Rpc_UpdateAllPlayerPass()
    //{
    //    for (int i = 0; i < NetworkPlayer.Players.Count; i++)
    //    {
    //        GameControl.actorControls[i].pass = NetworkPlayer.Players[i].Pass;

    //    }
    //}

    //[Rpc(RpcSources.All, RpcTargets.All)]
    //public void Rpc_UpdateAllPlayerHost()
    //{
    //    for (int i = 0; i < NetworkPlayer.Players.Count; i++)
    //    {
    //        GameControl.actorControls[i].Host = NetworkPlayer.Players[i].Host;

    //    }
    //}

    //private static void OnNetworkGameCounterChanged(Changed<NetworkGameManager> changed)
    //{
    //    changed.Behaviour.GameControl.gameCounter = changed.Behaviour.NetworkGameCounter;
    //}

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateGameCounter()
    {
        GameControl.gameCounter = NetworkGameCounter;
    }

    //private static void OnNetworkGameLimitChanged(Changed<NetworkGameManager> changed)
    //{
    //    changed.Behaviour.GameControl.gameLimit = changed.Behaviour.NetworkGameLimit;
    //}

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateGameLimit()
    {
        GameControl.gameLimit = NetworkGameLimit;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_PlayingActorStartAnimation()
    {
        for (int i = 0; i < GameControl.actorControls.Count; i++)
        {
            iTween.Stop(GameControl.actorControls[i].transform.GetChild(0).gameObject);
        }
        iTween.ScaleTo(GameControl.playingActors[GameControl.playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "time", .6f, "easetype", iTween.EaseType.linear, "loopType", iTween.LoopType.pingPong));

    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateNetworkCompletedHand()
    {
        NetworkCardListFromCardList(NetworkCompletedHand, GameControl.CompletedHand);

    }

    private static void OnNetworkFirstCardChanged(Changed<NetworkGameManager> changed)
    {
        changed.Behaviour.GameControl.NetworkFirstGroundCard();
    }

    private static void OnNetworkDeckCountChanged(Changed<NetworkGameManager> changed)
    {
        //if (changed.Behaviour.NetworkDeckCount == 68)
        //{
        //    changed.Behaviour.Rpc_ResetDeck();
        //}
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_NewGame()
    {

        GameControl.FriendsModeNewGame();

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_AddCardToCompletedHand(NetworkCard networkCard)
    {

        NetworkCompletedHand.Add(networkCard);
        // NetworkCompletedHandCardsCount = NetworkCompletedHand.Count();

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_ClearCompletedHand()
    {
        NetworkCompletedHand.Clear();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_BetUp(int i)
    {
        GameControl.BetUP(i);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_BetUpTurn(bool value)
    {
        GameControl.betUpTurn = value;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateBetUp(bool value)
    {
        GameControl.betUp = value;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateDesicionActors()
    {
        GameControl.UpdateDesicitonActors();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateNewDesicitonActors(int i)
    {
        if (!GameControl.newDesicitonActors.Contains(GameControl.orderOfPlayActors[i]))
        {
            GameControl.newDesicitonActors.Add(GameControl.orderOfPlayActors[i]);

        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_CheckLastPlayer()
    {
        GameControl.CheckLastPlayer();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_UpdateNetworkPassCounter(int i)
    {
        GameControl.networkPassCounter = i;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_RemoveNetworkPlayer(NetworkPlayer networkPlayer)
    {

        //NetworkPlayerList.Remove(networkPlayer);

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void Rpc_CheckLobbyStart()
    {
        //var lobbyUI = GameObject.FindObjectOfType<LobbyUI>();
        if (GameControl.LobbyPanel.GetComponent<LobbyUI>().IsAllReady())
        {
            NetworkUIManager.Instance.CloseLobbyPanel();
            FindObjectOfType<GameControl>().Invoke("FriendsModeStartGame", 1f);
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;
        }
    }
}
