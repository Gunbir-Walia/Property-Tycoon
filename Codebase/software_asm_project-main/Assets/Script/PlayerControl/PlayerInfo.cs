using Cinemachine;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class PlayerInfo : MonoBehaviour
{
    /// <summary>Player ID</summary>
    public int playerID;
    /// <summary>Player's money amount</summary>
    public int money;
    /// <summary>Player's name</summary>
    public string playerName;
    /// <summary>Number of rounds finished by the player</summary>
    public int finishRounds = 0;
    /// <summary>Current tile index the player is on</summary>
    public int curTile = -1;
    /// <summary>Flag indicating if the player is passing a go tile</summary>
    public bool passingGO = false;
    /// <summary>Number of houses owned by the player</summary>
    public int ownedHouses = 0;
    /// <summary>Number of hotels owned by the player</summary>
    public int ownedHotels = 0;
    /// <summary>Rounds left in jail for the player</summary>
    public int roundsLeftInJail = 0;
    /// <summary>Model index of the player's token</summary>
    public int tokenModel = -1;
    /// <summary>Flag indicating if the player is an AI</summary>
    public bool isAI { get; private set; }
    /// <summary>List of cards retained by the player</summary>
    public List<BoardCards> RetainCards = new List<BoardCards>();

    /// <summary>Set of properties owned by the player</summary>
    public HashSet<BoardPlaceData> ownedProperties = new HashSet<BoardPlaceData>();

    NavMeshAgent _agent;
    GameController Controller { get { return GameController.Instance; } }
    public NavMeshAgent Agent {
        get
        {
            if(gameObject.scene.buildIndex != 1)
            {
                Debug.Log("Current scene is not correct, will not find the agent");
                return null;
            }
            if (_agent == null)
            {
                Debug.Log($"trying to allocate {playerName}'s agent");
                Agent = gameObject.GetComponent<NavMeshAgent>();
            }
            if (_agent == null)
            {
                Debug.LogWarning($"cannot allocate {playerName}'s agent");
            }
            return _agent;
        }
        set
        {
            _agent = value;
        }
    }
    CinemachineVirtualCamera _playerCam;
    public CinemachineVirtualCamera PlayerCamera { 
        get
        {
            if (_playerCam == null)
                _playerCam = GetComponentInChildren<CinemachineVirtualCamera>();
            if (_playerCam == null)
                Debug.LogWarning("Cannot find player's virtual camera");
            return _playerCam;

        }
        set
        {
            _playerCam = value;
        }
    }

    /// <summary>Sets up the player with initial values<br/></summary>
    /// <param name="ID">Player ID</param>
    /// <param name="tokenModel">Model index of the player's token</param>
    /// <param name="name">Player's name</param>
    /// <param name="isAI">Flag indicating if the player is an AI</param>
    /// <param name="start_money">Initial amount of money for the player</param>
    public void SetupPlayer(int ID, int tokenModel, string name, bool isAI = false, int start_money = 1500)
    {
        playerID = ID;
        money = start_money;
        playerName = name;
        this.tokenModel = tokenModel;
        PlayerCamera = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    /// <summary>Sets the number of rounds the player will stay in jail</summary>
    /// <param name="rounds">Number of rounds to stay in jail</param>
    public void StayJailForRounds (int rounds) { 
        roundsLeftInJail = rounds;
    } 

    /// <summary>Checks if the player is currently in jail</summary>
    /// <returns>True if player is in jail, otherwise false</returns>
    public bool IsInJail() { return roundsLeftInJail > 0; }

    /// <summary>Checks if the player has a jail free card<br/></summary>
    /// <returns>True if player has a jail free card, otherwise false</returns>
    public bool HasJailFreeCard()
    {
        foreach (BoardCards card in RetainCards)
        {
            if(card.CardType == CardType.JAILFREE)
                return true;
        }
        return false;
    }

    /// <summary>Saves a card to the player's retained cards list<br/></summary>
    /// <param name="card">Card to be saved</param>
    public void SaveCard(BoardCards card)
    {
        RetainCards.Add(card);
    }

    /// <summary>Uses a card of a specific type and removes it from the player's retained cards list<br/></summary>
    /// <param name="cardType">Type of card to use</param>
    /// <returns>The used card or null if no such card is found</returns>
    public BoardCards UseCard(CardType cardType)
    {
        if (RetainCards.Count > 0)
        {
            foreach (BoardCards card in RetainCards)
            {
                if (card.CardType == cardType)
                    RetainCards.Remove(card);
                return card;
            }
        }
        return null;
    }

    /// <summary>Changes the player's money amount</summary>
    /// <param name="amount">Amount of money to change (positive or negative)</param>
    public void PlayerMoneyChange(int amount)
    {
        Controller.PlayerMoneyChange(this, amount);
    }
    /// <summary>
    /// Pays a specified amount of money to another player<br/>
    ///  - if the player will be bankruptcy if he make the payment, sell all of his property and give all of the money he can have to the payer<br/>
    /// </summary>
    /// <param name="receiver">The player receiving the payment</param>
    /// <param name="amount">Amount of money to pay</param>
    public void PayToPlayer(PlayerInfo receiver, int amount)
    {
        if (HasEnoughMoney(amount))
        {// if player dont have enough money to pay, sell all the property and give all money to receiver
            Controller.PlayerMoneyChange(receiver, GetOverallDeposit());
            Controller.PlayerLose(this);
        }
        else
        {
            Controller.PlayerMoneyChange(receiver, amount);
            Controller.PlayerMoneyChange(this, -1 * amount);
        }
    }
    /// <summary>Pays a specified amount of money to a facility<br/></summary>
    /// <param name="receiver">The facility receiving the payment</param>
    /// <param name="amount">Amount of money to pay</param>
    public void PayToFacility(PayTarget receiver, int amount)
    {
        Controller.PlayerMoneyChange(this, -1 * amount);
        switch (receiver)
        {
            case PayTarget.PARK:
                Controller.ParkMoney += amount;
                return;
            case PayTarget.BANK:
                //BankMoney += amount; // bank has infinate money
                return;
        }
    }

    /// <summary>Calculates the total deposit value of the player including money and properties</summary>
    /// <returns>The total deposit value of the player</returns>
    public int GetOverallDeposit()
    {
        int sumMoney = money;
        foreach (BoardPlaceData board in ownedProperties)
        {
            if (board.ownerID == playerID)
            {
                sumMoney += board.GetOverallPrice();
            }
        }
        return sumMoney;
    }

    /// <summary>
    /// Checks if the player has enough money to pay a specified amount
    /// </summary>
    /// <param name="moneyDecrease">Amount of money needed</param>
    /// <returns>True if player has enough money, otherwise false</returns>
    public bool HasEnoughMoney(int moneyDecrease)
    {
        if(GetOverallDeposit() > moneyDecrease) return true;
        return false;
    }

    /// <summary>Checks if the player owns all properties of a specific color set</summary>
    /// <param name="color">The color set of the properties to check</param>
    /// <returns>True if player owns all properties of the specified color set, otherwise false</returns>
    public bool IsPlayerOwnsAllPropertyInColor(PropertyColor color)
    {
        foreach (BoardPlaceData board in Controller.BoardDatas.boardDataList)
        {
            if (board.ownerID != playerID && board.propertyColor == color)
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>Counts the number of properties owned by the player of a specific type</summary>
    /// <param name="propertyType">The type of properties to count</param>
    /// <returns>The count of owned properties of the specified type</returns>
    public int CountOwnedProperties(BoardType propertyType)
    {
        int count = 0;
        foreach (BoardPlaceData board in Controller.BoardDatas.boardDataList)
        {
            if (board.boardType == propertyType &&
                playerID == board.ownerID)
            {
                count++;
            }
        }
        return count;
    }
    /// <summary>Checks if the player can receive rent</summary>
    /// <returns>True if player is not in jail, otherwise false</returns>
    public bool CanReceiveRent()
    {
        return !IsInJail();
    }

    /// <summary>Checks if the player owns any property</summary>
    /// <returns>True if player owns any property, otherwise false</returns>
    public bool OwnedAnyProperty()
    {
        foreach (BoardPlaceData board in Controller.BoardDatas.boardDataList)
        {
            if (board.ownerID == playerID)
            {
                return true;
            }
        }
        return false;
    }
    
}

