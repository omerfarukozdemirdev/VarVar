using System;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public event Action<GameState> OnGameStateChanged;

    [SerializeField] private GameState _currentGameState;

    void Awake()
    {
        Instance = this;
    }

    public void ChangeGameState(GameState newGameState)
    {
        if (IsServer)
        {
            ChangeGameStateRpc(newGameState);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeGameStateRpc(GameState newGameState)
    {
        _currentGameState = newGameState;
        OnGameStateChanged?.Invoke(newGameState);
        Debug.Log($"Game State: {newGameState}");
    }

    public GameState GetGameState()
    {
        return _currentGameState;
    }
}
