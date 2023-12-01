using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject desicitonPanel;
    [SerializeField] GameObject desicitonPanelBetUpBTN;
    [SerializeField] Text desicitonPanelBetText;
    [SerializeField] GameObject rewardBetUp;
    [SerializeField] Text rewardBetUpText;
    [SerializeField] Text rewardMoneyText;
    [SerializeField] Text deckCountText;

    public GameObject completeHandWarningPanel;
    public GameObject completeHandWarningPanelWarning;

    public int rewardMoney;

    public Transform moneyAtractorParent;

    [Space]
    public GameConfig gameConfig;

    public List<ActorControl> actorControls = new List<ActorControl>();
    public List<ActorControl> orderOfPlayActors = new List<ActorControl>();
    public List<ActorControl> desicionActors = new List<ActorControl>();
    public List<ActorControl> playingActors = new List<ActorControl>();
    public List<ActorControl> newDesicitonActors = new List<ActorControl>();

    public List<Card> deck;
    public List<Card> throwedCards = new List<Card>();
    public GameObject[] throwedCardObjs;
    public GameObject lastThrowedCard;
    [SerializeField] GameObject throwedCardBtn;
    [SerializeField] GameObject deckCardBtn;

    public int cardDealerInd;
    public int orderOfPlayInd;
    public int desicionInd;
    public int playingInd;

    public int passCount;
    public bool betUp;
    public bool betUpTurn;
    [SerializeField] private ActorControl betUPActor;

    public TableAnimationControl tableAnimationControl;
    [HideInInspector] public PlayerControl playerControl;

    [SerializeField] List<int> values = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

    [HideInInspector] public MakeNoise makeNoise;

    public int gameCounter;
    public int gameLimit;
    public Text gameTourText;
    public CoinController coinController;
    public DrinkController drinkController;
    public int drinkCounter;
    [SerializeField] GameObject statisticPanel;

    //public GameObject playerWinPanel;
    //public GameObject ClosedRoomMenuPanel;
    //public Vector3[] actorPositions;
    //public Vector3[] actorRotations;
    //public GameObject[] actorLocations;
    //public List<int> NewPlayerIndexList = new List<int>();

    //public List<Card> CompletedHand = new List<Card>();

    //public bool Host;
    //public int networkPassCounter;
    public LobbyUIManager lobbyUIManager;

    public bool TestMode;

    public NetworkHandler networkHandler;
    public NetworkPlayer myNetworkPlayer;
    public NetworkGlobals networkGlobals;

    private void Awake()
    {
        tableAnimationControl = FindObjectOfType<TableAnimationControl>();
        playerControl = FindObjectOfType<PlayerControl>();
        makeNoise = FindObjectOfType<MakeNoise>();

        //completeHandBtn.SetActive(false);
        completeHandWarningPanel.SetActive(false);

        throwedCardObjs = new GameObject[52];
        for (int i = 0; i < throwedCardObjs.Length; i++)
        {
            throwedCardObjs[i] = Instantiate(Resources.Load("CardSprite")) as GameObject;
            throwedCardObjs[i].SetActive(false);
        }

        actorControls[0].actorAvatar.sprite = gameConfig.avatars[gameConfig.avatarInd];

        string pName = "Guest";

        if (PlayerPrefs.HasKey("PlayerName"))
            pName = PlayerPrefs.GetString("PlayerName");

        actorControls[0].actorName = pName;

        gameCounter = 1;
        gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();

        lobbyUIManager.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (GameManager.Instance.IsMultiplayer())
        {
            networkHandler = FindObjectOfType<NetworkHandler>();
            networkHandler.StartQuickGame();

            lobbyUIManager.gameObject.SetActive(true);
            return;
        }

        Invoke("StartGame", .5f);
        //switch (GameManager.Instance.CurrentGameMode)
        //{
        //    case GameManager.GameMode.Quick:
        //        Invoke("StartGame", .5f);
        //        break;
        //    case GameManager.GameMode.Friends:
        //        break;
        //    case GameManager.GameMode.Tournament:
        //        break;
        //}
    }

    private void Update()
    {
        if (deck.Count == 0 || !tableAnimationControl.cardCloseParent.gameObject.activeSelf)
            deckCountText.text = "";
        else
            deckCountText.text = deck.Count.ToString();
    }

    void ResetValues()
    {
        for (int i = 0; i < throwedCardObjs.Length; i++)
        {
            throwedCardObjs[i].transform.SetParent(null);
            throwedCardObjs[i].SetActive(false);
            throwedCardObjs[i].transform.position = Vector3.zero;
            throwedCardObjs[i].transform.rotation = Quaternion.Euler(Vector3.zero);
            throwedCardObjs[i].transform.localScale = Vector3.one;
            iTween.Stop(throwedCardObjs[i]);
        }

        orderOfPlayActors = new List<ActorControl>();
        desicionActors = new List<ActorControl>();
        playingActors = new List<ActorControl>();
        deck = new List<Card>();
        throwedCards = new List<Card>();
        orderOfPlayInd = 0;
        desicionInd = 0;
        playingInd = 0;

        betUp = false;
        SetRewardBetUpText();

        //NetworkGameManager.Instance?.Rpc_UpdateBetUp(false);

        betUpTurn = false;
        betUPActor = null;

        cardDealerInd++;
        if (cardDealerInd > actorControls.Count - 1)
            cardDealerInd = 0;

        rewardMoney = 0;
        passCount = 0;

        //drinkController.drinkButton.SetActive(false);
        newDesicitonActors.Clear();
        //networkPassCounter = 0;
        /*
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            for (int i = 0; i < playingActors.Count; i++)
            {
                // iTween.Stop(playingActors[i].transform.GetChild(0).gameObject);
                //iTween.ScaleTo(playingActors[i].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "time", 0, "easetype", iTween.EaseType.easeOutQuad));
            }

            if (Host)
            {
                NetworkPlayer.Local.RPC_ChangeDesicionInd(0);
                //NetworkGameManager.Instance?.RPC_ChangeDesicionIndAll();

                NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd(0);
                NetworkGameManager.Instance.NetworkThrowedCards.Clear();
            }
        }
        */
    }

    public void Menu()
    {
        if (GameManager.Instance.IsMultiplayer())
            networkHandler.Disconnect();

        FindObjectOfType<MakeNoise>().PlaySFX(26, 0);
        gameConfig.cardDealerInd = -1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void NextTourQuick()
    {
        NextTour();
    }

    //public void NextTourFriends()
    //{
    //    NetworkGameManager.Instance.RPC_ResetAllPlayer();
    //    NetworkGameManager.Instance.RPC_NextTour();
    //}

    public void NextTour()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(9, 0);

        //cardDealerInd++;
        //if (cardDealerInd > actorControls.Count - 1)
        //    cardDealerInd = 0;

        //gameConfig.cardDealerInd = cardDealerInd;
        //PlayerPrefs.SetInt("CD", cardDealerInd);

        //UnityEngine.SceneManagement.SceneManager.LoadScene(2);

        actorControls.ForEach(x => x.ResetValues());

        gameCounter++;

        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance.NetworkGameCounter = gameCounter;
        //    NetworkGameManager.Instance?.RPC_UpdateGameCounter();
        //}

        FindObjectOfType<HandCompletedPanel>(true).ClosePanel();

        //for (int i = 0; i < throwedCardObjs.Length; i++)
        //{
        //    throwedCardObjs[i].transform.SetParent(null);
        //    throwedCardObjs[i].SetActive(false);
        //    throwedCardObjs[i].transform.position = Vector3.zero;
        //    throwedCardObjs[i].transform.rotation = Quaternion.Euler(Vector3.zero);
        //    throwedCardObjs[i].transform.localScale = Vector3.one;
        //    iTween.Stop(throwedCardObjs[i]);
        //}
        ResetValues();

        gameConfig.cardDealerInd = cardDealerInd;

        tableAnimationControl.Reset();
        playerControl.ResetValues();


        Invoke("StartGame", 1f);
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    Invoke("StartGame", 1f);

        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance.NetworkCardDealerInd = cardDealerInd;

        //    NetworkGameManager.Instance?.RPC_ChangeCardDealerInd();
        //    for (int i = 0; i < playingActors.Count; i++)
        //    {
        //        // iTween.Stop(playingActors[i].transform.GetChild(0).gameObject);
        //        iTween.ScaleTo(playingActors[i].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "time", 0, "easetype", iTween.EaseType.easeOutQuad));
        //    }

        //    Invoke("FriendsModeStartGame", 1f);
        //}
    }

    //public void Rpc_NewGame()
    //{
    //    NetworkGameManager.Instance?.Rpc_NewGame();
    //}

    //public void FriendsModeNewGame()
    //{
    //    gameCounter = 0;
    //    NetworkGameManager.Instance.NetworkGameCounter = gameCounter;
    //    NetworkGameManager.Instance?.RPC_UpdateGameCounter();
    //    NetworkGameManager.Instance.Rpc_UpdateNetworkPassCounter(0);
    //    NetworkGameManager.Instance.RPC_ResetAllPlayer();
    //    NextTour();
    //}

    //public void FriendsModeStartGame()
    //{
    //    if (NetworkPlayer.Local)
    //    {
    //        SetTable();

    //    }

    //    if (NetworkPlayer.Local.IsLeader)
    //    {
    //        CreateDeck();
    //        ShuffleDeck();
    //        DealCardsToActors();
    //        NetworkGameManager.Instance.NetworkGameLimit = gameLimit;
    //        NetworkGameManager.Instance.RPC_UpdateGameLimit();
    //        NetworkGameManager.Instance.Rpc_UpdateDeck();
    //        NetworkPlayer.Local.RPC_UpdateAllPlayerHost();
    //    }


    //    ChooseRandomCardDealer();
    //    SortOrderOfPlayActors();
    //    DisableEnableTakeCardBtns(false);
    //    //playingInd = 0;

    //    if (networkPassCounter != orderOfPlayActors.Count - 1)
    //    {
    //        tableAnimationControl.StartGame();
    //    }


    //    for (int i = 0; i < NetworkPlayer.Players.Count; i++)
    //    {
    //        NetworkGameManager.Instance.GameControl.actorControls[i].actorName = NetworkPlayer.Players[i].Username.ToString();
    //    }


    //    gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();
    //}

    //public void SetTable()
    //{
    //    //oyuncuları masaya doğru sıraya göre oturtma
    //    if (NetworkPlayer.Local)
    //    {

    //        if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 0)
    //        {
    //            NewPlayerIndexList = new List<int>() { 0, 1, 2, 3 };
    //        }
    //        else if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 1)
    //        {
    //            NewPlayerIndexList = new List<int>() { 3, 0, 1, 2 };
    //        }
    //        else if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 2)
    //        {
    //            NewPlayerIndexList = new List<int>() { 2, 3, 0, 1 };
    //        }
    //        else if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 3)
    //        {
    //            NewPlayerIndexList = new List<int>() { 1, 2, 3, 0 };
    //        }



    //        for (int i = 0; i < NetworkPlayer.Players.Count; i++)
    //        {
    //            actorLocations[i].transform.position = actorPositions[NewPlayerIndexList[i]];
    //            actorLocations[i].transform.rotation = Quaternion.Euler(actorRotations[NewPlayerIndexList[i]]);
    //            actorControls[i].SetNameText(NetworkPlayer.Players[i].Username.ToString());
    //            // 0-1-2-3 for 1
    //            // 3-0-1-2 for 2
    //            // 2-3-0-1 for 3
    //            // 1-2-3-1 for 4
    //        }
    //    }
    //}

    void StartGame()
    {
        CreateDeck();
        ShuffleDeck();

        DealCardsToActors();

        ChooseRandomCardDealer();

        SortOrderOfPlayActors();

        PrepeareStartGame();
    }

    public void PrepeareStartGame()
    {
        DisableEnableTakeCardBtns(false);

        tableAnimationControl.StartGame();

        gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();
    }

    public void DisableEnableTakeCardBtns(bool tf)
    {
        throwedCardBtn.SetActive(tf);
        deckCardBtn.SetActive(tf);
    }

    public void CreateDeck()
    {
        deck = new List<Card>(); // Kart listesini başlat

        // İki adet 52 kartlık desteyi oluştur
        for (int i = 0; i < 2; i++)
        {
            // Her bir sıra için kartları oluştur
            for (int _suit = 1; _suit < 5; _suit++)
            {
                // Her bir değer için kartları oluştur
                for (int _value = 1; _value <= 13; _value++)
                {
                    // Yeni kartı oluştur
                    Card newCard = new Card() { suit = (CardSuit)_suit, value = _value };
                    // Desteye ekle
                    deck.Add(newCard);
                }
            }
        }
    }

    public void ShuffleDeck()
    {
        // Kartları karıştır
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    Card FindCardInDeck(Card card)
    {
        for (int d = 0; d < deck.Count; d++)
        {
            if (deck[d].suit == card.suit &&
                deck[d].value == card.value)
            {
                return deck[d];
            }
        }

        return null;
    }

    public void DealCardsToActors()
    {
        values = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
        for (int i = 1; i < actorControls.Count; i++)
        {
            List<Card> predefinedCards = new List<Card>(PredefinedCards());
            int remainingCards = 9 - predefinedCards.Count;

            List<Card> randomCards = new List<Card>();
            for (int c = 0; c < remainingCards; c++)
            {
                randomCards.Add(deck[0]);
                deck.Remove(deck[0]);

            }

            actorControls[i].cardsInHand = new List<Card>();

            for (int p = 0; p < predefinedCards.Count; p++)
            {
                actorControls[i].cardsInHand.Add(predefinedCards[p]);
            }
            for (int r = 0; r < randomCards.Count; r++)
            {
                actorControls[i].cardsInHand.Add(randomCards[r]);
            }

            actorControls[i].ArrangeHand();

        }

        List<Card> cards = new List<Card>();

        for (int c = 0; c < 9; c++)
        {
            cards.Add(deck[0]);
            deck.Remove(deck[0]);

        }

        actorControls[0].cardsInHand = new List<Card>(cards);
        actorControls[0].ArrangeHand();

        //for (int i = 0; i < actorControls.Count; i++)
        //{
        //    NetworkGameManager.Instance?.UpdateAllCards(i);
        //}
    }

    List<Card> PredefinedCards()
    {
        List<Card> predefinedCards = new List<Card>();

        // Matches
        int val = values[Random.Range(0, values.Count)];
        values.Remove(val);

        int count = Random.Range(1, 3); // 2 - 5

        if (count == 1)
            return predefinedCards;

        for (int i = 0; i < count; i++)
        {
            Card card = new Card() { suit = CardSuit.Spades, value = val };

            switch (i)
            {
                case 1:
                    card.suit = CardSuit.Diamonds;
                    break;
                case 2:
                    card.suit = CardSuit.Clubs;
                    break;
                case 3:
                    card.suit = CardSuit.Hearts;
                    break;
            }

            Card c = FindCardInDeck(card);
            if (c != null)
            {
                predefinedCards.Add(c);
                deck.Remove(c);

            }
        }

        return predefinedCards;
    }

    public void ChooseRandomCardDealer()
    {
        //if (PlayerPrefs.HasKey("CD"))
        //    cardDealerInd = PlayerPrefs.GetInt("CD");
        //else
        //    cardDealerInd = Random.Range(0, actorControls.Count);

        if (gameConfig.cardDealerInd != -1)
        {
            cardDealerInd = gameConfig.cardDealerInd;
        }
        else
        {
            cardDealerInd = Random.Range(0, gameLimit);
            //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            //{
            //    cardDealerInd = 0;
            //}
        }
    }

    public void SortOrderOfPlayActors()
    {
        orderOfPlayActors = new List<ActorControl>();

        int startInd = cardDealerInd;
        //int startInd;

        //startInd = 0;
        //startInd = cardDealerInd;

        for (int i = 0; i < actorControls.Count; i++)
        {
            startInd++;
            if (startInd > actorControls.Count - 1)
                startInd = 0;

            orderOfPlayActors.Add(actorControls[startInd]);
        }
    }

    public void StartBets()
    {
        actorControls[cardDealerInd].SetMoney(200);
        orderOfPlayActors[orderOfPlayInd].SetMoney(500);

        SetDesitionActors();
    }

    void SetDesitionActors()
    {
        desicionActors = new List<ActorControl>();

        for (int i = 1; i < orderOfPlayActors.Count; i++)
            desicionActors.Add(orderOfPlayActors[i]);

        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    for (int i = 1; i < orderOfPlayActors.Count; i++)
        //        desicionActors.Add(orderOfPlayActors[i]);
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    for (int i = 1; i < orderOfPlayActors.Count; i++)
        //        desicionActors.Add(orderOfPlayActors[i]);
        //}

        desicionInd = 0;
    }


    public void UpdateDesicitonActors()
    {
        desicionActors = new List<ActorControl>(newDesicitonActors);
    }

    public void ActorDecisiton()
    {
        if (desicionInd > desicionActors.Count - 1)
        {
            // Debug.Log(playingInd);

            if (betUp && !betUpTurn)
            {
                newDesicitonActors = new List<ActorControl>();

                for (int i = 0; i < orderOfPlayActors.Count - 1; i++)
                    if (!orderOfPlayActors[i].pass && orderOfPlayActors[i].moneyIn < 1000 && betUPActor != orderOfPlayActors[i])
                    {
                        newDesicitonActors.Add(orderOfPlayActors[i]);
                        //NetworkGameManager.Instance?.Rpc_UpdateNewDesicitonActors(i);
                    }

                desicionActors = new List<ActorControl>(newDesicitonActors);
                //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
                //{
                //    desicionActors = new List<ActorControl>(newDesicitonActors);
                //}
                //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
                //{
                //    NetworkGameManager.Instance.Rpc_UpdateDesicionActors();
                //}

                if (desicionActors.Count > 0)
                {
                    desicionInd = 0;
                    //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
                    //{
                    //    NetworkPlayer.Local.RPC_ChangeDesicionInd(desicionInd);
                    //    //NetworkGameManager.Instance?.RPC_ChangeDesicionIndAll();
                    //}
                    betUpTurn = true;
                    //NetworkGameManager.Instance?.Rpc_BetUpTurn(true);
                    ActorDecisiton();
                    return;
                }
            }

            for (int i = 0; i < orderOfPlayActors.Count; i++)
                if (orderOfPlayActors[i] == actorControls[cardDealerInd])
                {
                    if (i < orderOfPlayActors.Count - 1)
                        playingInd = i + 1;
                    else
                        playingInd = 0;
                   // NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd(playingInd);
                }


            while (orderOfPlayActors[playingInd].pass)
            {
                playingInd++;
                //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd(playingInd);

                if (playingInd > orderOfPlayActors.Count - 1)
                    playingInd = 0;
                //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd(playingInd);
            }


            playingActors = new List<ActorControl>();
            for (int i = 0; i < orderOfPlayActors.Count; i++)
            {
                orderOfPlayActors[i].DisableSpeechBaloon();

                if (!orderOfPlayActors[i].pass)
                    playingActors.Add(orderOfPlayActors[i]);
            }
            // Debug.Log(playingInd);

            StartCoroutine(StartPlaying());
            //if (networkPassCounter != orderOfPlayActors.Count - 1)
            //{
            //    StartCoroutine(StartPlaying());
            //}
            return;
        }

        if (desicionActors[desicionInd].player)
        {
            OpenDesicitonPanel();
        }
        else
        {
            if (GameManager.Instance.IsMultiplayer())
            {
                desicionActors[desicionInd].BlinkAvatar();
            }
            else
            {
                if (!desicionActors[desicionInd].pass)
                    desicionActors[desicionInd].DecideBet();

                desicionInd++;
            }

            //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            //{
            //    if (!desicionActors[desicionInd].pass)
            //        desicionActors[desicionInd].DecideBet();

            //    desicionInd++;
            //}
        }
    }

    public void BetUP(ActorControl actorControl)
    {
        if (betUp)
            return;

        betUp = true;
        betUPActor = actorControl;

        SetRewardBetUpText();
    }

    public void BetUP(int i)
    {
        if (betUp)
            return;

        betUp = true;
        //NetworkGameManager.Instance.Rpc_UpdateBetUp(true);
        //Debug.Log(i);
        betUPActor = desicionActors[i];

        SetRewardBetUpText();
        BlinkRewardBetUpText();
    }

    void SetRewardBetUpText()
    {
        if (betUp)
            rewardBetUpText.text = "4";
        else
            rewardBetUpText.text = "1";
    }

    void BlinkRewardBetUpText()
    {
        rewardBetUp.SetActive(false);
        rewardBetUp.SetActive(true);
    }

    IEnumerator StartPlaying()
    {
        //Debug.Log("start");
        yield return new WaitForSeconds(.1f);

        makeNoise.PlaySFX(12, 0);
        for (int i = 0; i < orderOfPlayActors.Count; i++)
        {
            if (orderOfPlayActors[i].moneyIn > 0)
                orderOfPlayActors[i].MoneyAtractor();

            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(1.2f);

        makeNoise.PlaySFX(13, 0);

        StartCoroutine(FirstGroundCard());
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    StartCoroutine(FirstGroundCard());
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance.NetworkFirstCard = NetworkGameManager.Instance.NetworkDeck[0];
        //}
        yield return new WaitForSeconds(2.5f);

        NextActor();
        //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd();

        //drinkController.drinkButton.SetActive(true);

        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    //drinkController.drinkButton.SetActive(true);
        //}
    }

    public void NextActor()
    {
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    for (int i = 0; i < actorControls.Count; i++)
        //    {
        //        iTween.ScaleTo(actorControls[i].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
        //    }
        //    iTween.ScaleTo(playingActors[playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "time", .6f, "easetype", iTween.EaseType.linear, "loopType", iTween.LoopType.pingPong));
        //}

        makeNoise.PlaySFX(14, 0);
        // Debug.Log(playingInd);
        if (playingActors[playingInd].player)
        {
            DisableEnableTakeCardBtns(true);
            iTween.ScaleTo(playingActors[playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
            //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            //{
            //    iTween.ScaleTo(playingActors[playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
            //}
        }
        else
        {
            playingActors[playingInd].PlayCard();
            //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            //{
            //    playingActors[playingInd].PlayCard();
            //}
            //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            //{

            //}
        }

        playingInd++;
        if (playingInd > playingActors.Count - 1)
            playingInd = 0;
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    playingInd++;
        //    if (playingInd > playingActors.Count - 1)
        //        playingInd = 0;
        //}
    }

    //public void NetworkFirstGroundCard()
    //{
    //    StartCoroutine(FirstGroundCard());
    //}

    IEnumerator FirstGroundCard()
    {
        //Debug.Log("First Ground card");
        lastThrowedCard = throwedCardObjs[throwedCards.Count];
        //if (Host)
        //{
        //    NetworkGameManager.Instance?.Rpc_UpdateNetworkLastThrowedCard();
        //}

        Card card = new Card();
        card = deck[0];
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    card = deck[0];
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    card = NetworkGameManager.Instance.NetworkCardToCard(NetworkGameManager.Instance.NetworkDeck[0]);
        //}

        SpriteRenderer spriteRenderer = lastThrowedCard.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(card, gameConfig.deckStyles[gameConfig.deckStyleInd]);
        spriteRenderer.sortingOrder = throwedCards.Count;
        spriteRenderer.size = new Vector2(2.56f, 3.5f);

        tableAnimationControl.FirsGroundCard();

        yield return new WaitForSeconds(1f);

        lastThrowedCard.transform.SetParent(tableAnimationControl.throwedCardsPos);
        lastThrowedCard.transform.localPosition = Vector3.zero;
        lastThrowedCard.transform.rotation = Quaternion.Euler(0, 180, 0);
        lastThrowedCard.SetActive(true);

        deck.Remove(deck[0]);
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    deck.Remove(deck[0]);

        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    if (Host)
        //    {
        //        NetworkGameManager.Instance.Rpc_RemoveCardFromDeck(0);
        //    }
        //}
        throwedCards.Add(card);
        //NetworkGameManager.Instance?.NetworkCardListFromCardList(NetworkGameManager.Instance.NetworkThrowedCards, throwedCards);
    }

    public void PickCard(ActorControl actorControl, bool fromDeck)
    {
        makeNoise.PlaySFX(15, 0);

        if (fromDeck)
        {
            CardClose cardClose = tableAnimationControl.cardCloses[tableAnimationControl.cardCloses.Count - 1];
            cardClose.gameObject.SetActive(true);
            cardClose.Pick(actorControl.actorTransform.GetChild(0).position);
            tableAnimationControl.cardCloses.Remove(cardClose);

            actorControl.AddCard(deck[0]);
            deck.Remove(deck[0]);
            //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            //{
            //    deck.Remove(deck[0]);
            //}
            //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            //{
            //    NetworkGameManager.Instance.Rpc_RemoveCardFromDeck(0);
            //}
            CheckDeckCardCount();
        }
        else
        {
            Card cardType = throwedCards[throwedCards.Count - 1];
            throwedCards.Remove(cardType);
            //NetworkGameManager.Instance?.Rpc_UpdateThrowedCards();

            actorControl.AddCard(cardType);
            StartCoroutine(PickCardFromThrowed(actorControl.actorTransform.GetChild(0).position));
        }

        actorControl.PlayCard();
    }

    IEnumerator PickCardFromThrowed(Vector3 pos)
    {
        pos.y = transform.position.y;
        iTween.MoveTo(lastThrowedCard, iTween.Hash("position", pos, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));
        iTween.RotateTo(lastThrowedCard, iTween.Hash("y", 0, "time", .2f));
        iTween.ScaleTo(lastThrowedCard, iTween.Hash("scale", Vector3.one * .5f, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));

        yield return new WaitForSeconds(.2f);
        lastThrowedCard.SetActive(false);
        lastThrowedCard.transform.position = Vector3.zero;
        lastThrowedCard.transform.localScale = Vector3.one;
        lastThrowedCard.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void ThrowCard(Card cardType, ActorControl actorControl)
    {
        makeNoise.PlaySFX(16, 0);

        throwedCards.Add(cardType);
        actorControl.RemoveCard(cardType);
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    throwedCards.Add(cardType);
        //    actorControl.RemoveCard(cardType);
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance?.Rpc_AddThrowedCards(NetworkGameManager.Instance.CardToNetworkCard(cardType));
        //    NetworkPlayer.Players[actorControls.IndexOf(playerControl.actorControl)].RPC_RemoveCard(NetworkGameManager.Instance.CardToNetworkCard(cardType));
        //}

        float yOffset = 0;//(throwedCards.Count * .0001f) + .0001f;
        //lastThrowedCard = Instantiate(cardImages.cardOpens[CardSpriteConverter.GetCardSpriteInd(cardType)]);
        //lastThrowedCard = Instantiate(Resources.Load("CardSprite")) as GameObject;
        lastThrowedCard = throwedCardObjs[throwedCards.Count];
        lastThrowedCard.SetActive(true);

        //NetworkGameManager.Instance?.Rpc_UpdateNetworkLastThrowedCard();

        SpriteRenderer spriteRenderer = lastThrowedCard.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(cardType, gameConfig.deckStyles[gameConfig.deckStyleInd]);
        spriteRenderer.sortingOrder = throwedCards.Count;
        spriteRenderer.size = new Vector2(2.56f, 3.5f);

        //NetworkGameManager.Instance?.Rpc_AddSpriteRendererToThrowedCard(NetworkGameManager.Instance.CardToNetworkCard(cardType));
        tableAnimationControl.ThrowCard(lastThrowedCard, actorControl, yOffset);
    }

    public void TakeCardFromThrowed()
    {
        makeNoise.PlaySFX(15, 0);

        DisableEnableTakeCardBtns(false);

        Card cardType = throwedCards[throwedCards.Count - 1];
        //NetworkGameManager.Instance?.Rpc_UpdateThrowedCards();

        throwedCards.Remove(cardType);
        lastThrowedCard.SetActive(false);
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    throwedCards.Remove(cardType);
        //    lastThrowedCard.SetActive(false);
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance.Rpc_RemoveThrowedCards(NetworkGameManager.Instance.CardToNetworkCard(cardType));
        //    NetworkGameManager.Instance.RPC_PickedCardFromThrowed();
        //}

        playerControl.TakeCard(cardType);
    }

    public void TakeCardFromDeck()
    {
        //Debug.Log("Player deckten Kart cekti");

        makeNoise.PlaySFX(15, 0);

        DisableEnableTakeCardBtns(false);

        CardClose cardClose = tableAnimationControl.cardCloses[tableAnimationControl.cardCloses.Count - 1];
        cardClose.gameObject.SetActive(false);
        tableAnimationControl.cardCloses.Remove(cardClose);

        playerControl.TakeCard(deck[0]);

        deck.Remove(deck[0]);
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    deck.Remove(deck[0]);
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance.Rpc_RemoveCardFromDeck(0);
        //    NetworkGameManager.Instance?.Rpc_PlayerTakeCardAnimations();
        //}

        CheckDeckCardCount();
    }

    void CheckDeckCardCount()
    {
        if (deck.Count == 0)
        {
            deck = new List<Card>(throwedCards);
            ShuffleDeck();

            throwedCards.Clear();

            tableAnimationControl.CreateDeckFromThrowedCards(deck.Count);
        }
    }

    void OpenDesicitonPanel()
    {
        makeNoise.PlaySFX(11, 0);

        if (betUp)
        {
            desicitonPanelBetText.text = "$1000";
            desicitonPanelBetUpBTN.SetActive(false);
        }
        else
        {
            desicitonPanelBetText.text = "$500";
            desicitonPanelBetUpBTN.SetActive(true);
        }

        desicitonPanel.SetActive(true);
    }

    public void CloseDesicionPanel(int ind)
    {
        switch (ind)
        {
            case 0: // VAR

                playerControl.actorControl.pass = false;
                playerControl.actorControl.betUp = false;
                //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
                //{
                //    NetworkPlayer.Local?.RPC_ChangePassState(false);
                //    NetworkPlayer.Local?.RPC_ChangeBetUpState(false);
                //}

                break;
            case 1: // PASS

                playerControl.actorControl.pass = true;
                playerControl.actorControl.betUp = false;
                //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
                //{
                //    NetworkPlayer.Local?.RPC_ChangePassState(true);
                //    NetworkPlayer.Local?.RPC_ChangeBetUpState(false);
                //}

                break;
            case 2: // BETUP

                playerControl.actorControl.pass = false;
                playerControl.actorControl.betUp = true;
                //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
                //{
                //    NetworkPlayer.Local?.RPC_ChangePassState(false);
                //    NetworkPlayer.Local?.RPC_ChangeBetUpState(true);
                //}

                break;
        }

        playerControl.actorControl.DecidePlayer();
        desicionInd++;
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    playerControl.actorControl.DecidePlayer();
        //    desicionInd++;
        //}
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkGameManager.Instance.Rpc_DecidePlayer();

        //    desicionInd++;
        //    //Debug.Log(NetworkPlayer.Local.HasInputAuthority);
        //    NetworkPlayer.Local.RPC_ChangeDesicionInd(desicionInd);
        //    //NetworkGameManager.Instance?.RPC_ChangeDesicionIndAll();

        //    NetworkGameManager.Instance?.Rpc_CheckLastPlayer();
        //}

        desicitonPanel.SetActive(false);
    }

    //public void UpdateLastPlayerHand(ActorControl actorControl)
    //{
    //    CompletedHand = new List<Card>(actorControl.cardsInHand);
    //    NetworkGameManager.Instance.Rpc_ClearCompletedHand();
    //    foreach (Card cardOfCompletedHand in CompletedHand)
    //    {
    //        NetworkGameManager.Instance.Rpc_AddCardToCompletedHand(NetworkGameManager.Instance.CardToNetworkCard(cardOfCompletedHand));
    //    }
    //}

    //public void CheckLastPlayer()
    //{
    //    networkPassCounter = 0;
    //    foreach (NetworkPlayer networkPlayer in NetworkPlayer.Players)
    //    {
    //        if (networkPlayer.Pass)
    //        {
    //            networkPassCounter++;
    //        }
    //    }

    //    if (networkPassCounter == orderOfPlayActors.Count - 1)
    //    {
    //        for (int i = 0; i < NetworkPlayer.Players.Count; i++)
    //        {
    //            if (!NetworkPlayer.Players[i].Pass)
    //            {
    //                Debug.Log(i);
    //                UpdateLastPlayerHand(actorControls[i]);
    //                FindObjectOfType<HandCompletedPanel>(true).OpenPanel(actorControls[i]);
    //            }
    //        }
    //    }
    //}

    bool CheckPlayerHandCompleted()
    {
        if (TestMode)
            return true;

        return FindObjectOfType<PlayerHandChecker>().CheckHandCompleted(playerControl.GetUIOrderedCards());
    }

    void PlayerHandCompleted()
    {
        makeNoise.PlaySFX(18, 0);
        makeNoise.PlaySFX(25, 0);

        coinController.EarnCoin(100);
        playerControl.actorControl.totalCoins += 100;

        FindObjectOfType<HandCompletedPanel>(true).OpenPanel(playerControl.actorControl);
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //    FindObjectOfType<HandCompletedPanel>(true).OpenPanel(playerControl.actorControl);

        //playerWinPanel.SetActive(true);
        FindObjectOfType<MoneyController>().EarnMoney(rewardMoney);
        actorControls[0].winCounter++;
        actorControls[0].totalWinMoney += rewardMoney;

        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    NetworkPlayer.Players[actorControls.IndexOf(playerControl.actorControl)].RPC_SetHandCompleted(true);
        //    NetworkGameManager.Instance.Rpc_ShowHandCompletedPanel(actorControls.IndexOf(playerControl.actorControl));
        //}
    }

    public void OpenCompleteHandWarningPanel()
    {
        if (CheckPlayerHandCompleted())
        {
            PlayerHandCompleted();
        }
        else
        {
            makeNoise.PlaySFX(11, 0);
            //completeHandBtn.SetActive(false);
            completeHandWarningPanel.SetActive(true);

            playerControl.throwedCardArea.SetActive(false);
            playerControl.finishCardArea.SetActive(false);
        }
    }

    public void CloseCompleteHandWarningPanel(bool handCompleted)
    {
        if (handCompleted)
        {
            if (CheckPlayerHandCompleted())
            {
                //completeHandBtn.SetActive(false);
                completeHandWarningPanel.SetActive(false);
                PlayerHandCompleted();
            }
            else
            {
                makeNoise.PlaySFX(8, 0);
                completeHandWarningPanelWarning.SetActive(false);
                completeHandWarningPanelWarning.SetActive(true);
            }
        }
        else
        {
            makeNoise.PlaySFX(17, 0);
            //completeHandBtn.SetActive(true);
            completeHandWarningPanel.SetActive(false);
            //playerControl.throwedCardArea.SetActive(true);
            playerControl.HoldedCardInFinishAreaToCardsInHand();
        }
    }

    public void SetRewardMoney(int value)
    {
        rewardMoney += value;

        rewardMoneyText.text = "$" + rewardMoney;
        rewardMoneyText.gameObject.SetActive(false);
        rewardMoneyText.gameObject.SetActive(true);
    }

    public void BackMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void NewGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    //public void MenuFriendMode()
    //{
    //    if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
    //    {
    //        NetworkSpawner networkSpawner = GameObject.FindObjectOfType<NetworkSpawner>();
    //        if (networkSpawner != null)
    //        {
    //            networkSpawner.LeaveSession();
    //        }
    //    }

    //    Menu();
    //}

    public void OpenStatisticPanel()
    {
        statisticPanel.SetActive(true);
        makeNoise.PlaySFX(27, 0);
    }

    public void CloseStatisticPanel()
    {
        statisticPanel.SetActive(false);
        makeNoise.PlaySFX(17, 0);
    }

    public void OpenPlayerHandCompletedPanel()
    {
        //playerWinPanel.SetActive(false);
        FindObjectOfType<HandCompletedPanel>(true).OpenPanel(playerControl.actorControl);
        makeNoise.PlaySFX(27, 0);
    }


    // Multiplayer
    public void UpdateLobbyPlayerNames()
    {
        NetworkPlayer[] networkPlayers = FindObjectsOfType<NetworkPlayer>();

        lobbyUIManager.SetPlayerCountText(networkPlayers.Length, networkHandler.maxPlayer);

        lobbyUIManager.ResetPlayerNames();
        for (int i = 0; i < networkPlayers.Length; i++)
            lobbyUIManager.ActivatePlayerName(i, networkPlayers[i].nickName.ToString());
    }

    public int GetNetworkPlayerCount()
    {
        return FindObjectsOfType<NetworkPlayer>().Length;
    }

    public void SetMultiplayerActors()
    {
        gameLimit = networkHandler.maxPlayer;

        for (int i = 0; i < actorControls.Count; i++)
            actorControls[i].gameObject.SetActive(false);

        List<ActorControl> AC = new List<ActorControl>();
        for(int i = 0; i < gameLimit; i++)
        {
            AC.Add(actorControls[i]);
            actorControls[i].gameObject.SetActive(true);
        }
        actorControls = AC;

        for (int i = 0; i < actorControls.Count; i++)
        {
            actorControls[i].SetNameText(networkGlobals.orderedNetworkPlayers[i].nickName.ToString());
        }
    }

}


