using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public enum PlayerType { human, AI };
public class PlayerSlots : MonoBehaviour
{
    public GameObject playerSlotPrefab; // Prefab to instantiate
    public Transform playerContainer; // UI container for player slots
    public Button addPlayerButton; // Green "+" button
    public GameObject MenuButtons; // Button Group in the menu

    private int _playerCount = 0;
    public int maxHumanPlayers = 5;
    public int maxPlayers = 6; // Max 5 players (1 real + 5 AI combination)
    const float _playerSlotWidth = 350f;
    public List<PlayerConfigurations> PlayerList { get; private set; } // Track player slots
    
    PlayerConfigurator _configurator;
    PlayerConfigMenu _configMenu;
    PlayerConfigurations _curSelectPlayer;
    
    /// <summary>
    /// Initializes the PlayerSlots class with a given configurator.
    /// </summary>
    /// <param name="configurator">The PlayerConfigurator to be used for configuring players.</param>
    public void Initialize(PlayerConfigurator configurator)
    {
        PlayerList = new List<PlayerConfigurations>();
        addPlayerButton.onClick.AddListener(AddPlayerSlot);
        _configMenu = GetComponent<PlayerConfigMenu>();
        _configurator = configurator;
        for (int i = 0; i < 2; i++)
        { // 2 Players are compulsory
            AddPlayerSlot();
        }
        PlayerList[0].isAI = true;
    }

    /// <summary>
    /// Adds a new player to the player container and obey these rules:<br />
    ///     - In default, a human player will be added.<br />
    ///     - if the number of human players exceed the setting max number, the new player added will be an AI player.<br />
    ///     - Will skip if the number of players exceed the allowed max players
    /// </summary>
    public void AddPlayerSlot()
    {
        if (_playerCount < maxPlayers)
        {
            _playerCount++;
            GameObject newSlotObj = Instantiate(playerSlotPrefab, playerContainer);
            PlayerConfigurations slotScript = newSlotObj.GetComponent<PlayerConfigurations>();
            PlayerList.Add(slotScript);
            slotScript.AvatarButton.onClick.AddListener(() => SelectPlayer(slotScript));
            SetPlayerTokenToNextAvailable();

            // make the new player as AI if reached the maximum human player num
            int humanPlayerCount = CountNumPlayerType(PlayerType.human);
            Debug.Log("Player count: " + PlayerList.Count);
            slotScript.isAI = humanPlayerCount > maxHumanPlayers;
            
            // Move "+" button to the last position
            addPlayerButton.transform.SetAsLastSibling();
            SetButtonsState();

            SetContainerWidth();
            void SetPlayerTokenToNextAvailable()
            {
                int index = _configurator.GetNextAvailableToken(0);
                if (index >= 0)
                {
                    slotScript.selectedTokenIndex = index;
                    
                    _configurator.SelectToken(slotScript, index, _configurator.GetUniqueRandomName());
                }
            }
        }
    }

    /// <summary>
    /// Selects a player and sets the selector menu active.
    /// </summary>
    /// <param name="player">The player to be selected.</param>
    public void SelectPlayer(PlayerConfigurations player)
    {
        _configMenu.SetSelectorMenuActive(true);
        _curSelectPlayer = player;
        _configurator.OnSelectPlayer(player);
        SetAllPlayerSelectable(false); //prevent clicking it when selector menu opened
    }

    /// <summary>
    /// Handles the quit event of the selector menu.
    /// </summary>
    public void OnQuitSelectorMenu()
    {
        _configMenu.SetSelectorMenuActive(false);
        SetAllPlayerSelectable(true);
    }

    /// <summary>
    /// Sets the interactable state of all player avatar buttons.
    /// </summary>
    /// <param name="b">The interactable state to be set. True to enable, false to disable.</param>
    void SetAllPlayerSelectable(bool b)
    {
        foreach (PlayerConfigurations player in PlayerList)
        {
            player.AvatarButton.interactable = b;
        }
    }

    /// <summary>
    /// Adjusts the width of the player container based on the number of player slots.
    /// </summary>
    void SetContainerWidth()
    {
        playerContainer.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, _playerSlotWidth * _playerCount + 200f);
    }

    /// <summary>
    /// Removes a specified player from the player list and destroys the player slot.
    /// </summary>
    /// <param name="player">The player to be removed.</param>
    public void RemovePlayer(PlayerConfigurations player)
    {
        if (_playerCount > 2) // Ensure at least 2 players remain
        {
            PlayerList.Remove(player);
            _configurator.DeselectToken(player);
            Destroy(player.gameObject);
            _playerCount--;
            SetContainerWidth();
            SetButtonsState();
        }
    }

    /// <summary>
    /// Checks if there are no human players outside the current selection.
    /// </summary>
    /// <returns>True if no human players are outside the current selection, otherwise false.</returns>
    public bool NoHumanPlayerOutsideSelection()
    {
        if (_curSelectPlayer == null) return true;
        foreach (PlayerConfigurations thisPlayer in PlayerList)
        {
            if (thisPlayer != _curSelectPlayer && !thisPlayer.isAI)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if there is a duplicate player name in the player list.
    /// </summary>
    /// <param name="name">The name to be checked for duplicates.</param>
    /// <returns>True if the name is duplicated, otherwise false.</returns>
    public bool HasDuplicateName(string name)
    {
        if (_curSelectPlayer == null) return false;
        foreach (PlayerConfigurations thisPlayer in PlayerList)
        {
            if (thisPlayer != _curSelectPlayer && thisPlayer.PlayerName.text.Equals(name))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the current number of players.
    /// </summary>
    /// <returns>The current number of players.</returns>
    public int GetPlayerCount()
    {
        return _playerCount;
    }

    /// <summary>
    /// Counts the number of players of a specific type in the player list.
    /// </summary>
    /// <param name="playerType">The type of player to count (human or AI).</param>
    /// <returns>The number of players of the specified type.</returns>
    public int CountNumPlayerType(PlayerType playerType)
    {
        bool isAI = playerType == PlayerType.AI;
        return PlayerList.Count(player => player.isAI == isAI);
    }

    /// <summary>
    /// Sets the state of the buttons based on the current number of players.
    /// </summary>
    void SetButtonsState()
    {
        // Show "+" button again if player count drops below max
        addPlayerButton.gameObject.SetActive(_playerCount < maxPlayers);
        // cannot delele if less than 2 players remain
        _configurator.RemovePlayer.interactable = _playerCount > 2;
    }
}
