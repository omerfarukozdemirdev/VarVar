using UnityEngine;
using UnityEngine.UI;

public class SelectAvatarPanel : MonoBehaviour
{
    [SerializeField] private Transform avatarContent;
    [SerializeField] private MenuController menuController;

    private void Start()
    {
        for (int i = 0; i < menuController.gameConfig.avatars.Length; i++)
        {
            int index = i;
            avatarContent.GetChild(i).GetComponent<SelectAvatarButton>().Init(menuController.gameConfig.avatars[i]);
            var button = avatarContent.GetChild(i).GetComponent<Button>();
            button.onClick.AddListener(() => OnAvatarSelected(index));
        }
    }

    private void OnAvatarSelected(int index)
    {
        PlayerPrefs.SetInt(Constants.PlayerData.PlayerAvatarKey, index);
        menuController.SetAvatar(index);
    }
}
