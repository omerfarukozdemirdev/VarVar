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
    private WaitForSecondsRealtime _waitingSeconds = new WaitForSecondsRealtime(1f);

    [SerializeField] private float _migrationWaitTime = 10;
    private Coroutine _migrationCoroutine;
    private int _targetMigrationCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playersLoaded.Value = 0;

            if (MigrationBackup.Players != null && MigrationBackup.Players.Count > 0)
            {
                _targetMigrationCount = MigrationBackup.Players.Count-1;
                _migrationCoroutine = StartCoroutine(MigrationWaitTimer());
            }

            _playersLoaded.OnValueChanged += OnPlayersLoadedChanged;
        }

        if (IsClient)
        {
            SetPlayersLoadedRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayersLoadedRpc()
    {
        _playersLoaded.Value++;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartCountdownRpc()
    {
        OnAllPlayersConnected?.Invoke();
        if(GameControl.Instance.IsNetworkMigrationActive)
            GameControl.Instance.HandleMigrationStateChanged(MigrationState.MigrateHandle);
        StartCoroutine(CountdownCoroutine());
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

        if (GameManager.Instance.GetGameState() != GameState.Playing)
            GameManager.Instance.ChangeGameState(GameState.PrepareGame);
        
        if(GameControl.Instance.IsNetworkMigrationActive)
            GameControl.Instance.HandleMigrationStateChanged(MigrationState.MigrateComplete);
    }

    private void AnimateText()
    {
        _countdownText.transform.localScale = Vector3.one;
    }

    private IEnumerator MigrationWaitTimer()
    {
        yield return new WaitForSecondsRealtime(_migrationWaitTime);

        int currentCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (currentCount >= 2)
        {
            StartCountdownRpc();
        }
        else
        {
            HostSingleton.Instance.HostManager.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneNames.Menu);
            Time.timeScale = 1;
        }
    }

    private void OnPlayersLoadedChanged(int oldPlayerCount, int newPlayerCount)
    {
        if (!IsServer) return;

        if (_migrationCoroutine != null && newPlayerCount >= _targetMigrationCount)
        {
            StopCoroutine(_migrationCoroutine);
            _migrationCoroutine = null;

            StartCountdownRpc();
            return;
        }


        if (_migrationCoroutine == null && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            StartCountdownRpc();
        }
    }
}
