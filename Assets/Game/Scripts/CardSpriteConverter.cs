using UnityEngine;

public class CardSpriteConverter
{
    public static Sprite GetCardSpriteInd(Card card, DeckStyle deckStyle)
    {
        switch (card.suit)
        {
            case CardSuit.Clubs:

                return deckStyle.clubs[card.value - 1];

            case CardSuit.Diamonds:

                return deckStyle.diamonds[card.value - 1];

            case CardSuit.Hearts:

                return deckStyle.hearts[card.value - 1];

            case CardSuit.Spades:

                return deckStyle.spades[card.value - 1];

            case CardSuit.Joker:

                return deckStyle.jocker;
        }

        return null;  
    }
}
