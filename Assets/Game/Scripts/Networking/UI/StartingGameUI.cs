using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using System;

public class StartingGameUI : NetworkBehaviour
{
    public static StartingGameUI Instance { get; private set; }

    public event Action OnAllPlayersConnected;

    [Header("References")]
    [SerializeField] private TMP_Text _countdownText;

    [Header("Settings")]
    [SerializeField] private float _animationDuration = 0.5f;

    [SerializeField]
    private NetworkVariable<int> _playersLoaded = new NetworkVariable<int>
            (0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
    private WaitForSeconds _waitingSeconds = new WaitForSeconds(1f);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            SetPlayersLoadedRpc();
        }

        if (IsServer)
        {
            OnSinglePlayerConnected();
            _playersLoaded.OnValueChanged += OnPlayersLoadedChanged;
        }
    }

    private void OnPlayersLoadedChanged(int oldPlayerCount, int newPlayerCount)
    {
        if (IsServer && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StartCountdownRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayersLoadedRpc()
    {
        _playersLoaded.Value++;
        Debug.Log("Client Scene Loaded. Total Loaded: " + _playersLoaded.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartCountdownRpc()
    {
        OnAllPlayersConnected?.Invoke();
        StartCoroutine(CountdownCoroutine());
    }

    private void OnSinglePlayerConnected()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            Debug.Log("Single Player Connected");
            WaitingForPlayersUI.Instance.Hide();
            StartCoroutine(CountdownCoroutine());
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        _countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; --i)
        {
            _countdownText.text = i.ToString();
            AnimateText();
            yield return _waitingSeconds;
        }


        _countdownText.text = "GO!";
        AnimateText();
        yield return _waitingSeconds;

        _countdownText.gameObject.SetActive(false);

        GameManager.Instance.ChangeGameState(GameState.Playing);
    }

    private void AnimateText()
    {
        _countdownText.transform.localScale = Vector3.one;
        //_countdownText.transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-30f, 30f));
    }
}
