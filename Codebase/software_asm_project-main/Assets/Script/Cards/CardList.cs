using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardList", menuName = "Game Functions/Card List")]
public class CardList : ScriptableObject
{
    [Space(10)]
    public CardListType ListType;
    [Space(10)]
    public List<BoardCards> cardList = new List<BoardCards>();

    private void Awake()
    {
        foreach (BoardCards card in cardList)
        {
            card.BelongedCardList = ListType;
        }
    }
}
