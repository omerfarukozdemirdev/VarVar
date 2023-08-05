using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameConfig gameConfig;

    [SerializeField] private InputField playerName;
    [SerializeField] private Image Avatar;
    [SerializeField] private SpriteRenderer[] cards;
    [SerializeField] private MeshRenderer[] cardBacks;
    [SerializeField] private SpriteRenderer desk;
    [SerializeField] private SpriteRenderer background;

    private void Awake()
    {
        string pName = "Player";

        if(PlayerPrefs.HasKey("PlayerName"))
            pName = PlayerPrefs.GetString("PlayerName");

        playerName.text = pName;

        gameConfig.avatarInd = PlayerPrefs.GetInt("Avatar");
        SetAvatar();

        gameConfig.deckStyleInd = PlayerPrefs.GetInt("DeckStyle");
        SetDeckStyle();

        gameConfig.cardBackInd = PlayerPrefs.GetInt("DeckBack");
        SetDeckBack();

        gameConfig.tableInd = PlayerPrefs.GetInt("Desk");
        SetDesk();

        gameConfig.backgroundInd = PlayerPrefs.GetInt("BG");
        SetBG();
    }

    public void PlayGame()
    {
        gameConfig.cardDealerInd = -1;

        FindObjectOfType<MakeNoise>().PlaySFX(10, 0);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }

    public void SetPlayerName()
    {
        PlayerPrefs.SetString("PlayerName" , playerName.text);
    }

    public void SetAvatar()
    {
        Avatar.sprite = gameConfig.avatars[gameConfig.avatarInd];
    }

    public void SetDeckStyle()
    {
        for(int i = 0; i < cards.Length; i++)
        {
            Card card = new Card();
            card.value = i + 1;

            switch (i % 4)
            {
                case 0:
                    card.suit = CardSuit.Spades;
                    break;
                case 1:
                    card.suit = CardSuit.Diamonds;
                    break;
                case 2:
                    card.suit = CardSuit.Hearts;
                    break;
                case 3:
                    card.suit = CardSuit.Clubs;
                    break;
            }

            if (i == cards.Length - 1)
                card.suit = CardSuit.Joker;

            cards[i].sprite = CardSpriteConverter.GetCardSpriteInd(card, gameConfig.deckStyles[gameConfig.deckStyleInd]);
            cards[i].size = new Vector2(1.52f, 2.17f);
        }
    }

    public void SetDeckBack()
    {
        for(int i = 0; i < cardBacks.Length; i++)
            cardBacks[i].sharedMaterial = gameConfig.cardBacks[gameConfig.cardBackInd];
    }

    public void SetDesk()
    {
        desk.sprite = gameConfig.tables[gameConfig.tableInd];
    }

    public void SetBG()
    {
        background.sprite = gameConfig.backGrounds[gameConfig.backgroundInd];
    }
}
