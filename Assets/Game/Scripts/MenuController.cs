using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameConfig gameConfig;

    //[SerializeField] private InputField playerName;
    [SerializeField] private TMP_InputField playerNameTmp;
    [SerializeField] private Image Avatar;
    [SerializeField] private SpriteRenderer[] cards;
    [SerializeField] private MeshRenderer[] cardBacks;
    //[SerializeField] private SpriteRenderer desk;
    //[SerializeField] private SpriteRenderer background;
    //[SerializeField] private InputField firstPlayerName;
    [SerializeField] private TMP_InputField firstPlayerNameTmp;
    [SerializeField] private GameObject firstPlayerNameCanvas;

    private void Awake()
    {
        string pName = "Guest";

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            pName = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            firstPlayerNameCanvas.SetActive(true);
        }

        playerNameTmp.text = pName;

        gameConfig.avatarInd = 0;//PlayerPrefs.GetInt("Avatar");
        SetAvatar();

        //gameConfig.deckStyleInd = 0;//PlayerPrefs.GetInt("DeckStyle");
        //SetDeckStyle();

        //gameConfig.cardBackInd = 0;// PlayerPrefs.GetInt("DeckBack");
        //SetDeckBack();

        //gameConfig.tableInd = 0;// PlayerPrefs.GetInt("Desk");
        //SetDesk();

        //gameConfig.backgroundInd = 0;// PlayerPrefs.GetInt("BG");
        //SetBG();

        FindObjectOfType<MakeNoise>().PlaySFX(29, 0);
    }

    public void PlayGame()
    {
        GameManager.Instance.CurrentGameMode = GameManager.GameMode.Quick;
        gameConfig.cardDealerInd = -1;

        FindObjectOfType<MakeNoise>().PlaySFX(30, 0);
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    public void PlayMultiplayerGame()
    {
        GameManager.Instance.CurrentGameMode = GameManager.GameMode.Friends;
        gameConfig.cardDealerInd = -1;

        FindObjectOfType<MakeNoise>().PlaySFX(30, 0);
        UnityEngine.SceneManagement.SceneManager.LoadScene(4);
    }

    public void SetPlayerName()
    {
        PlayerPrefs.SetString("PlayerName", playerNameTmp.text);
    }

    public void SetFirstPlayerName()
    {
        PlayerPrefs.SetString("PlayerName", firstPlayerNameTmp.text);
        firstPlayerNameCanvas.SetActive(false);
        playerNameTmp.text = PlayerPrefs.GetString("PlayerName");
    }

    public void SetAvatar()
    {
        Avatar.sprite = gameConfig.avatars[gameConfig.avatarInd];
    }

    //public void SetDeckStyle()
    //{
    //    for (int i = 0; i < cards.Length; i++)
    //    {
    //        Card card = new Card();
    //        card.value = i + 1;

    //        switch (i % 4)
    //        {
    //            case 0:
    //                card.suit = CardSuit.Spades;
    //                break;
    //            case 1:
    //                card.suit = CardSuit.Diamonds;
    //                break;
    //            case 2:
    //                card.suit = CardSuit.Hearts;
    //                break;
    //            case 3:
    //                card.suit = CardSuit.Clubs;
    //                break;
    //        }

    //        if (i == cards.Length - 1)
    //            card.suit = CardSuit.Joker;

    //        cards[i].sprite = CardSpriteConverter.GetCardSpriteInd(card, gameConfig.deckStyles[gameConfig.deckStyleInd]);
    //        cards[i].size = new Vector2(1.52f, 2.17f);
    //    }
    //}

    //public void SetDeckBack()
    //{
    //    for (int i = 0; i < cardBacks.Length; i++)
    //        cardBacks[i].sharedMaterial = gameConfig.cardBacks[gameConfig.cardBackInd];
    //}

    //public void SetDesk()
    //{
    //    desk.sprite = gameConfig.tables[gameConfig.tableInd];
    //}

    //public void SetBG()
    //{
    //    background.sprite = gameConfig.backGrounds[gameConfig.backgroundInd];
    //}
}
