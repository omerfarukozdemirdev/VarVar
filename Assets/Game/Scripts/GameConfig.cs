using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Soner/GameConfig", order = 2)]
public class GameConfig : ScriptableObject
{
    public int cardDealerInd;

    public int avatarInd;
    public Sprite[] avatars;
    public Sprite[] maleAvatars;
    public Sprite[] femaleAvatars;


    public int deckStyleInd;
    public DeckStyle[] deckStyles;

    public int cardBackInd;
    public Material[] cardBacks;
    public Sprite[] cardBackSprites;

    public int backgroundInd;
    public Sprite[] backGrounds;

    public int tableInd;
    public Sprite[] tables;

    public ShopMenuCoins[] shopMenuCoins;

}

[System.Serializable]
public class ShopMenuCoins
{
    public int coincount;
    public float prize;
}
