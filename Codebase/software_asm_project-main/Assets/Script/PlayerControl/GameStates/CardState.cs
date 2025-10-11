using UnityEngine;

public class CardState : GameBaseStates
{
    /// <summary>
    /// Initializes a new instance of the CardState class.
    /// </summary>
    /// <param name="manager">The GameStateManager instance managing the game states.</param>
    CardManager CardMan;
    public CardState(GameStateManager manager)
        : base(manager) {
        CardMan = CardManager.Instance;
    }

    /// <summary>
    /// Returns the current game state.
    /// </summary>
    /// <returns>The current game state as an enum value of GameStates.</returns>
    public override GameStates CurGameState()
    {
        return GameStates.CARD;
    }

    /// <summary>
    /// Raises the state action when entering the CardState.
    /// </summary>
    protected override void RaiseStateAction()
    {
        BoardPlaceData boardPlaceData = Controller.BoardDatas.boardDataList[Controller.CurPlayerTileGoal];
        UIcontr.BoardDetailUI.SetActive(true);
        UIcontr.BoardUIScript.SetTitleAndDesc(boardPlaceData.boardName, GameLocalization.Instance.BoardUIMessages[2]);
        UIcontr.BoardUIScript.SetButtons("Pick card", () => { PickCard(); }, true);
    }

    /// <summary>
    /// Picks a card from the current board's card list and sets it up.
    /// </summary>
    public void PickCard()
    {
        BoardPlaceData curBoardData = Controller.BoardDatas.boardDataList[Controller.CurPlayerTileGoal];
        BoardCards card = CardMan.pickCardFrom(curBoardData.cardList);
        Debug.Log($"pick card: {card.name}");
        if (card != null) CardMan.SetUpCard(card);
    }
}
