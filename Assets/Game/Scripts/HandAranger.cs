using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public enum CardSuit
{
    Spades = 1,
    Diamonds,
    Hearts,
    Clubs,
    Joker
}

public struct NetworkCardData : INetworkSerializable, System.IEquatable<NetworkCardData>
{
    public CardSuit suit;
    public int value;

    // INetworkSerializable uygulaması
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref suit);
        serializer.SerializeValue(ref value);
    }

    public bool Equals(NetworkCardData other)
    {
        return suit == other.suit && value == other.value;
    }
}

[System.Serializable]
public class Card
{
    public CardSuit suit;
    public int value;
}

public static class NetworkSerializationExtensions
{
    public static NetworkCardData ToNetworkCard(this Card card)
    {
        return new NetworkCardData
        {
            suit = card.suit,
            value = card.value
        };
    }

    public static Card ToCard(this NetworkCardData networkData)
    {
        return new Card
        {
            suit = networkData.suit,
            value = networkData.value
        };
    }
}

[System.Serializable]
public class PerGrp
{
    public List<Card> cardsWith;
    public List<Card> missingCards;
    //public Card critiqueMissingCard;
}

public class HandAranger : MonoBehaviour
{
    private Card joker = new Card() { suit = CardSuit.Joker, value = -1 };

    private List<Card> cardsInHand = new List<Card>();

    private List<Card> SpadesCards = new List<Card>();
    private List<Card> HeartsCards = new List<Card>();
    private List<Card> DiamondsCards = new List<Card>();
    private List<Card> ClubsCards = new List<Card>();

    private List<PerGrp> runs = new List<PerGrp>();
    private List<PerGrp> runGrps = new List<PerGrp>();

    private List<PerGrp> matches = new List<PerGrp>();
    private List<PerGrp> matchGrps = new List<PerGrp>();

    private List<Card> remainingCards = new List<Card>();
    private List<Card> missingCards = new List<Card>();

    private bool handCompleted;
    private int completeStep;

    //void Start()
    //{
    //    for(int i = 0; i < 9; i++)
    //    {
    //        Card card = new Card();
    //        card.suit = GetRandomEnum<CardSuit>(1);
    //        card.value = Random.Range(1,14);

    //        cardsInHand.Add(card);
    //    }
    //}

    //T GetRandomEnum<T>(int c)
    //{
    //    System.Array A = System.Enum.GetValues(typeof(T));
    //    T V = (T)A.GetValue(Random.Range(0, A.Length - c));
    //    return V;
    //}

    public List<Card> Arrange(List<Card> cards)
    {
        cardsInHand = new List<Card>(cards);

        if(cardsInHand.Contains(joker))
            cardsInHand.Remove(joker);

        int ind = 1;
        int laststep;
        int step;

        laststep = StrategyA();

        step = StrategyB();
        if (step < laststep)
        {
            laststep = step;
            ind = 2;
        }

        step = StrategyC();
        if (step < laststep)
        {
            laststep = step;
            ind = 3;
        }

        step = StrategyD();
        if (step < laststep)
        {
            laststep = step;
            ind = 4;
        }

        switch (ind)
        {
            case 1:
                StrategyA();
                break;
            case 2:
                StrategyB();
                break;
            case 3:
                StrategyC();
                break;
            case 4:
                StrategyD();
                break;
        }

        handCompleted = false;

        if(laststep == 0)
        {
            handCompleted = true;
            remainingCards.Add(joker);
        }
        else
        {
            handCompleted = (runGrps.Count + matchGrps.Count == 1) && remainingCards.Count <= 1;

            if (runGrps.Count > 0 && matchGrps.Count == 0)
                runGrps[0].cardsWith.Add(joker);

           else if (runGrps.Count == 0 && matchGrps.Count > 0)
                matchGrps[0].cardsWith.Add(joker);

            else if (runGrps.Count > 0 && matchGrps.Count > 0)
            {
                if(Random.Range(0,2) == 0)
                    matchGrps[0].cardsWith.Add(joker);
                else
                    runGrps[0].cardsWith.Add(joker);
            }
            else
                remainingCards.Add(joker);
        }

        List<Card> arrangedCards = new List<Card>();

        for(int i = 0; i < runs.Count; i++)
            for (int c = 0; c < runs[i].cardsWith.Count; c++)
                arrangedCards.Add(runs[i].cardsWith[c]);

        for (int i = 0; i < matches.Count; i++)
            for (int c = 0; c < matches[i].cardsWith.Count; c++)
                arrangedCards.Add(matches[i].cardsWith[c]);

        for (int i = 0; i < runGrps.Count; i++)
            for (int c = 0; c < runGrps[i].cardsWith.Count; c++)
                arrangedCards.Add(runGrps[i].cardsWith[c]);

        for (int i = 0; i < matchGrps.Count; i++)
            for (int c = 0; c < matchGrps[i].cardsWith.Count; c++)
                arrangedCards.Add(matchGrps[i].cardsWith[c]);

        for (int i = 0; i < remainingCards.Count; i++)
            arrangedCards.Add(remainingCards[i]);

        return arrangedCards;
    }

