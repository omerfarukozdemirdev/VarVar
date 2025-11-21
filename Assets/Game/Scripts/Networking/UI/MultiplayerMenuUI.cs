using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenuUI : MonoBehaviour
{
    public static MultiplayerMenuUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private LobbiesListUI _lobbiesListUI;
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _lobbiesButton;
    [SerializeField] private TMP_InputField _joinCodeInputField;
    [SerializeField] private GameObject _lobbiesParentGameObject;
    [SerializeField] private RectTransform _lobbiesBackgroundTransform;
    [SerializeField] private TMP_Text _welcomeText;
    [SerializeField] private TMP_Text _warningText;

    [Header("Settings")]
    [SerializeField] private float _animationDuration = 1f;
    [SerializeField] private float _textAnimationDuration;
    [SerializeField] private float _fadeDuration;

    private RectTransform _warningTransform;
    private bool _isAnimating;

    private void Awake()
    {
        Instance = this;

        _warningTransform = _warningText.GetComponent<RectTransform>();

        _hostButton.onClick.AddListener(StartHost);
        _clientButton.onClick.AddListener(StartClient);
        _lobbiesButton.onClick.AddListener(OpenLobbies);
    }

    private void Start()
    {
        _warningText.text = string.Empty;
        _warningText.alpha = 0f;
        _warningTransform.gameObject.SetActive(false);

        _lobbiesListUI.RefreshList();
    }

    private void OnEnable()
    {
        var playerName = PlayerPrefs.GetString(Constants.PlayerData.PlayerNameKey, string.Empty);
        _welcomeText.text = $"welcome, <color=yellow>{playerName}</color>";
    }

    private async void StartHost()
    {
        _hostButton.interactable = false;
        await HostSingleton.Instance.HostManager.StartHostAsync();
    }

    private async void StartClient()
    {
        if(_joinCodeInputField.text == string.Empty || _joinCodeInputField.text.Contains(" "))
        {
            AnimateWarningText("Enter a valid Join Code!");
            return;
        }

        await ClientSingleton.Instance.ClientManager.StartClientAsync(_joinCodeInputField.text);
    }

    private void OpenLobbies()
    {
        _lobbiesParentGameObject.SetActive(true);

        _lobbiesListUI.RefreshList();
    }

    public async void AnimateWarningText(string message)
    {
        if(_isAnimating) { return; }

        _warningText.text = message;

        _isAnimating = true;
        _warningTransform.gameObject.SetActive(true);
        await UniTask.Delay(2000);
        _warningTransform.gameObject.SetActive(false);
        _isAnimating = false;
    }
}
