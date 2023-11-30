using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    public static readonly List<NetworkPlayer> players = new List<NetworkPlayer>();
    public string nickName;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();

        if (Object.HasInputAuthority)
        {
            nickName = PlayerPrefs.GetString("PlayerName");
            RPC_SetPlayerStats(nickName);

        }

        gameControl.lobbyUIManager.SetPlayerCountText(players.Count, gameControl.networkHandler.maxPlayer);

        gameControl.lobbyUIManager.ResetPlayerNames();
        for (int i = 0; i < players.Count; i++)
            gameControl.lobbyUIManager.ActivatePlayerName(i, players[i].nickName);
    }


    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.Proxies)]
    public void RPC_SetPlayerStats(string _nickName)
    {
        Debug.Log("RPC_SetPlayerStats");
        nickName = _nickName;
    }
}