    public bool HandCompleted()
    {
        return handCompleted;
    }
    public int HandCompletedStep()
    {
        return completeStep;
    }

    public List<Card> MissingCards()
    {
        missingCards = new List<Card>();

        for (int i = 0; i < runGrps.Count; i++)
            for (int m = 0; m < runGrps[i].missingCards.Count; m++)
                missingCards.Add(runGrps[i].missingCards[m]);

        for (int i = 0; i < matchGrps.Count; i++)
            for (int m = 0; m < matchGrps[i].missingCards.Count; m++)
                missingCards.Add(matchGrps[i].missingCards[m]);

        return missingCards;
    }

    public List<Card> RemainingCards()
    {
        return remainingCards;
    }

    int StrategyA()
    {
        remainingCards = new List<Card>(cardsInHand);

        SeperateSuits(remainingCards);
        FindRuns();

        FindMatches(remainingCards);

        SeperateSuits(remainingCards);
        FindRunGRPs();

        FindMatchesGrp(remainingCards);

        CanCompleteMatchesFromRun();

        return CompleteStep();
    }

    int StrategyB()
    {
        remainingCards = new List<Card>(cardsInHand);

        SeperateSuits(remainingCards);
        FindRuns();

        FindMatches(remainingCards);

        FindMatchesGrp(remainingCards);

        SeperateSuits(remainingCards);
        FindRunGRPs();

        CanCompleteMatchesFromRun();

        return CompleteStep();
    }

    int StrategyC()
    {
        remainingCards = new List<Card>(cardsInHand);

        FindMatches(remainingCards);

        SeperateSuits(remainingCards);
        FindRuns();

        FindMatchesGrp(remainingCards);

        SeperateSuits(remainingCards);
        FindRunGRPs();

        return CompleteStep();
    }

    int StrategyD()
    {
        remainingCards = new List<Card>(cardsInHand);

        FindMatches(remainingCards);

        SeperateSuits(remainingCards);
        FindRuns();

        SeperateSuits(remainingCards);
        FindRunGRPs();

        FindMatchesGrp(remainingCards);

        return CompleteStep();
    }

    void CanCompleteMatchesFromRun()
    {
        List<int> inds = new List<int>();
        for(int r = 0; r < runs.Count; r++)
        {
            if(runs[r].cardsWith.Count > 3)
                inds.Add(r);
        }

        List<PerGrp> perGrps = new List<PerGrp>();
        for(int i = 0; i < inds.Count; i++)
        {
            for (int mg = 0; mg < matchGrps.Count; mg++)
            {
                for (int m = 0; m < matchGrps[mg].missingCards.Count; m++)
                {
                    if(runs[i].cardsWith[0].suit == matchGrps[mg].missingCards[m].suit)
                    {
                        if (runs[i].cardsWith[0].value == matchGrps[mg].missingCards[m].value)
                        {
                            matchGrps[mg].cardsWith.Add(runs[i].cardsWith[0]);

                            perGrps.Add(matchGrps[mg]);

                            runs[i].cardsWith.Remove(runs[i].cardsWith[0]);
                        }
                        if (runs[i].cardsWith[runs[i].cardsWith.Count - 1].value == matchGrps[mg].missingCards[m].value)
                        {
                            matchGrps[mg].cardsWith.Add(runs[i].cardsWith[runs[i].cardsWith.Count - 1]);

                            perGrps.Add(matchGrps[mg]);

                            runs[i].cardsWith.Remove(runs[i].cardsWith[runs[i].cardsWith.Count - 1]);
                        }
                    }
                }
            }
        }

        for(int i = 0; i < perGrps.Count; i++)
        {
            matches.Add(perGrps[i]);
            matchGrps.Remove(perGrps[i]);
        }

        MatchesMissingCards();
        MatchGRPSMissingCards();
        RunMissingCards();
    }

