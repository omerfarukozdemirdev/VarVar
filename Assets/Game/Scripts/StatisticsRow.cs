using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsRow : MonoBehaviour
{
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text winNumberText;
    [SerializeField] private Text passNumberText;
    [SerializeField] private Text betMoneyText;
    [SerializeField] private Text winMoneyText;
    [SerializeField] private ActorControl actor;
    [SerializeField] private StatisticsPanel statisticsPanel;

    void Awake()
    {
        statisticsPanel = FindObjectOfType<StatisticsPanel>();
        actor = statisticsPanel.Actors[transform.GetSiblingIndex() - 1];
    }

    private void OnEnable()
    {
        playerNameText.text = actor.actorName;
        winNumberText.text = actor.winCounter.ToString();
        passNumberText.text = actor.passCounter.ToString();
        betMoneyText.text = actor.totalBetMoney.ToString();
        winMoneyText.text = actor.totalWinMoney.ToString();
        //
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
