using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardActions
{
    protected GameController Controller { get { return GameController.Instance; } }
    protected GameMethods GameMethod { get { return GameMethods.Instance; } }
    protected UI_Controller UIcontr { get { return UI_Controller.Instance; } }

    public CardActions() { }
    /// <summary>
    /// Executes the actions associated with the given card.
    /// </summary>
    /// <param name="pickedCard">The card to execute actions for.</param>
    public abstract void ExecuteCardActions(BoardCards pickedCard);
}

public class CardActionManager
{
    public Dictionary<CardType, CardActions> _actionTypes = new Dictionary<CardType, CardActions>();

    public CardActionManager()
    {
        _actionTypes[CardType.MOVE] = new CardActionMove();
        _actionTypes[CardType.PAY] = new CardActionPay();
        _actionTypes[CardType.JAILFREE] = new CardActionJailFree();
        _actionTypes[CardType.REPAIR] = new CardActionRepair();
        _actionTypes[CardType.TakeOtherCard] = new CardActionTakeOtherCard();
    }

    /// <summary>
    /// Sets the card action for a specific card type.
    /// </summary>
    /// <param name="cardType">The type of the card to set the action for.</param>
    /// <returns>The card action associated with the given card type.</returns>
    public CardActions SetCardAction(CardType cardType)
    {
        return _actionTypes[cardType];
    }
}

public class CardActionMove : CardActions
{
    public CardActionMove() { }
    /// <summary>
    /// Executes the movement actions based on the card's move type.
    /// </summary>
    /// <param name="pickedCard">The card containing the movement instructions.</param>
    public override void ExecuteCardActions(BoardCards pickedCard)
    {
        int curTile = Controller.CurPlayerTileGoal;
        switch (pickedCard.MoveType)
        {
            case PlayerMoveType.FORWARD:
                if (pickedCard.IsMoveFixStep)
                {
                    int targetTile = 0;
                    if (curTile + pickedCard.MoveSteps > 39)
                        targetTile -= 39;
                    targetTile += curTile + pickedCard.MoveSteps;
                    GameMethod.GoToTile(targetTile);
                }
                else
                {
                    GameMethod.GoToTile(pickedCard.MoveToBoard.tileID);
                }
                break;
            case PlayerMoveType.BACKWARD:
                if (pickedCard.IsMoveFixStep)
                {
                    int targetTile = 0;
                    if (curTile < pickedCard.MoveSteps)
                        targetTile = 40;
                    targetTile += curTile - pickedCard.MoveSteps;
                    Debug.Log($"{nameof(curTile)}: {curTile}; MoveSteps: {pickedCard.MoveSteps}; reverse move to {targetTile}");
                    GameMethod.BackwardMoveToTile(targetTile);
                }
                else
                {
                    GameMethod.BackwardMoveToTile(pickedCard.MoveToBoard.tileID);
                }
                break;
            case PlayerMoveType.TELEPORT:
                if (pickedCard.MoveToBoard.tileID == GameConstants.JailTileNo)
                {
                    if (!Controller.CurPlayer.HasJailFreeCard())
                        UIcontr.BoardUIScript.SetTitleAndDesc("Jail", GameLocalization.Instance.JailMessages[0]);
                    //GotoJail
                    Controller.EnterJail();
                }
                else
                    GameMethod.TeleportToTile(pickedCard.MoveToBoard.tileID);
                break;
        }
        
    }
}

public class CardActionPay : CardActions
{
    public CardActionPay(){ }
    /// <summary>
    /// Executes the payment actions based on the card's payment instructions.
    /// </summary>
    /// <param name="pickedCard">The card containing the payment instructions.</param>
    public override void ExecuteCardActions(BoardCards pickedCard)
    {
        CardActionPayMoney(pickedCard.PayFrom, pickedCard.MoneyPaid * -1);
        if(pickedCard.PayFrom != PayTarget.ALLPLAYER)
            CardActionPayMoney(pickedCard.PayTo, pickedCard.MoneyPaid);
        else /* if all other players gives money to current player,
             current player should receive a total of paid money from other * player numbers -1 (exclude receiver himself) */
            CardActionPayMoney(pickedCard.PayTo, pickedCard.MoneyPaid * (Controller.Players.Count -1));
        Controller.CanFinishRound();
    }
    /// <summary>
    /// Handles the payment of money from one target to another.
    /// </summary>
    /// <param name="payer">The target from which money is paid.</param>
    /// <param name="amount">The amount of money to be paid.</param>
    void CardActionPayMoney(PayTarget payer, int amount)
    {
        switch (payer)
        {
            case PayTarget.BANK:
                //Controller.BankMoney += amount; //bank has infinate money
                break;
            case PayTarget.PLAYER:
                Controller.CurPlayer.PlayerMoneyChange(amount);
                break;
            case PayTarget.ALLPLAYER:
                foreach (var player in Controller.Players)
                {
                    if (player != Controller.CurPlayer)
                    {
                        player.PayToPlayer(Controller.CurPlayer, amount);
                    }
                }
                break;
            case PayTarget.PARK:
                Controller.ParkMoney += amount;
                break;
        }
    }
}

public class CardActionJailFree : CardActions
{
    public CardActionJailFree(){ }
    /// <summary>
    /// Retains a jail free card for the current player and allows the player to finish the round.
    /// </summary>
    /// <param name="pickedCard">The jail free card to retain.</param>
    public override void ExecuteCardActions(BoardCards pickedCard)
    {
        Controller.CurPlayer.RetainCards.Add(pickedCard);
        Controller.CanFinishRound();
    }
}

public class CardActionRepair : CardActions
{
    public CardActionRepair(){ }
    /// <summary>
    /// Fines on the cureent player base on the number of owned houses and hotels.
    /// </summary>
    /// <param name="pickedCard">The card containing the repair instructions.</param>
    public override void ExecuteCardActions(BoardCards pickedCard)
    {
        PlayerInfo player = Controller.CurPlayer;
        player.PlayerMoneyChange(
            -1* (pickedCard.HouseRepairPrice * player.ownedHouses + pickedCard.HotelRepairPrice * player.ownedHotels));
        Controller.CanFinishRound();
    }
}

public class CardActionTakeOtherCard : CardActions
{
    CardManager CardMan { get { return CardManager.Instance; } }
    public CardActionTakeOtherCard() { }
    /// <summary>
    /// Executes the action of taking another card from a specific list.
    /// </summary>
    /// <param name="pickedCard">The card that triggered the action of taking another card.</param>
    public override void ExecuteCardActions(BoardCards pickedCard)
    {
        BoardCards card = CardMan.pickCardFrom(CardListType.OpportunityKnocks);
        if (card != null) CardMan.SetUpCard(card);
    }
}
