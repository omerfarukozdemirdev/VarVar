using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameStat
    {
        menu,
        lobby,
        game,
        handCompleted
    }

    public enum GameMode
    {
        Single,
        Multiplayer
    }

    public GameMode gameMode;
    public GameStat gameStat;

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
