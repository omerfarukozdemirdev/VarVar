using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class DrinkController : MonoBehaviour
{

    public DrinkScriptableObject[] drinks;
    [SerializeField] private DrinkButton drinkButtonPrefab;
    [SerializeField] private Transform drinkButtonsRoot;
    [SerializeField] private GameObject drinkPanel;

    [SerializeField] private GameControl gameControl;

    public GameObject drinkButton;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < drinks.Length; i++)
        {
            DrinkButton drinkButton = Instantiate(drinkButtonPrefab, drinkButtonsRoot);
            drinkButton.DrinkID = i;
            drinkButton.DrinkIconImage.sprite = drinks[i].Icon;
            drinkButton.DrinkNameText.text = drinks[i].Name;
            drinkButton.DrinkPriceText.text = drinks[i].Price.ToString();
            drinkButton.GetComponent<Button>().onClick.AddListener(() => DrinkButtonOnClick(drinkButton.DrinkID));

        }
    }

    void DrinkButtonOnClick(int index)
    {
        var drinkContent = gameControl.actorControls[0].drink.transform.GetChild(0);
        drinkPanel.SetActive(false);
        drinkContent.GetComponent<Image>().sprite = drinks[index].Icon;
        iTween.ScaleTo(drinkContent.gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));
        iTween.ScaleTo(drinkContent.gameObject, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce,"delay",12f));
        gameControl.coinController.SpendCoin(drinks[index].Price);
        gameControl.actorControls[0].totalCoins -= drinks[index].Price;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
