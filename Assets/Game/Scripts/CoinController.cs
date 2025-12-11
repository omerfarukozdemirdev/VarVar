using UnityEngine;
using TMPro;

public class CoinController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinCountTmp;
    [SerializeField] private GameObject coinAttraction;
    private int coinCount;

    private void Awake()
    {
        coinCount = PlayerPrefs.GetInt("CoinCount");
        SetCoinCountText();
    }

    public void EarnCoin(int count)
    {
        coinCount += count;
        PlayerPrefs.SetInt("CoinCount", coinCount);

        //coinAttraction.SetActive(false);
        //coinAttraction.SetActive(true);
        SetCoinCountText();

    }

    public void EarningCoin()
    {
        FindObjectOfType<MakeNoise>().PlaySFX(22, 0);
        SetCoinCountText();
    }

    public bool SpendCoin(int count)
    {
        if (count > coinCount)
        {
            FindObjectOfType<MakeNoise>().PlaySFX(8, 0);
            return false;
        }

        coinCount -= count;
        PlayerPrefs.SetInt("CoinCount", coinCount);

        SetCoinCountText();
        FindObjectOfType<MakeNoise>().PlaySFX(22, 0);

        return true;
    }

    public void SetCoinCountText()
    {
        coinCountTmp.text = coinCount.ToString();
    }
}
