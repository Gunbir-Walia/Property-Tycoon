using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JailState : GameBaseStates
{
    public JailState(GameStateManager manager)
        : base(manager) { }

    /// <summary> Returns the current game state. </summary>
    /// <returns>The current game state, which is GameStates.DICE.</returns>
    public override GameStates CurGameState()
    {
        return GameStates.DICE;
    }

    /// <summary>
    /// Method to raise the state-specific action when the player is in jail. <br/>
    ///  - Sends the current player to jail and disables repeating the round if any. <br/>
    ///  - if the player has a jail free card, use it, otherwise pop window and provide options to pay to get out the jail or stay in jail.
    /// </summary>
    protected override void RaiseStateAction()
    {
        Controller.CurPlayerSentToJail = true;
        Controller.RepeatRound = false; // should not continue if you are arrested.
        if (!Controller.CurPlayer.HasJailFreeCard())
        {
            UIcontr.BoardUIScript.SetButtons(
                "Pay £50", () => { Controller.PayToGetOutJail(); }, 
                () => { return Controller.CurPlayer.money >= GameConstants.MoneyPaidToReleaseJail; },
                "Stay in Jail", () => { Controller.StayInJail(); });
        }
        gameMethods.MoveToJail();
    }
}
