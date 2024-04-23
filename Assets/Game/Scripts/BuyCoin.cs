using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyCoin : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private int amount;
    [SerializeField] private int price;

    private MoneyController moneyController;
    private CoinController coinController;

    // Start is called before the first frame update
    void Start()
    {
        moneyController = FindObjectOfType<MoneyController>();
        coinController = FindObjectOfType<CoinController>();
        amountText.text = amount.ToString() + " Coin";
        priceText.text = price.ToString() + " Chip";
    }

    public void Buy()
    {
        if(moneyController.moneyCount > price)
        {
            moneyController.SpendMoney(price);
            coinController.EarnCoin(amount);
            coinController.SetCoinCountText();
        }
        else
        {
            FindObjectOfType<MenuController>().noChipPopup.SetActive(true);
        }
    }
}
