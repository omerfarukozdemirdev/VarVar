using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Fusion;

public class HandCompletedPanel : MonoBehaviour
{
    [SerializeField] GameObject panelBG;
    [SerializeField] Image actorAvatar;
    [SerializeField] Text actorNameText;
    [SerializeField] Text rewardText;
    public Image[] cardSprites;

    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject mainMenuButton;
    [SerializeField] private Text completeHeaderText;

    private GameControl gameControl;

    private void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();
        panelBG.SetActive(false);
    }

    public void OpenPanel(ActorControl actorControl)
    {
        if (gameControl.networkPassCounter == gameControl.orderOfPlayActors.Count - 1 && gameControl.actorControls.Count > 2)
        {
            completeHeaderText.text = "Herkes Pass Dedi";
        }
        else
        {
            completeHeaderText.text = "Hand Completed";

        }

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

        List<Card> cards;
        if (actorControl.player)
        {
            cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());

            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            {
                gameControl.CompletedHand.Clear();
                gameControl.CompletedHand = cards;

            }
        }
        else
        {
            cards = actorControl.cardsInHand;

        }

        for (int i = 0; i < 10; i++)
        {
            //cardSprites[i].sprite = gameControl.cardImages.cardImages[CardSpriteConverter.GetCardSpriteInd(cards[i])];
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Quick)
            {
                cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(cards[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

            }
            else if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            {
                cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(NetworkGameManager.Instance.NetworkCardToCard(NetworkGameManager.Instance.NetworkCompletedHand[i]), gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

            }

            cardSprites[i].gameObject.SetActive(true);
        }

        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
        {
            if (!gameControl.Host)
            {
                nextButton.SetActive(false);
                mainMenuButton.SetActive(false);

            }
            gameControl.playerWinPanel.SetActive(false);
        }

        panelBG.SetActive(true);
    }

    public void ClosePanel()
    {
        panelBG.SetActive(false);
    }
}
