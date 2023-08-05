using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHandChecker : MonoBehaviour
{
    [SerializeField] List<PossibilityArrays> possibilityArrays = new List<PossibilityArrays>();

    List<int> runPattern = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 1 };

    public bool CheckHandCompleted(List<Card> cards)
    {
        for(int i = 0; i < possibilityArrays.Count; i++)
        {
            int completedCount = 0;
            for (int j = 0; j < possibilityArrays[i].perCounts.Length; j++)
            {
                List<Card> newCards = GetCards(cards, possibilityArrays[i].perCounts, j);

                if (CheckMatch(newCards) || CheckRun(newCards))
                    completedCount++;
            }

            if (completedCount == possibilityArrays[i].perCounts.Length)
                return true;
        }

        return false;
    }
    
    List<Card> GetCards(List<Card> cards , int[] perCounts, int perInd)
    {
        int startInd = 0; 
        for(int i = 0; i < perInd; i++)
        {
            startInd += perCounts[i];
        }

        List<Card> c = new List<Card>();
        for (int i = startInd; i < startInd + perCounts[perInd]; i++)
        {
            c.Add(cards[i]);
        }

        return c;
    }

    bool CheckRunPatterMatch(List<Card> cards, int startInd, bool hasJocker)
    {
        int matchCount = 0;

        List<int> pattern = new List<int>();
        for(int i = 0; i < cards.Count; i++)
        {
            pattern.Add(runPattern[startInd]);
            startInd++;
        }

        for(int i = 0; i < cards.Count; i++)
        {
            for(int j = 0; j < pattern.Count; j++)
            {
                if (cards[i].value == pattern[j])
                    matchCount++;
            }
        }

        if (hasJocker)
            matchCount++;

        return matchCount == cards.Count;
    }

    bool CheckRun(List<Card> cards)
    {
        int sameSuitCount = GetSameSuitCount(cards);
        bool hasJocker = HasJocker(cards);

        int cardsCount = cards.Count;
        if (hasJocker)
            cardsCount--;

        if (sameSuitCount != cardsCount)
            return false;

        List<Card> ordered = new List<Card>(cards);
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            Card n1 = ordered[i];
            Card n2 = ordered[i + 1];
            if (n1.value > n2.value)
            {
                Card temp = ordered[i];
                ordered[i] = ordered[i + 1];
                ordered[i + 1] = temp;
                i = -1;
            }
        }

        for (int i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].value == ordered[i - 1].value)
                return false;
        }

        for (int i = 0; i < (runPattern.Count - cards.Count) + 1; i++)
        {
            if (CheckRunPatterMatch(cards, i, hasJocker))
                return true;
        }

        return false;

        /*
        int matchCount = 0;
        int startInd = 0; 
        int ind = 0;

        bool firstCardIsjocker = cards[0].suit == CardSuit.Joker;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].suit != CardSuit.Joker)
            {
                ind = i;
                break;
            }
        }
        for (int i = 0; i < pattern.Count; i++)
        {
            if (cards[ind].value == pattern[i])
            {
                startInd = i;
                break;
            }
        }

        int patternCount = pattern.Count;
        if (firstCardIsjocker)
        {
            patternCount++;
            matchCount++;
        }

        if (startInd + cards.Count > patternCount)
            return false;

        for(int i = ind; i < cards.Count; i++)
        {
            if (cards[i].value == pattern[startInd] || cards[i].suit == CardSuit.Joker)
                matchCount++;
            else
                break;

            startInd++;
        }

        return matchCount == cards.Count;
                */
    }

    bool CheckMatch(List<Card> cards)
    {
        int sameSuitCount = GetSameSuitCount(cards);
        int sameValueCount = GetSameValueCount(cards);
        bool hasJocker = HasJocker(cards);

        int cardsCount = cards.Count;
        if (hasJocker)
            cardsCount--;

        return sameSuitCount == 0 && sameValueCount == cardsCount;
    }

    int GetSameSuitCount(List<Card> cards)
    {
        int sameSuitCount;
        int[] suitsCount = new int[] { 0,0,0,0 };

        for (int i = 0; i < cards.Count; i++)
        {
            switch (cards[i].suit)
            {
                case CardSuit.Spades:
                    suitsCount[0]++;
                    break;
                case CardSuit.Hearts:
                    suitsCount[1]++;
                    break;
                case CardSuit.Diamonds:
                    suitsCount[2]++;
                    break;
                case CardSuit.Clubs:
                    suitsCount[3]++;
                    break;
            }
        }

        sameSuitCount = suitsCount[0];
        for (int i = 1; i < suitsCount.Length; i++)
            if (sameSuitCount < suitsCount[i])
                sameSuitCount = suitsCount[i];

        if (sameSuitCount == 1)
            sameSuitCount = 0;

        return sameSuitCount;
    }

    int GetSameValueCount(List<Card> cards)
    {
        int sameValueCount = 1;

        int ind = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].suit != CardSuit.Joker)
            {
                ind = i;
                break;
            }
        }
        for (int i = 0; i < cards.Count; i++)
        {
            if (i != ind && cards[ind].value == cards[i].value && cards[i].suit != CardSuit.Joker)
                sameValueCount++;
        }

        return sameValueCount;
    }

    bool HasJocker(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
            if (cards[i].suit == CardSuit.Joker)
                return true;

        return false;
    }
}

[System.Serializable]
public class PossibilityArrays
{
    public string possibilityName;
    public int[] perCounts;
}
