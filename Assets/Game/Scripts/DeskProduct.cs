using UnityEngine;
using UnityEngine.UI;

public class DeskProduct : MonoBehaviour
{
    [SerializeField] Image deskIcon;
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

        deskIcon.sprite = menuController.gameConfig.tables[ind];

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
        return PlayerPrefs.GetInt("DeskLck" + ind) == 1 || ind < 1;
    }

    public void DeskBtnClicked()
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
                PlayerPrefs.SetInt("DeskLck" + ind, 1);
                SetPrice();
            }
        }
    }

    void SetInd()
    {
        menuController.gameConfig.tableInd = ind;
        PlayerPrefs.SetInt("Desk", ind);

        menuController.SetDesk();

        FindObjectOfType<MakeNoise>().PlaySFX(23, 0);
    }
}
