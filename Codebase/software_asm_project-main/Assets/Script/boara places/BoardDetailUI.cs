using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardDetailUI : MonoBehaviour
{
    public TMP_Text _boardName;
    public TMP_Text _boardDescription;
    public TMP_Text _proertyPrice;
    public TMP_Text _proertyHouses;

    // Cards
    public Button _firstButton;
    public Button _secondButton;

    BoardPlaceData curBoard;
    GameController Controller { get { return GameController.Instance; } }
    GameMethods gameMethod { get { return GameMethods.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    Func<bool> _firstButtonCondition;
    Func<bool> _secondButtonCondition;

    private void Awake()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
        // control the button interactable when condition is set and button is active
        if (gameObject.activeSelf && _firstButtonCondition != null)
            _firstButton.interactable = _firstButtonCondition();
        if(gameObject.activeSelf && _secondButtonCondition != null)
            _secondButton.interactable = _secondButtonCondition();
    }

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

    /// <summary>
    /// Method to show a card and deactivate the current UI object
    /// </summary>
    public void ShowCard()
    {
        Controller.OnPickCard();
        gameObject.SetActive(false);
    }

    /// <summary>Method to show board detail information based on provided board data</summary>
    /// <param name="data">The board data to be displayed</param>
    public void ShowBoardDetail(BoardPlaceData data)
    {
        curBoard = data;
        _boardName.text = data.boardName;
        switch (data.boardType)
        {
            case BoardType.Tax:
                _boardDescription.text = "You are taxed " + -1 * data.moneyChange + "£ here";
                break;
            case BoardType.Card:
                _boardDescription.text = GameLocalization.Instance.BoardUIMessages[2];
                break;
            case BoardType.Jail:
                _boardDescription.text = GameLocalization.Instance.BoardUIMessages[3];
                break;
            case BoardType.Parking:
                _boardDescription.text = $"You can collect £{Controller.ParkMoney} here";
                break;
        }
    }

    /// <summary>
    /// Method to set the title and description of the board detail UI
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    // Input: string title - The title to be set
    //        string message - The description to be set
    public void SetTitleAndDesc(string title, string message)
    {
        _boardName.text = title;
        _boardDescription.text = message;
        _proertyPrice.text = "";
        _proertyHouses.text = "";
    }

    /// <summary>
    /// Method to set up buttons with specified conditions and callbacks
    /// </summary>
    /// <param name="firstButtonName">The name for the first button</param>
    /// <param name="firstButtonCallback">The callback for the first button</param>
    /// <param name="firstButtonInteractable">Whether the first button is interactable</param>
    /// <param name="secondButtonName">The name for the second button</param>
    /// <param name="secondButtonCallback">The callback for the second button</param>
    /// <param name="secondButtonInteractable">Whether the second button is interactable</param>
    public void SetButtons(
        string firstButtonName = null,
        Action firstButtonCallback = null, 
        bool firstButtonInteractable = true,
        string secondButtonName = null, 
        Action secondButtonCallback = null, 
        bool secondButtonInteractable = true)
    {
        if (firstButtonName != null)
        {
            _firstButton.SetupButton(firstButtonName, firstButtonCallback, firstButtonInteractable);
            _firstButton.onClick.AddListener(() => gameObject.SetActive(false));
            _firstButton.SetButtonPadding(10f);
        }
        if (secondButtonName != null)
        {
            _secondButton.SetupButton(secondButtonName, secondButtonCallback, secondButtonInteractable);
            _secondButton.onClick.AddListener(() => gameObject.SetActive(false));
            _secondButton.SetButtonPadding(10f);
        }
        else
        {
            _secondButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Method to set up buttons with specified conditions and callbacks
    /// </summary>
    /// <param name="firstButtonName"> The name for the first button</param>
    /// <param name="firstButtonCallback"> The callback for the first button </param>
    /// <param name="firstButtonCondition"> The condition for the first button's interactability </param>
    /// <param name="secondButtonName"> The name for the second button </param>
    /// <param name="secondButtonCallback"> The callback for the second button </param>
    /// <param name="secondButtonCondition"> The condition for the second button's interactability </param>
    public void SetButtons(
        string firstButtonName = null, 
        Action firstButtonCallback = null, 
        Func<bool> firstButtonCondition = null,

        string secondButtonName = null, 
        Action secondButtonCallback = null, 
        Func<bool> secondButtonCondition = null)
    {
        if (firstButtonName != null)
        {
            _firstButton.gameObject.SetActive(true);
            _firstButton.SetButtonTitle(firstButtonName, 10f);
            _firstButton.AddSingleEventToOnClick(firstButtonCallback);
            if (firstButtonCondition != null) 
                _firstButton.interactable = firstButtonCondition();
            _firstButtonCondition = firstButtonCondition;
            _firstButton.onClick.AddListener(() => gameObject.SetActive(false));
            
        }
        if (secondButtonName != null)
        {
            _secondButton.gameObject.SetActive(true);
            _secondButton.SetButtonTitle(secondButtonName, 10f);
            _secondButton.AddSingleEventToOnClick(secondButtonCallback);
            if (secondButtonCondition != null)
                _secondButton.interactable = secondButtonCondition();
            _secondButtonCondition = secondButtonCondition;
            _secondButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
        else
        {
            _secondButton.gameObject.SetActive(false);
            _secondButtonCondition = null;
        }
    }
}
