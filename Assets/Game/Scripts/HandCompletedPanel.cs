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
        GameManager.Instance.gameStat = GameManager.GameStat.handCompleted;

        completeHeaderText.text = "Hand Completed";
        //if (gameControl.networkPassCounter == gameControl.orderOfPlayActors.Count - 1 && gameControl.actorControls.Count > 2)
        //{
        //    completeHeaderText.text = "Herkes Pass Dedi";
        //}
        //else
        //{
        //    completeHeaderText.text = "Hand Completed";
        //}

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

        List<Card> cards = new List<Card>();

        if (GameManager.Instance.IsMultiplayer())
        {
            NetworkPlayer networkPlayer = new NetworkPlayer();
            for(int i = 0; i < gameControl.networkGlobals.orderedNetworkPlayers.Count; i++)
            {
                if(gameControl.networkGlobals.orderedNetworkPlayers[i].completedHandCards.Count > 0)
                {
                    networkPlayer = gameControl.networkGlobals.orderedNetworkPlayers[i];
                    break;
                }
            }

            for (int i = 0; i < networkPlayer.completedHandCards.Count; i++)
                cards.Add(NetworkCardConverter.NetworkCardToCard(networkPlayer.completedHandCards[i]));
        }
        else
        {
            if (actorControl.player)
            {
                cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());
            }
            else
            {
                cards = actorControl.cardsInHand;
            }
        }

        //for (int i = 0; i < 11; i++)
        for (int i = 0; i < cards.Count; i++)
        {
            //cardSprites[i].sprite = gameControl.cardImages.cardImages[CardSpriteConverter.GetCardSpriteInd(cards[i])];
            cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(cards[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);
            cardSprites[i].gameObject.SetActive(true);
        }

        if (GameManager.Instance.IsMultiplayer())
        {
            if (!gameControl.networkHandler.isHost)
            {
                nextButton.SetActive(false);
            }
        }

        panelBG.SetActive(true);
    }

    public void ClosePanel()
    {
        panelBG.SetActive(false);
    }
}
