using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    private string product1 = "com.gamebro.varvar.product1";
    private string product2 = "com.gamebro.varvar.product2";
    private string product3 = "com.gamebro.varvar.product3";

    
    public void OnPurchaseComplete(Product product)
    {
        if(product.definition.id== product1)
        {
            Debug.Log("5000");
            GetReward(5000);
        }
        
        if(product.definition.id== product2)
        {
            Debug.Log("10000");
            GetReward(10000);
        }

        if (product.definition.id == product3)
        {
            Debug.Log("15000");
            GetReward(15000);
        }
    }

    public void OnPurchaseFailed(Product product,PurchaseFailureReason failureReason)
    {
        Debug.Log(product.definition.id + " failed because " + failureReason);
    }

    void GetReward(int amount)
    {
        FindObjectOfType<MoneyController>().IAPBuyChip(amount);
    }
}
