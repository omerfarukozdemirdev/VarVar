using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsRow : MonoBehaviour
{
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text gameNumberText;
    [SerializeField] private Image passImage;
    [SerializeField] private Image winImage;
    [SerializeField] private Text gameMoneyText;
    [SerializeField] private Text totalMoneyText;
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
        passImage.sprite = actor.pass ? statisticsPanel.TrueSprite : statisticsPanel.FalseSprite;
        winImage.sprite = actor.handCompleted ? statisticsPanel.TrueSprite : statisticsPanel.FalseSprite;
        gameMoneyText.text = actor.handCompleted ? statisticsPanel.GameControl.rewardMoney.ToString() : "0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
