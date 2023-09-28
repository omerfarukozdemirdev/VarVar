using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    public NetworkUIManager NetworkUIManager;
    public GameControl GameControl;


    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    public override void Spawned()
    {
        base.Spawned();

        NetworkUIManager=NetworkUIManager.Instance;
        GameControl = FindObjectOfType<GameControl>();

        NetworkUIManager.Setup();
        
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
