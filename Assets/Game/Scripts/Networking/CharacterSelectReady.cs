using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    public event Action OnReadyChanged;
    public event Action OnUnreadyChanged;
    public event Action OnAllPlayersReady;

    private readonly NetworkList<ulong> _readyClientIds = new NetworkList<ulong>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallbackServer;
        }

        _readyClientIds.OnListChanged += HandleReadyListChanged;
    }

    public override void OnNetworkDespawn()
    {
        _readyClientIds.OnListChanged -= HandleReadyListChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallbackServer;
        }
    }

    private void HandleReadyListChanged(NetworkListEvent<ulong> changeEvent)
    {
        OnReadyChanged?.Invoke();

        if ((int)changeEvent.Type == 2)
        {
            OnUnreadyChanged?.Invoke();
        }

        if (IsServer && AreAllPlayersReady())
        {
            OnAllPlayersReadyToAllRpc();
        }
    }

    private void OnClientDisconnectCallbackServer(ulong clientId)
    {
        if (_readyClientIds.Contains(clientId))
        {
            _readyClientIds.Remove(clientId);
        }
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyRpc();
    }

    public void SetPlayerUnready()
    {
        SetPlayerUnreadyRpc();
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerReadyRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!_readyClientIds.Contains(clientId))
        {
            _readyClientIds.Add(clientId);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerUnreadyRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (_readyClientIds.Contains(clientId))
        {
            _readyClientIds.Remove(clientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnAllPlayersReadyToAllRpc()
    {
        OnAllPlayersReady?.Invoke();
    }

    public bool AreAllPlayersReady()
    {
        if (!IsServer)
            return false;

        return _readyClientIds.Count == NetworkManager.Singleton.ConnectedClientsIds.Count;
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return _readyClientIds.Contains(clientId);
    }
}