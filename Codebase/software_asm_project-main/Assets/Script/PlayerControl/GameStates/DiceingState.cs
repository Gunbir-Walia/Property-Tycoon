using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceingState : GameBaseStates
{
    int _throwDoubleTimes = 0;
    public DiceingState(GameStateManager manager) 
        : base(manager){ }

    /// <summary>
    /// Returns the current game state.
    /// </summary>
    /// <returns>The current game state, which is DICE.</returns>
    public override GameStates CurGameState()
    {
        return GameStates.DICE;
    }

    /// <summary>
    /// Raises the state action
    /// </summary>
    protected override void RaiseStateAction()
    {
        ThrowDice();
    }

    /// <summary>
    /// Simulates the dice throw for the current player. <br/>
    ///  - if the player throws double dice, repeat round, if throw again for three times, go to jail. <br/>
    /// </summary>
    public void ThrowDice()
    {
        // to pervent triggering event on "GO" at start
        if (Controller.CurPlayerTileGoal < 0)
            Controller.SetPlayerGoalTile(0);
        if (Controller.DicePoints[0] == Controller.DicePoints[1])
        {
            //if player throw double dice, repeat round, if throw again, go to jail
            Controller.RepeatRound = true;
            _throwDoubleTimes++;
            if(_throwDoubleTimes >= 3)
            {
                Controller.RepeatRound = false;
                _throwDoubleTimes = 0;
                if (!Controller.CurPlayer.HasJailFreeCard())
                {
                    UIcontr.BoardUIScript.SetTitleAndDesc("Jail", GameLocalization.Instance.JailMessages[2]);
                }
                Controller.EnterJail();
                return;
            }
        }
        else
        {
            Controller.RepeatRound = false;
            _throwDoubleTimes = 0;
        }

        int _curPlayerTile = Controller.CurPlayer.curTile + Controller.DicePoints[0] + Controller.DicePoints[1];
        if (_curPlayerTile >= 40)
        {
            _curPlayerTile -= 40;
            Controller.CurPlayer.finishRounds++;
        }
        gameMethods.GoToTile(_curPlayerTile);
    }

    
}
