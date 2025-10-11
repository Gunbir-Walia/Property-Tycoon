using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotifyPopupWindow : MonoBehaviour
{
    public TMP_Text popupText;
    public Button _firstButton;
    public Button _secondButton;
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }
    private System.Func<bool> _firstButtonCondition;
    private System.Func<bool> _secondButtonCondition;

    private void OnEnable()
    {
        if (UIcontroller != null)
            UIcontroller.OnAnyUIEnabled();
    }
    private void OnDisable()
    {
        if (UIcontroller.gameObject != null)
            UIcontroller.OnAnyUIDisabled();
        _firstButtonCondition = null;
        _secondButtonCondition = null;
    }

    private void Update()
    {
        // Update the button interactable when the button is active and the corresponding condition is set.
        if (gameObject.activeSelf && _firstButtonCondition != null)
            _firstButton.interactable = _firstButtonCondition();
        if (gameObject.activeSelf && _secondButtonCondition != null)
            _secondButton.interactable = _secondButtonCondition();
    }

    /// <summary>
    /// Pops up the notification window with specified text and button configurations.
    /// </summary>
    /// <param name="text">The text to display in the popup.</param>
    /// <param name="firstButtonCallback">The callback to invoke when the first button is clicked. Defaults to null.</param>
    /// <param name="firstbuttonName">The name of the first button. Defaults to "OK".</param>
    /// <param name="firstButtonCondition">The condition that determines if the first button is interactable. Defaults to null.</param>
    /// <param name="secondButtonCallback">The callback to invoke when the second button is clicked. Defaults to null.</param>
    /// <param name="secondbuttonName">The name of the second button. Defaults to "OK".</param>
    /// <param name="secondButtonCondition">The condition that determines if the second button is interactable. Defaults to null.</param>
    public void PopWindow(string text,
        System.Action firstButtonCallback = null,
        string firstbuttonName = "OK",
        System.Func<bool> firstButtonCondition = null,

        System.Action secondButtonCallback = null,
        string secondbuttonName = "OK",
        System.Func<bool> secondButtonCondition = null
    )
    {
        gameObject.SetActive(true);
        _firstButton.SetupButton(firstbuttonName,
        () => {
            firstButtonCallback?.Invoke();
            gameObject.SetActive(false);
        });
        if (firstButtonCondition != null) 
            _firstButton.interactable = firstButtonCondition();
        _firstButtonCondition = firstButtonCondition;
        _secondButtonCondition = secondButtonCondition;

        if (secondButtonCallback != null)
        {
            _secondButton.SetupButton(secondbuttonName,
            () => {
                secondButtonCallback?.Invoke();
                gameObject.SetActive(false);
            });
            if (secondButtonCondition != null) 
                _secondButton.interactable = secondButtonCondition();
        } else
        {
            _secondButton.gameObject.SetActive(false);
        }
        popupText.text = text;
    }
}
