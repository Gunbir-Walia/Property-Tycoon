using Cinemachine;
using UnityEngine;

public static class CornerID
{
    public const int BottomLeft = 0;
    public const int TopLeft = 10; 
    public const int TopRight = 20;
    public const int BottomRight = 30;
}
public class GameMethods : Singleton<GameMethods>
{
    GameController Controller;
    CardManager CardMan;
    UI_Controller UIcontr;
    CameraControl camController;

    int _curPlayerTileGoal = -1;
    int _curAgentTileGoal = -1;
    bool backwardMove = false;
    bool isLookAtPlayer = false;

    delegate void camDelegate();
    camDelegate camTriggerEvent;

    const int BoardDistTooFar = 18;

    private void Start()
    {
        Controller = GameController.Instance;
        CardMan = CardManager.Instance;
        UIcontr = UI_Controller.Instance;
    }

    public void Initialize()
    {
        _curPlayerTileGoal = -1;
        _curAgentTileGoal = -1;
        backwardMove = false;
        isLookAtPlayer = false;
        FindObjectsInScene();
    }

    public void FindObjectsInScene()
    {
        camController = GameObject.FindGameObjectWithTag("Director").GetComponent<CameraControl>();
    }

    public void MoveToJail()
    {
        TeleportToTile(GameConstants.JailTileNo);
        UIcontr.BoardDetailUI.SetActive(true);
        if (Controller.CurPlayer.HasJailFreeCard())
        {
            BoardCards playerRetainCard = null;
            foreach (BoardCards card in Controller.CurPlayer.RetainCards)
            {
                if (card.CardType == CardType.JAILFREE)
                {
                    playerRetainCard = card;
                    Controller.CurPlayer.RetainCards.Remove(card);
                    break;
                }
            }
            if (playerRetainCard != null && playerRetainCard.BelongedCardList == CardListType.PotLuck)
            {
                CardMan.AddCardToPotLuckCards(playerRetainCard);
            }
            else if (playerRetainCard != null && playerRetainCard.BelongedCardList == CardListType.OpportunityKnocks)
            {
                CardMan.AddCardToOpportunityKnocksCards(playerRetainCard);
            }
            UIcontr.BoardUIScript.SetTitleAndDesc("Jail", GameLocalization.Instance.JailMessages[1]);
            UIcontr.BoardUIScript.SetButtons("OK", () => { Controller.CanFinishRound(); }, true);
        }
    }

    public void onEnteringBoard(BoardPlaceData data)
    {
        if (data.tileID == 0)
        {
            Controller.OnEnteringGo();
        }
        if (!Controller.EnteredTile)
        {
            if (_curPlayerTileGoal == data.tileID)
            {
                Controller.CurPlayer.Agent.speed = 3.5f;
                //contr.CurPlayer.Agent.isStopped = true;
                Controller.EnteredTile = true;
                Controller.OnEnteringBoard(data);
                _curAgentTileGoal = -1;
            }
            else if (_curAgentTileGoal == data.tileID)
            {
                if (backwardMove)
                {
                    SetTempGoalReverse(_curAgentTileGoal, _curPlayerTileGoal);
                }
                else
                {
                    SetTempGoal(_curAgentTileGoal, _curPlayerTileGoal);
                }
            }
        }
    }

    void PlayerMoveCamTransition()
    {
        if (!isLookAtPlayer)
        {
            isLookAtPlayer = true;
            UIcontr.DiceButton.SetActive(false);
            CinemachineVirtualCamera playerCam = Controller.CurPlayer.PlayerCamera;
            // set cam transition from current camera to player camera
            camController.SetCameraTransition(playerCam, () => {
                CamFinishTransition();
            });
            //camController.PlayCamTransition();
        }
        else
        { // camera is already on player, move player directly
            camTriggerEvent();
        }
    }

    public void RoundFinishCamTransition()
    {
        isLookAtPlayer = true;
        CinemachineVirtualCamera newPlayerCam = Controller.CurPlayer.PlayerCamera;
        // set cam transition from current camera to new player camera
        camController.SetCameraTransition(newPlayerCam, () => {
            UIcontr.DiceButton.SetActive(true);
        });
    }

    public void GoToTile(int tile)
    {// the (goal tile number < starting tile) only happens when finish a round
        Controller.EnteredTile = false;
        UIcontr.SetAllButtonsDisable();
        Controller.CurPlayer.passingGO = tile < Controller.CurPlayer.curTile;
        Debug.Log($"Current tile: {Controller.CurPlayer.curTile}, target tile: {tile}, passingGo: {Controller.CurPlayer.passingGO}");
        backwardMove = false;
        camTriggerEvent = ForwardToTile;
        _curPlayerTileGoal = tile;
        PlayerMoveCamTransition();
    }

    void ForwardToTile()
    {
        SetTempGoal(Controller.CurPlayerTileGoal, _curPlayerTileGoal);
        MovePlayer(_curPlayerTileGoal);
    }

