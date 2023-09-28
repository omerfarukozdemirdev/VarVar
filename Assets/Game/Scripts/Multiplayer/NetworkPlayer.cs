using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using static UnityEngine.CullingGroup;
using System.Linq;

public class NetworkPlayer : NetworkBehaviour
{
    public static readonly List<NetworkPlayer> Players = new List<NetworkPlayer>();

    public static Action<NetworkPlayer> PlayerJoined;
    public static Action<NetworkPlayer> PlayerLeft;
    public static Action<NetworkPlayer> PlayerChanged;
    public static NetworkPlayer Local;

    [Networked(OnChanged = nameof(OnStateChanged))] public NetworkBool IsReady { get; set; }
    [Networked(OnChanged = nameof(OnStateChanged))] public NetworkString<_32> Username { get; set; }

    public bool IsLeader => Object != null && Object.IsValid && Object.HasStateAuthority;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        base.Spawned();

        if (Object.HasInputAuthority)
        {
            Local = this;

            PlayerChanged?.Invoke(this);
            RPC_SetPlayerStats("Player_" + (Players.Count+1));

        }

        Players.Add(this);
        
        PlayerJoined?.Invoke(this);


    }

    private void OnDisable()
    {
        // OnDestroy does not get called for pooled objects
        PlayerLeft?.Invoke(this);
        Players.Remove(this);
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority, InvokeResim = true)]
    private void RPC_SetPlayerStats(NetworkString<_32> username)
    {
        Username = username;
    }

    public static void RemovePlayer(NetworkRunner runner, PlayerRef p)
    {
        var networkPlayer = Players.FirstOrDefault(x => x.Object.InputAuthority == p);
        if (networkPlayer != null)
        {
            Players.Remove(networkPlayer);
            runner.Despawn(networkPlayer.Object);
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_ChangeReadyState(NetworkBool state)
    {
        Debug.Log($"Setting {Object.Name} ready state to {state}");
        IsReady = state;
    }

    private static void OnStateChanged(Changed<NetworkPlayer> changed) => PlayerChanged?.Invoke(changed.Behaviour);

}
