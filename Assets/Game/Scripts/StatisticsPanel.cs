using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsPanel : MonoBehaviour
{
    public ActorControl[] Actors;
    public Sprite TrueSprite;
    public Sprite FalseSprite;
    public GameControl GameControl;

    private void Awake()
    {
        GameControl = FindObjectOfType<GameControl>();

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
