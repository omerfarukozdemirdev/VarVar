using UnityEngine;
using Unity.Netcode;

public class GameModeChecker : MonoBehaviour
{
    public static GameModeChecker Instance { get; private set; }

    public bool IsMultiplayerActive
    {
        get
        {
            if (NetworkManager.Singleton == null)
            {
                return false;
            }

            return NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient;
        }
    }

    public bool IsSinglePlayerActive => !IsMultiplayerActive;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}