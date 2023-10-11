using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIManager : NetworkBehaviour
{

    public static NetworkUIManager Instance { get; private set; }
    public NetworkGameManager NetworkGameManager;

    public LobbyUI LobbyUI;

    public List<int> NewPlayerIndexList = new List<int>();


    private void Awake()
    {

        Instance = this;

    }

    public void Setup()
    {

        NetworkGameManager = NetworkGameManager.Instance;
        LobbyUI = FindObjectOfType<LobbyUI>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CloseLobbyPanel()
    {
        LobbyUI.gameObject.SetActive(false);
    }

    public void SetTable()
    {
        //oyuncuları masaya doğru sıraya göre oturtma
        if (NetworkPlayer.Local)
        {

            for (int i = NetworkPlayer.Players.IndexOf(NetworkPlayer.Local); i < NetworkPlayer.Players.Count; i++)
            {
                NewPlayerIndexList.Add(i);
            }

            for (int i = 0; i < NetworkPlayer.Players.IndexOf(NetworkPlayer.Local); i++)
            {
                NewPlayerIndexList.Add(i);

            }

            for (int i = 0; i < NetworkPlayer.Players.Count; i++)
            {
                //NetworkGameManager.GameControl.actorControls[i].SetNameText( NetworkPlayer.Players[NewPlayerIndexList[i]].Username.ToString());
                NetworkGameManager.GameControl.actorLocations[i].transform.position = NetworkGameManager.GameControl.actorPositions[NewPlayerIndexList[i]];
                NetworkGameManager.GameControl.actorLocations[i].transform.rotation = Quaternion.Euler(NetworkGameManager.GameControl.actorRotations[NewPlayerIndexList[i]]);
                NetworkGameManager.GameControl.actorControls[i].SetNameText(NetworkPlayer.Players[i].Username.ToString());

            }
        }
    }
}
