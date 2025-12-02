using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject desicitonPanel;
    [SerializeField] GameObject desicitonPanelBetUpBTN;
    [SerializeField] Text desicitonPanelBetText;
    [SerializeField] GameObject rewardBetUp;
    [SerializeField] Text rewardBetUpText;
    [SerializeField] Text rewardMoneyText;
    [SerializeField] Text deckCountText;
    [SerializeField] GameObject startingGameUI;
    [SerializeField] GameObject waitingForPlayersUI;

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
    public EmojiController emojiController;

    [SerializeField] GameObject statisticPanel;

    public GameObject disconnetPopup;
    public TextMeshProUGUI disconnetPopupNickName;

    public bool TestMode;

    public float timeOutTimer; //oyuncunun eli oynaması için timer
    public float betTimer; // oyuncunun bet yapması için timer
    private Coroutine betTimerCoroutine;
    private float betTimerCurrentTime = 0f;
    public Image betTimerFilled;

    [SerializeField] private SelectSoloModePanel selectSoloModePanel;
    public int playerCount; // oyuncu sayısı
    public List<ActorControl> players = new List<ActorControl>();
    public int networkPlayingInd;
    public int networkWinInd;
    public int networkReceivedClientHandCounter;

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Awake()
    {
        tableAnimationControl = FindObjectOfType<TableAnimationControl>();
        playerControl = FindObjectOfType<PlayerControl>();
        makeNoise = FindObjectOfType<MakeNoise>();

        completeHandWarningPanel.SetActive(false);

        throwedCardObjs = new GameObject[104];
        for (int i = 0; i < throwedCardObjs.Length; i++)
        {
            throwedCardObjs[i] = Instantiate(Resources.Load("CardSprite")) as GameObject;
            throwedCardObjs[i].SetActive(false);
        }

        if(GameModeChecker.Instance.IsSinglePlayerActive)
        {
            actorControls[0].actorAvatar.sprite = gameConfig.avatars[PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey)];

            string pName = "Guest";

            if (PlayerPrefs.HasKey(Constants.PlayerData.PlayerNameKey))
                pName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey);

            actorControls[0].actorName = pName;
        }

        gameCounter = 1;
        gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();

        InitTimers();
    }

    private void Start()
    {
        if (GameModeChecker.Instance.IsMultiplayerActive)
        {
            //actorControls.ForEach(x => x.gameObject.SetActive(false));
            selectSoloModePanel.gameObject.SetActive(false);
        }
        else
        {
            startingGameUI.SetActive(false);
            waitingForPlayersUI.SetActive(false);
            selectSoloModePanel.gameObject.SetActive(true);
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

        playerControl.timerPause = false;

        drinkController.drinkButton.SetActive(false);
        emojiController.emojiButton.SetActive(false);

        if (GameModeChecker.Instance.IsMultiplayerActive)
        {
            networkPlayingInd = 0;
            networkReceivedClientHandCounter = 0;
        }
    }

    public void NewGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    public void Menu()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(26, 0);
        gameConfig.cardDealerInd = -1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void NextTour()
    {
        if(GameModeChecker.Instance.IsMultiplayerActive)
        {
            NextTourClientRpc();
        }
        else
        {
            NextTouring();
            Invoke("StartGame", 1f);
        }
    }

    // seçilen oyuncu sayısına göre oyuncuları oluşturma
    public void SetPlayers()
    {
        actorControls.ForEach(x => x.actorTransform.gameObject.SetActive(false));
        actorControls.ForEach(x => x.gameObject.SetActive(false));
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
            if (GameModeChecker.Instance.IsMultiplayerActive)
                return;

            if (!desicionActors[desicionInd].pass)
                desicionActors[desicionInd].DecideBet();

            desicionInd++;
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

        //if (GameModeChecker.Instance.IsMultiplayerActive)
        //{

        //    yield break;
        //}

        NextActor();

        drinkController.drinkButton.SetActive(true);
        emojiController.emojiButton.SetActive(true);

        actorControls.ForEach(x =>
        {
            if (!x.player)
            {
                x.StartEmojiCoroutine();
            }
        });
    }

    public void NextActor()
    {
        makeNoise.PlaySFX(14, 0);

        if (playingActors[playingInd].player)
        {
            makeNoise.PlaySFX(31, 0);

            DisableEnableTakeCardBtns(true);
            iTween.ScaleTo(playingActors[playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

            playerControl.StartTimer(playingActors[playingInd]);

        }
        else
        {
            if (!GameModeChecker.Instance.IsMultiplayerActive)
                playingActors[playingInd].PlayCard();
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

    public void PickCard(ActorControl actorControl, bool fromDeck)
    {
        makeNoise.PlaySFX(15, 0);

        if (fromDeck)
        {
            CardClose cardClose = tableAnimationControl.cardCloses[tableAnimationControl.cardCloses.Count - 1];
            cardClose.gameObject.SetActive(true);
            cardClose.Pick(actorControl.actorTransform.GetChild(0).position);
            tableAnimationControl.cardCloses.Remove(cardClose);

            if(GameModeChecker.Instance.IsSinglePlayerActive)
                actorControl.AddCard(deck[0]);

            deck.Remove(deck[0]);

            CheckDeckCardCount();
        }
        else
        {
            Card cardType = throwedCards[throwedCards.Count - 1];
            throwedCards.Remove(cardType);

            if (GameModeChecker.Instance.IsSinglePlayerActive)
                actorControl.AddCard(cardType);

            StartCoroutine(PickCardFromThrowed(actorControl.actorTransform.GetChild(0).position));
        }

        if (GameModeChecker.Instance.IsSinglePlayerActive)
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
        ThrowingCard(cardType, actorControl);
    }

    public void ThrowingCard(Card cardType, ActorControl actorControl)
    {
        makeNoise.PlaySFX(16, 0);

        throwedCards.Add(cardType);

        if (GameModeChecker.Instance.IsSinglePlayerActive ||(GameModeChecker.Instance.IsMultiplayerActive && playingActors[networkPlayingInd].player))
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
        playerControl.lastTakedCardFromThrowed = cardType;

        if (GameModeChecker.Instance.IsMultiplayerActive)
        {
            SendTakeCardFromThrowedServerRpc();
        }
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

        if (GameModeChecker.Instance.IsMultiplayerActive)
        {
            SendTakeCardFromDeckServerRpc();
        }
    }

    public void CheckDeckCardCount()
    {
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

        if (GameModeChecker.Instance.IsMultiplayerActive)
        {
            SendPlayerDecisionServerRpc((byte)actorControls.IndexOf(playerControl.actorControl), (byte)ind);
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
        OpenHandCompletedPanel(playerControl.actorControl);

        if (GameModeChecker.Instance.IsMultiplayerActive)
        {
            SendPlayerHandCompletedServerRpc((byte)actorControls.IndexOf(playerControl.actorControl));
        }
    }

    public void OpenHandCompletedPanel(ActorControl actorControl)
    {
        StopAllActorEmojies();

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

    public void StopAllActorEmojies()
    {
        actorControls.ForEach(x =>
        {
            x.StopEmojiCoroutine();
        });
    }

    public void OpenCompleteHandWarningPanel()
    {
        if (CheckPlayerHandCompleted())
        {
            playerControl.StopTimer(playerControl.actorControl);
            PlayerHandCompleted();
        }
        else
        {
            makeNoise.PlaySFX(11, 0);
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
            completeHandWarningPanel.SetActive(false);
            playerControl.HoldedCardInFinishAreaToCardsInHand();
            playerControl.timerPause = false;
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
        FindObjectOfType<HandCompletedPanel>(true).OpenPanel(playerControl.actorControl);
        makeNoise.PlaySFX(27, 0);
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


    // multi
    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            PlayerNetworkController[] networkPlayers = FindObjectsByType<PlayerNetworkController>(FindObjectsSortMode.None)
                                                .OrderBy(p => p.OwnerClientId)
                                                .ToArray();

            playerCount = MultiplayerGameManager.Instance.GetLobby().MaxPlayers;
            gameLimit = playerCount;

            SetMultiPlayers();

            if (NetworkManager.Singleton.IsHost)
            {
                CreateDeck();
                ShuffleDeck();
                ServerDealInitialHands(networkPlayers);
                ChooseRandomCardDealer();
                SendDeckToClients();
                SetCardDealerClientRpc((byte)cardDealerInd);
                PrepareGameClientRpc();
            }
        }
    }

    public void SetMultiPlayers()
    {
        PlayerNetworkController[] networkPlayers = FindObjectsByType<PlayerNetworkController>(FindObjectsSortMode.None)
                                                .OrderBy(p => p.OwnerClientId)
                                                .ToArray();
        playerCount = networkPlayers.Length;
        actorControls.ForEach(x => x.actorTransform.gameObject.SetActive(false));
        actorControls.ForEach(x => x.gameObject.SetActive(false));
        actorControls.Clear();

        for (int i = 0; i < playerCount; i++)
        {
            PlayerNetworkController currentNetworkPlayer = networkPlayers[i];
            ActorControl currentActor = players[i];

            actorControls.Add(currentActor);
            //actorControls[i].gameObject.SetActive(true);
            currentActor.gameObject.SetActive(true);
            currentActor.actorTransform.gameObject.SetActive(true);
            currentActor.SetActorName(networkPlayers[i].PlayerName.Value.ToString());
            currentActor.SetAvatar(networkPlayers[i].PlayerAvatarIndex.Value);
            currentActor.player = currentNetworkPlayer.IsLocalPlayer;
            if (currentActor.player)
                playerControl.actorControl = currentActor;
        }
    }

    public void ServerDealInitialHands(PlayerNetworkController[] networkPlayers)
    {
        const int initialHandSize = 9;

        for (int i = 0; i < actorControls.Count; i++)
        {
            List<Card> initialHand = new List<Card>();
            for (int c = 0; c < initialHandSize; c++)
            {
                if (deck.Count > 0)
                {
                    initialHand.Add(deck[0]);
                    deck.RemoveAt(0);
                }
            }

            // Sunucu belleğinde oyuncunun elini kaydet
            //actorControls[i].cardsInHand = initialHand;
            //actorControls[i].ArrangeHand();

            // Card listesini NetworkCardData dizisine dönüştür
            NetworkCardData[] networkData = initialHand
                .Select(c => new NetworkCardData { suit = c.suit, value = c.value })
                .ToArray();

            // Hedef Client ID'yi al
            ulong targetClientId = networkPlayers[i].OwnerClientId;

            //// Hedefli RPC Parametrelerini Oluştur
            //ClientRpcParams clientRpcParams = new ClientRpcParams
            //{
            //    Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { targetClientId } }
            //};

            // Hedef Client'a Kartları Gönder
            //if(i!=0)
            ReceiveHandClientRpc(networkData, (byte)i);
        }
    }

    // Hedef Client'a kartları gönderir
    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveHandClientRpc(NetworkCardData[] initialHandData, byte playerIndex)
    {
        List<Card> initialHand = initialHandData
        .Select(nd => new Card { suit = nd.suit, value = nd.value })
        .ToList();

        // RPC ile gelen index'e göre ActorControl'ü bul
        ActorControl targetActor = actorControls[playerIndex];

        if (!targetActor.player)
        {
            return;
        }

        targetActor.cardsInHand = initialHand;
        targetActor.ArrangeHand();
    }

    public void SendDeckToClients()
    {
        NetworkCardData[] networkData = deck
                .Select(c => new NetworkCardData { suit = c.suit, value = c.value })
                .ToArray();

        ReceiveDeckClientRpc(networkData);
    }

    // Clientlara deck gönderir
    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveDeckClientRpc(NetworkCardData[] serverDeck)
    {
        List<Card> clientDeck = serverDeck
        .Select(nd => new Card { suit = nd.suit, value = nd.value })
        .ToList();

        deck = clientDeck;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetCardDealerClientRpc(byte dealerIndex)
    {
        cardDealerInd = dealerIndex;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PrepareGameClientRpc()
    {
        SortOrderOfPlayActors();
        PrepeareStartGame();
    }

    public void StartMultiplayerBettingPhase()
    {
        ReceiveDecisionTurnClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveDecisionTurnClientRpc()
    {
        if (desicionActors[desicionInd].player)
        {
            OpenDesicitonPanel();
        }
    }

    [Rpc(SendTo.Server)]
    public void SendPlayerDecisionServerRpc(byte actorIndex, byte decisionType)
    {
        ActorControl actor = actorControls[actorIndex];

        switch (decisionType)
        {
            case 0: // VAR
                actor.pass = false;
                actor.betUp = false;
                break;
            case 1: // PASS
                actor.pass = true;
                actor.betUp = false;
                break;
            case 2: // BETUP
                actor.pass = false;
                actor.betUp = true;
                break;
        }

        MultiplayerCurrentDecisionClientRpc(actorIndex, decisionType);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void MultiplayerCurrentDecisionClientRpc(byte actorIndex, byte decisionType)
    {
        ActorControl actor = actorControls[actorIndex];

        actor.pass = decisionType == 1;
        desicionInd++;
        actor.DecidePlayer();
    }

    [Rpc(SendTo.Server)]
    public void SendTakeCardFromThrowedServerRpc()
    {
        SendTakeCardFromThrowedClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendTakeCardFromThrowedClientRpc()
    {
        if (!playingActors[networkPlayingInd].player)
        {
            PickCard(playingActors[networkPlayingInd], false);
        }
    }

    [Rpc(SendTo.Server)]
    public void SendTakeCardFromDeckServerRpc()
    {
        SendTakeCardFromDeckClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendTakeCardFromDeckClientRpc()
    {
        if (!playingActors[networkPlayingInd].player)
        {
            PickCard(playingActors[networkPlayingInd], true);
        }
    }

    [Rpc(SendTo.Server)]
    public void SendThrowCardServerRpc(NetworkCardData networkThrowedCardData)
    {
        SendThrowCardClientRpc(networkThrowedCardData);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendThrowCardClientRpc(NetworkCardData networkThrowedCardData)
    {
        if (!playingActors[networkPlayingInd].player)
        {
            Card card = networkThrowedCardData.ToCard();
            ThrowCard(card, playingActors[networkPlayingInd]);
        }
    }

    [Rpc(SendTo.Server)]
    public void SendPlayerHandCompletedServerRpc(byte actorIndex)
    {
        networkWinInd = actorIndex;
        PingHandEndClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PingHandEndClientRpc()
    {
        if (playerControl.actorControl.player)
        {
            NetworkCardData[] networkclientHand = playerControl.GetInHandsCards()
                .Select(c => new NetworkCardData { suit = c.suit, value = c.value })
                .ToArray();

            SendClientHandServerRpc((byte)actorControls.IndexOf(playerControl.actorControl), networkclientHand);
        }
    }

    [Rpc(SendTo.Server)]
    public void SendClientHandServerRpc(byte actorIndex,NetworkCardData[] playerCards)
    {
        networkReceivedClientHandCounter++;

        List<Card> clientDeck = playerCards
        .Select(nd => new Card { suit = nd.suit, value = nd.value })
        .ToList();

        actorControls[actorIndex].cardsInHand = clientDeck;

        if (networkReceivedClientHandCounter >= playerCount)
        {
            for(int i = 0; i < actorControls.Count; i++)
            {
                NetworkCardData[] networkCardData = actorControls[i].cardsInHand
                .Select(c => new NetworkCardData { suit = c.suit, value = c.value })
                .ToArray();

                var playerIsWin = (i == networkWinInd);

                SendAllHandClientRpc((byte)i, networkCardData, playerIsWin);
            }
        }

        SendPlayerHandCompletedClientRpc((byte)networkWinInd);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendAllHandClientRpc(byte playerIndex,NetworkCardData[] playerCards, bool isPlayerWin)
    {
        List<Card> clientDeck = playerCards
            .Select(nd => new Card { suit = nd.suit, value = nd.value })
            .ToList();

        actorControls[playerIndex].cardsInHand = clientDeck;
        actorControls[playerIndex].handWin = isPlayerWin;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendPlayerHandCompletedClientRpc(byte winPlayerIndex)
    {
        if(playerControl.actorControl.handWin)
            return;

        OpenHandCompletedPanel(actorControls[winPlayerIndex]);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void NextTourClientRpc()
    {
        NextTouring();

        PlayerNetworkController[] networkPlayers = FindObjectsByType<PlayerNetworkController>(FindObjectsSortMode.None)
                                    .OrderBy(p => p.OwnerClientId)
                                    .ToArray();

        playerCount = MultiplayerGameManager.Instance.GetLobby().MaxPlayers;
        gameLimit = playerCount;

        SetMultiPlayers();

        if (NetworkManager.Singleton.IsHost)
        {
            CreateDeck();
            ShuffleDeck();
            ServerDealInitialHands(networkPlayers);
            ChooseRandomCardDealer();
            SendDeckToClients();
            SetCardDealerClientRpc((byte)cardDealerInd);
            PrepareGameClientRpc();
        }
    }
}

