using UnityEngine;
using UnityEngine.UI;

public class BGProduct : MonoBehaviour
{
    [SerializeField] Image bgIcon;
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

        bgIcon.sprite = menuController.gameConfig.backGrounds[ind];

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
        return PlayerPrefs.GetInt("BGLck" + ind) == 1 || ind < 1;
    }

    public void BGBtnClicked()
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
                PlayerPrefs.SetInt("BGLck" + ind, 1);
                SetPrice();
            }
        }
    }

    void SetInd()
    {
        menuController.gameConfig.backgroundInd = ind;
        PlayerPrefs.SetInt("BG", ind);

        menuController.SetBG();

        FindObjectOfType<MakeNoise>().PlaySFX(23, 0);
    }
}
