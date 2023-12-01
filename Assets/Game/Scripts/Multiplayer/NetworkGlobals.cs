using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkGlobals : NetworkBehaviour
{
    [UnitySerializeField]
    [Networked]
    [Capacity(7)]
    public NetworkLinkedList<NetworkPlayer> orderedNetworkPlayers => default;

    private GameControl gameControl;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();
        gameControl.networkGlobals = this;

        // Oturma düzenini belirle
        if (gameControl.networkHandler.isHost)
        {
            NetworkPlayer[] networkPlayers = FindObjectsOfType<NetworkPlayer>();

            for (int i = 0; i < networkPlayers.Length; i++)
            {
                if (networkPlayers[i].localPlayer)
                    orderedNetworkPlayers.Add(networkPlayers[i]);
            }
            for (int i = 0; i < networkPlayers.Length; i++)
            {
                if (!networkPlayers[i].localPlayer)
                    orderedNetworkPlayers.Add(networkPlayers[i]);
            }

            RPC_StartMultiplayerGame();
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_StartMultiplayerGame()
    {
        gameControl.StartMultiplayerGame();
    }
}
