using UnityEngine;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] playerNameTexts;
    [SerializeField] private TextMeshProUGUI playerCountText;

    private void Awake()
    {
        ResetPlayerNames();
    }

    public void SetPlayerCountText(int value, int maxPlayer)
    {
        playerCountText.text = value + " / " + maxPlayer;
    }
    public void SetMesssage(string messsage)
    {
        playerCountText.text = messsage;
    }

    public void ActivatePlayerName(int ind, string name)
    {
        playerNameTexts[ind].transform.parent.gameObject.SetActive(true);
        playerNameTexts[ind].text = name;
    }

    public void ResetPlayerNames()
    {
        for(int i = 0; i < playerNameTexts.Length; i++)
            playerNameTexts[i].transform.parent.gameObject.SetActive(false);
    }
}
