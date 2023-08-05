using UnityEngine;
using UnityEngine.UI;

public class DeckbackProduct : MonoBehaviour
{
    [SerializeField] Image deckbackIcon;
    [SerializeField] Text priceText;
    [SerializeField] GameObject coinIcon;
    int price;
    int ind;

    CoinController coinController;
    MenuController menuController;

    public void Setup(int aIndex, CoinController cc, MenuController mc)
    {
        ind = aIndex;
        coinController = cc;
        menuController = mc;

        deckbackIcon.sprite = menuController.gameConfig.cardBackSprites[ind];

        SetPrice();
    }

    void SetPrice()
    {
        if (Unlocked())
        {
            coinIcon.SetActive(false);
            return;
        }

        price = 100 + (20 * ind);
        priceText.text = price.ToString();
    }

    bool Unlocked()
    {
        return PlayerPrefs.GetInt("DeckBackLck" + ind) == 1 || ind < 1;
    }

    public void DeckBtnClicked()
    {
        if (Unlocked())
        {
            SetInd();
        }
        else
        {
            if (coinController.SpendCoin(price))
            {
                SetInd();
                PlayerPrefs.SetInt("DeckBackLck" + ind, 1);
                SetPrice();
            }
        }
    }

    void SetInd()
    {
        menuController.gameConfig.cardBackInd = ind;
        PlayerPrefs.SetInt("DeckBack", ind);

        menuController.SetDeckBack();

        FindObjectOfType<MakeNoise>().PlaySFX(23, 0);
    }
}
