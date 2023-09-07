using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ActorControl : MonoBehaviour
{
    public bool player;

    public Transform actorTransform;
    public List<Card> cardsInHand;
    public List<Card> missingCards;
    public List<Card> remainingCards;

    [SerializeField] GameObject speechBalloon;
    [SerializeField] GameObject money;
    [SerializeField] Text moneyText;
    public Image actorAvatar;
    public string actorName;
    [SerializeField] Text actorNameText;
    public int moneyIn;

    [SerializeField] GameObject moneyAtractorParticle;

    [SerializeField] private int handCompleteStep;
    public bool handCompleted;

    public bool pass;
    public bool betUp;

    private HandAranger handAranger;
    private GameControl gameControl;

    public int winCounter;
    public int passCounter;
    public int totalWinMoney;
    public int totalBetMoney;
    public int totalCoins;
    public GameObject drink;

    private NameGenerator nameGenerator;

    public Gender gender;


    private void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();
        handAranger = FindObjectOfType<HandAranger>();

        speechBalloon.SetActive(false);
        money.SetActive(false);
        moneyAtractorParticle.SetActive(false);



        winCounter = 0;
        passCounter = 0;
        totalWinMoney = 0;
        totalBetMoney = 0;
    }

    private void Start()
    {
        if (!player)
        {
            totalCoins = 150;


            nameGenerator = new NameGenerator();

            InitGender();
        }
    }

    void SetRandomGender()
    {
        Gender[] genders = (Gender[])System.Enum.GetValues(typeof(Gender));

        Gender randomGender = genders[Random.Range(0, genders.Length)];

        gender = randomGender;
    }

    void InitGender()
    {
        SetRandomGender();

        switch (gender)
        {
            case Gender.Male:
                actorName = nameGenerator.GetRandomMaleName();
                actorAvatar.sprite= gameControl.gameConfig.maleAvatars[Random.Range(0, gameControl.gameConfig.maleAvatars.Length)];
                break;
            case Gender.Female:
                actorName = nameGenerator.GetRandomFemaleName();
                actorAvatar.sprite = gameControl.gameConfig.femaleAvatars[Random.Range(0, gameControl.gameConfig.femaleAvatars.Length)];
                break;
            default:
                break;
        }

        actorNameText.text = actorName;

    }

    public void ResetValues()
    {
        cardsInHand = new List<Card>();
        missingCards = new List<Card>();
        remainingCards = new List<Card>();

        speechBalloon.SetActive(false);
        money.SetActive(false);
        moneyAtractorParticle.SetActive(false);

        pass = false;
        betUp = false;

        handCompleteStep = 0;
        handCompleted = false;

        moneyIn = 0;

        GetComponent<CanvasGroup>().alpha = 1;
        speechBalloon.transform.localScale = Vector3.one;
        money.transform.localScale = Vector3.one;

        
    }

    public void DisableSpeechBaloon()
    {
        iTween.ScaleTo(speechBalloon, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));
    }

    public void ArrangeHand()
    {
        cardsInHand = new List<Card>(handAranger.Arrange(cardsInHand));

        missingCards = new List<Card>(handAranger.MissingCards());
        remainingCards = new List<Card>(handAranger.RemainingCards());

        handCompleted = handAranger.HandCompleted();
        handCompleteStep = handAranger.HandCompletedStep();
    }

    public void PlayerArrangeHand()
    {
        handAranger.Arrange(cardsInHand);

        missingCards = new List<Card>(handAranger.MissingCards());
        remainingCards = new List<Card>(handAranger.RemainingCards());

        handCompleted = handAranger.HandCompleted();
        handCompleteStep = handAranger.HandCompletedStep();
    }

    public void AddCard(Card card)
    {
        cardsInHand.Add(card);
        ArrangeHand();
    }

    public void RemoveCard(Card card)
    {
        cardsInHand.Remove(card);
        ArrangeHand();
    }

    public void SetMoney(int value)
    {
        int oldMoneyIn = 0;
        if (moneyIn > 0)
            oldMoneyIn = moneyIn;

        moneyIn = value;
        moneyText.text = "$" + value;

        money.SetActive(true);
        iTween.ScaleFrom(money, iTween.Hash("scale", Vector3.zero, "time", .75f, "easetype", iTween.EaseType.easeOutBounce));

        if (player)
            FindObjectOfType<MoneyController>().SpendMoney(value - oldMoneyIn);
    }

    public void MoneyAtractor()
    {
        moneyAtractorParticle.transform.SetParent(gameControl.moneyAtractorParent);
        moneyAtractorParticle.SetActive(false);
        moneyAtractorParticle.SetActive(true);

        gameControl.SetRewardMoney(moneyIn);

        iTween.ScaleTo(money, iTween.Hash("scale", Vector3.zero, "time", .75f, "easetype", iTween.EaseType.easeOutBounce));
    }

    public void DecideBet()
    {
        pass = true;
        betUp = false;

        if (handCompleted)
        {
            betUp = true;
            pass = false;
        }
        else
        {
            if (handCompleteStep < 3) // 3
            {
                pass = false;

                if (Random.Range(0, 2) == 0)
                    betUp = true;
            }
            else if (handCompleteStep < 5)
            {
                if (gameControl.betUp)
                {
                    if (Random.Range(0, 4) == 0)
                        pass = false;
                }
                else
                {
                    pass = false;
                }

            }
            else if (handCompleteStep < 7)
            {
                if (gameControl.betUp)
                {
                    if (Random.Range(0, 8) == 0)
                        pass = false;
                }
                else
                {
                    if (Random.Range(0, 4) == 0)
                        pass = false;
                }
            }
        }

        if (gameControl.passCount > 2)
            pass = false;

        if (pass)
            gameControl.passCount++;

        StartCoroutine(Decide());
    }

    public void DecidePlayer()
    {
        StartCoroutine(Decide());
    }

    IEnumerator Decide()
    {
        gameControl.makeNoise.PlaySFX(14, 0);
        iTween.ScaleTo(transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

        for (int i = 0; i < speechBalloon.transform.childCount; i++)
            speechBalloon.transform.GetChild(i).gameObject.SetActive(false);

        if (player)
            yield return new WaitForSeconds(.1f);
        else
            yield return new WaitForSeconds(Random.Range(.5f, 1.2f));

        if (pass)
        {
            gameControl.makeNoise.PlaySFX(8, 0);

            speechBalloon.SetActive(true);
            speechBalloon.transform.GetChild(1).gameObject.SetActive(true);

            iTween.ScaleFrom(speechBalloon, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));

            yield return new WaitForSeconds(.5f);

            GetComponent<CanvasGroup>().alpha = .1f;
            iTween.ScaleTo(transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * .9f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

            passCounter++;
        }
        else
        {
            if (betUp)
            {
                speechBalloon.SetActive(true);

                if (gameControl.betUp)
                {
                    speechBalloon.transform.GetChild(0).gameObject.SetActive(true);
                    gameControl.makeNoise.PlaySFX(9, 0);
                }

                else
                {
                    speechBalloon.transform.GetChild(2).gameObject.SetActive(true);
                    gameControl.makeNoise.PlaySFX(10, 0);
                }

                gameControl.BetUP(this);
            }
            else
            {
                speechBalloon.SetActive(true);
                speechBalloon.transform.GetChild(0).gameObject.SetActive(true);

                gameControl.makeNoise.PlaySFX(9, 0);
            }

            iTween.ScaleFrom(speechBalloon, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));

            yield return new WaitForSeconds(.5f);

            if(gameControl.betUp)
                SetMoney(1000);
            else
                SetMoney(500);

            gameControl.makeNoise.PlaySFX(7, 0);

            iTween.ScaleTo(transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
        }


        yield return new WaitForSeconds(.5f);

        gameControl.ActorDecisiton();
    }

    public void PlayCard()
    {
        if (cardsInHand.Count > 10)
        {
            if (handCompleted)
            {
                gameControl.makeNoise.PlaySFX(18,0);
                gameControl.makeNoise.PlaySFX(25, 0);

                FindObjectOfType<HandCompletedPanel>(true).OpenPanel(this);
                winCounter++;
                totalWinMoney += gameControl.rewardMoney;
                totalCoins += 100;
                gameControl.coinController.EarnCoin(100);
                return;
            }

            StartCoroutine(ThrowCard());
            return;
        }

        StartCoroutine(PickCard());
    }

    IEnumerator PickCard()
    {
        iTween.ScaleTo(transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

        yield return new WaitForSeconds(Random.Range(.1f, .6f));

        bool pickFromDeck = true;

        if (gameControl.throwedCards.Count > 0)
        {
            Card throwedCard = gameControl.throwedCards[gameControl.throwedCards.Count - 1];
            for (int i = 0; i < missingCards.Count; i++)
            {
                if (throwedCard.suit == missingCards[i].suit &&
                    throwedCard.value == missingCards[i].value)
                {
                    pickFromDeck = false;
                    break;
                }
            }
        }

        gameControl.PickCard(this, pickFromDeck);
    }

    IEnumerator ThrowCard()
    {
        yield return new WaitForSeconds(Random.Range(.5f, .8f));

        Card card;

        if (remainingCards.Count > 1)
        {
            card = remainingCards[Random.Range(0, remainingCards.Count)];
            while (card.suit == CardSuit.Joker)
                card = remainingCards[Random.Range(0, remainingCards.Count)];
        }
        else
        {
            card = cardsInHand[Random.Range(0, cardsInHand.Count)];
            while (card.suit == CardSuit.Joker)
                card = cardsInHand[Random.Range(0, cardsInHand.Count)];
        }

        gameControl.ThrowCard(card, this);

        yield return new WaitForSeconds(.5f);
        iTween.ScaleTo(transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

        TakeDrink();
    }

    void TakeDrink()
    {
        gameControl.drinkCounter++;
        if (!pass)
        {
            if (gameControl.drinkCounter % 5 == 0)
            {
                var drinkContent = drink.transform.GetChild(0);
                drinkContent.GetComponent<Image>().sprite = gameControl.drinkController.drinks[Random.Range(0,gameControl.drinkController.drinks.Length)].Icon;
                iTween.ScaleTo(drinkContent.gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));
                iTween.ScaleTo(drinkContent.gameObject, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce, "delay", 12f));
            }
        }
    }


}

public enum Gender
{
    Male,
    Female
}