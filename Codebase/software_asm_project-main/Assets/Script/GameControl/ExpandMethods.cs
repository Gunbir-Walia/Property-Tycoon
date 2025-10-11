using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public static class ExpandIntMethod
{
    /// <summary>
    /// Determines if the integer value is between the specified min and max board IDs, considering a circular board with 40 IDs.
    /// </summary>
    /// <param name="x">The integer value to check.</param>
    /// <param name="min">The minimum board ID.</param>
    /// <param name="max">The maximum board ID.</param>
    /// <returns>True if x is between min and max, otherwise false.</returns>
    public static bool BetweenBoardID(this int x, int min, int max)
    {
        if (min > max)
        {
            max += 40;
        }
        return x >= min && x <= max;
    }

    /// <summary>
    /// Calculates the distance between two board IDs on a circular board with 40 IDs.
    /// </summary>
    /// <param name="start">The starting board ID.</param>
    /// <param name="end">The ending board ID.</param>
    /// <returns>The distance from start to end.</returns>
    public static int DistanceToBoard(this int start, int end)
    {
        if (start > end)
        {
            end += 40;
        }
        return end - start;
    }
}

/// <summary>
/// Provides extension methods for the Button class in Unity.
/// </summary>
public static class ExpandButtonMethods
{
    /// <summary>
    /// Adds a single action to the button's onClick event and removes any existing listeners.
    /// </summary>
    /// <param name="button">The button to add the action to.</param>
    /// <param name="action">The action to perform when the button is clicked.</param>
    public static void AddSingleEventToOnClick(this Button button, System.Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { action(); });
    }

    /// <summary>
    /// Sets up a button with a title, action, interactability, and visibility.
    /// </summary>
    /// <param name="button">The button to set up.</param>
    /// <param name="buttonName">The title to display on the button.</param>
    /// <param name="action">The action to perform when the button is clicked.</param>
    /// <param name="buttonInteractable">Determines if the button is interactable. Default is true.</param>
    /// <param name="showButton">Determines if the button is visible. Default is true.</param>
    public static void SetupButton(this Button button, string buttonName, 
        System.Action action, bool buttonInteractable = true, bool showButton = true)
    {
        button.gameObject.SetActive(showButton);
        if (showButton == false) return;
        button.SetButtonTitle(buttonName);
        if (action != null)
        {
            button.AddSingleEventToOnClick(() => { action(); });
            button.interactable = buttonInteractable;
        }
    }

    /// <summary>
    /// Sets the title of the button and adjusts its padding.
    /// </summary>
    /// <param name="button">The button to set the title for.</param>
    /// <param name="title">The title to set.</param>
    /// <param name="padding">The padding to apply around the text. Default is 5f.</param>
    public static void SetButtonTitle(this Button button, string title, float padding = 5f)
    {
        TMP_Text buttonTitle = button.GetComponentInChildren<TMP_Text>();
        buttonTitle.text = title;
        button.SetButtonPadding(padding);
    }

    /// <summary>
    /// Adjusts the width of the button based on its text width and a specified padding.
    /// </summary>
    /// <param name="button">The button to adjust the width for.</param>
    /// <param name="padding">The padding to apply around the text. Default is 5f.</param>
    public static void SetButtonPadding(this Button button, float padding = 5f)
    {
        float textWidth = button.GetComponentInChildren<TMP_Text>().preferredWidth;

        // calculate button width with padding
        float buttonWidth = textWidth + padding;
        // set button width
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonWidth);
    }
}
