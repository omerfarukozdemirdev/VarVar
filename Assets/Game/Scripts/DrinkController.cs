using UnityEngine;
using UnityEngine.UI;

public class DrinkController : MonoBehaviour
{
    public DrinkScriptableObject[] drinks;
    [SerializeField] private DrinkButton drinkButtonPrefab;
    [SerializeField] private Transform drinkButtonsRoot;
    [SerializeField] private GameObject drinkPanel;

    [SerializeField] private GameControl gameControl;
    [SerializeField] private GameObject errorDrinkPanel;
    public GameObject drinkButton;

    private MakeNoise makeNoise;

    void Awake()
    {
        makeNoise = FindObjectOfType<MakeNoise>();

    }

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
        if (gameControl.actorControls[0].totalCoins >= gameControl.drinkController.drinks[index].Price)
        {
            CloseDrinkPanel();
            CloseDrinkButton();
            var drinkContent = gameControl.actorControls[0].drink.transform.GetChild(0);
            drinkContent.GetComponent<Image>().sprite = drinks[index].Icon;
            iTween.ScaleTo(drinkContent.gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));
            iTween.ScaleTo(drinkContent.gameObject, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce, "delay", 12f, "onComplete", "OpenDrinkButton", "onCompleteTarget", gameObject));
            gameControl.coinController.SpendCoin(drinks[index].Price);
            gameControl.actorControls[0].totalCoins -= drinks[index].Price;
        }
        else
        {
            OpenErrorDrinkPanel();
        }

       

    }

    public void OpenDrinkPanel()
    {
        drinkPanel.SetActive(true);
        makeNoise.PlaySFX(27, 0);

    }

    public void CloseDrinkPanel()
    {
        drinkPanel.SetActive(false);
        makeNoise.PlaySFX(17, 0);

    }

    void CloseDrinkButton()
    {
        drinkButton.SetActive(false);

    }

    void OpenDrinkButton()
    {
        drinkButton.SetActive(true);

    }


    public void OpenErrorDrinkPanel()
    {
        errorDrinkPanel.SetActive(true);
        makeNoise.PlaySFX(27, 0);

    }

    public void CloseErrorDrinkPanel()
    {
        errorDrinkPanel.SetActive(false);
        makeNoise.PlaySFX(17, 0);

    }
}
