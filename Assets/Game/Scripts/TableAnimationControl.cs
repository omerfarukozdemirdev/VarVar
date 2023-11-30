using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableAnimationControl : MonoBehaviour
{
    [SerializeField] CamController camController;
    [SerializeField] GameObject dealModel;
    [SerializeField] GameObject cardsBlockPrefab;
    [SerializeField] Transform cardBlockParent;
    private List<CardsBlock> cardsBlock;
    [SerializeField] GameObject cardClosePrefab;
    public Transform cardCloseParent;
    public List<CardClose> cardCloses;

    public Transform throwedCardsPos;

    private GameControl gameControl;

    void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();

        CreateCardsBlocks();
        CreateCardCloses(104);
    }

    public void ResetValues()
    {
        //for (int i = 0; i < throwedCardsPos.childCount; i++)
        //    Destroy(throwedCardsPos.GetChild(i).gameObject);

        //for (int i = 0; i < cardCloses.Count; i++)
        //    Destroy(cardCloses[i].gameObject);
    }

    #region Setup
    void CreateCardsBlocks()
    {
        cardsBlock = new List<CardsBlock>();

        for (int i = 0; i < 13; i++)
        {
            GameObject g = Instantiate(cardsBlockPrefab, cardBlockParent);
            g.transform.localPosition = new Vector3(0, i * 0.02f, 0);
            g.transform.localScale = Vector3.one;
            SkinnedMeshRenderer[] skinnedMeshRenderers = g.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int m = 0; m < skinnedMeshRenderers.Length; m++)
                skinnedMeshRenderers[m].sharedMaterial = gameControl.gameConfig.cardBacks[gameControl.gameConfig.cardBackInd];

            cardsBlock.Add(g.GetComponent<CardsBlock>());
        }

        DisableCardsBlocks();
    }
    void DisableCardsBlocks()
    {
        for (int i = 0; i < cardsBlock.Count; i++)
        {
            cardsBlock[i].gameObject.SetActive(false);
        }
    }
    void CreateCardCloses(int count)
    {
        List<CardClose> oldList = new List<CardClose>(cardCloses);
        for (int i = 0; i < oldList.Count; i++)
            oldList[i].transform.SetParent(cardCloseParent);

        cardCloses = new List<CardClose>();
        bool dontInstatiate = oldList.Count > 0;

        GameObject g;
        for (int i = 0; i < count; i++)
        {
            if (dontInstatiate)
            {
                g = oldList[i].gameObject;
                g.SetActive(true);
            }
            else
            {
                g = Instantiate(cardClosePrefab, cardCloseParent);
                g.GetComponentInChildren<MeshRenderer>().sharedMaterial = gameControl.gameConfig.cardBacks[gameControl.gameConfig.cardBackInd];
            }

            g.transform.localPosition = new Vector3(0, i * 0.0025f, 0);
            g.transform.localScale = Vector3.one;
            cardCloses.Add(g.GetComponent<CardClose>());
        }

        DisableCardCloses();
    }
    void DisableCardCloses()
    {
        cardCloseParent.gameObject.SetActive(false);
    }
    #endregion


    public void StartGame()
    {
        StartCoroutine(ArrangeCardsBlocks());
    }

    IEnumerator ArrangeCardsBlocks()
    {
        camController.CamStart();

        DisableCardsBlocks();

        yield return new WaitForSecondsRealtime(.5f);

        for (int i = 0; i < cardsBlock.Count; i++)
        {
            cardsBlock[i].gameObject.SetActive(true);
            gameControl.makeNoise.PlaySFX(0, 0);
            yield return new WaitForSecondsRealtime(.01f);
        }

        StartCoroutine(MoveDealModel());
    }

    IEnumerator MoveDealModel()
    {
        yield return new WaitForSecondsRealtime(.5f);

        gameControl.makeNoise.PlaySFX(1, 0);
        iTween.MoveTo(dealModel, iTween.Hash("position", gameControl.actorControls[gameControl.cardDealerInd].actorTransform.GetChild(0).position, "time", .4f, "easetype", iTween.EaseType.easeOutQuad));

        yield return new WaitForSecondsRealtime(.6f);
        StartCoroutine(SortCardsBlocks());
    }

    IEnumerator SortCardsBlocks()
    {
        gameControl.makeNoise.PlaySFX(2, 0);
        for (int i = 0; i < cardsBlock.Count; i++)
        {
            cardsBlock[i].CardsOpen();
            yield return new WaitForSecondsRealtime(.001f);
        }

        yield return new WaitForSecondsRealtime(.4f);

        gameControl.makeNoise.PlaySFX(3, 0);
        for (int i = 0; i < cardsBlock.Count; i++)
        {
            cardsBlock[i].CardsFlop();
            yield return new WaitForSecondsRealtime(.025f);
        }

        yield return new WaitForSecondsRealtime(.25f);

        gameControl.makeNoise.PlaySFX(4, 0);
        for (int i = 0; i < cardsBlock.Count; i++)
        {
            cardsBlock[i].CardsClose();
        }

        yield return new WaitForSecondsRealtime(.6f);

        DisableCardsBlocks();
        cardCloseParent.gameObject.SetActive(true);

        StartCoroutine(DealCards());
        camController.CamGameplay();
    }

    IEnumerator DealCards()
    {
        gameControl.makeNoise.PlaySFX(5, 0);
        int[] dealCounts = new int[] { 2, 3, 4 };

        for (int i = 0; i < dealCounts.Length; i++)
        {
            for (int s = 0; s < gameControl.orderOfPlayActors.Count; s++)
            {
                for (int j = 0; j < dealCounts[i]; j++)
                {
                    cardCloses[cardCloses.Count - 1].Deal(gameControl.orderOfPlayActors[s].actorTransform.GetChild(0).position);
                    cardCloses.Remove(cardCloses[cardCloses.Count - 1]);
                    yield return new WaitForSecondsRealtime(.025f);
                }
            }
        }

        gameControl.makeNoise.PlaySFX(6, 0);
        gameControl.playerControl.EnableUICards();

        yield return new WaitForSecondsRealtime(.75f);

        gameControl.makeNoise.PlaySFX(7, 0);
        if (gameControl.actorControls.Count > 1)
        {
            gameControl.StartBets();

        }

        yield return new WaitForSecondsRealtime(1f);

        if (gameControl.actorControls.Count > 1)
        {
            gameControl.ActorDecisiton();
        }
    }

    public void CreateDeckFromThrowedCards(int deckCount)
    {
        //for (int i = 0; i < throwedCardsPos.childCount; i++)
        //    Destroy(throwedCardsPos.GetChild(i).gameObject);

        for (int i = 0; i < throwedCardsPos.childCount; i++)
            throwedCardsPos.GetChild(i).gameObject.SetActive(false);

        StartCoroutine(DeckFromThrowedCards(deckCount));
    }

    IEnumerator DeckFromThrowedCards(int deckCount)
    {
        yield return new WaitForSecondsRealtime(.25f);

        //for (int i = 0; i < cardCloses.Count; i++)
        //    Destroy(cardCloses[i].gameObject);

        CreateCardCloses(deckCount);

        for (int i = 0; i < cardsBlock.Count; i++)
        {
            cardsBlock[i].gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(.001f);
        }

        DisableCardsBlocks();
        cardCloseParent.gameObject.SetActive(true);
    }

    public void ThrowCard(GameObject throwedCard, ActorControl actorControl, float yOffset)
    {
        StartCoroutine(ThrowingCard(throwedCard, actorControl, yOffset));
    }

    IEnumerator ThrowingCard(GameObject throwedCard, ActorControl actorControl, float yOffset)
    {
        if (actorControl.player)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 fPos = new Vector3(pos.x, 2, 3.5f);
            throwedCard.transform.position = fPos;

            iTween.ScaleTo(actorControl.transform.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
        }
        else
        {
            throwedCard.transform.SetParent(actorControl.actorTransform);
            throwedCard.transform.localPosition = new Vector3(0, .1f, 0);
            throwedCard.transform.localScale = Vector3.one * .5f;
        }

        throwedCard.transform.SetParent(throwedCardsPos);

        Vector3 destPos = throwedCardsPos.position;
        destPos.x += Random.Range(-.3f, .3f);
        destPos.z += Random.Range(-.3f, .3f);
        destPos.y = yOffset;

        iTween.MoveTo(throwedCard, iTween.Hash("position", destPos, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
        iTween.RotateTo(throwedCard, iTween.Hash("y", Random.Range(500, 900), "time", .3f));
        iTween.ScaleTo(throwedCard, iTween.Hash("scale", Vector3.one * 1.1f, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));

        //NetworkGameManager.Instance?.Rpc_UpdateNetworkPlayingInd();

        yield return new WaitForSecondsRealtime(1f);

        // NetworkGameManager.Instance?.Rpc_PlayerThrowCardAnimations();

        gameControl.NextActor();
        //if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
        //{
        //    gameControl.NextActor();
        //}
    }

    public void Reset()
    {
        for (int i = 0; i < cardCloses.Count; i++)
            Destroy(cardCloses[i].gameObject);
        cardCloses.Clear();
        CreateCardCloses(104);
        //StartCoroutine(ArrangeCardsBlocks());
    }

    public void FirsGroundCard()
    {
        //firstCard.transform.SetParent(throwedCardsPos);
        cardCloses[cardCloses.Count - 1].FirstPick();
        cardCloses.Remove(cardCloses[cardCloses.Count - 1]);
    }
}
