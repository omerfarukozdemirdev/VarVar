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
        //if(GameManager.Instance.GetGameState() == GameState.Playing)
        //    return;

        if (IsServer)
        {
            _playersLoaded.Value = 0; // Sayacý sýfýrla

            // Migration Kontrolü: Eðer yedek listede oyuncu varsa, Migration modundayýzdýr.
            if (MigrationBackup.Players != null && MigrationBackup.Players.Count > 0)
            {
                _targetMigrationCount = MigrationBackup.Players.Count-1;
                Debug.Log($"MIGRATION: {_targetMigrationCount} oyuncu bekleniyor. Süre baþladý.");

                // Geri sayýmý baþlat
                _migrationCoroutine = StartCoroutine(MigrationWaitTimer());
            }
            //else
            //{
            //    // Normal oyun baþlangýcý (Tek kiþilik test vs için)
            //    OnSinglePlayerConnected();
            //}

            _playersLoaded.OnValueChanged += OnPlayersLoadedChanged;
        }

        if (IsClient)
        {
            SetPlayersLoadedRpc();
        }
    }

    //private void OnPlayersLoadedChanged(int oldPlayerCount, int newPlayerCount)
    //{
    //    if (IsServer && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
    //    {
    //        Debug.Log(oldPlayerCount + "   " + newPlayerCount);
    //        StartCountdownRpc();
    //    }
    //}

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
        GameControl.Instance.HandleMigrationStateChanged(MigrationState.MigrateHandle);
        StartCoroutine(CountdownCoroutine());
    }

    //private void OnSinglePlayerConnected()
    //{
    //    if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
    //    {
    //        Debug.Log("Single Player Connected");
    //        WaitingForPlayersUI.Instance.Hide();
    //        StartCoroutine(CountdownCoroutine());
    //    }
    //}

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
        

        GameControl.Instance.HandleMigrationStateChanged(MigrationState.MigrateComplete);
    }

    private void AnimateText()
    {
        _countdownText.transform.localScale = Vector3.one;
        //_countdownText.transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-30f, 30f));
    }

    // A. Bekleme Mantýðý (Coroutine)
    private IEnumerator MigrationWaitTimer()
    {
        // Belirlenen süre kadar bekle (örn: 20 sn)
        yield return new WaitForSecondsRealtime(_migrationWaitTime);

        Debug.Log("MIGRATION: Süre doldu! Mevcut durum kontrol ediliyor...");

        // Süre bittiðinde içeride kaç kiþi var?
        int currentCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (currentCount >= 2)
        {
            // Yeterli oyuncu var (en az 2), oyunu baþlat!
            Debug.Log("MIGRATION: Yeterli çoðunluk saðlandý. Oyun Baþlýyor!");
            StartCountdownRpc();
        }
        else
        {
            // Yeterli oyuncu yok, oyunu iptal et ve menüye dön.
            Debug.Log("MIGRATION: Yeterli oyuncu baðlanamadý. Oyun Ýptal.");
            HostSingleton.Instance.HostManager.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene(Constants.SceneNames.Menu);
            Time.timeScale = 1;
        }
    }

    // B. Erken Baþlatma Kontrolü (OnPlayersLoadedChanged içine minik bir ekleme)
    // Mevcut OnPlayersLoadedChanged fonksiyonunu bununla DEÐÝÞTÝR veya güncelle:
    private void OnPlayersLoadedChanged(int oldPlayerCount, int newPlayerCount)
    {
        if (!IsServer) return;

        // EÐER MIGRATION MODUNDAYSAK VE HERKES GELDÝYSE
        if (_migrationCoroutine != null && newPlayerCount >= _targetMigrationCount)
        {
            Debug.Log("MIGRATION: Herkes eksiksiz geldi! Süreyi beklemeden baþlatýlýyor.");
            StopCoroutine(_migrationCoroutine); // Sayacý durdur
            _migrationCoroutine = null;

            //GameManager.Instance.ChangeGameState(GameState.MigrateComplete);


            StartCountdownRpc(); // Hemen baþlat
            return;
        }

        // Normal oyun akýþý (Lobby fulllendiðinde baþlatma mantýðý buradaydý)
        // Eðer migration deðilse eski mantýðýn burada çalýþmaya devam eder:
        if (_migrationCoroutine == null && newPlayerCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            // Burasý normal akýþta lobby dolunca çalýþýyordu, 
            // Migration varken burasý çakýþmasýn diye _migrationCoroutine null kontrolü ekledik.
            // (Ancak senin normal oyun baþlatma mantýðýn StartingGameUI'da tam olarak ConnectedClientsList.Count'a baðlýydý, onu koruduk)
            StartCountdownRpc();
        }
    }
}
