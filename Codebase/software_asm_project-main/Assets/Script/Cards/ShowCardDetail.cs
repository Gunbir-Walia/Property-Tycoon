using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowCardDetail : MonoBehaviour
{
    public TMP_Text _cardDesc;
    public TMP_Text _coundDownText;
    public Image BG;
    [Tooltip("Auto close time in second")]
    public int closeTime = 3;

    Sprite DEFAULT_BG;
    

    public GameObject _cardDetailObj;
    public GameObject _pickCardOptions;

    GameController Controller { get { return GameController.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    private void Awake()
    {
        DEFAULT_BG = Resources.Load<Sprite>("DefaultCardSprite");
    }

    private void OnDisable()
    {
        if (UIcontroller.gameObject != null)
            UIcontroller.OnAnyUIDisabled();
    }

    private void OnEnable()
    {
        if (UIcontroller != null)
            UIcontroller.OnAnyUIEnabled();
        _coundDownText.gameObject.SetActive(false);
        _pickCardOptions.gameObject.SetActive(false);
        _cardDetailObj.transform.localPosition = new Vector3(0, -20, 0);
    }

    /// <summary>
    /// Displays the details of a card with a given description and background sprite.
    /// </summary>
    /// <param name="desc">The description text of the card to be displayed.</param>
    /// <param name="bg">The background sprite of the card to be displayed.</param>
    public void ShowDetail(string desc, Sprite bg)
    {
        _cardDesc.text = desc;
        BG.sprite = bg;
    }

    /// <summary>
    /// Closes the card detail window and raises a card action in the game controller.
    /// </summary>
    public void DefaultCloseWindowAction()
    {
        gameObject.SetActive(false);
        Controller.RaiseCardAction();
    }

    /// <summary>
    /// Prepares to close the card detail window by showing the countdown text and starting the countdown coroutine.
    /// </summary>
    public void DefaultCardPreAction()
    {
        _coundDownText.gameObject.SetActive(true);
        StartCoroutine(Countdown(closeTime));
    }

    /// <summary>
    /// Sets the card action to PAY in the game controller and then closes the card detail window.
    /// </summary>
    public void PickCardPayOption()
    { // pay 10pound 
        Controller.CardActMan.SetCardAction(CardType.PAY);
        DefaultCloseWindowAction();
    }

    /// <summary>
    /// Prepares to pick a card by adjusting the card detail object's position and showing the pick card options.
    /// </summary>
    public void PickCardPreAction()
    {
        _cardDetailObj.transform.localPosition = new Vector3(0, 25, 0);
        _pickCardOptions.gameObject.SetActive(true);
    }

    /// <summary>
    /// A coroutine that counts down from a specified time and closes the card detail window when the countdown reaches zero.
    /// </summary>
    /// <param name="time">The countdown time in seconds.</param>
    /// <returns>An IEnumerator that can be used to run the countdown coroutine.</returns>
    IEnumerator Countdown(int time)
    {
        int countdown = time;
        while (countdown > 0)
        {
            _coundDownText.text = $"will auto close in {countdown} seconds";
            countdown--;
            yield return new WaitForSeconds(1f);
        }
        if (gameObject.activeSelf)
            DefaultCloseWindowAction();
    }
}
