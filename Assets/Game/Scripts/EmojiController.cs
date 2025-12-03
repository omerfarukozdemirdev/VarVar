using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiController : MonoBehaviour
{
    public Sprite[] emojis;
    [SerializeField] private EmojiButton emojiButtonPrefab;
    [SerializeField] private Transform emojiButtonsRoot;
    [SerializeField] private GameObject emojiPanel;
    [SerializeField] private GameControl gameControl;

    public GameObject emojiButton;

    private MakeNoise makeNoise;

    void Awake()
    {
        makeNoise = FindObjectOfType<MakeNoise>();

    }

    void Start()
    {
        for (int i = 0; i < emojis.Length; i++)
        {
            EmojiButton emojiButton = Instantiate(emojiButtonPrefab, emojiButtonsRoot);
            emojiButton.emojiID = i;
            emojiButton.EmojiIconImage.sprite = emojis[i];
            emojiButton.GetComponent<Button>().onClick.AddListener(() => EmojiButtonOnClick(emojiButton.emojiID));
        }
    }

    void EmojiButtonOnClick(int index)
    {
        gameControl.SendEmojiServerRpc((byte)gameControl.actorControls.IndexOf(gameControl.playerControl.actorControl), (byte)index);
        CloseEmojiPanel();
        CloseEmojiButton();

        if (GameModeChecker.Instance.IsMultiplayerActive)
            return;

        var emojiContent = gameControl.actorControls[0].emoji.transform.GetChild(0);
        emojiContent.GetComponent<Image>().sprite = emojis[index];
        iTween.ScaleTo(emojiContent.gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easetype", iTween.EaseType.easeOutBounce));
        iTween.ScaleTo(emojiContent.gameObject, iTween.Hash("scale", Vector3.zero, "time", .3f, "easetype", iTween.EaseType.easeOutBounce, "delay", 12f, "onComplete", "OpenEmojiButton", "onCompleteTarget", gameObject));
    }
    public void OpenEmojiPanel()
    {
        emojiPanel.SetActive(true);
        makeNoise.PlaySFX(27, 0);
    }

    public void CloseEmojiPanel()
    {
        emojiPanel.SetActive(false);
        makeNoise.PlaySFX(17, 0);
    }

    void CloseEmojiButton()
    {
        emojiButton.SetActive(false);

    }

    void OpenEmojiButton()
    {
        emojiButton.SetActive(true);

    }
}
