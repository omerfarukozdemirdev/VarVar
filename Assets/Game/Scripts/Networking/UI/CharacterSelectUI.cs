using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public static CharacterSelectUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _copyButton;
    [SerializeField] private TMP_Text _readyText;
    [SerializeField] private TMP_Text _joinCodeText;
    [SerializeField] private TMP_Text _roomPlayersCountText;
    [SerializeField] private TMP_Text _roomPlayersStateText;
    [SerializeField] private Image _copiedImage;

    [Header("Settings")]
    [SerializeField] private Sprite _tickSprite;
    [SerializeField] private Sprite _crossSprite;
    [SerializeField] private Sprite _greenButtonSprite;
    [SerializeField] private Sprite _redButtonSprite;

    private bool _isPlayerReady;

    private void Awake()
    {
        Instance = this;

        _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        _readyButton.onClick.AddListener(OnReadyButtonClicked);
        _startButton.onClick.AddListener(OnStartButtonClicked);
        _copyButton.onClick.AddListener(OnCopyButtonClicked);
    }

    private void Start()
    {
        _startButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

        _copiedImage.sprite = _crossSprite;

        CharacterSelectReady.Instance.OnAllPlayersReady += CharacterSelectReady_OnAllPlayersReady;
        CharacterSelectReady.Instance.OnUnreadyChanged += CharacterSelectReady_OnUnreadyChanged;
        MultiplayerGameManager.Instance.OnPlayerDataNetworkListChanged += MultiplayerGameManager_OnPlayerDataNetworkListChanged;

        SetJoinCodeInfo();
        SetRoomPlayerCountInfo();
    }

    private void MultiplayerGameManager_OnPlayerDataNetworkListChanged()
    {
        SetRoomPlayerCountInfo();

        if (NetworkManager.Singleton.ConnectedClientsList.Count == MultiplayerGameManager.Instance.GetLobby().MaxPlayers)
        {
            if (CharacterSelectReady.Instance.AreAllPlayersReady())
                _roomPlayersStateText.text = "Oda sahibinin oyunu baþlatmasý bekleniyor...";
            else
                _roomPlayersStateText.text = "Tüm oyuncularýn hazýr olmasý bekleniyor...";
        }
        else
        {
            _roomPlayersStateText.text = "Oyuncular bekleniyor...";
        }
    }

    private void SetJoinCodeInfo()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _joinCodeText.text = HostSingleton.Instance.HostManager.GetLobby().LobbyCode;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            _joinCodeText.text = ClientSingleton.Instance.ClientManager.GetLobby().LobbyCode;
        }
    }

    private void SetRoomPlayerCountInfo()
    {
        int currentConnectedPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        _roomPlayersCountText.text = currentConnectedPlayers.ToString() + "/" + MultiplayerGameManager.Instance.GetLobby().MaxPlayers.ToString();
    }

    private void CharacterSelectReady_OnUnreadyChanged()
    {
        if (NetworkManager.Singleton.IsHost)
            SetStartButtonInteractable(false);

        if (NetworkManager.Singleton.ConnectedClientsList.Count == MultiplayerGameManager.Instance.GetLobby().MaxPlayers)
        {
            _roomPlayersStateText.text = "Tüm oyuncularýn hazýr olmasý bekleniyor...";
        }
        else
        {
            _roomPlayersStateText.text = "Oyuncular bekleniyor...";
        }
    }

    private void CharacterSelectReady_OnAllPlayersReady()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == MultiplayerGameManager.Instance.GetLobby().MaxPlayers)
        {
            _roomPlayersStateText.text = "Oda sahibinin oyunu baþlatmasý bekleniyor...";
            if (NetworkManager.Singleton.IsHost)
                SetStartButtonInteractable(true);
        }
        else
        {
            _roomPlayersStateText.text = "Oyuncular bekleniyor...";
            if (NetworkManager.Singleton.IsHost)
                SetStartButtonInteractable(false);
        }
    }

    private async void OnStartButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            try
            {
                await LobbyService.Instance.UpdateLobbyAsync(HostSingleton.Instance.HostManager.GetLobby().Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            "GameStarted", new DataObject(
                                visibility: DataObject.VisibilityOptions.Public,
                                value: "true")
                        }
                    }
                });

                await LobbyService.Instance.DeleteLobbyAsync(HostSingleton.Instance.HostManager.GetLobby().Id);
            }
            catch (LobbyServiceException lobbyServiceException)
            {
                Debug.LogError($"Failed to update or delete lobby: {lobbyServiceException}");
            }

            NetworkManager.Singleton.SceneManager.LoadScene(Constants.SceneNames.Game, LoadSceneMode.Single);
        }
    }

    private void OnCopyButtonClicked()
    {
        _copiedImage.sprite = _tickSprite;
        GUIUtility.systemCopyBuffer = _joinCodeText.text;
    }

    private void SetStartButtonInteractable(bool isActive)
    {
        if(_startButton != null)
        {
            _startButton.interactable = isActive;
        }
    }

    private void OnReadyButtonClicked()
    {
        _isPlayerReady = !_isPlayerReady;

        if (_isPlayerReady)
        {
            SetPlayerReady();
        }
        else
        {
            SetPlayerUnready();
        }
    }

    private void SetPlayerReady()
    {
        CharacterSelectReady.Instance.SetPlayerReady();
        _readyText.text = "Ready";
        _readyButton.image.sprite = _greenButtonSprite;
    }

    private void SetPlayerUnready()
    {
        CharacterSelectReady.Instance.SetPlayerUnready();
        _readyText.text = "Not Ready";
        _readyButton.image.sprite = _redButtonSprite;
    }

    private void OnMainMenuButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            HostSingleton.Instance.HostManager.Shutdown();
        }

        ClientSingleton.Instance.ClientManager.Disconnect();
    }

    public bool IsPlayerReady()
    {
        return _isPlayerReady;
    }
}
