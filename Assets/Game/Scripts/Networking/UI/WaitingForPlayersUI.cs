using System;
using UnityEngine;

public class WaitingForPlayersUI : MonoBehaviour
{
    public static WaitingForPlayersUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Show();

        StartingGameUI.Instance.OnAllPlayersConnected += OnAllPlayersConnected;
    }

    private void OnAllPlayersConnected()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
