using UnityEngine;
using TMPro;

public class MoneyController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyCountTmp;

    public int moneyCount;

    private void Awake()
    {
        moneyCount = PlayerPrefs.GetInt("MoneyCount");
        //moneyCount = Mathf.Clamp(moneyCount, 1000, moneyCount);

        SetMoneyCountText();
    }

    public void EarnMoney(int count)
    {
        moneyCount += count;
        PlayerPrefs.SetInt("MoneyCount", moneyCount);
    }

    public void EarningMoney()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(22, 0);
        SetMoneyCountText();
    }

    public bool SpendMoney(int count)
    {
        if (count > moneyCount)
        {
            FindObjectOfType<MakeNoise>().PlaySFX(8, 0);
            return false;
        }

        moneyCount -= count;
        //moneyCount = Mathf.Clamp(moneyCount, 1000, moneyCount);

        PlayerPrefs.SetInt("MoneyCount", moneyCount);

        SetMoneyCountText();
        FindObjectOfType<MakeNoise>().PlaySFX(22, 0);

        return true;
    }

    void SetMoneyCountText()
    {
        moneyCountTmp.text = moneyCount.ToString();
    }

    public void IAPBuyChip(int amount)
    {
        EarnMoney(amount);
        SetMoneyCountText();
    }
}
