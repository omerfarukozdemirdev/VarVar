using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    [Networked] public NetworkString<_32> nickName { get; set; }

    [UnitySerializeField]
    [Networked]
    [Capacity(7)]
    public NetworkLinkedList<NetworkPlayer> orderedNetworkPlayers => default;

    public bool localPlayer;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();

        localPlayer = Object.HasInputAuthority;

        if (localPlayer)
        {
            nickName = PlayerPrefs.GetString("PlayerName");
            gameControl.myNetworkPlayer = this;
        }

        gameControl.UpdateLobbyPlayerNames();

        if(gameControl.networkHandler.maxPlayer == gameControl.GetNetworkPlayerCount())
        {
            gameControl.StartMultiplayerGame();
        }
    }

    //[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    //public void RPC_SetPlayerStats(List<NetworkPlayer> _nickName)
    //{
    //    Debug.Log("RPC_SetPlayerStats");
    //}
}
