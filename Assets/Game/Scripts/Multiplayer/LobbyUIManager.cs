using TMPro;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public TextMeshProUGUI roomMaxPlayerText;
    [SerializeField] private TextMeshProUGUI[] playerNameTexts;
    [SerializeField] private TextMeshProUGUI playerCountText;
    public GameObject playersWaitingText;

    private void Awake()
    {
        playerCountText.text = "Test için düzenlendi. Kaç kişilik oynamak istiyorsanız o butona basın. Tüm arkadaşların aynı butona basmalı";

        roomMaxPlayerText.gameObject.SetActive(false);
        playersWaitingText.SetActive(false);
        roomMaxPlayerText.text = FindObjectOfType<NetworkHandler>().maxPlayer + " OYUNCU";

        ResetPlayerNames();
    }

    public void SetRoomMaxPlayer(int value)
    {
        FindObjectOfType<NetworkHandler>().maxPlayer = value;
        roomMaxPlayerText.text = value + " OYUNCU";
        FindObjectOfType<NetworkHandler>().StartQuickGame();
    }

    public void SetPlayerCountText(int value, int maxPlayer)
    {
        if (!playersWaitingText.activeSelf)
            playersWaitingText.SetActive(true);

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
        for (int i = 0; i < playerNameTexts.Length; i++)
            playerNameTexts[i].transform.parent.gameObject.SetActive(false);
    }
}