    public void BackwardMoveToTile(int tile)
    {
        Controller.EnteredTile = false;
        UIcontr.SetAllButtonsDisable();
        backwardMove = true;
        camTriggerEvent = BackwardToTile;
        _curPlayerTileGoal = tile;
        PlayerMoveCamTransition();
    }
    void BackwardToTile()
    {
        SetTempGoalReverse(Controller.CurPlayerTileGoal, _curPlayerTileGoal);
        MovePlayer(_curPlayerTileGoal);
    }

    public void CamFinishTransition()
    {
        //Debug.Log("Finish cam transition");
        if (camTriggerEvent != null) camTriggerEvent();
    }

    void MovePlayer(int tile)
    {
        if (Controller.CurPlayerTileGoal < 0)
        {
            Controller.SetPlayerGoalTile(0); // on game starting, this value will be -1.
        }
        Controller.EnteredTile = false;
        Debug.Log($"[{Controller.CurPlayer.playerName}] is going to tile: {Controller.BoardDatas.boardDataList[tile].boardName} at tile no.{tile}");
        if (IsDistTooFar(Controller.CurPlayerTileGoal, tile))
        {
            Controller.CurPlayer.Agent.speed = 6f; //speed up player if going to tile which is far away
        } else
        {
            _curAgentTileGoal = -1;
        }
        Controller.SetPlayerGoalTile(tile);
    }

    // this function will not trigger anything else, it just sent the player to a tile
    public void TeleportToTile(int tile)
    {
        _curPlayerTileGoal = tile;
        Controller.SetPlayerGoalTile(tile);
        Vector3 targetPos = Controller.Tiles[tile].transform.position;
        targetPos.y += 1f;
        Controller.CurPlayer.gameObject.transform.position = targetPos;
        Controller.SetPlayerGoalTile(tile);
        Controller.CurPlayer.Agent.SetDestination(targetPos); // prevent player token move away
    }

    void SetTempGoal(int startID, int goalID)
    {
        // if player on left side
        if (startID < CornerID.TopLeft)
        {
            // if goal is too far away from startpoint, set the goal to BottomLeft corner, otherwise set directly to goal
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.TopLeft : goalID);
        }
        // if player on top side
        else if (startID.BetweenBoardID(CornerID.TopLeft, CornerID.TopRight-1))
        {
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.TopRight : goalID);
        }
        // if player on right side
        else if (startID.BetweenBoardID(CornerID.TopRight, CornerID.BottomRight-1))
        {
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.BottomRight : goalID);
        }
        // if player on bottom side
        else if (startID.BetweenBoardID(CornerID.BottomRight, 39/*BottomLeft Corner-1*/))
        {
            AgentSetGoal(IsDistTooFar(startID, goalID) ? 
                CornerID.BottomLeft : goalID);
        }
    }

    void SetTempGoalReverse(int startID, int goalID)
    {
        // if player on bottom side
        if (startID.BetweenBoardID(CornerID.BottomRight, 39))
        {
            // if goal is too far away from startpoint, set the goal to BottomRight corner, otherwise set directly to goal
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.TopRight : goalID);
        }
        // if player on right side
        else if (startID.BetweenBoardID(CornerID.TopRight, CornerID.BottomRight -1))
        {
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.TopLeft : goalID);
        }
        // if player on top side
        else if (startID.BetweenBoardID(CornerID.TopLeft, CornerID.TopRight -1))
        {
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.BottomLeft : goalID);
        }
        // if player on left side (usually won't need this)
        else if (startID < CornerID.TopLeft)
        {
            AgentSetGoal(IsDistTooFar(startID, goalID) ?
                CornerID.BottomRight : goalID);
        }
    }

    void AgentSetGoal(int goalID)
    {
        Controller.AgentSetDestination(goalID);
        _curAgentTileGoal = goalID;
    }

    bool IsDistTooFar(int startID, int goalID)
    {
        //Debug.Log($"Start tile: {startID}, goal tile: {goalID}");
        if (backwardMove)
        {
            return goalID.DistanceToBoard(startID) > BoardDistTooFar;
        }
        else
        {
            return startID.DistanceToBoard(goalID) > BoardDistTooFar;
        }
    }

    public int GetUtilitesRent(int landlordID)
    {
        int ownedUtilies = CountOwnedProperties(BoardType.Utility, landlordID);
        int dicePointsSum = Controller.DicePoints[0] + Controller.DicePoints[1];
        return dicePointsSum * GameConstants.UtilitesRentMultiplier[ownedUtilies];
    }

    public int GetStationRent(int landlordID)
    {
        return GameConstants.StationRent[CountOwnedProperties(BoardType.Station, landlordID)];
    }

    public int CountOwnedProperties(BoardType propertyType, int owner_id)
    {
        int count = 0;
        foreach (BoardPlaceData board in Controller.BoardDatas.boardDataList)
        {
            if (board.boardType == propertyType &&
                owner_id == board.ownerID)
            {
                count++;
            }
        }
        return count;
    }

    

}