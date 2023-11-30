using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private GameControl gameControl;

    public override void Spawned()
    {
        gameControl = FindObjectOfType<GameControl>();
    }



}
