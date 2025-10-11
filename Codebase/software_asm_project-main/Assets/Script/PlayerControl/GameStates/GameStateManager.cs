using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameStateManager
{
    Dictionary<GameStates, GameBaseStates> _states = new Dictionary<GameStates, GameBaseStates>();
    GameController Controller { get { return GameController.Instance; } }

    public GameStateManager()
    {
        _states[GameStates.DICE] = new DiceingState(this);
        _states[GameStates.Default] = new DefaultState(this);
        _states[GameStates.CARD] = new CardState(this);
        _states[GameStates.JAIL] = new JailState(this);
    }

    /// <summary>Gets the DiceingState.</summary>
    /// <returns>The DiceingState instance.</returns>
    public GameBaseStates Diceing() { return _states[GameStates.DICE]; }

    /// <summary>Gets the DefaultState.</summary>
    /// <returns>The DefaultState instance.</returns>
    public GameBaseStates Default() { return _states[GameStates.Default]; }

    /// <summary>Gets the CardState. </summary>
    /// <returns>The CardState instance.</returns>
    public GameBaseStates CARD() { return _states[GameStates.CARD]; }

    /// <summary>Gets the JailState.</summary>
    /// <returns>The JailState instance.</returns>
    public GameBaseStates JAIL() { return _states[GameStates.JAIL]; }

    /// <summary>Retrieves the GameBaseStates instance based on the provided GameStates.</summary>
    /// <param name="state">The state to retrieve.</param>
    /// <returns>The GameBaseStates instance corresponding to the provided GameStates.</returns>
    public GameBaseStates GetGameState(GameStates state) { return _states[state]; }

    /// <summary>Retrieves the GameBaseStates instance based on the provided BoardType.</summary>
    /// <param name="type">The type of board to determine the state.</param>
    /// <returns>The GameBaseStates instance corresponding to the provided BoardType.</returns>
    public GameBaseStates GetGameStateByBoardType(BoardType type)
    {
        switch (type)
        {
            case BoardType.Card:
                return _states[GameStates.CARD];
            case BoardType.ToJail:
                return _states[GameStates.JAIL];
            default:
                return _states[GameStates.Default];
        }
    }
}
public enum GameStates { DICE, Default, PROPERTY, JAIL, CARD }
