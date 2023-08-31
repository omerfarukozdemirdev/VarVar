using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private CoinProduct[] coinProducts;

    private MenuController menuController;
    private CoinController coinController;
    private MakeNoise makeNoise;

    private void Awake()
    {
        menuController = FindObjectOfType<MenuController>();
        coinController = FindObjectOfType<CoinController>();
        makeNoise = FindObjectOfType<MakeNoise>();

        for (int i = 0; i < menuController.gameConfig.shopMenuCoins.Length; i++)
        {
            coinProducts[i].coinCountText.text = menuController.gameConfig.shopMenuCoins[i].coincount.ToString();
            coinProducts[i].priceText.text = "$" + menuController.gameConfig.shopMenuCoins[i].prize;
        }
    }

    public void BuyCoin(int ind)
    {
        coinController.EarnCoin(menuController.gameConfig.shopMenuCoins[ind].coincount);
        CloseShopMenu();
        makeNoise.PlaySFX(22, 0);
    }

    public void OpenShopMenu()
    {
        panel.SetActive(true);
        makeNoise.PlaySFX(27, 0);
    }

    public void CloseShopMenu()
    {
        panel.SetActive(false);
        makeNoise.PlaySFX(17,0);
    }

}

