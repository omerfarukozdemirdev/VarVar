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

        if (PlayerPrefs.HasKey(Constants.PlayerData.PlayerNameKey))
        {
            pName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey);
        }
        else
        {
            firstPlayerNameCanvas.SetActive(true);
        }

        playerNameTmp.text = pName;

        var avatarInd = PlayerPrefs.GetInt(Constants.PlayerData.PlayerAvatarKey,0);
        SetAvatar(avatarInd);

        InitTimers();

        audioManager = FindObjectOfType<MakeNoise>();

        InitSettings();

        audioManager.PlaySFX(29, 0);
    }

    private void Start()
    {
        //if (FindObjectOfType<MoneyController>().moneyCount<4000)
        //{
        //    playButton.interactable = false;
        //    lowMoneyMessage.SetActive(true);
        //}
    }

    public void PlayGame()
    {
        GoToPlayGame();
    }

    public void PlayMultiplayerGame()
    {
    }

    void GoToPlayGame()
    {
        gameConfig.cardDealerInd = -1;

        FindObjectOfType<MakeNoise>().PlaySFX(30, 0);
        UnityEngine.SceneManagement.SceneManager.LoadScene(4);
    }

    public void SetPlayerName()
    {
        PlayerPrefs.SetString(Constants.PlayerData.PlayerNameKey, playerNameTmp.text);

    }

    public void SetFirstPlayerName()
    {
        PlayerPrefs.SetString(Constants.PlayerData.PlayerNameKey, firstPlayerNameTmp.text);
        firstPlayerNameCanvas.SetActive(false);
        playerNameTmp.text = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey);
    }

    public void SetAvatar(int avatarIndex)
    {
        Avatar.sprite = gameConfig.avatars[avatarIndex];
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
