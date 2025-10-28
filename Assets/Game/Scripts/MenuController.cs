using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameConfig gameConfig;

    [SerializeField] private MakeNoise audioManager;
    [SerializeField] private TMP_InputField playerNameTmp;
    [SerializeField] private Image Avatar;
    [SerializeField] private SpriteRenderer[] cards;
    [SerializeField] private MeshRenderer[] cardBacks;
    [SerializeField] private TMP_InputField firstPlayerNameTmp;
    [SerializeField] private GameObject firstPlayerNameCanvas;
    [SerializeField] private TMP_InputField playTimeTmp;
    [SerializeField] private TMP_InputField betTimeTmp;
    [SerializeField] private UISwitcher.UISwitcher soundSwitcher;
    [SerializeField] private UISwitcher.UISwitcher musicSwitcher;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject lowMoneyMessage;
    public GameObject noChipPopup;

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

        audioManager = FindObjectOfType<MakeNoise>();

        InitSettings();

        audioManager.PlaySFX(29, 0);
    }

    private void Start()
    {
        GameManager.Instance.gameStat = GameManager.GameStat.menu;

        //if (FindObjectOfType<MoneyController>().moneyCount<4000)
        //{
        //    playButton.interactable = false;
        //    lowMoneyMessage.SetActive(true);
        //}
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
            PlayerPrefs.SetInt("PlayTime", 20);
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

    void InitSettings()
    {
        if (!PlayerPrefs.HasKey("Sound"))
        {
            PlayerPrefs.SetInt("Sound", 1);
        }

        soundSwitcher.isOn=(PlayerPrefs.GetInt("Sound") ==1);

        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetFloat("Music", 0.1f);
        }

        musicSwitcher.isOn = (PlayerPrefs.GetFloat("Music") == 0.1f);
    }

    public void SetSoundSwitch()
    {
        PlayerPrefs.SetInt("Sound", soundSwitcher.isOn ? 1 : 0);
        audioManager.SoundOnOff(soundSwitcher.isOn);

    }
    public void SetMusicSwitch()
    {
        PlayerPrefs.SetFloat("Music", musicSwitcher.isOn ? 0.1f : 0);
        audioManager.MusicOnOff(musicSwitcher.isOn);
    }
}
