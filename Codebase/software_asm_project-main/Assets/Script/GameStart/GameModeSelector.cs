using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameModeSelector : MonoBehaviour
{
    public int MinTimeLimit = 10;
    public int DefaultTimeLimit = 30;
    public int MaxTimeLimit = 9999;

    public TMP_InputField timeInput;
    public StartMenuManager manager;
    public GameObject gameModeSelectorParent;
    public GameObject playerSelectorParent;

    void Start()
    {
        timeInput.onValueChanged.AddListener(OnValueChanged);
        timeInput.onEndEdit.AddListener(OnEndEdit);
        ResetTime();
    }

    /// <summary>
    /// Handles the value change event of the time input field.
    /// Removes any non-digit characters and moves the caret to the end of the input field.
    /// </summary>
    /// <param name="value">The current value of the input field.</param>
    void OnValueChanged(string value)
    {
        //int maxBiddingAmount = 1000;
        if (!int.TryParse(value, out int result))
        {
            // remove char where is not numbers
            timeInput.text = new string(timeInput.text.Where(char.IsDigit).ToArray());
            timeInput.caretPosition = timeInput.text.Length; // move to end of input field
        }
    }

    /// <summary>
    /// Handles the end edit event of the time input field.
    /// Resets the time if the input is not a valid number or outside the allowed range.
    /// </summary>
    /// <param name="value">The final value entered in the input field.</param>
    void OnEndEdit(string value)
    {
        if (!int.TryParse(value, out int result))
            return;
        if (timeInput.text.Trim() == "" || !IsTimeVaild(result))
        {
            ResetTime();
        }
    }

    /// <summary>
    /// Checks if the provided time is within the valid range.
    /// </summary>
    /// <param name="time">The time value to check.</param>
    /// <returns>True if the time is within the range [MinTimeLimit, MaxTimeLimit], false otherwise.</returns>
    bool IsTimeVaild(int time)
    {
        return time >= MinTimeLimit && time <= MaxTimeLimit;
    }

    /// <summary>
    /// Resets the time input field to its default value and moves the caret to the end of the input field.
    /// </summary>
    void ResetTime()
    {
        timeInput.text = DefaultTimeLimit.ToString();
        timeInput.caretPosition = timeInput.text.Length; // move to end of input field
    }

    /// <summary>
    /// Handles the selection of the abridged game mode.
    /// Parses the time input, notifies the manager, and switches to the player selector.
    /// </summary>
    public void OnAbridgedModeSelected()
    {
        if (!int.TryParse(timeInput.text, out int result))
            return;
        manager.OnSelectAbridgedGame(result);
        gameModeSelectorParent.SetActive(false);
        playerSelectorParent.SetActive(true);
    }
}