    int CompleteStep()
    {
        completeStep = 0;

        completeStep += runGrps.Count;
        completeStep += matchGrps.Count;

        if (completeStep == 0 && remainingCards.Count <= 1)
            completeStep = 0;
        else
            completeStep += remainingCards.Count;

        return completeStep;
    }

    void SeperateSuits(List<Card> cards)
    {
        SpadesCards = new List<Card>();
        HeartsCards = new List<Card>();
        DiamondsCards = new List<Card>();
        ClubsCards = new List<Card>();

        if (cards.Count == 0)
            return;

        var ordered = cards.OrderBy(c => c.value).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            switch (ordered[i].suit)
            {
                case CardSuit.Spades:

                    SpadesCards.Add(ordered[i]);

                    break;
                case CardSuit.Hearts:

                    HeartsCards.Add(ordered[i]);

                    break;
                case CardSuit.Diamonds:

                    DiamondsCards.Add(ordered[i]);

                    break;
                case CardSuit.Clubs:

                    ClubsCards.Add(ordered[i]);

                    break;
            }
        }
    }

    void FindRuns()
    {
        runs = new List<PerGrp>();

        FindRun(SpadesCards);
        FindRun(HeartsCards);
        FindRun(DiamondsCards);
        FindRun(ClubsCards);

        RunMissingCards();

        for (int r = 0; r < runs.Count; r++)
            for (int i = 0; i < runs[r].cardsWith.Count; i++)
                remainingCards.Remove(runs[r].cardsWith[i]);
    }

    void FindRunGRPs()
    {
        runGrps = new List<PerGrp>();

        FindRunGrp(SpadesCards);
        FindRunGrp(HeartsCards);
        FindRunGrp(DiamondsCards);
        FindRunGrp(ClubsCards);

        RunGRPMissingCards();

        for (int r = 0; r < runGrps.Count; r++)
            for (int i = 0; i < runGrps[r].cardsWith.Count; i++)
                remainingCards.Remove(runGrps[r].cardsWith[i]);
    }

    void FindRun(List<Card> cards)
    {
        if (cards.Count < 3)
            return;

        PerGrp run = new PerGrp();
        run.cardsWith = new List<Card>() { cards[0] };
        run.missingCards = new List<Card>();

        for (int i = 1; i < cards.Count; i++)
        {
            var currentCard = cards[i];
            var previousCard = cards[i - 1];

            if (currentCard.value == previousCard.value + 1)
            {
                run.cardsWith.Add(currentCard);

                if (currentCard.value == 13)
                {
                    if (cards[0].value == 1 && cards[1].value == 1)
                    {
                        run.cardsWith.Add(cards[0]);
                    }
                    else if (cards[0].value == 1 && (cards[1].value + cards[2].value) != 5 )
                    {
                        run.cardsWith.Add(cards[0]);
                    }
                    else
                    {
                        bool found = false;
                        if(runs.Count > 0)
                        {
                            if (runs[0].cardsWith.Count > 3 && runs[0].cardsWith[0].value == 1)
                            {
                                run.cardsWith.Add(runs[0].cardsWith[0]);
     
                                runs[0].cardsWith.Remove(runs[0].cardsWith[0]);

                                found = true;
                            }
                        }

                        if (!found)
                        {
                            int ind = -1;
                            for (int m = 0; m < matches.Count; m++)
                            {
                                if (matches[m].cardsWith.Count == 4 && matches[m].cardsWith[0].value == 1)
                                {
                                    for (int c = 0; c < matches[m].cardsWith.Count; c++)
                                    {
                                        if (matches[m].cardsWith[c].suit == cards[0].suit)
                                        {
                                            ind = m;
                                            matches[m].missingCards.Add(matches[m].cardsWith[c]);
                                            run.cardsWith.Add(matches[m].cardsWith[c]);
                                        }
                                    }
                                }
                            }

                            if (ind != -1)
                            {
                                matches[ind].cardsWith.Remove(matches[ind].missingCards[0]);
                            }
                        }
                    }
               }
            }
            else
            {
                if (run.cardsWith.Count >= 3) // Run Completed
                {
                    runs.Add(run);
                }

                run = new PerGrp();
                run.cardsWith = new List<Card>() { currentCard };
                run.missingCards = new List<Card>();
            }
        }

        if (run.cardsWith.Count >= 3) // Run Completed
        {
            runs.Add(run);
        }
    }

    void RunMissingCards()
    {
        for (int i = 0; i < runs.Count; i++)
            runs[i].missingCards.Clear();

        for (int i = 0; i < runs.Count; i++)
        {
            if (runs[i].cardsWith[0].value > 1)
                runs[i].missingCards.Add(new Card() { suit = runs[i].cardsWith[0].suit, value = runs[i].cardsWith[0].value - 1 });

            if (runs[i].cardsWith[runs[i].cardsWith.Count - 1].value < 13 && runs[i].cardsWith[runs[i].cardsWith.Count - 1].value != 1)
                runs[i].missingCards.Add(new Card() { suit = runs[i].cardsWith[runs[i].cardsWith.Count - 1].suit, value = runs[i].cardsWith[runs[i].cardsWith.Count - 1].value + 1 });

            if (runs[i].cardsWith[runs[i].cardsWith.Count - 1].value == 13)
                runs[i].missingCards.Add(new Card() { suit = runs[i].cardsWith[runs[i].cardsWith.Count - 1].suit, value = 1 });
        }
    }

    void FindRunGrp(List<Card> cards)
    {
        if (cards.Count == 0)
            return;

        PerGrp runG = new PerGrp();

        runG.cardsWith = new List<Card>() { cards[0] };
        runG.missingCards = new List<Card>();

        for (int i = 1; i < cards.Count; i++)
        {
            var currentCard = cards[i];
            var previousCard = cards[i - 1];

            if (currentCard.value == previousCard.value + 1)
            {
                runG.cardsWith.Add(currentCard);
            }
            //else if (currentCard.value == previousCard.value + 2)
            //{
            //    runG.cardsWith.Add(currentCard);
            //    runG.missingCards.Add(new Card() { suit = currentCard.suit, value = previousCard.value + 1 });
            //    //runG.critiqueMissingCard = new Card() { suit = currentCard.suit, value = previousCard.value + 1 };
            //}
            else
            {
                if (runG.cardsWith.Count == 2)
                {
                    runGrps.Add(runG);
                }

                runG = new PerGrp();
                runG.cardsWith = new List<Card>() { currentCard };
                runG.missingCards = new List<Card>();
            }
        }

        if (runG.cardsWith.Count == 2)
        {
            runGrps.Add(runG);
        }
    }

    void RunGRPMissingCards()
    {
        for (int i = 0; i < runGrps.Count; i++)
        {
            if (runGrps[i].cardsWith[0].value > 1 && runGrps[i].cardsWith[1].value == runGrps[i].cardsWith[0].value + 1)
                runGrps[i].missingCards.Add(new Card() { suit = runGrps[i].cardsWith[0].suit, value = runGrps[i].cardsWith[0].value - 1 });

            if (runGrps[i].cardsWith[runGrps[i].cardsWith.Count - 1].value == runGrps[i].cardsWith[runGrps[i].cardsWith.Count - 2].value + 1)
            {
                if (runGrps[i].cardsWith[runGrps[i].cardsWith.Count - 1].value < 13)
                    runGrps[i].missingCards.Add(new Card() { suit = runGrps[i].cardsWith[0].suit, value = runGrps[i].cardsWith[runGrps[i].cardsWith.Count - 1].value + 1 });

                if (runGrps[i].cardsWith[runGrps[i].cardsWith.Count - 1].value == 13)
                    runGrps[i].missingCards.Add(new Card() { suit = runGrps[i].cardsWith[0].suit, value = 1 });
            }
        }
    }

    void FindMatches(List<Card> cards)
    {
        matches = new List<PerGrp>();

        if (cards.Count == 0)
            return;

        var ordered = cards.OrderBy(c => c.value).ToList();

        PerGrp match = new PerGrp();
        match.cardsWith = new List<Card>() { ordered[0] };
        match.missingCards = new List<Card>();

        for (int i = 1; i < ordered.Count; i++)
        {
            var currentCard = ordered[i];
            var previousCard = ordered[i - 1];

            if (currentCard.value == previousCard.value)
            {
                match.cardsWith.Add(currentCard);
            }
            else
            {
                if (match.cardsWith.Count >= 3)
                {
                    matches.Add(match);
                }

                match = new PerGrp();
                match.cardsWith = new List<Card>() { currentCard };
                match.missingCards = new List<Card>();
            }
        }

        if (match.cardsWith.Count >= 3)
        {
            matches.Add(match);
        }

        int spadesCount;
        int heartsCount;
        int diamondsCount;
        int clubsCount;

        List<PerGrp> notGrp = new List<PerGrp>();

        for (int m = 0; m < matches.Count; m++)
        {
            spadesCount = 0;
            heartsCount = 0;
            diamondsCount = 0;
            clubsCount = 0;

            for (int i = 0; i < matches[m].cardsWith.Count; i++)
            {
                     if (matches[m].cardsWith[i].suit == CardSuit.Spades)    spadesCount++;

                else if (matches[m].cardsWith[i].suit == CardSuit.Hearts)    heartsCount++;

                else if (matches[m].cardsWith[i].suit == CardSuit.Diamonds)  diamondsCount++;

                else if (matches[m].cardsWith[i].suit == CardSuit.Clubs)     clubsCount++;
            }

            // çift olanları çıkar
            List<Card> dublicated = new List<Card>();

            for (int i = 0; i < matches[m].cardsWith.Count; i++)
            {
                if (matches[m].cardsWith[i].suit == CardSuit.Spades && spadesCount > 1)
                {
                    dublicated.Add(matches[m].cardsWith[i]);
                    spadesCount--;
                }

                else if (matches[m].cardsWith[i].suit == CardSuit.Hearts && heartsCount > 1)
                {
                    dublicated.Add(matches[m].cardsWith[i]);
                    heartsCount--;
                }

                else if (matches[m].cardsWith[i].suit == CardSuit.Diamonds && diamondsCount > 1)
                {
                    dublicated.Add(matches[m].cardsWith[i]);
                    diamondsCount--;
                }

                else if (matches[m].cardsWith[i].suit == CardSuit.Clubs && clubsCount > 1)
                {
                    dublicated.Add(matches[m].cardsWith[i]);
                    clubsCount--;
                }
            }

            for (int i = 0; i < dublicated.Count; i++)
                matches[m].cardsWith.Remove(dublicated[i]);


            if(matches[m].cardsWith.Count < 3)
                notGrp.Add(matches[m]);
        }

        for(int i = 0; i < notGrp.Count; i++)
            matches.Remove(notGrp[i]);

        //for (int i = 0; i < per.Count; i++)
        //{
        //    matches.Add(per[i]);
        //    matchGrps.Remove(per[i]);
        //}

        MatchesMissingCards();

        for (int m = 0; m < matches.Count; m++)
            for (int i = 0; i < matches[m].cardsWith.Count; i++)
                remainingCards.Remove(matches[m].cardsWith[i]);
    }

    void MatchesMissingCards()
    {
        for (int m = 0; m < matches.Count; m++)
            matches[m].missingCards.Clear();

        int spadesCount;
        int heartsCount;
        int diamondsCount;
        int clubsCount;

        for (int m = 0; m < matches.Count; m++)
        {
            spadesCount = 0;
            heartsCount = 0;
            diamondsCount = 0;
            clubsCount = 0;

            for (int i = 0; i < matches[m].cardsWith.Count; i++)
            {
                if (matches[m].cardsWith[i].suit == CardSuit.Spades) spadesCount++;

                else if (matches[m].cardsWith[i].suit == CardSuit.Hearts) heartsCount++;

                else if (matches[m].cardsWith[i].suit == CardSuit.Diamonds) diamondsCount++;

                else if (matches[m].cardsWith[i].suit == CardSuit.Clubs) clubsCount++;
            }

            if (spadesCount == 0) matches[m].missingCards.Add(new Card() { suit = CardSuit.Spades, value = matches[m].cardsWith[0].value });

            if (heartsCount == 0) matches[m].missingCards.Add(new Card() { suit = CardSuit.Hearts, value = matches[m].cardsWith[0].value });

            if (diamondsCount == 0) matches[m].missingCards.Add(new Card() { suit = CardSuit.Diamonds, value = matches[m].cardsWith[0].value });

            if (clubsCount == 0) matches[m].missingCards.Add(new Card() { suit = CardSuit.Clubs, value = matches[m].cardsWith[0].value });
        }
    }

    void FindMatchesGrp(List<Card> cards)
    {
        matchGrps = new List<PerGrp>();

        if (cards.Count == 0)
            return;

        var ordered = cards.OrderBy(c => c.value).ToList();

        PerGrp match = new PerGrp();
        match.cardsWith = new List<Card>() { ordered[0] };
        match.missingCards = new List<Card>();

        for (int i = 1; i < ordered.Count; i++)
        {
            var currentCard = ordered[i];
            var previousCard = ordered[i - 1];

            if (currentCard.value == previousCard.value)
            {
                match.cardsWith.Add(currentCard);
            }
            else
            {
                if (match.cardsWith.Count == 2)
                {
                    matchGrps.Add(match);
                }

                match = new PerGrp();
                match.cardsWith = new List<Card>() { currentCard };
                match.missingCards = new List<Card>();
            }
        }

        if (match.cardsWith.Count == 2)
        {
            matchGrps.Add(match);
        }

        int spadesCount;
        int heartsCount;
        int diamondsCount;
        int clubsCount;

        List<PerGrp> notGrp = new List<PerGrp>();

        for (int m = 0; m < matchGrps.Count; m++)
        {
            spadesCount = 0;
            heartsCount = 0;
            diamondsCount = 0;
            clubsCount = 0;

            for (int i = 0; i < matchGrps[m].cardsWith.Count; i++)
            {
                if (matchGrps[m].cardsWith[i].suit == CardSuit.Spades) spadesCount++;

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Hearts) heartsCount++;

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Diamonds) diamondsCount++;

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Clubs) clubsCount++;
            }

            // çift olanları çıkar
            List<Card> dublicated = new List<Card>();

            for (int i = 0; i < matchGrps[m].cardsWith.Count; i++)
            {
                if (matchGrps[m].cardsWith[i].suit == CardSuit.Spades && spadesCount > 1)
                {
                    dublicated.Add(matchGrps[m].cardsWith[i]);
                    spadesCount--;
                }

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Hearts && heartsCount > 1)
                {
                    dublicated.Add(matchGrps[m].cardsWith[i]);
                    heartsCount--;
                }

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Diamonds && diamondsCount > 1)
                {
                    dublicated.Add(matchGrps[m].cardsWith[i]);
                    diamondsCount--;
                }

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Clubs && clubsCount > 1)
                {
                    dublicated.Add(matchGrps[m].cardsWith[i]);
                    clubsCount--;
                }
            }

            for (int i = 0; i < dublicated.Count; i++)
                matchGrps[m].cardsWith.Remove(dublicated[i]);

            if (matchGrps[m].cardsWith.Count < 2)
                notGrp.Add(matchGrps[m]);
        }

        for (int i = 0; i < notGrp.Count; i++)
            matchGrps.Remove(notGrp[i]);

        MatchGRPSMissingCards();

        for (int m = 0; m < matchGrps.Count; m++)
            for (int i = 0; i < matchGrps[m].cardsWith.Count; i++)
                remainingCards.Remove(matchGrps[m].cardsWith[i]);
    }

    void MatchGRPSMissingCards()
    {
        for (int m = 0; m < matchGrps.Count; m++)
            matchGrps[m].missingCards.Clear();

        int spadesCount;
        int heartsCount;
        int diamondsCount;
        int clubsCount;

        for (int m = 0; m < matchGrps.Count; m++)
        {
            spadesCount = 0;
            heartsCount = 0;
            diamondsCount = 0;
            clubsCount = 0;

            for (int i = 0; i < matchGrps[m].cardsWith.Count; i++)
            {
                if (matchGrps[m].cardsWith[i].suit == CardSuit.Spades) spadesCount++;

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Hearts) heartsCount++;

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Diamonds) diamondsCount++;

                else if (matchGrps[m].cardsWith[i].suit == CardSuit.Clubs) clubsCount++;
            }

            if (spadesCount == 0) matchGrps[m].missingCards.Add(new Card() { suit = CardSuit.Spades, value = matchGrps[m].cardsWith[0].value });

            if (heartsCount == 0) matchGrps[m].missingCards.Add(new Card() { suit = CardSuit.Hearts, value = matchGrps[m].cardsWith[0].value });

            if (diamondsCount == 0) matchGrps[m].missingCards.Add(new Card() { suit = CardSuit.Diamonds, value = matchGrps[m].cardsWith[0].value });

            if (clubsCount == 0) matchGrps[m].missingCards.Add(new Card() { suit = CardSuit.Clubs, value = matchGrps[m].cardsWith[0].value });
        }
    }
}
