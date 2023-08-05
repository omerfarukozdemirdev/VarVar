using UnityEngine;
using UnityEngine.UI;

public class AvatarProduct : MonoBehaviour
{
    [SerializeField] Image avatarIcon;
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

        avatarIcon.sprite = menuController.gameConfig.avatars[ind];

        SetPrice();
    }

    void SetPrice()
    {
        if(Unlocked())
        {
            coinIcon.SetActive(false);
            return;
        }

        price = 100 + (20 * ind);
        priceText.text = price.ToString();
    }

    bool Unlocked()
    {
        return PlayerPrefs.GetInt("AvatarLck" + ind) == 1 || ind < 2;
    }

    public void AvatarBtnClicked()
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
                PlayerPrefs.SetInt("AvatarLck" + ind, 1);
                SetPrice();
            }
        }
    }

    void SetInd()
    {
        menuController.gameConfig.avatarInd = ind;
        PlayerPrefs.SetInt("Avatar", ind);

        menuController.SetAvatar();

        FindObjectOfType<MakeNoise>().PlaySFX(23, 0);
    }
}
