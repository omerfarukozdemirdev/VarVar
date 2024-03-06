using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameConfig gameConfig;

    [SerializeField] private TMP_InputField playerNameTmp;
    [SerializeField] private Image Avatar;
    [SerializeField] private SpriteRenderer[] cards;
    [SerializeField] private MeshRenderer[] cardBacks;
    [SerializeField] private TMP_InputField firstPlayerNameTmp;
    [SerializeField] private GameObject firstPlayerNameCanvas;
    [SerializeField] private TMP_InputField playTimeTmp;
    [SerializeField] private TMP_InputField betTimeTmp;

    private void Awake()
    {
        string pName = "Guest";

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            pName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            firstPlayerNameCanvas.SetActive(true);
        }

        playerNameTmp.text = pName;

        gameConfig.avatarInd = 0;//PlayerPrefs.GetInt("Avatar");
        SetAvatar();

        InitTimers();

        FindObjectOfType<MakeNoise>().PlaySFX(29, 0);
    }

    private void Start()
    {
        GameManager.Instance.gameStat = GameManager.GameStat.menu;
    }

    public void PlayGame()
    {
        GameManager.Instance.gameMode = GameManager.GameMode.Single;

        GoToPlayGame();
    }

    public void PlayMultiplayerGame()
    {
        GameManager.Instance.gameMode = GameManager.GameMode.Multiplayer;

        GoToPlayGame();
    }

    void GoToPlayGame()
    {
        gameConfig.cardDealerInd = -1;

        FindObjectOfType<MakeNoise>().PlaySFX(30, 0);
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    public void SetPlayerName()
    {
        PlayerPrefs.SetString("PlayerName", playerNameTmp.text);
    }

    public void SetFirstPlayerName()
    {
        PlayerPrefs.SetString("PlayerName", firstPlayerNameTmp.text);
        firstPlayerNameCanvas.SetActive(false);
        playerNameTmp.text = PlayerPrefs.GetString("PlayerName");
    }

    public void SetAvatar()
    {
        Avatar.sprite = gameConfig.avatars[gameConfig.avatarInd];
    }

    void InitTimers()
    {
        if (!PlayerPrefs.HasKey("BetTime"))
        {
            PlayerPrefs.SetInt("BetTime", 12);
        }

        betTimeTmp.text = PlayerPrefs.GetInt("BetTime").ToString();
        
        if (!PlayerPrefs.HasKey("PlayTime"))
        {
            PlayerPrefs.SetInt("PlayTime", 15);
        }

        playTimeTmp.text = PlayerPrefs.GetInt("PlayTime").ToString();
    }

    public void SetPlayTime()
    {
        PlayerPrefs.SetInt("PlayTime", int.Parse(playTimeTmp.text));
    }

    public void SetBetTime()
    {
        PlayerPrefs.SetInt("BetTime", int.Parse(betTimeTmp.text));
    }
}
