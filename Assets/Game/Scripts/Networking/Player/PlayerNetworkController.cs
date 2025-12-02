using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNetworkController : NetworkBehaviour
{
    public static event Action<PlayerNetworkController> OnPlayerSpawned;
    public static event Action<PlayerNetworkController> OnPlayerDespawned;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<byte> PlayerAvatarIndex = new NetworkVariable<byte>();

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            UserData userData 
                = HostSingleton.Instance.HostManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            
            PlayerName.Value = userData.UserName;
            PlayerAvatarIndex.Value = userData.UserAvatarIndex;
            OnPlayerSpawned?.Invoke(this);
        }
    }



    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
