using System.Collections.Generic;

public class NetworkCardConverter
{
    public static int CardToInt(Card card)
    {
        if (card.suit == CardSuit.Joker)
            return 53;

        return ( ((int)card.suit - 1) * 13 ) + card.value;
    }

    public static Card IntToCard(int value)
    {
        Card card = new Card();

        if (value == 53)
        {
            card.suit = CardSuit.Joker;
            card.value = 53;

            return card;
        }

        card.suit = (CardSuit)( (value - 1) / 13) + 1;
        card.value = ( (value - 1) % 13 ) + 1;

        return card;
    }

    public static string CardsToString(List<Card> cards)
    {
        string cardkString = "";
        for (int i = 0; i < cards.Count; i++)
        {
            int value = NetworkCardConverter.CardToInt(cards[i]);

            if (i == cards.Count - 1)
                cardkString += value;
            else
                cardkString += value + ",";
        }

        return cardkString;
    }
    public static List<Card> StringToCards(string cardString)
    {
        List<Card> cards = new List<Card>();

        string[] parsed = cardString.Split(',');
        for (int i = 0; i < parsed.Length; i++)
        {
            int value = int.Parse(parsed[i]);
            cards.Add(NetworkCardConverter.IntToCard(value));
        }

        return cards;
    }
}
