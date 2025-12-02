using UnityEngine;
using UnityEngine.UI;

public class SelectAvatarButton : MonoBehaviour
{
    [SerializeField] private Image avatarImage;

    public void Init(Sprite avatarSprite)
    {
        avatarImage.sprite = avatarSprite;
    }
}
