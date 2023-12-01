

public class NetworkCardConverter
{
    public static NetworkCard CardToNetworkCard(Card card)
    {
        NetworkCard newNetworkCard = new NetworkCard() { suit = (CardSuit)card.suit, value = card.value };
        return newNetworkCard;
    }

    public static Card NetworkCardToCard(NetworkCard networkCard)
    {
        Card card = new Card() { suit = (CardSuit)networkCard.suit, value = networkCard.value };
        return card;
    }
}
