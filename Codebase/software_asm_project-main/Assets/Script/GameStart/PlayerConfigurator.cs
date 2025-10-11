using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Linq;
public class PlayerConfigurator : MonoBehaviour
{
    public Toggle aiToggle;
    public TMP_Text aiName;
    public GameObject nameTag;
    public TMP_InputField nameInput;
    public GameObject tokenControls;
    public RawImage tokenImage;
    public List<Sprite> tokenSprites;
    public Button RemovePlayer;
    public Button ConfirmPlayer;
    public PlayerSlots playerSlots;
    public GameObject NameWarningSign;
    public RandomNames randomNames;

    private int curTokenIndex = 0;
    //private int selectedTokenIndex = -1; // Track currently selected token
    public static PlayerConfigurations CurSelectPlayer;
    private HashSet<int> _selectedTokens = new HashSet<int>(); // Store selected tokens

    void Awake()
    {
        aiToggle.onValueChanged.AddListener(OnAIOptionToggled);
        nameInput.onEndEdit.AddListener(OnInputEndEdit);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickExit();
        }
    }

    #region Methods trigger by buttons
    /// <summary>
    /// Confirms the player's configuration and adds them to the game.
    /// </summary>
    public void OnClickConfirm()
    {
        string playerName = nameInput.text;
        if (playerName.Trim().Equals(""))
        {
            playerName = GetUniqueRandomName();
        }
        SelectToken(CurSelectPlayer, curTokenIndex, playerName);
        CurSelectPlayer = null;
        playerSlots.OnQuitSelectorMenu();
    }

    /// <summary>
    /// Removes the currently selected player from the game.
    /// </summary>
    public void OnClickRemovePlayer()
    {
        DeselectToken(CurSelectPlayer);
        playerSlots.RemovePlayer(CurSelectPlayer);
        playerSlots.OnQuitSelectorMenu();
    }
    
    /// <summary>
    /// Handles the selection of a player in the setup screen.
    /// </summary>
    /// <param name="player">The player configuration to select.</param>
    public void OnSelectPlayer(PlayerConfigurations player)
    {
        CurSelectPlayer = player;
        DisplayToken(player.selectedTokenIndex);
        DeselectToken(CurSelectPlayer); // Free token selected by this player
        nameInput.text = player.PlayerName.text;
        for (int i = 0; i< tokenControls.transform.childCount; i++)
        {
            tokenControls.transform.GetChild(i).GetComponent<Button>().interactable 
                = playerSlots.GetPlayerCount() < tokenSprites.Count;
        }
        // prevent setting all player as AI
        aiToggle.isOn = CurSelectPlayer.isAI;
        
        bool canToggleAI = true;
        // At least one human player
        if (OnlyOneAndSelectedPlayerType(PlayerType.human))
            canToggleAI = false;
        // At least one AI if 6 players
        if (playerSlots.PlayerList.Count == playerSlots.maxPlayers &&
            OnlyOneAndSelectedPlayerType(PlayerType.AI))
            canToggleAI = false;
        aiToggle.interactable = canToggleAI;
        NameWarningSign.SetActive(false);
        Debug.Log($"Select {player.PlayerName.text}, curToken: {curTokenIndex}");

        bool OnlyOneAndSelectedPlayerType(PlayerType playerType)
        {
            bool isAI = playerType == PlayerType.AI;
            return playerSlots.CountNumPlayerType(playerType) == 1 && CurSelectPlayer.isAI == isAI;
        }
    }

    /// <summary>
    /// Selects the next available token for the player.
    /// </summary>
    public void OnClickNextToken()
    {
        int index = GetNextAvailableToken(GetNextTokenIndex(curTokenIndex));
        if (index >= 0)
        {
            DisplayToken(index);
        }
    }

    /// <summary>
    /// Selects the previous available token for the player.
    /// </summary>
    public void OnClickPreviousToken()
    {
        int index = GetPreviousAvailableToken(GetPreviousTokenIndex(curTokenIndex));
        if (index >= 0)
        {
            DisplayToken(index);
        }
    }

    /// <summary>
    /// Handles the exit of the player configuration menu.
    /// </summary>
    public void OnClickExit()
    {
        // add the player current selected token back to list and change nothing
        if (IsTokenAvailable(CurSelectPlayer.selectedTokenIndex))
            _selectedTokens.Add(CurSelectPlayer.selectedTokenIndex);
        curTokenIndex = -1;
        CurSelectPlayer = null;
        playerSlots.OnQuitSelectorMenu();
    }

    /// <summary>
    /// Generates a random unique name for the player.
    /// </summary>
    public void OnClickGenRandomName()
    {
        nameInput.text = GetUniqueRandomName();
    }

    /// <summary>
    /// Toggles the AI option for the player.
    /// </summary>
    /// <param name="b">Boolean indicating the new state of the AI toggle.</param>
    public void OnAIOptionToggled(bool b)
    {
        Debug.Log($"is AI: {b}");
        //if (b)
        //{
        //    aiToggle.isOn = b;
        //}
        //else if (CountAIPlayers(true) == 0)
        //{
        //    aiToggle.isOn = true;
        //}
    }

    /// <summary>
    /// Handles the end of text editing in the name input field.
    /// </summary>
    /// <param name="input">The entered player name.</param>
    void OnInputEndEdit(string input)
    {
        bool hasDuplicateName = playerSlots.HasDuplicateName(input);
        NameWarningSign.SetActive(hasDuplicateName);
        ConfirmPlayer.interactable = !hasDuplicateName;
    }

    #endregion

    /// <summary>
    /// Enables or disables the UI based on the provided boolean.
    /// </summary>
    /// <param name="isEnabled">Boolean indicating if the UI should be enabled.</param>
    public void EnableUI(bool isEnabled)
    {
        nameInput.interactable = !isEnabled;
        if(isEnabled == false)
        {
            nameInput.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Finds the next available token index after the given index.
    /// </summary>
    /// <param name="index">The starting token index.</param>
    /// <returns>The next available token index or -1 if none is available.</returns>
    public int GetNextAvailableToken(int index)
    {
        if (_selectedTokens.Count == tokenSprites.Count) 
            return -1; // no available token, usually should not happen;

        for (int i = 0; i < tokenSprites.Count; i++)
        {
            if (IsTokenAvailable(index))
                return index;
            else
                index = GetNextTokenIndex(index);
        }

        return -1; // cannot find any available token, usually should not happen;
    }

    /// <summary>
    /// Finds the previous available token index before the given index.
    /// </summary>
    /// <param name="index">The starting token index.</param>
    /// <returns>The previous available token index or -1 if none is available.</returns>
    public int GetPreviousAvailableToken(int index)
    {
        if (_selectedTokens.Count == tokenSprites.Count)
            return -1; // no available token, usually should not happen;

        for (int i = 0; i < tokenSprites.Count; i++)
        {
            if (IsTokenAvailable(index))
                return index;
            else
                index = GetPreviousTokenIndex(index);
        }

        return -1;// cannot find any available token, usually should not happen;
    }

    /// <summary>
    /// Calculates the next token index in a circular manner.
    /// </summary>
    /// <param name="index">The current token index.</param>
    /// <returns>The next token index.</returns>
    int GetNextTokenIndex(int index)
    {
        return (index + 1) % tokenSprites.Count;
    }

    /// <summary>
    /// Calculates the previous token index in a circular manner.
    /// </summary>
    /// <param name="index">The current token index.</param>
    /// <returns>The previous token index.</returns>
    int GetPreviousTokenIndex(int index) 
    { 
        return (index - 1 + tokenSprites.Count) % tokenSprites.Count; 
    }

    /// <summary>
    /// Displays the token at the specified index.
    /// </summary>
    /// <param name="index">The index of the token to display.</param>
    private void DisplayToken(int index)
    {
        if (tokenSprites.Count == 0) return;
        curTokenIndex = index;
        tokenImage.texture = tokenSprites[index].texture;
    }

    /// <summary>
    /// Checks if a token at the specified index is available for selection.
    /// </summary>
    /// <param name="tokenIndex">The index of the token to check.</param>
    /// <returns>Boolean indicating if the token is available.</returns>
    bool IsTokenAvailable(int tokenIndex)
    {
        if (tokenSprites == null || tokenSprites.Count == 0 || 
            tokenIndex < 0 || tokenIndex >= tokenSprites.Count) 
            return false;

        //print all contents in _selectedTokens to Debug.Log
        //string debug = "Selected Tokens: ";
        //foreach (var token in _selectedTokens)
        //    debug += token.ToString() + ",";
        //Debug.Log(debug);

        return !_selectedTokens.Contains(tokenIndex);
    }

    /// <summary>
    /// Selects a token for the player and updates the player's configuration.
    /// </summary>
    /// <param name="player">The player configuration to update.</param>
    /// <param name="tokenIndex">The index of the token to select.</param>
    /// <param name="playerName">The name to assign the player.</param>
    public void SelectToken(PlayerConfigurations player, int tokenIndex, string playerName)
    {
        if (IsTokenAvailable(tokenIndex))// just double check
        {
            Debug.Log($"{playerName} select token {tokenIndex}");
            player.selectedTokenIndex = tokenIndex;
            player.SetImage(tokenSprites[tokenIndex]);
            player.SetPlayerName(playerName);
            player.isAI = aiToggle.isOn;
            _selectedTokens.Add(tokenIndex);
        }
    }

    /// <summary>
    /// Deselects the token currently selected by the player.
    /// </summary>
    /// <param name="player">The player configuration to update.</param>
    public void DeselectToken(PlayerConfigurations player)
    {
        int tokenIndex = player.selectedTokenIndex;
        if (tokenIndex >= 0 && _selectedTokens.Contains(tokenIndex))
        {
            _selectedTokens.Remove(tokenIndex);
        }
        //player.selectedTokenIndex = -1;
    }

    /// <summary>
    /// Generates a unique random name for the player.
    /// </summary>
    /// <returns>A unique random name.</returns>
    public string GetUniqueRandomName()
    {
        while (true)
        {
            string randomName = randomNames.GetRandomName();
            if (!playerSlots.HasDuplicateName(randomName)) return randomName;
        }
    }
}
