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

        List<Card> cards = new List<Card>();

        if (GameManager.Instance.IsMultiplayer())
        {
            //NetworkPlayer networkPlayer = new NetworkPlayer();
            //for(int i = 0; i < gameControl.networkGlobals.orderedNetworkPlayers.Count; i++)
            //{
            //    if (gameControl.networkGlobals.orderedNetworkPlayers[i].completedHandCardsByte.Count > 0)
            //    {
            //        networkPlayer = gameControl.networkGlobals.orderedNetworkPlayers[i];
            //        break;
            //    }
            //}

            //for (int i = 0; i < networkPlayer.completedHandCardsByte.Count; i++)
            //    cards.Add(NetworkCardConverter.IntToCard((int)networkPlayer.completedHandCardsByte[i]));

            cards = actorControl.cardsInHand;
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
            nextButton.SetActive(false);

            if (!gameControl.networkHandler.isHost)
            {
                //nextButton.SetActive(false);

                // Reset Network Variables
                gameControl.myNetworkPlayer.gotPlayInd = false;
                gameControl.networkGlobals.gotDeck = false;
            }
        }

        panelBG.SetActive(true);
    }

    public void ClosePanel()
    {
        panelBG.SetActive(false);
    }
}
