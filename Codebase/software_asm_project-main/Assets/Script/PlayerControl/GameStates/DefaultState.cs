using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultState : GameBaseStates
{
    public DefaultState(GameStateManager manager) 
        : base(manager){}

    /// <summary>
    /// Returns the current game state, which is Default.
    /// </summary>
    /// <returns>The current game state.</returns>
    public override GameStates CurGameState()
    {
        return GameStates.Default;
    }

    /// <summary>
    /// Raises the state action by invoking the RaiseBoardAction method with the current player's tile goal board place data.
    /// </summary>
    protected override void RaiseStateAction()
    {
        RaiseBoardAction(Controller.BoardDatas.boardDataList[Controller.CurPlayerTileGoal]);
    }

    /// <summary>
    /// Raises the board action based on the board type and display the UI accordingly.
    /// </summary>
    /// <param name="data">The board place data containing details about the current board.</param>
    private void RaiseBoardAction(BoardPlaceData data)
    {
        //string boardDesc = "";
        PlayerInfo curPlayer = Controller.CurPlayer;
        PlayerInfo landlord = null;

        if (data.ownerID > -1)
        {
            landlord = Controller.Players[data.ownerID];
        }
        switch (data.boardType)
        {
            case BoardType.Property:
                UIcontr.ShowPropertyUI(data);
                break;
            case BoardType.Tax:
                if (Controller.CurPlayer.HasEnoughMoney(data.moneyChange)) { 
                    UIcontr.BoardUIScript.SetTitleAndDesc(data.boardName, $"You are taxed for £{-1 * data.moneyChange} here.");
                    UIcontr.BoardUIScript.SetButtons(
                        "Pay Tax",
                        () => {
                            Controller.CurPlayer.PlayerMoneyChange(-data.moneyChange);
                            Controller.CanFinishRound();
                        },
                        () => { return Controller.CurPlayer.money >= data.moneyChange; });
                    UIcontr.BoardDetailUI.SetActive(true);
                } else
                {
                    Controller.PlayerLose(Controller.CurPlayer);
                }
                break;
            case BoardType.Utility or BoardType.Station:
                UIcontr.UtilitesUI.SetActive(true);
                UIcontr.UtilitesUIScript.SetPropertyDetail(data);
                break;
            case BoardType.Parking:
                // take all the fines on parking
                if (Controller.ParkMoney > 0)
                {
                    UIcontr.BoardUIScript.SetTitleAndDesc(data.boardName, $"You can collect £{Controller.ParkMoney} here");
                    UIcontr.BoardUIScript.SetButtons(
                        "Collect Money",
                        () => {
                            Controller.FacilityPayToPlayer(PayTarget.PARK, curPlayer, Controller.ParkMoney);
                            Controller.CanFinishRound();
                        }, true);
                }
                else
                {
                    UIcontr.BoardUIScript.SetTitleAndDesc(data.boardName, GameLocalization.Instance.BoardUIMessages[4]);
                    UIcontr.BoardUIScript.SetButtons("OK", () => { Controller.CanFinishRound(); }, true);
                }
                UIcontr.BoardDetailUI.SetActive(true);
                break;
            case BoardType.Jail:
                UIcontr.BoardUIScript.SetTitleAndDesc(data.boardName, GameLocalization.Instance.BoardUIMessages[3]);
                UIcontr.BoardUIScript.SetButtons("OK", () => { Controller.CanFinishRound(); }, true);
                UIcontr.BoardDetailUI.SetActive(true);
                break;
        }
    }
}
