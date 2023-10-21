using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    //public GameObject completeHandBtn;
    public GameObject completeHandWarningPanel;
    public GameObject completeHandWarningPanelWarning;

    public int rewardMoney;

    public Transform moneyAtractorParent;

    [Space]
    public GameConfig gameConfig;

    [SerializeField] SpriteRenderer background;
    [SerializeField] SpriteRenderer table;

    public List<ActorControl> actorControls = new List<ActorControl>();
    public List<ActorControl> orderOfPlayActors = new List<ActorControl>();
    public List<ActorControl> desicionActors = new List<ActorControl>();
    public List<ActorControl> playingActors = new List<ActorControl>();
    public List<Card> deck;
    public List<Card> throwedCards = new List<Card>();
    public GameObject[] throwedCardObjs;
    public GameObject lastThrowedCard;
    [SerializeField] GameObject throwedCardBtn;
    [SerializeField] GameObject deckCardBtn;

    public int cardDealerInd;
    [SerializeField] private int orderOfPlayInd;
    public int desicionInd;
    public int playingInd;

    [HideInInspector] public int passCount;
    [HideInInspector] public bool betUp;
    private bool betUpTurn;
    private ActorControl betUPActor;

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
    [SerializeField] GameObject playerWinPanel;

    public Vector3[] actorPositions;
    public Vector3[] actorRotations;
    public GameObject[] actorLocations;
    public List<int> NewPlayerIndexList = new List<int>();

    public bool Host;



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

        background.sprite = gameConfig.backGrounds[gameConfig.backgroundInd];
        table.sprite = gameConfig.tables[gameConfig.tableInd];

        actorControls[0].actorAvatar.sprite = gameConfig.avatars[gameConfig.avatarInd];

        string pName = "Player";

        if (PlayerPrefs.HasKey("PlayerName"))
            pName = PlayerPrefs.GetString("PlayerName");

        actorControls[0].actorName = pName;

        //for (int i = 1; i < actorControls.Count; i++)
        //    actorControls[i].actorAvatar.sprite = gameConfig.avatars[Random.Range(0, gameConfig.avatars.Length)];

        gameCounter = 1;
        gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();

    }

    private void Start()
    {


        switch (GameManager.Instance.CurrentGameMode)
        {
            case GameManager.GameMode.Quick:
                Invoke("StartGame", .5f);
                break;
            case GameManager.GameMode.Friends:
                break;
            case GameManager.GameMode.Tournament:
                break;
        }

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
        orderOfPlayActors = new List<ActorControl>();
        desicionActors = new List<ActorControl>();
        playingActors = new List<ActorControl>();
        deck = new List<Card>();
        throwedCards = new List<Card>();
        orderOfPlayInd = 0;
        desicionInd = 0;
        playingInd = 0;

        betUp = false;
        betUpTurn = false;
        betUPActor = null;

        cardDealerInd++;
        if (cardDealerInd > actorControls.Count - 1)
            cardDealerInd = 0;
        rewardMoney = 0;
        passCount = 0;
        drinkController.drinkButton.SetActive(false);

    }

    public void Menu()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(26, 0);
        gameConfig.cardDealerInd = -1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

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

        FindObjectOfType<HandCompletedPanel>(true).ClosePanel();
        for (int i = 0; i < throwedCardObjs.Length; i++)
        {
            throwedCardObjs[i].transform.SetParent(null);
            throwedCardObjs[i].SetActive(false);
        }
        ResetValues();

        gameConfig.cardDealerInd = cardDealerInd;

        tableAnimationControl.Reset();

        playerControl.ResetValues();


        Invoke("StartGame", 1f);

    }

    public void FriendsModeStartGame()
    {
        if (NetworkPlayer.Local)
        {
            SetTable();

        }


        if (NetworkPlayer.Local.IsLeader)
        {
            CreateDeck();
            ShuffleDeck();
            DealCardsToActors();



        }


        NetworkGameManager.Instance.Rpc_UpdateDeck();
        ChooseRandomCardDealer();
        SortOrderOfPlayActors();
        DisableEnableTakeCardBtns(false);
        //playingInd = 0;
        tableAnimationControl.StartGame();





        // gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();
    }

    public void SetTable()
    {
        //oyuncuları masaya doğru sıraya göre oturtma
        if (NetworkPlayer.Local)
        {

            if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 0)
            {
                NewPlayerIndexList = new List<int>() { 0, 1, 2, 3 };
            }
            else if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 1)
            {
                NewPlayerIndexList = new List<int>() { 3, 0, 1, 2 };
            }
            else if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 2)
            {
                NewPlayerIndexList = new List<int>() { 2, 3, 0, 1 };
            }
            else if (NetworkPlayer.Players.IndexOf(NetworkPlayer.Local) == 3)
            {
                NewPlayerIndexList = new List<int>() { 1, 2, 3, 0 };
            }



            // for (int i = NetworkPlayer.Players.IndexOf(NetworkPlayer.Local); i < NetworkPlayer.Players.Count; i++)
            // {
            //     NewPlayerIndexList.Add(i);
            // }

            // for (int i = 0; i < NetworkPlayer.Players.IndexOf(NetworkPlayer.Local); i++)
            // {
            //     NewPlayerIndexList.Add(i);

            // }

            for (int i = 0; i < NetworkPlayer.Players.Count; i++)
            {
                //NetworkGameManager.GameControl.actorControls[i].SetNameText( NetworkPlayer.Players[NewPlayerIndexList[i]].Username.ToString());
                actorLocations[i].transform.position = actorPositions[NewPlayerIndexList[i]];
                actorLocations[i].transform.rotation = Quaternion.Euler(actorRotations[NewPlayerIndexList[i]]);
                actorControls[i].SetNameText(NetworkPlayer.Players[i].Username.ToString());
                // 0-1-2-3 for 1
                // 3-0-1-2 for 2
                // 2-3-0-1 for 3
                // 1-2-3-1 for 4
            }
        }
    }

    void StartGame()
    {
        CreateDeck();
        ShuffleDeck();

        DealCardsToActors();

        ChooseRandomCardDealer();

        SortOrderOfPlayActors();

        DisableEnableTakeCardBtns(false);

        tableAnimationControl.StartGame();

        gameTourText.text = gameCounter.ToString() + " / " + gameLimit.ToString();
    }

    void DisableEnableTakeCardBtns(bool tf)
    {
        throwedCardBtn.SetActive(tf);
        deckCardBtn.SetActive(tf);
    }

    void CreateDeck()
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

    void ShuffleDeck()
    {
        // Kartları karıştır
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        NetworkGameManager.Instance?.UpdateNetworkDeck();
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

    void DealCardsToActors()
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

        for (int i = 0; i < actorControls.Count; i++)
        {
            NetworkGameManager.Instance?.UpdateAllCards(i);
        }


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

    void ChooseRandomCardDealer()
    {
        //if (PlayerPrefs.HasKey("CD"))
        //    cardDealerInd = PlayerPrefs.GetInt("CD");
        //else
        //    cardDealerInd = Random.Range(0, actorControls.Count);

        if (gameConfig.cardDealerInd != -1)
            cardDealerInd = gameConfig.cardDealerInd;
        else
            cardDealerInd = Random.Range(0, actorControls.Count);

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            cardDealerInd = 0;
        }
    }

    void SortOrderOfPlayActors()
    {
        orderOfPlayActors = new List<ActorControl>();

        int startInd;


        startInd = 0;
        startInd = cardDealerInd;

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

    public void ActorDecisiton()
    {
        if (desicionInd > desicionActors.Count - 1)
        {
            Debug.Log(playingInd);

            if (betUp && !betUpTurn)
            {
                List<ActorControl> newDesicitonActors = new List<ActorControl>();

                for (int i = 0; i < orderOfPlayActors.Count - 1; i++)
                    if (!orderOfPlayActors[i].pass && orderOfPlayActors[i].moneyIn < 1000 && betUPActor != orderOfPlayActors[i])
                        newDesicitonActors.Add(orderOfPlayActors[i]);

                desicionActors = new List<ActorControl>(newDesicitonActors);

                if (desicionActors.Count > 0)
                {
                    desicionInd = 0;
                    if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
                    {
                        NetworkGameManager.Instance.Rpc_ChangeDesicionInd(desicionInd);

                    }
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
            Debug.Log(playingInd);
            //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd(playingInd);

            StartCoroutine(StartPlaying());
            return;
        }

        if (desicionActors[desicionInd].player)
        {
            OpenDesicitonPanel();
        }
        else
        {
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
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

    void SetRewardBetUpText()
    {
        if (betUp)
            rewardBetUpText.text = "4";
        else
            rewardBetUpText.text = "1";

        rewardBetUp.SetActive(false);
        rewardBetUp.SetActive(true);
    }

    IEnumerator StartPlaying()
    {
        Debug.Log("start");
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
        //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd();

        drinkController.drinkButton.SetActive(true);
    }

    public void NextActor()
    {

        makeNoise.PlaySFX(14, 0);
        Debug.Log(playingInd);
        if (playingActors[playingInd].player)
        {
            DisableEnableTakeCardBtns(true);
            iTween.ScaleTo(playingActors[playingInd].transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
        }
        else
        {
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            {
                playingActors[playingInd].PlayCard();
            }
            else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            {
                //NetworkGameManager.Instance.Rpc_PlayCard();
                //playingActors[playingInd].PlayCard();
                //Debug.Log("buraya bak");
            }

        }

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        {
            playingInd++;
            if (playingInd > playingActors.Count - 1)
                playingInd = 0;
        }
        //else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        //{
        //    playingInd++;
        //    if (playingInd > playingActors.Count - 1)
        //        playingInd = 0;
        //    NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd(playingInd);

        //}
        Debug.Log(playingInd);


    }

    IEnumerator FirstGroundCard()
    {


        lastThrowedCard = throwedCardObjs[throwedCards.Count];

        Card card = deck[0];

        SpriteRenderer spriteRenderer = lastThrowedCard.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(card, gameConfig.deckStyles[gameConfig.deckStyleInd]);
        spriteRenderer.sortingOrder = throwedCards.Count;
        spriteRenderer.size = new Vector2(2.56f, 3.5f);

        tableAnimationControl.FirsGroundCard(lastThrowedCard);
        yield return new WaitForSeconds(1f);
        lastThrowedCard.SetActive(true);
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        {
            deck.Remove(deck[0]);

        }
        else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            if (Host)
            {
                NetworkGameManager.Instance.Rpc_RemoveCardFromDeck(0);

            }

        }
        throwedCards.Add(card);
        NetworkGameManager.Instance?.NetworkCardListFromCardList(NetworkGameManager.Instance.NetworkThrowedCards, throwedCards);
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
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            {
                deck.Remove(deck[0]);

            }
            else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            {
                NetworkGameManager.Instance.Rpc_RemoveCardFromDeck(0);

            }
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
    }

    public void ThrowCard(Card cardType, ActorControl actorControl)
    {
        makeNoise.PlaySFX(16, 0);

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        { 
            throwedCards.Add(cardType);

        }
        else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkGameManager.Instance?.Rpc_AddThrowedCards(NetworkGameManager.Instance.CardToNetworkCard(cardType));

        }



        actorControl.RemoveCard(cardType);

        float yOffset = 0;//(throwedCards.Count * .0001f) + .0001f;
        //lastThrowedCard = Instantiate(cardImages.cardOpens[CardSpriteConverter.GetCardSpriteInd(cardType)]);
        //lastThrowedCard = Instantiate(Resources.Load("CardSprite")) as GameObject;
        lastThrowedCard = throwedCardObjs[throwedCards.Count];
        lastThrowedCard.SetActive(true);

        SpriteRenderer spriteRenderer = lastThrowedCard.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = CardSpriteConverter.GetCardSpriteInd(cardType, gameConfig.deckStyles[gameConfig.deckStyleInd]);
        spriteRenderer.sortingOrder = throwedCards.Count;
        spriteRenderer.size = new Vector2(2.56f, 3.5f);
        NetworkGameManager.Instance?.Rpc_AddSpriteRendererToThrowedCard(NetworkGameManager.Instance.CardToNetworkCard(cardType));
        tableAnimationControl.ThrowCard(lastThrowedCard, actorControl, yOffset);
    }

    public void TakeCardFromThrowed()
    {
        makeNoise.PlaySFX(15, 0);

        DisableEnableTakeCardBtns(false);

        Card cardType = throwedCards[throwedCards.Count - 1];
        throwedCards.Remove(cardType);
        //NetworkGameManager.Instance?.Rpc_UpdateThrowedCards();

        lastThrowedCard.SetActive(false);

        playerControl.TakeCard(cardType);
    }

    public void TakeCardFromDeck()
    {
        makeNoise.PlaySFX(15, 0);

        DisableEnableTakeCardBtns(false);

        CardClose cardClose = tableAnimationControl.cardCloses[tableAnimationControl.cardCloses.Count - 1];
        cardClose.gameObject.SetActive(false);
        tableAnimationControl.cardCloses.Remove(cardClose);

        playerControl.TakeCard(deck[0]);
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        {
            deck.Remove(deck[0]);

        }
        else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {

            NetworkGameManager.Instance.Rpc_RemoveCardFromDeck(0);

        }
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
                NetworkPlayer.Local?.RPC_ChangePassState(false);
                NetworkPlayer.Local?.RPC_ChangeBetUpState(false);



                break;
            case 1: // PASS

                playerControl.actorControl.pass = true;
                playerControl.actorControl.betUp = false;
                NetworkPlayer.Local?.RPC_ChangePassState(true);
                NetworkPlayer.Local?.RPC_ChangeBetUpState(false);

                break;
            case 2: // BETUP

                playerControl.actorControl.pass = false;
                playerControl.actorControl.betUp = true;
                NetworkPlayer.Local?.RPC_ChangePassState(false);
                NetworkPlayer.Local?.RPC_ChangeBetUpState(true);

                break;
        }


        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        {
            playerControl.actorControl.DecidePlayer();

            desicionInd++;
        }
        else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            NetworkGameManager.Instance.Rpc_DecidePlayer();

            desicionInd++;

            NetworkGameManager.Instance.Rpc_ChangeDesicionInd(desicionInd);

        }

        desicitonPanel.SetActive(false);
    }

    bool CheckPlayerHandCompleted()
    {
        return FindObjectOfType<PlayerHandChecker>().CheckHandCompleted(playerControl.GetUIOrderedCards());
    }

    void PlayerHandCompleted()
    {
        makeNoise.PlaySFX(18, 0);
        makeNoise.PlaySFX(25, 0);

        coinController.EarnCoin(100);
        playerControl.actorControl.totalCoins += 100;
        //FindObjectOfType<HandCompletedPanel>(true).OpenPanel(playerControl.actorControl);
        playerWinPanel.SetActive(true);
        FindObjectOfType<MoneyController>().EarnMoney(rewardMoney);
        actorControls[0].winCounter++;
        actorControls[0].totalWinMoney += rewardMoney;

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
        playerWinPanel.SetActive(false);
        FindObjectOfType<HandCompletedPanel>(true).OpenPanel(playerControl.actorControl);
        makeNoise.PlaySFX(27, 0);
    }
}


