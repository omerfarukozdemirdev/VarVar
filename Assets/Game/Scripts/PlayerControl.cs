using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    public ActorControl actorControl;

    [Header("Setup")]
    [SerializeField] GridLayoutGroup gridLayoutGRP;
    [SerializeField] float gridSpacingCollaps = 130;
    [SerializeField] float gridSpacingExpand = 150;
    [SerializeField] int gridPaddingCollaps = 0;
    [SerializeField] int gridPaddingExpand = 90;
    [SerializeField] float gridAnimTime = .3f;
    [SerializeField] float cardMoveTime = .25f;

    [Space]
    [SerializeField] Transform[] cardIns;
    public GameObject cardsInLastSlot;
    [SerializeField] Transform pickedCardParent;
    public GameObject throwedCardArea;
    private Animator throwedCardAreaAnimator;
    public GameObject finishCardArea;
    private Animator finishCardAreaAnimator;

    [SerializeField] Text totalMoneyText;

    private Transform cardPicked = null;
    private float cardPickOffsetX;

    public Card lastTakedCardFromThrowed;
    private Card holdedCardInFinishArea;

    private GameControl gameControl;

    [SerializeField] private float currentTime = 0f;
    public Coroutine timerCoroutine;
    public bool timerPause;

    private void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();
        throwedCardAreaAnimator = throwedCardArea.GetComponent<Animator>();
        finishCardAreaAnimator = finishCardArea.GetComponent<Animator>();

        actorControl.player = true;

        actorControl.totalCoins = PlayerPrefs.GetInt("CoinCount");
        DisableUICards();
    }

    public void ResetValues()
    {
        cardPicked = null;
        lastTakedCardFromThrowed = null;

        DisableUICards();

        Set();
    }

    private void Start()
    {
        Set();
    }

    void Set()
    {
        throwedCardArea.SetActive(false);
        finishCardArea.SetActive(false);
        totalMoneyText.text = "$0";
    }

    void Update()
    {
        if (cardPicked != null)
        {
            Vector3 scPoint = Input.mousePosition;
            scPoint.x += cardPickOffsetX;
            cardPicked.position = scPoint;

            int closestSlot = GetClosestSlot(scPoint.x);

            if (cardIns[closestSlot].GetChild(0).childCount > 0)
            {
                GameObject closestCard = cardIns[closestSlot].GetChild(0).GetChild(0).gameObject;

                closestCard.transform.SetParent(cardIns[GetEmptySlot()].GetChild(0));
                MoveToZero(closestCard);
            }

            bool inThrowedArea = false;
            bool inFinishArea = false;
            Vector3 pos = Input.mousePosition;
            if (pos.x >= throwedCardArea.transform.GetChild(0).position.x && pos.x <= throwedCardArea.transform.GetChild(1).position.x &&
                pos.y >= throwedCardArea.transform.GetChild(2).position.y && pos.y <= throwedCardArea.transform.GetChild(3).position.y)
            {
                throwedCardAreaAnimator.SetBool("Blink", true);
                inThrowedArea = true;
            }

            if (pos.x >= finishCardArea.transform.GetChild(0).position.x && pos.x <= finishCardArea.transform.GetChild(1).position.x &&
                pos.y >= finishCardArea.transform.GetChild(2).position.y && pos.y <= finishCardArea.transform.GetChild(3).position.y)
            {
                inFinishArea = true;
            }

            throwedCardAreaAnimator.SetBool("Blink", inThrowedArea);
            finishCardAreaAnimator.SetBool("Blink", inFinishArea);

            if (Input.GetMouseButtonUp(0))
            {
                var isCardAreasActive = throwedCardArea.activeSelf || finishCardArea.activeSelf;
                if (isCardAreasActive && !gameControl.completeHandWarningPanel.activeSelf && cardPicked.GetComponentInChildren<CardTypeHolder>().cardType.suit != CardSuit.Joker)
                {
                    var isCardLastTakedFromThrowed = lastTakedCardFromThrowed != null && cardPicked.GetComponentInChildren<CardTypeHolder>().cardType.suit == lastTakedCardFromThrowed.suit
                        && cardPicked.GetComponentInChildren<CardTypeHolder>().cardType.value == lastTakedCardFromThrowed.value;

                    if (inThrowedArea && !isCardLastTakedFromThrowed)
                    {
                        StopTimer(actorControl);
                        ThrowCard();
                        return;
                    }
                    if (inFinishArea)
                    {
                        timerPause=true;
                        HoldCardInFinishArea();
                        return;
                    }
                }

                CardReleased();
            }
        }
    }

    public List<Card> GetUIOrderedCards()
    {
        List<Card> cards = new List<Card>();

        for (int i = 0; i < cardIns.Length - 1; i++)
        {
            CardTypeHolder cardTypeHolder = cardIns[i].GetComponentInChildren<CardTypeHolder>();
            cards.Add(cardTypeHolder.cardType);
        }

        return cards;
    }

    public void SetMoneyText(int value)
    {
        totalMoneyText.text = "$" + value;
        totalMoneyText.gameObject.SetActive(false);
        totalMoneyText.gameObject.SetActive(true);
    }

    void DisableUICards()
    {
        for (int i = 0; i < cardIns.Length; i++)
        {
            cardIns[i].gameObject.SetActive(false);
        }

        cardsInLastSlot.SetActive(false);
    }

    public void EnableUICards()
    {
        StartCoroutine(UICardsActive());
    }

    IEnumerator UICardsActive()
    {
        for (int i = 0; i < cardIns.Length - 1; i++)
        {
            cardIns[i].gameObject.SetActive(true);
            CardTypeHolder cardTypeHolder = cardIns[i].GetComponentInChildren<CardTypeHolder>();

            cardTypeHolder.cardType = actorControl.cardsInHand[i];
            cardIns[i].GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = CardSpriteConverter.GetCardSpriteInd(actorControl.cardsInHand[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

            yield return new WaitForSeconds(.01f);
        }
    }

    void ThrowedFinishHandAreaEnable()
    {
        throwedCardArea.SetActive(false);

        var isThrowedCardAreaActive = cardsInLastSlot.activeSelf && cardPicked.GetComponentInChildren<CardTypeHolder>().cardType.suit != CardSuit.Joker;

        var isCardLastTakedFromThrowed = lastTakedCardFromThrowed != null && cardPicked.GetComponentInChildren<CardTypeHolder>().cardType.suit == lastTakedCardFromThrowed.suit
                       && cardPicked.GetComponentInChildren<CardTypeHolder>().cardType.value == lastTakedCardFromThrowed.value;

        throwedCardArea.SetActive(isThrowedCardAreaActive && !isCardLastTakedFromThrowed);

        finishCardArea.SetActive(false);
        finishCardArea.SetActive(isThrowedCardAreaActive);
    }

    public void CardPicked(Transform pickedCard)
    {
        gameControl.makeNoise.PlaySFX(19, 0);

        cardPicked = pickedCard;

        cardPickOffsetX = cardPicked.position.x - Input.mousePosition.x;

        Animator animator = cardPicked.GetChild(0).GetComponent<Animator>();
        animator.ResetTrigger("Released");
        animator.SetTrigger("Picked");

        cardPicked.SetParent(pickedCardParent);
        RotateToZero(cardPicked.gameObject);
        ChangeGridSpacing(gridSpacingExpand, gridPaddingExpand);

        ThrowedFinishHandAreaEnable();
    }

    void CardReleased()
    {
        gameControl.makeNoise.PlaySFX(20, 0);

        Animator animator = cardPicked.GetChild(0).GetComponent<Animator>();
        animator.ResetTrigger("Picked");
        animator.SetTrigger("Released");

        cardPicked.SetParent(cardIns[GetEmptySlot()].GetChild(0));
        MoveToZero(cardPicked.gameObject);

        cardPicked = null;
        ChangeGridSpacing(gridSpacingCollaps, gridPaddingCollaps);

        throwedCardAreaAnimator.SetTrigger("Out");
        finishCardAreaAnimator.SetTrigger("Out");
    }

    public void TakeCard(Card cardType)
    {
        StartCoroutine(TakingCard(cardType));
    }

    IEnumerator TakingCard(Card cardType)
    {
        actorControl.AddCard(cardType);

        cardsInLastSlot.SetActive(true);
        cardIns[cardIns.Length - 1].gameObject.SetActive(true);

        Transform takenCard = cardsInLastSlot.transform.GetChild(0).GetChild(0).GetChild(0);

        takenCard.GetChild(0).GetComponent<Image>().sprite = CardSpriteConverter.GetCardSpriteInd(cardType, gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

        takenCard.GetChild(0).GetComponent<Animator>().SetTrigger("Take");
        takenCard.GetComponentInChildren<CardTypeHolder>().cardType = cardType;

        takenCard.SetParent(pickedCardParent);
        RotateToZero(takenCard.gameObject);
        ChangeGridSpacing(gridSpacingExpand, gridPaddingExpand);

        cardPickOffsetX = 0;
        takenCard.transform.position = Input.mousePosition;

        yield return new WaitForSeconds(.1f);

        int ind = GetClosestSlot(takenCard.transform.position.x);
        for (int i = ind; i < cardIns.Length - 1; i++)
        {
            GameObject c = cardIns[i].GetChild(0).GetChild(0).gameObject;
            c.transform.SetParent(cardIns[i + 1].GetChild(0));
            MoveToZero(c);
        }

        cardPicked = takenCard;

        yield return new WaitForSeconds(.1f);

        if (cardPicked != null)
        {
            if (!Input.GetMouseButton(0))
            {
                CardReleased();
            }
            else
            {
                if (timerCoroutine != null)
                {
                    ThrowedFinishHandAreaEnable();
                }
            }
        }
    }

    void ThrowCard()
    {
        lastTakedCardFromThrowed = null;
        throwedCardArea.SetActive(false);
        finishCardArea.SetActive(false);

        Card card = cardPicked.GetComponentInChildren<CardTypeHolder>().cardType;
        gameControl.ThrowCard(card, actorControl);

        int ind = GetEmptySlot();
        for (int i = ind; i < cardIns.Length - 1; i++)
        {
            GameObject c = cardIns[i + 1].GetChild(0).GetChild(0).gameObject;
            c.transform.SetParent(cardIns[i].GetChild(0));
            MoveToZero(c);
        }

        cardsInLastSlot.SetActive(false);
        cardIns[cardIns.Length - 1].gameObject.SetActive(false);
        cardPicked.SetParent(cardIns[cardIns.Length - 1].GetChild(0));
        cardPicked.transform.localPosition = Vector3.zero;
        cardPicked.transform.localEulerAngles = Vector3.zero;

        cardPicked = null;
        ChangeGridSpacing(gridSpacingCollaps, gridPaddingCollaps);
    }

    void HoldCardInFinishArea()
    {
        throwedCardArea.SetActive(false);
        finishCardArea.SetActive(false);

        Card card = cardPicked.GetComponentInChildren<CardTypeHolder>().cardType;

        actorControl.RemoveCard(card);

        holdedCardInFinishArea = card;

        int ind = GetEmptySlot();
        for (int i = ind; i < cardIns.Length - 1; i++)
        {
            GameObject c = cardIns[i + 1].GetChild(0).GetChild(0).gameObject;
            c.transform.SetParent(cardIns[i].GetChild(0));
            MoveToZero(c);
        }

        cardsInLastSlot.SetActive(false);
        cardIns[cardIns.Length - 1].gameObject.SetActive(false);
        cardPicked.SetParent(cardIns[cardIns.Length - 1].GetChild(0));
        cardPicked.transform.localPosition = Vector3.zero;
        cardPicked.transform.localEulerAngles = Vector3.zero;

        cardPicked = null;
        ChangeGridSpacing(gridSpacingCollaps, gridPaddingCollaps);

        gameControl.OpenCompleteHandWarningPanel();
    }

    public void HoldedCardInFinishAreaToCardsInHand()
    {
        StartCoroutine(HoldedCardInFinishAreaToCardsInHandMove());
    }

    IEnumerator HoldedCardInFinishAreaToCardsInHandMove()
    {
        actorControl.AddCard(holdedCardInFinishArea);

        cardsInLastSlot.SetActive(true);
        cardIns[cardIns.Length - 1].gameObject.SetActive(true);

        Transform takenCard = cardsInLastSlot.transform.GetChild(0).GetChild(0).GetChild(0);

        takenCard.GetChild(0).GetComponent<Image>().sprite = CardSpriteConverter.GetCardSpriteInd(holdedCardInFinishArea, gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);
        takenCard.GetChild(0).GetComponent<Animator>().SetTrigger("Take");
        takenCard.GetComponentInChildren<CardTypeHolder>().cardType = holdedCardInFinishArea;

        takenCard.SetParent(pickedCardParent);
        RotateToZero(takenCard.gameObject);
        ChangeGridSpacing(gridSpacingExpand, gridPaddingExpand);

        takenCard.transform.position = cardIns[10].transform.position;

        yield return new WaitForSeconds(.2f);

        Animator animator = takenCard.GetChild(0).GetComponent<Animator>();
        animator.ResetTrigger("Picked");
        animator.SetTrigger("Released");

        takenCard.SetParent(cardIns[10].GetChild(0));
        MoveToZero(takenCard.gameObject);

        gameControl.makeNoise.PlaySFX(20, 0);

        ChangeGridSpacing(gridSpacingCollaps, gridPaddingCollaps);
    }

    void MoveToZero(GameObject go)
    {
        iTween.MoveTo(go, iTween.Hash("position", Vector3.zero, "islocal", true, "time", cardMoveTime, "easetype", iTween.EaseType.easeOutBack));
        RotateToZero(go);
        go.transform.localScale = Vector3.one;
    }

    void RotateToZero(GameObject go)
    {
        iTween.RotateTo(go, iTween.Hash("rotation", Vector3.zero, "islocal", true, "time", cardMoveTime));
    }

    int GetEmptySlot()
    {
        for (int i = 0; i < cardIns.Length; i++)
            if (cardIns[i].gameObject.activeSelf)
                if (cardIns[i].GetChild(0).childCount == 0)
                    return i;

        return -1;
    }

    int GetClosestSlot(float xPos)
    {
        int closestSlotInd = 0;
        float dist = Mathf.Abs(xPos - cardIns[closestSlotInd].position.x);
        for (int i = 0; i < cardIns.Length; i++)
        {
            if (cardIns[i].gameObject.activeSelf)
            {
                var d = Mathf.Abs(xPos - cardIns[i].position.x);
                if (d < dist)
                {
                    closestSlotInd = i;
                    dist = d;
                }
            }
        }

        return closestSlotInd;
    }

    void ChangeGridSpacing(float spacing, int padding)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", gridLayoutGRP.spacing.x, "to", spacing, "time", gridAnimTime, "easetype", iTween.EaseType.easeOutBack, "onupdatetarget", gameObject, "onupdate", "UpdateGridSpacing"));
        iTween.ValueTo(gameObject, iTween.Hash("from", gridLayoutGRP.padding.bottom, "to", padding, "time", gridAnimTime, "easetype", iTween.EaseType.easeOutBack, "onupdatetarget", gameObject, "onupdate", "UpdateGridPadding"));
    }

    void UpdateGridSpacing(float newValue)
    {
        gridLayoutGRP.spacing = new Vector2(newValue, 0);
    }
    void UpdateGridPadding(int newValue)
    {
        gridLayoutGRP.padding = new RectOffset(0, 0, 0, newValue);
    }

    #region Oyuncu Timer
    // sıra oyuncuya geldiğinde timer başlar ve bu süre içerisinde hiç işlem yapmazsa oto kart çekilip atılır.
    // eğer bu süre içerisinde kart çekmiş ve atmamış ise sadece oto kart atılır.
    IEnumerator TimeEndCoroutine(ActorControl currentActorController)
    {
        yield return new WaitForSeconds(0.5f);

        iTween.ScaleTo(currentActorController.transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one * 1.2f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

        if (!currentActorController.gameControl.playerControl.cardsInLastSlot.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(Random.Range(.1f, .6f));

            TimeOutPickCard(currentActorController);
        }

        yield return new WaitForSeconds(1f);

        if (currentActorController.gameControl.playerControl.actorControl.cardsInHand.Count == 11)
        {
            currentActorController.gameControl.playerControl.TimeOutThrowCard(currentActorController);
        }

        SetControllableCards(true);
    }

    //süre bittiğinde
    void TimeEnd(ActorControl currentActorController)
    {
        if (cardPicked != null)
            CardReleased();
        throwedCardArea.SetActive(false);
        finishCardArea.SetActive(false);
        gameControl.DisableEnableTakeCardBtns(false);
        SetControllableCards(false);
        currentTime = 0;
        actorControl.timerCircle.gameObject.SetActive(false);
        StartCoroutine(TimeEndCoroutine(currentActorController));
    }

    // süre başladığında
    void TimeStart(ActorControl currentActorController)
    {
        currentTime = 0;
        currentActorController.timerCircle.gameObject.SetActive(true);
    }

    // süreç boyunca olacaklar
    IEnumerator TimerCoroutine(ActorControl currentActorController)
    {
        TimeStart(currentActorController);

        while (currentTime < currentActorController.gameControl.timeOutTimer)
        {
            if(timerPause)
            {
                yield return null;
            }
            else
            {
                currentTime += Time.deltaTime;
                currentActorController.timerCircle.fillAmount = Mathf.Lerp(0.7f, 0f, currentTime / currentActorController.gameControl.timeOutTimer);

                yield return null;
            }
        }

        if (currentActorController.player)
        {
            TimeEnd(currentActorController);
        }
    }

    // süreyi başlat
    public void StartTimer(ActorControl currentActorController)
    {
        timerCoroutine = StartCoroutine(TimerCoroutine(currentActorController));
    }

    // süreyi durdurup işlemi kestiğinde. yani süre bitmeden kart çekip atıldığında
    public void StopTimer(ActorControl currentActorController)
    {
        if (timerCoroutine == null)
            return;

        StopCoroutine(timerCoroutine);
        timerCoroutine = null;
        currentTime = 0;

        currentActorController.timerCircle.gameObject.SetActive(false);
        currentActorController.gameControl.DisableEnableTakeCardBtns(false);
        currentActorController.gameControl.playerControl.throwedCardArea.SetActive(false);
        currentActorController.gameControl.playerControl.finishCardArea.SetActive(false);
    }

    public void TimeOutThrowCard(ActorControl currentActorController)
    {
        throwedCardArea.SetActive(false);
        finishCardArea.SetActive(false);

        int ind = 0;

        Transform cardThrowed = null;

        for (int i = cardIns.Length-1; i >= 0; i--)
        {
            Card card = cardIns[i].GetComponentInChildren<CardTypeHolder>().cardType;
            if (card.suit != CardSuit.Joker)
            {
                if (lastTakedCardFromThrowed != null && card.suit == lastTakedCardFromThrowed.suit && card.value == lastTakedCardFromThrowed.value)
                    continue;

                ind = i;
                cardThrowed = cardIns[i].GetChild(0).GetChild(0);
                gameControl.ThrowCard(card, actorControl);
                break;
            }
        }

        for (int i = ind; i < cardIns.Length - 1; i++)
        {
            GameObject c = cardIns[i + 1].GetChild(0).GetChild(0).gameObject;
            c.transform.SetParent(cardIns[i].GetChild(0));
            MoveToZero(c);
        }

        cardsInLastSlot.SetActive(false);
        cardIns[cardIns.Length - 1].gameObject.SetActive(false);
        cardThrowed.SetParent(cardIns[cardIns.Length - 1].GetChild(0));
        cardThrowed.transform.localPosition = Vector3.zero;
        cardThrowed.transform.localEulerAngles = Vector3.zero;

        lastTakedCardFromThrowed = null;
        ChangeGridSpacing(gridSpacingCollaps, gridPaddingCollaps);
    }

    // // oyuncunun süresi bittiğinde oto kapalı desteden kart çeker
    public void TimeOutPickCard(ActorControl currentActorController)
    {
        currentActorController.gameControl.makeNoise.PlaySFX(15, 0);

        CardClose cardClose = currentActorController.gameControl.tableAnimationControl.cardCloses[currentActorController.gameControl.tableAnimationControl.cardCloses.Count - 1];
        cardClose.gameObject.SetActive(true);
        cardClose.Pick(actorControl.actorTransform.GetChild(0).position);
        gameControl.tableAnimationControl.cardCloses.Remove(cardClose);

        currentActorController.cardsInHand.Add(currentActorController.gameControl.deck[0]);

        cardsInLastSlot.GetComponentInChildren<CardTypeHolder>(true).cardType = gameControl.deck[0];

        currentActorController.gameControl.playerControl.cardsInLastSlot.SetActive(true);
        currentActorController.gameControl.playerControl.cardsInLastSlot.transform.GetChild(0).gameObject.SetActive(true);
        currentActorController.gameControl.playerControl.cardsInLastSlot.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = CardSpriteConverter.GetCardSpriteInd(actorControl.cardsInHand.Last(), gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

        currentActorController.gameControl.deck.Remove(gameControl.deck[0]);

        currentActorController.gameControl.CheckDeckCardCount();
    }

    // süre bittiğinde kartların raycasttarget ı kapatılıyor ve kart atıldığında açılıyor. böylece oto kart çekilip oto kart atıldığı anda 
    // kartlara dokunulup hareket edilmesi engelleniyor
    void SetControllableCards(bool state)
    {
        for (int i = 0; i < cardIns.Length; i++)
        {
            if (!cardIns[i].GetComponentInChildren<Image>())
                continue;
            cardIns[i].GetComponentInChildren<Image>().raycastTarget = state;
        }
    }
    #endregion
}
