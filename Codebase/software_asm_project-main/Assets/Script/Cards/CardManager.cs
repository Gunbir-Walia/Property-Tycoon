using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CardListType { None = 0, OpportunityKnocks = 1, PotLuck = 2 }
public class CardManager : Singleton<CardManager>
{
    public CardList _opportunityKnocks;
    public CardList _potLuck;

    List<BoardCards> _opportunityKnocksCards;
    List<BoardCards> _potLuckCards;
    GameController Controller;
    UI_Controller UIcontr;
    void OnEnable()
    {
        Controller = GameController.Instance;
        UIcontr = UI_Controller.Instance;
        _opportunityKnocksCards = new List<BoardCards>();
        _potLuckCards = new List<BoardCards>();
    }

    /// <summary>
    /// Shuffles all cards in OpportunityKnocks and PotLuck lists.
    /// </summary>
    public void SuffleAllCards()
    {
        SuffleCards(GetCardListOf(CardListType.OpportunityKnocks), _opportunityKnocksCards);
        SuffleCards(GetCardListOf(CardListType.PotLuck), _potLuckCards);
    }

    /// <summary>
    /// Picks and returns the first card from the specified shuffled card list.
    /// </summary>
    /// <param name="cardListType">The type of card list to pick from.</param>
    /// <returns>The first card from the shuffled list, or null if the list type is not recognized.</returns>
    public BoardCards pickCardFrom(CardListType cardListType)
    {
        switch (cardListType)
        {
            case CardListType.OpportunityKnocks: return PickFirstCardIn(_opportunityKnocksCards);
            case CardListType.PotLuck: return PickFirstCardIn(_potLuckCards);
        }
        return null;
    }

    /// <summary>
    /// Returns the specified card list based on its type.
    /// </summary>
    /// <param name="listType">The type of card list to retrieve.</param>
    /// <returns>The card list of the specified type, or null if the type is not recognized.</returns>
    public CardList GetCardListOf(CardListType listType)
    {
        switch (listType)
        {
            case CardListType.OpportunityKnocks: return _opportunityKnocks;
            case CardListType.PotLuck: return _potLuck;
        }
        return null;
    }

    /// <summary>
    /// Returns the shuffled list of cards based on its type.
    /// </summary>
    /// <param name="listType">The type of shuffled card list to retrieve.</param>
    /// <returns>The shuffled card list of the specified type, or null if the type is not recognized.</returns>
    public List<BoardCards> GetSuffledCardListOf(CardListType listType)
    {
        switch (listType)
        {
            case CardListType.OpportunityKnocks: return _opportunityKnocksCards;
            case CardListType.PotLuck: return _potLuckCards;
        }
        return null;
    }

    /// <summary>
    /// Sets up the card for the game controller and UI, then performs the appropriate pre-action.
    /// </summary>
    /// <param name="card">The card to set up.</param>
    public void SetUpCard(BoardCards card)
    {
        Controller.CardAction = Controller.CardActMan.SetCardAction(card.CardType);
        UIcontr.CardUI.SetActive(true);
        UIcontr.CardUIScript.ShowDetail(card.CardDesc, card.CardSprite);
        if (card.CardType == CardType.TakeOtherCard)
        {
            UIcontr.CardUIScript.PickCardPreAction();
        }
        else
        {
            UIcontr.CardUIScript.DefaultCardPreAction();
        }
        Controller.CurCard = card;
    }

    /// <summary>
    /// Shuffles the cards in the specified card list and adds them to the corresponding shuffled card list.
    /// </summary>
    /// <param name="cards">The card list to shuffle.</param>
    /// <param name="cardList">The list to which the shuffled cards will be added.</param>
    void SuffleCards(CardList cards, List<BoardCards> cardList)
    {
        // use a list of int instead of checking id of cards directly
        List<int> ID_List = new List<int>();
        HashSet<int> exclude = new HashSet<int>();
        for (int i = 0; i < cards.cardList.Count; i++)
        {
            int random = GenRandNumberExcList(0, cards.cardList.Count, exclude);
            exclude.Add(random);
            // add number to list
            ID_List.Add(random);
        }
        // add cards to list according to the List of int
        string intListShow = "cards suffled list: ";
        foreach (int i in ID_List)
        {
            cardList.Add(cards.cardList[i]);
        }
        intListShow += string.Join(", ", ID_List);
        Debug.Log(intListShow);
    }

    /// <summary>
    /// Picks and removes the first card from the specified list, and moves it to the end if it's not a JAILFREE card.
    /// </summary>
    /// <param name="cards">The list of cards to pick from.</param>
    /// <returns>The first card from the list.</returns>
    public static BoardCards PickFirstCardIn(List<BoardCards> cards)
    {
        BoardCards pickedCard = cards[0];
        cards.Remove(pickedCard);
        // make the card to the end of list
        if (pickedCard.CardType != CardType.JAILFREE)
            cards.Add(pickedCard);
        return pickedCard;
    }

    /// <summary>
    /// Generates a random number within a specified range, excluding numbers that are in the provided list.
    /// </summary>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <param name="exclude">A list of numbers to exclude from the random generation.</param>
    /// <returns>A random number within the specified range, excluding the numbers in the provided list.</returns>
    int GenRandNumberExcList(int min, int max, HashSet<int> exclude)
    {
        var range = Enumerable.Range(min, max).Where(i => !exclude.Contains(i));

        var rand = new System.Random();
        int index = rand.Next(min, max - exclude.Count);
        return range.ElementAt(index);
    }

    /// <summary>
    /// Adds a card to the PotLuck shuffled card list.
    /// </summary>
    /// <param name="card">The card to add.</param>
    public void AddCardToPotLuckCards(BoardCards card)
    {
        _potLuckCards.Add(card);
    }

    /// <summary>
    /// Adds a card to the OpportunityKnocks shuffled card list.
    /// </summary>
    /// <param name="card">The card to add.</param>
    public void AddCardToOpportunityKnocksCards(BoardCards card)
    {
        _opportunityKnocksCards.Add(card);
    }

    /// <summary>
    /// Moves a specified card to the top of the shuffled card list of the given type.
    /// </summary>
    /// <param name="card">The card to move to the top.</param>
    /// <param name="cardList">The type of card list to modify.</param>
    public void PopCardToTop(BoardCards card, CardListType cardList)
    {
        List<BoardCards> list = GetSuffledCardListOf(cardList);
        list.Remove(card);
        list.Insert(0, card);
        int[] cardIDs = new int[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            cardIDs[i] = list[i].CardID;
        }
        Debug.Log("new card list: " + string.Join(", ", cardIDs));
    }
}
