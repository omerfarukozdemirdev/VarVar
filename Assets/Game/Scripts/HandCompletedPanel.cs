using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HandCompletedPanel : MonoBehaviour
{
    [SerializeField] GameObject panelBG;
    [SerializeField] Image actorAvatar;
    [SerializeField] Text actorNameText;
    [SerializeField] Text rewardText;
    [SerializeField] Image[] cardSprites;

    private GameControl gameControl;

    private void Awake()
    {
        gameControl = FindObjectOfType<GameControl>();
        panelBG.SetActive(false);
    }

    public void OpenPanel(ActorControl actorControl)
    {
        actorAvatar.sprite = actorControl.actorAvatar.sprite;
        actorNameText.text = actorControl.actorName;
        rewardText.text = gameControl.rewardMoney.ToString();

        for(int i = 0; i < cardSprites.Length; i++)
            cardSprites[i].gameObject.SetActive(false);

        List<Card> cards;
        if (actorControl.player)
        {
            cards = new List<Card>(gameControl.playerControl.GetUIOrderedCards());
        }
        else
        {
            cards = actorControl.cardsInHand;
        }

        for(int i = 0; i < cards.Count; i++)
        {
            //cardSprites[i].sprite = gameControl.cardImages.cardImages[CardSpriteConverter.GetCardSpriteInd(cards[i])];
            cardSprites[i].sprite = CardSpriteConverter.GetCardSpriteInd(cards[i], gameControl.gameConfig.deckStyles[gameControl.gameConfig.deckStyleInd]);

            cardSprites[i].gameObject.SetActive(true);
        }

        panelBG.SetActive(true);
    }

    public void ClosePanel()
    {
        panelBG.SetActive(false);
    }
}
