using UnityEngine;
using UnityEngine.UI;

public class MarketController : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [SerializeField] private GameObject avatarProductPrefab;
    [SerializeField] private Transform avatarProductParent;
  
    [SerializeField] private GameObject deckProductPrefab;
    [SerializeField] private Transform deckProductParent;

    [SerializeField] private GameObject deckBackProductPrefab;
    [SerializeField] private Transform deckBackProductParent;

    [SerializeField] private GameObject deskProductPrefab;
    [SerializeField] private Transform deskProductParent;
                      
    [SerializeField] private GameObject bgProductPrefab;
    [SerializeField] private Transform bgProductParent;
                      
    [SerializeField] private GameObject[] tabs;
    [SerializeField] private Sprite[] tabSprites;
    [SerializeField] private Image tabFrame;

    private MenuController menuController;
    private CoinController coinController;
    private MakeNoise makeNoise;

    void Start()
    {
        menuController = FindObjectOfType<MenuController>();
        coinController = FindObjectOfType<CoinController>();
        makeNoise = FindObjectOfType<MakeNoise>();

        CreateAvatars();
        CreateDecks();
        CreateRooms();

        SetTab(0);
    }

    public void OpenInventory()
    {
        panel.SetActive(true);
        makeNoise.PlaySFX(27, 0);
    }

    public void CloseInventory()
    {
        panel.SetActive(false);
        makeNoise.PlaySFX(17, 0);
    }

    public void ChangeTab(int ind)
    {
        SetTab(ind);
        makeNoise.PlaySFX(28, 0);
    }

    void SetTab(int ind)
    {
        for (int i = 0; i < tabs.Length; i++)
            tabs[i].SetActive(false);

        tabs[ind].SetActive(true);
        tabFrame.sprite = tabSprites[ind];
    }

    void CreateAvatars()
    {
        for (int i = 0; i < menuController.gameConfig.avatars.Length; i++)
        {
            GameObject ap = Instantiate(avatarProductPrefab, avatarProductParent);
            AvatarProduct avatarProduct = ap.GetComponent<AvatarProduct>();
            avatarProduct.Setup(i, coinController, menuController);
        }

        avatarProductParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    void CreateDecks()
    {
        for (int i = 0; i < menuController.gameConfig.deckStyles.Length; i++)
        {
            GameObject dp = Instantiate(deckProductPrefab, deckProductParent);
            DeckProduct deckProduct = dp.GetComponent<DeckProduct>();
            deckProduct.Setup(i, coinController, menuController);
        }

        deckProductParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        for (int i = 0; i < menuController.gameConfig.cardBackSprites.Length; i++)
        {
            GameObject dbp = Instantiate(deckBackProductPrefab, deckBackProductParent); 
            DeckbackProduct deckbackProduct = dbp.GetComponent<DeckbackProduct>();
            deckbackProduct.Setup(i, coinController, menuController);
        }

        deckBackProductParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    void CreateRooms()
    {
        for (int i = 0; i < menuController.gameConfig.tables.Length; i++)
        {
            GameObject dp = Instantiate(deskProductPrefab, deskProductParent);
            DeskProduct deskProduct = dp.GetComponent<DeskProduct>();
            deskProduct.Setup(i, coinController, menuController);
        }

        deskProductParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        for (int i = 0; i < menuController.gameConfig.backGrounds.Length; i++)
        {
            GameObject bgp = Instantiate(bgProductPrefab, bgProductParent);
            BGProduct bgProduct = bgp.GetComponent<BGProduct>();
            bgProduct.Setup(i, coinController, menuController);
        }

        bgProductParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
