using UnityEngine;

[CreateAssetMenu(fileName = "DeckStyle", menuName = "Soner/DeckStyle", order = 2)]
public class DeckStyle : ScriptableObject
{
    public Sprite[] spades;
    public Sprite[] diamonds;
    public Sprite[] hearts;
    public Sprite[] clubs;
    public Sprite jocker;
    public Sprite deckShow;
}
