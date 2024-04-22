using System.Collections;
using System.Collections.Generic;
using TMPro;
using UltimateSlider;
using UnityEngine;

public class SelectSoloModePanel : MonoBehaviour
{
    private GameControl gameControl;
    private MoneyController moneyController;

    [SerializeField] private SliderManager sliderManager;
    [SerializeField] GameObject playButton;
    [SerializeField] TextMeshProUGUI totalChipText;

    // Start is called before the first frame update
    void Start()
    {
        gameControl = FindObjectOfType<GameControl>();
        moneyController = FindObjectOfType<MoneyController>();
        

        totalChipText.text=moneyController.moneyCount.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnStartSliding()
    {
        for (int i = 0; i < sliderManager.objectsofSlides.Count; i++)
        {
            sliderManager.objectsofSlides[i].GetComponent<CanvasGroup>().alpha = 0.3f;

        }

        sliderManager.objectsofSlides[sliderManager.currentSliderNumber].GetComponent<CanvasGroup>().alpha = 1.0f;

        gameControl.playerCount=4+sliderManager.currentSliderNumber;

        if (moneyController.moneyCount >= 1000 * (4 + sliderManager.currentSliderNumber))
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }
    }

    public void OnUpdateSliding()
    {

    }

    public void OnEndSliding()
    {

    }

    public void PlayGame()
    {
        gameControl.SoloStartGame();
    }

}
