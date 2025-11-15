using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandCompletedPanel : MonoBehaviour
{
    [SerializeField] GameObject panelBG;
    [SerializeField] Image actorAvatar;
    [SerializeField] Text actorNameText;
    [SerializeField] Text rewardText;
    public Image[] cardSprites;

    public GameObject nextButton;
    public GameObject mainMenuButton;
    [SerializeField] private Text completeHeaderText;

    private GameControl gameControl;

    private List<Card> cards;
    public List<GameObject> loserPlayerTabs;
    public GameObject winnerPlayerTab;
    
    private void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();
        panelBG.SetActive(false);
    }
    
    public void OpenPanel(ActorControl actorControl)
    {
        GameManager.Instance.gameStat = GameManager.GameStat.handCompleted;

        if(actorControl == null)
        {
            completeHeaderText.text = "Herkes Pass Dedi";

            actorAvatar.gameObject.SetActive(false);
            actorNameText.gameObject.SetActive(false);
            rewardText.gameObject.SetActive(false);
            for (int i = 0; i < cardSprites.Length; i++)
                cardSprites[i].gameObject.SetActive(false);

            nextButton.SetActive(false);
            mainMenuButton.SetActive(true);

            panelBG.SetActive(true);
            return;
        }
        
        InitTabArea(actorControl);
        
        completeHeaderText.text = "Hand Completed";

        gameControl.actorControls.ForEach(x => x.totalBetMoney = x.totalBetMoney + x.moneyIn);

        if (gameControl.gameCounter == gameControl.gameLimit)
        {
            nextButton.SetActive(false);
            mainMenuButton.SetActive(true);
        }
        else
        {
            nextButton.SetActive(true);
            mainMenuButton.SetActive(false);
        }

        actorAvatar.sprite = actorControl.actorAvatar.sprite;
        actorNameText.text = actorControl.actorName;
        rewardText.text = gameControl.rewardMoney.ToString();

        for (int i = 0; i < cardSprites.Length; i++)
            cardSprites[i].gameObject.SetActive(false);

        cards = new List<Card>();

        if (actorControl.player)
        {
            cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());
        }
        else
        {
            cards = actorControl.cardsInHand;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(cards[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);
            cardSprites[i].gameObject.SetActive(true);
        }

        panelBG.SetActive(true);
    }

    public void ClosePanel()
    {
        panelBG.SetActive(false);
    }

    void InitTabArea(ActorControl winnerActorControl)
    {
        foreach (var tab in loserPlayerTabs)
        {
            tab.SetActive(false);
        }

        foreach (var actorControl in gameControl.actorControls)
        {
            var index = gameControl.actorControls.IndexOf(actorControl);
            var tab = loserPlayerTabs[index];
            var button = tab.GetComponent<Button>();
        
            if (winnerActorControl==actorControl)
            {
                winnerPlayerTab.transform.GetChild(1).GetComponent<Text>().text = winnerActorControl.actorName;
                winnerPlayerTab.GetComponent<Button>().onClick.AddListener(delegate { ShowWinnerPlayerHand(winnerActorControl); });
            }
            else
            {
                tab.SetActive(true);
                tab.transform.GetChild(1).GetComponent<Text>().text = actorControl.actorName;
                button.onClick.AddListener(delegate { ShowLoserPlayerHand(actorControl); });
            }
        }
    }

    public void ShowWinnerPlayerHand(ActorControl actorControl)
    {
        UpdatePlayerInfos(actorControl);
        ShowPlayerHand(true, cards);
    }

    public void ShowLoserPlayerHand(ActorControl actorControl)
    {
        UpdatePlayerInfos(actorControl);
        ShowPlayerHand(false, actorControl.cardsInHand);
    }

    void ShowPlayerHand(bool isWin, List<Card> cards)
    {
        cardSprites[cardSprites.Length - 1].gameObject.SetActive(false);

        int totalCard = cardSprites.Length;
        totalCard--;
        
        for (int i = 0; i < totalCard; i++)
        {
            cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(cards[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);
            cardSprites[i].gameObject.SetActive(true);
        }
    }

    void UpdatePlayerInfos(ActorControl actorControl)
    {
        actorAvatar.sprite = actorControl.actorAvatar.sprite;
        actorNameText.text = actorControl.actorName;
    }

}
