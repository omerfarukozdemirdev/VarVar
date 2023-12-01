using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    [Networked] public NetworkString<_32> nickName { get; set; }

    public int playInd;
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

        if(gameControl.networkHandler.maxPlayer == gameControl.GetNetworkPlayerCount() && gameControl.networkHandler.isHost)
        {
            gameControl.networkHandler.SpawnNetworkGlobals();
        }
    }

    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.InputAuthority)]
    public void RPC_PlayInd(int ind)
    {
        playInd = ind;
        gameControl.playerControl.actorControl = gameControl.actorControls[playInd];
        gameControl.playerControl.actorControl.player = true;
    }

    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.InputAuthority)]
    public void RPC_CardsInHand(NetworkCard _card)
    {
        gameControl.actorControls[playInd].cardsInHand.Add(NetworkCardConverter.NetworkCardToCard(_card));   
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void RPC_DecidePlayer(bool pass, bool betUp, int ind)
    {
        gameControl.actorControls[ind].pass = pass;
        gameControl.actorControls[ind].betUp = betUp;

        gameControl.DecidePlayer();
    }
}
