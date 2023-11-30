using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameMode
    {
        Single,
        Multiplayer
    }

    public GameMode gameMode;
    //public GameMode CurrentGameMode;

    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public bool IsMultiplayer()
    {
        return gameMode == GameMode.Multiplayer;
    }

}
