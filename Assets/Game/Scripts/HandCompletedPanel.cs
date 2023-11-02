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


    private GameControl gameControl;

    private void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();
        panelBG.SetActive(false);
    }

    public void OpenPanel(ActorControl actorControl)
    {

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
                //NetworkGameManager.Instance.Rpc_UpdateNetworkCompletedHand();

            }
        }
        else
        {
            cards = actorControl.cardsInHand;

            // if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Friends)
            // {
            //     NetworkGameManager.Instance.CardListFromNetworkCardList(cards, NetworkGameManager.Instance.NetworkCompletedHand);
            // }
        }

        for (int i = 0; i < cards.Count; i++)
        {
            //cardSprites[i].sprite = gameControl.cardImages.cardImages[CardSpriteConverter.GetCardSpriteInd(cards[i])];
            cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(cards[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

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
