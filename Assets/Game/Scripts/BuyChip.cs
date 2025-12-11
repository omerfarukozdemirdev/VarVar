using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyChip : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private int amount;
    [SerializeField] private int price;

    void Start()
    {
        amountText.text = amount.ToString() + " Chip";

        if (price > 0)
            priceText.text = price.ToString() + "";
        else
            priceText.text = "Free";
    }

    public void GetReward()
    {
        FindObjectOfType<MoneyController>().IAPBuyChip(amount);
    }
}
