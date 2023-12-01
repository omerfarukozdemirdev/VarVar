using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    [Networked] public NetworkString<_32> nickName { get; set; }

    //[UnitySerializeField][Networked][Capacity(11)]
    //public NetworkLinkedList<NetworkCard> cardsInHand => default;

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
    }

    [Rpc(sources: RpcSources.Proxies, targets: RpcTargets.InputAuthority)]
    public void RPC_CardsInHand(NetworkCard _card)
    {
        gameControl.actorControls[playInd].cardsInHand.Add(NetworkCardConverter.NetworkCardToCard(_card));   
    }
}
