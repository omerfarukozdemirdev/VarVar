using UnityEngine;

public class CardsBlock : MonoBehaviour
{
    [SerializeField] Animator cardsAnimator;

    public void CardsOpen()
    {
        cardsAnimator.SetTrigger("Open");
    }

    public void CardsFlop()
    {
        cardsAnimator.SetTrigger("Flop");
    }

    public void CardsClose()
    {
        cardsAnimator.SetTrigger("Close");
    }
}
