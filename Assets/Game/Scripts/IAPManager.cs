using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    private string product1 = "com.varvar.game.product1";
    private string product2 = "com.varvar.game.product2";
    private string product3 = "com.varvar.game.product3";


    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == product1)
        {
            GetReward(2000);
        }

        if (product.definition.id == product2)
        {
            GetReward(5000);
        }

        if (product.definition.id == product3)
        {
            GetReward(12500);
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(product.definition.id + " failed because " + failureReason);
    }

    void GetReward(int amount)
    {
        FindObjectOfType<MoneyController>().IAPBuyChip(amount);
    }
}
