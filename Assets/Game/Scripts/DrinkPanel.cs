using UnityEngine;
using UnityEngine.UI;

public class DrinkPanel : MonoBehaviour
{
    [SerializeField] private GameControl gameControl;
    [SerializeField] private Transform content;

    private void OnEnable()
    {
        for (int i = 0; i < gameControl.drinkController.drinks.Length; i++)
        {
            if (gameControl.actorControls[0].totalCoins >= gameControl.drinkController.drinks[i].Price)
            {
                content.GetChild(i).GetComponent<Button>().interactable = true;
            }
            else
            {
                content.GetChild(i).GetComponent<Button>().interactable = false;
            }
        }
    }
}
