using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    private string chip5000 = "com.company.varvar.chip5000";
    private string chip10000 = "com.company.varvar.chip10000";
    private string chip15000 = "com.company.varvar.chip15000";

    
    public void OnPurchaseComplete(Product product)
    {
        if(product.definition.id==chip5000)
        {
            Debug.Log("5000");
            GetReward(5000);
        }
        
        if(product.definition.id==chip10000)
        {
            Debug.Log("10000");
            GetReward(10000);
        }

        if (product.definition.id == chip15000)
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
