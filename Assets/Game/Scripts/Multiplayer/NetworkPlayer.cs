using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    [Networked] public NetworkString<_32> nickName { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            nickName = PlayerPrefs.GetString("PlayerName");
        }

        gameControl = FindObjectOfType<GameControl>();
        gameControl.UpdateLobbyPlayerNames(); 
    }

    //[Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.Proxies)]
    //public void RPC_SetPlayerStats(string _nickName)
    //{
    //    Debug.Log("RPC_SetPlayerStats");
    //    nickName = _nickName;
    //}
}
