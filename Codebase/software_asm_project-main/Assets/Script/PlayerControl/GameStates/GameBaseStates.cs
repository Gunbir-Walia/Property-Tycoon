using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameBaseStates
{
    //bool _isRootState = false;
    //GameBaseStates _curSubState;

    // getter and setter
    //protected bool IsRootState { get { return _isRootState; } set { _isRootState = value; } }
    //public GameBaseStates SubState { get { return _curSubState; } }
    protected GameBaseStates SuperState { get; set; }
    protected GameController Controller { get; private set; }
    protected GameMethods gameMethods { get; private set; }
    protected GameStateManager StateMan { get; private set; }
    protected UI_Controller UIcontr { get; private set; }

    /// <summary> Initializes a new instance of the GameBaseStates class. </summary>
    /// <param name="manager">The GameStateManager instance associated with this state.</param>
    public GameBaseStates(GameStateManager manager)
    {
        StateMan = manager;
        Controller = GameController.Instance;
        gameMethods = GameMethods.Instance;
        UIcontr = UI_Controller.Instance;
    }

    public virtual void EnterState() { }

    /// <summary> Returns the current game state. </summary>
    /// <returns>The current game state as an enum value of GameStates.</returns>
    public abstract GameStates CurGameState();

    /// <summary>
    /// Raises the state-specific action.
    /// </summary>
    protected abstract void RaiseStateAction();

    //protected virtual void InitializeSubStates() { }
    protected virtual void PreActions() { }
    protected virtual void ExitState() { }

    /// <summary>
    /// Raises state events by calling PreActions and RaiseEvents.
    /// </summary>
    public void RaiseStateEvents()
    {
        PreActions();
        //_curSubState?.PreActions();

        RaiseEvents();
        //_curSubState?.RaiseEvents();
    }

    /// <summary> Switches the current state to a new state. </summary>
    /// <param name="newState">The new state to switch to.</param>
    public void SwitchState(GameBaseStates newState)
    {
        ExitState();
        //newState.InitializeSubStates();
        newState.EnterState();
        Controller.CurState = newState;

        //if (IsRootState)
        //{
        //    //Controller.CurState = newState;
        //}
        //else if (SuperState != null)
        //{
        //    SuperState.SetSubState(newState);
        //}
    }

    //protected void SetSubState(GameBaseStates newSubState)
    //{
    //    _curSubState = newSubState;
    //    newSubState.SuperState = this;
    //}

    /// <summary>
    /// Raises events by calling RaiseStateAction.
    /// </summary>
    protected void RaiseEvents()
    {
        RaiseStateAction();
    }
}
