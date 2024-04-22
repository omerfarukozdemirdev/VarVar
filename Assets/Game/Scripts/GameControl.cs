using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [HideInInspector] public int gameLimit;
    public Text gameTourText;
    public CoinController coinController;
    public DrinkController drinkController;
    public int drinkCounter;
    [SerializeField] GameObject statisticPanel;

    public LobbyUIManager lobbyUIManager;
    public GameObject disconnetPopup;
    public TextMeshProUGUI disconnetPopupNickName;

    public bool TestMode;

    public NetworkHandler networkHandler;
    public NetworkPlayer myNetworkPlayer;
    public NetworkGlobals networkGlobals;

    public float timeOutTimer; //oyuncunun eli oynaması için timer
    public float betTimer; // oyuncunun bet yapması için timer
    private Coroutine betTimerCoroutine;
    private float betTimerCurrentTime = 0f;
    public Image betTimerFilled;

    [SerializeField] private SelectSoloModePanel selectSoloModePanel;
    public int playerCount; // oyuncu sayısı
    public List<ActorControl> players = new List<ActorControl>();

    private void Awake()
    {
        tableAnimationControl = FindObjectOfType<TableAnimationControl>();
        playerControl = FindObjectOfType<PlayerControl>();
        makeNoise = FindObjectOfType<MakeNoise>();

        //completeHandBtn.SetActive(false);
        completeHandWarningPanel.SetActive(false);

        throwedCardObjs = new GameObject[104];
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

        InitTimers();

        lobbyUIManager.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (GameManager.Instance.IsMultiplayer())
        {
            GameManager.Instance.gameStat = GameManager.GameStat.lobby;

            networkHandler = FindObjectOfType<NetworkHandler>();
            //networkHandler.StartQuickGame();

            lobbyUIManager.gameObject.SetActive(true);
        }
        else
        {
            selectSoloModePanel.gameObject.SetActive (true);
        }
    }

    private void Update()
    {
        if (deck.Count == 0 || !tableAnimationControl.cardCloseParent.gameObject.activeSelf)
            deckCountText.text = "";
        else
            deckCountText.text = deck.Count.ToString();
    }

    void InitTimers()
    {
        betTimer = PlayerPrefs.GetInt("BetTime");
        timeOutTimer = PlayerPrefs.GetInt("PlayTime");
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

        betUpTurn = false;
        betUPActor = null;

        cardDealerInd++;
        if (cardDealerInd > actorControls.Count - 1)
            cardDealerInd = 0;

        rewardMoney = 0;
        passCount = 0;

        newDesicitonActors.Clear();
    }

    public void NewGame()
    {
        if (GameManager.Instance.IsMultiplayer())
        {
            Menu();
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    public void Menu()
    {
        if (GameManager.Instance.IsMultiplayer())
            networkHandler.Disconnect();

        FindObjectOfType<MakeNoise>().PlaySFX(26, 0);
        gameConfig.cardDealerInd = -1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void NextTour()
    {
        if (GameManager.Instance.IsMultiplayer())
        {
            myNetworkPlayer.RPC_NextTour();
            return;
        }

        NextTouring();
        Invoke("StartGame", 1f);
    }

    // seçilen oyuncu sayısına göre oyuncuları oluşturma
    public void SetPlayers()
    {
        actorControls.ForEach(x=>x.actorTransform.gameObject.SetActive(false));
        actorControls.ForEach(x=>x.gameObject.SetActive(false));
        actorControls.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            actorControls.Add(players[i]);
            actorControls[i].gameObject.SetActive(true);
            actorControls[i].actorTransform.gameObject.SetActive(true);
        }
    }

    // solo lobby deki play butonundan çağrılıyor
    public void SoloStartGame()
    {
        gameLimit = playerCount;
        SetPlayers();
        Invoke("StartGame", .5f);
        selectSoloModePanel.gameObject.SetActive(false);

    }

    public void NextTouring()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(9, 0);

        actorControls.ForEach(x => x.ResetValues());

        gameCounter++;

        FindObjectOfType<HandCompletedPanel>(true).ClosePanel();
        ResetValues();

        gameConfig.cardDealerInd = cardDealerInd;

        tableAnimationControl.Reset();
        playerControl.ResetValues();
    }

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
        if (gameConfig.cardDealerInd != -1)
        {
            cardDealerInd = gameConfig.cardDealerInd;
        }
        else
        {
            cardDealerInd = Random.Range(0, gameLimit);
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
            if (betUp && !betUpTurn)
            {
                newDesicitonActors = new List<ActorControl>();

                for (int i = 0; i < orderOfPlayActors.Count - 1; i++)
                    if (!orderOfPlayActors[i].pass && orderOfPlayActors[i].moneyIn < 1000 && betUPActor != orderOfPlayActors[i])
                    {
                        newDesicitonActors.Add(orderOfPlayActors[i]);
                    }

                desicionActors = new List<ActorControl>(newDesicitonActors);

                if (desicionActors.Count > 0)
                {
                    desicionInd = 0;
                    betUpTurn = true;

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
                }


            while (orderOfPlayActors[playingInd].pass)
            {
                playingInd++;

                if (playingInd > orderOfPlayActors.Count - 1)
                    playingInd = 0;
            }


            playingActors = new List<ActorControl>();
            for (int i = 0; i < orderOfPlayActors.Count; i++)
            {
                orderOfPlayActors[i].DisableSpeechBaloon();

                if (!orderOfPlayActors[i].pass)
                    playingActors.Add(orderOfPlayActors[i]);
            }

            if (playingActors.Count <= 1)
            {
                FindObjectOfType<HandCompletedPanel>(true).OpenPanel(null);
                return;
            }

            StartCoroutine(StartPlaying());

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

        yield return new WaitForSeconds(2.5f);

        NextActor();
    }

    public void NextActor()
    {
        makeNoise.PlaySFX(14, 0);

        if (playingActors[playingInd].player)
        {
            DisableEnableTakeCardBtns(true);
            iTween.ScaleTo(playingActors[playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

            playerControl.StartTimer(playingActors[playingInd]);

        }
        else
        {
            if (GameManager.Instance.IsMultiplayer())
            {
                playingActors[playingInd].BlinkAvatar();

                if (playingActors[playingInd].onlineBot)
                {
                    //playingActors[playingInd].gameControl.playerControl.StartTimer(playingActors[playingInd]);
                    //playingActors[playingInd].gameControl.playerControl.TimeOutPickCard(playingActors[playingInd]);
                    //playingActors[playingInd].gameControl.playerControl.TimeOutThrowCard(playingActors[playingInd]);
                    //playingActors[playingInd].player = false;
                    //StartCoroutine(OnlineBotPlayCoroutine());

                    if (networkHandler.isHost)
                    {

                    }
                }
            }
            else
            {
                playingActors[playingInd].PlayCard();
            }
        }
        playingInd++;
        if (playingInd > playingActors.Count - 1)
            playingInd = 0;
    }

    IEnumerator FirstGroundCard()
    {
        lastThrowedCard = throwedCardObjs[throwedCards.Count];

        Card card = new Card();
        card = deck[0];

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

        throwedCards.Add(card);
    }

    public void PickCard(ActorControl actorControl, bool fromDeck, bool onlineBot)
    {
        makeNoise.PlaySFX(15, 0);

        if (fromDeck)
        {
            CardClose cardClose = tableAnimationControl.cardCloses[tableAnimationControl.cardCloses.Count - 1];
            cardClose.gameObject.SetActive(true);
            cardClose.Pick(actorControl.actorTransform.GetChild(0).position);
            tableAnimationControl.cardCloses.Remove(cardClose);

            if (!GameManager.Instance.IsMultiplayer())
                actorControl.AddCard(deck[0]);

            if (onlineBot)
                actorControl.AddCard(deck[0]);


            deck.Remove(deck[0]);

            CheckDeckCardCount();
        }
        else
        {
            Card cardType = throwedCards[throwedCards.Count - 1];
            throwedCards.Remove(cardType);

            if (!GameManager.Instance.IsMultiplayer())
                actorControl.AddCard(cardType);

            StartCoroutine(PickCardFromThrowed(actorControl.actorTransform.GetChild(0).position));
        }

        if (!GameManager.Instance.IsMultiplayer())
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
        if (GameManager.Instance.IsMultiplayer())
        {
            myNetworkPlayer.RPC_ThrowCard((byte)NetworkCardConverter.CardToInt(cardType), (byte)myNetworkPlayer.playInd);
        }
        else
        {
            ThrowingCard(cardType, actorControl);
        }
    }

    public void ThrowingCard(Card cardType, ActorControl actorControl)
    {
        makeNoise.PlaySFX(16, 0);

        throwedCards.Add(cardType);
        actorControl.RemoveCard(cardType);

        float yOffset = 0;
        lastThrowedCard = throwedCardObjs[throwedCards.Count];
        lastThrowedCard.SetActive(true);

        SpriteRenderer spriteRenderer = lastThrowedCard.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(cardType, gameConfig.deckStyles[gameConfig.deckStyleInd]);
        spriteRenderer.sortingOrder = throwedCards.Count;
        spriteRenderer.size = new Vector2(2.56f, 3.5f);

        tableAnimationControl.ThrowCard(lastThrowedCard, actorControl, yOffset);
    }

    public void TakeCardFromThrowed()
    {
        makeNoise.PlaySFX(15, 0);
        DisableEnableTakeCardBtns(false);

        Card cardType = throwedCards[throwedCards.Count - 1];
        throwedCards.Remove(cardType);
        lastThrowedCard.SetActive(false);

        playerControl.TakeCard(cardType);

        if (GameManager.Instance.IsMultiplayer())
            myNetworkPlayer.RPC_TakeCard(0, (byte)myNetworkPlayer.playInd);
    }

    public void TakeCardFromDeck()
    {
        makeNoise.PlaySFX(15, 0);
        DisableEnableTakeCardBtns(false);

        CardClose cardClose = tableAnimationControl.cardCloses[tableAnimationControl.cardCloses.Count - 1];
        cardClose.gameObject.SetActive(false);
        tableAnimationControl.cardCloses.Remove(cardClose);

        playerControl.TakeCard(deck[0]);
        deck.Remove(deck[0]);


        CheckDeckCardCount();

        if (GameManager.Instance.IsMultiplayer())
            myNetworkPlayer.RPC_TakeCard(1, (byte)myNetworkPlayer.playInd);
    }

    public void CheckDeckCardCount()
    {
        if (GameManager.Instance.IsMultiplayer())
        {
            if (networkHandler.isHost)
            {
                if (deck.Count == 0)
                {
                    deck = new List<Card>(throwedCards);
                    ShuffleDeck();

                    //networkGlobals.UpdateNetworkDeckCards();
                    //networkGlobals.RPC_DeckFromThrowed()
                    networkGlobals.UpdateNetworkDeckCardsFromThrowed();
                }
            }
            return;
        }

        if (deck.Count == 0)
        {
            deck = new List<Card>(throwedCards);
            ShuffleDeck();

            DeckFromThrowed();
        }
    }

    public void DeckFromThrowed()
    {
        throwedCards.Clear();
        tableAnimationControl.CreateDeckFromThrowedCards(deck.Count);
    }

    // var pas panelinin açılması
    void OpenDesicitonPanel()
    {
        makeNoise.PlaySFX(11, 0);

        BetStartTimer();

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

    // var pas paneli karar butonları fonksiyonu
    public void CloseDesicionPanel(int ind)
    {
        switch (ind)
        {
            case 0: // VAR

                playerControl.actorControl.pass = false;
                playerControl.actorControl.betUp = false;

                break;
            case 1: // PASS

                playerControl.actorControl.pass = true;
                playerControl.actorControl.betUp = false;

                break;
            case 2: // BETUP

                playerControl.actorControl.pass = false;
                playerControl.actorControl.betUp = true;

                break;
        }

        if (GameManager.Instance.IsMultiplayer())
        {
            myNetworkPlayer.RPC_DecidePlayer((byte)playerControl.actorControl.pass.GetHashCode(), (byte)playerControl.actorControl.betUp.GetHashCode(), (byte)myNetworkPlayer.playInd);
        }
        else
        {
            playerControl.actorControl.DecidePlayer();
            desicionInd++;
        }

        StopTimer();
        desicitonPanel.SetActive(false);
    }


    bool CheckPlayerHandCompleted()
    {
        if (TestMode)
            return true;

        return FindObjectOfType<PlayerHandChecker>().CheckHandCompleted(playerControl.GetUIOrderedCards());
    }

    void PlayerHandCompleted()
    {
        if (GameManager.Instance.IsMultiplayer())
        {
            myNetworkPlayer.UpdateNetworkCompletedHandCards();
        }
        else
        {
            OpenHandCompletedPanel(playerControl.actorControl);
        }
    }

    public void OpenHandCompletedPanel(ActorControl actorControl)
    {
        makeNoise.PlaySFX(18, 0);
        makeNoise.PlaySFX(25, 0);

        FindObjectOfType<HandCompletedPanel>(true).OpenPanel(actorControl);

        if (actorControl.player)
        {
            coinController.EarnCoin(100);
            FindObjectOfType<MoneyController>().EarnMoney(rewardMoney);
        }

        actorControl.totalCoins += 100;
        actorControl.winCounter++;
        actorControl.totalWinMoney += rewardMoney;
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
        //NetworkPlayer[] networkPlayers = FindObjectsOfType<NetworkPlayer>();

        //lobbyUIManager.SetPlayerCountText(networkPlayers.Length, networkHandler.maxPlayer);

        //lobbyUIManager.ResetPlayerNames();
        //for (int i = 0; i < networkPlayers.Length; i++)
        //    lobbyUIManager.ActivatePlayerName(i, networkPlayers[i].nickName.ToString());

        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();
        lobbyUIManager.SetPlayerCountText(networkRunner.ActivePlayers.Count(), networkHandler.maxPlayer);

        lobbyUIManager.ResetPlayerNames();

        var index = 0;
        foreach (PlayerRef playerRef in networkRunner.ActivePlayers)
        {
            lobbyUIManager.ActivatePlayerName(index, networkRunner.GetPlayerObject(playerRef).GetComponent<NetworkPlayer>().nickName.ToString());
            index++;
        }
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
        for (int i = 0; i < gameLimit; i++)
        {
            AC.Add(actorControls[i]);
            actorControls[i].gameObject.SetActive(true);
        }
        actorControls = AC;

        //for (int i = 0; i < actorControls.Count; i++)
        //{
        //    actorControls[i].actorName = networkGlobals.orderedNetworkPlayers[i].nickName.ToString();
        //    actorControls[i].SetNameText(actorControls[i].actorName);
        //}

        var index = 0;
        foreach (PlayerRef playerRef in networkGlobals.Runner.ActivePlayers)
        {
            actorControls[index].actorName = networkGlobals.Runner.GetPlayerObject(playerRef).GetComponent<NetworkPlayer>().nickName.ToString();
            actorControls[index].SetNameText(actorControls[index].actorName);
            index++;
        }
    }

    public void DealCardsToMultiplayerActors()
    {
        for (int i = 0; i < actorControls.Count; i++)
        {
            List<Card> cards = new List<Card>();

            for (int c = 0; c < 9; c++)
            {
                cards.Add(deck[0]);
                deck.Remove(deck[0]);
            }

            actorControls[i].cardsInHand = new List<Card>(cards);
            actorControls[i].ArrangeHand();
        }
    }

    public void OpenDisconnetPopup(PlayerRef playerRef)
    {
        disconnetPopupNickName.text = actorControls[playerRef].actorName;
        disconnetPopup.SetActive(true);
    }

    public void SetOnlineBot(PlayerRef playerRef)
    {
        actorControls[playerRef].onlineBot = true;
        actorControls[playerRef].SetNameText("BOT");
    }

    IEnumerator OnlineBotPlayCoroutine()
    {
        playingActors[playingInd].gameControl.PickCard(playingActors[playingInd], true, true);
        yield return new WaitForSeconds(0.5f);
        playingActors[playingInd].gameControl.ThrowingCard(playingActors[playingInd].cardsInHand.Last(), playingActors[playingInd]);
    }

    #region Bet Timer region
    // süreyi başlat
    public void BetStartTimer()
    {
        betTimerCoroutine = StartCoroutine(BetTimerCoroutine());
    }

    // süre başladığında
    void BetTimeStarted()
    {
        betTimerCurrentTime = 0;
        betTimerFilled.gameObject.SetActive(true);
    }

    // süreç boyunca olacaklar
    IEnumerator BetTimerCoroutine()
    {
        BetTimeStarted();

        while (betTimerCurrentTime < betTimer)
        {
            betTimerCurrentTime += Time.deltaTime;

            betTimerFilled.fillAmount = 1f - (betTimerCurrentTime / betTimer);
            //currentActorController.timerCircle.fillAmount = Mathf.Lerp(0.7f, 0f, currentTime / currentActorController.gameControl.timeOutTimer);

            yield return null;
        }

        BetTimeEnded();
    }

    //süre bittiğinde
    void BetTimeEnded()
    {
        betTimerCurrentTime = 0;
        CloseDesicionPanel(1);
    }

    // süreyi durdurup işlemi kestiğinde. yani süre bitmeden oyuncu var ya da pass dediğinde
    public void StopTimer()
    {
        if (betTimerCoroutine == null)
            return;

        StopCoroutine(betTimerCoroutine);
    }
    #endregion
}


