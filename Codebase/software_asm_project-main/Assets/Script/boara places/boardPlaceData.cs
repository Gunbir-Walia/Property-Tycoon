using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Functions/Board Places")]
public class BoardPlaceData : ScriptableObject
{
    public string boardName;
    
    public BoardType boardType;
    public Sprite boardBG;
    [Header("Property Variables")]
    public int propertyPrice;
    //public int utilites_price;
    public List<int> rentPrice;
    public PropertyColor propertyColor;

    [Header("Tax or give money")]
    public int moneyChange;

    [HideInInspector] public int house_price;
    public int house_num = 0;
    [HideInInspector] public int ownerID = -1;
    [HideInInspector] public int tileID;
    [HideInInspector] public GameObject[] housesObject;

    [Header("Card types")]
    public CardListType cardList;
    GameController Controller { get { return GameController.Instance; } }

    private void OnEnable()
    {
        house_num = 0;
        ownerID = -1;
        if (boardType != BoardType.Property) propertyColor = PropertyColor.NONE;
        if (boardType == BoardType.Station)
        {
            propertyPrice = 200;
            rentPrice = new List<int>() { 25, 50, 100, 200 };
        }
        if (boardName.Equals("Pot Luck"))
        {
            cardList = CardListType.PotLuck;
            propertyPrice = 0;
            propertyColor = PropertyColor.NONE;
        }
        else if (boardName.Equals("Opportunity Knocks"))
        {
            cardList = CardListType.OpportunityKnocks;
            propertyPrice = 0;
            propertyColor = PropertyColor.NONE;
        }
        else
        {
            cardList = CardListType.None;
        }
        switch (propertyColor){
            case PropertyColor.BROWN:
                house_price = 50;
                break;
            case PropertyColor.BLUE:
                house_price = 50;
                break;
            case PropertyColor.PURPLE:
                house_price = 100;
                break;
            case PropertyColor.ORANGE:
                house_price = 100;
                break;
            case PropertyColor.RED:
                house_price = 150;
                break;
            case PropertyColor.YELLOW:
                house_price = 150;
                break;
            case PropertyColor.GREEN:
                house_price = 200;
                break;
            case PropertyColor.DEEPBLUE:
                house_price = 200;
                break;
            case PropertyColor.NONE:
                house_price = 0;
                break;
        }
    }

    /// <summary>
    /// Returns the price to upgrade the property.
    /// </summary>
    /// <returns>The price to upgrade the property.</returns>
    public int GetUpgradePrice() {
        if (house_num > 5) return 0;
        if (house_num == 5) return 5 * house_price;
        return house_price;
    }

    /// <summary>
    /// Returns the rent price based on the number of houses.
    /// </summary>
    /// <returns>The rent price.</returns>
    public int GetRentPrice() { return rentPrice[house_num]; }

    /// <summary>
    /// Returns the overall price of the property including all houses.
    /// </summary>
    /// <returns>The overall price of the property.</returns>
    public int GetOverallPrice()
    {
        int price = propertyPrice;
        if (house_num >= 5) price += 9 * house_price;
        else if (house_num > 0) 
            price += house_num * house_price;
        return price;
    }

    /// <summary>
    /// Returns true if the difference of houses between all properties in the set is more than 1.
    /// </summary>
    /// <returns>True if the property can improve its house, otherwise false.</returns>
    public bool CanPropertyImproveHouse()
    {
        foreach (BoardPlaceData board in Controller.BoardDatas.boardDataList)
        {
            if (board.propertyColor == propertyColor)
            {
                if (house_num < board.house_num)
                    continue;
                if(Mathf.Abs(board.house_num - (house_num + 1)) > 1)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns true if any property has built a hotel in the color set.
    /// </summary>
    /// <param name="color">The color of the property set to check. Defaults to the property's color.</param>
    /// <returns>True if any property in the set has a hotel, otherwise false.</returns>
    public bool IsAnyPropertyHasHotelInSet(PropertyColor color = PropertyColor.NONE)
    {
        if(color == PropertyColor.NONE) color = propertyColor;
        foreach (BoardPlaceData board in Controller.BoardDatas.boardDataList)
        {
            if (board.propertyColor == color && board.house_num == 5)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the landlord of the property.
    /// </summary>
    /// <returns>The PlayerInfo object of the landlord.</returns>
    public PlayerInfo Landlord()
    {
        return Controller.Players[ownerID];
    }

    /// <summary>
    /// Sells the property and removes it from the landlord's owned properties.
    /// </summary>
    public void SoldProperty()
    {
        Landlord().ownedProperties.Remove(this);
        ownerID = -1;
    }

    /// <summary>
    /// Updates the visibility of house objects based on the number of houses.
    /// </summary>
    public void OnHousesChange()
    {
        for (int i = 0; i < 5; i++)
        {
            housesObject[i].SetActive(i < house_num);
        }
    }
}
public enum BoardType
{
    Property, Utility, Tax, GO, Card, Fine, Jail, Parking, ToJail, Station
}
public enum PropertyColor
{
    BROWN = 0, BLUE = 1, PURPLE = 2, ORANGE = 3, 
    RED = 4, YELLOW = 5, GREEN = 6, DEEPBLUE = 7, NONE
}
