using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Controller : Singleton<UI_Controller>
{
    /// <summary>Finds and initializes UI objects in the scene.</summary>
    public void FindObjectsInScene()
    {
        UIManager = FindAnyObjectByType<UI_Manager>();

        AllUIObjects[UIType.DiceButton] = UIManager.DiceButton.gameObject;

        AllUIObjects[UIType.CardUI] = UIManager.CardUI;
        CardUIScript = CardUI.GetComponent<ShowCardDetail>();

        AllUIObjects[UIType.BoardDetailUI] = UIManager.BoardDetailUI;
        BoardUIScript = BoardDetailUI.GetComponent<BoardDetailUI>();

        AllUIObjects[UIType.HUD] = UIManager.HUD;
        HudScript = HUD.GetComponent<HUD_Manager>();

        AllUIObjects[UIType.PropertyUI] = UIManager.PropertyUI;
        PropertyUIScript = PropertyUI.GetComponent<PropertyUIControl>();

        AllUIObjects[UIType.BiddingUI] = UIManager.BiddingUI;
        BiddingUIScript = BiddingUI.GetComponent<BiddingUIControl>();

        AllUIObjects[UIType.NotifyWindow] = UIManager.NotifyWindow;
        NotifyWindowScript = NotifyWindow.GetComponent<NotifyPopupWindow>();

        AllUIObjects[UIType.UtilitesUI] = UIManager.UtilitesUI;
        UtilitesUIScript = UtilitesUI.GetComponent<UtilitesUIController>();

        AllUIObjects[UIType.SellPropertyUI] = UIManager.SellingUI;
        SellingUIScript = SellPropertyUI.GetComponent<SellingUIController>();

        AllUIObjects[UIType.PauseMenu] = UIManager.PauseMenu;
        PauseMenuScript = PauseMenu.GetComponent<PauseMenu>();

        AllUIObjects[UIType.PropertyReviewUI] = UIManager.PropertyReviewUI;
        PropertyReviewUIScript = PropertyReviewUI.GetComponent<PropertyReviewController>();

        HudScript._timerParent.SetActive(GameMonitor.Instance.curGameMode == GameMode.AbridgedGame);
        ReviewPropertyButton.onClick.AddListener(ShowReviewPropertyUI);
        SellPropertyButton.onClick.AddListener(ShowSellPropertyUI);
        DisableAllWindows();
        HUD.SetActive(true);
    }

    /// <summary>Pops a window with the given text and button options.</summary>
    /// <param name="text">The text to display in the window.</param>
    /// <param name="onButtonClick">Action to perform when the main button is clicked.</param>
    /// <param name="buttonName">Name of the main button.</param>
    /// <param name="buttonCondition">Condition to enable the main button.</param>
    /// <param name="secondButtonCallback">Action to perform when the second button is clicked.</param>
    /// <param name="secondbuttonName">Name of the second button.</param>
    /// <param name="secondButtonCondition">Condition to enable the second button.</param>
    public void PopWindow(string text, System.Action onButtonClick = null
        , string buttonName = "OK", System.Func<bool> buttonCondition = null,

        System.Action secondButtonCallback = null,
        string secondbuttonName = "OK",
        System.Func<bool> secondButtonCondition = null)
    {
        NotifyWindowScript.PopWindow(text, onButtonClick, buttonName, buttonCondition, secondButtonCallback, secondbuttonName, secondButtonCondition);
    }

    /// <summary>Disables all UI windows except the HUD and DiceButton.</summary>
    public void DisableAllWindows()
    {
        foreach (var keyPair in AllUIObjects)
        {
            if (keyPair.Value != null && keyPair.Key != UIType.HUD && keyPair.Key != UIType.DiceButton)
            {
                keyPair.Value.SetActive(false);
            }
        }

    }

    /// <summary>Shows a message on the board UI.</summary>
    /// <param name="title">The title of the message.</param>
    /// <param name="msg">The message content.</param>
    public void ShowMsgOnBoardUI(string title, string msg)
    {
        BoardDetailUI.SetActive(true);
        //BoardUIScript.SetButtons(title, msg);
        Debug.Log($"ShowMsg title: {title}, msg:{msg}");
    }

    /// <summary>Shows the property UI for the given board place data.</summary>
    /// <param name="data">The board place data to display.</param>
    public void ShowPropertyUI(BoardPlaceData data)
    {
        PropertyUI.SetActive(true);
        PropertyUIScript.SetPropertyDetail(data);
    }

    /// <summary>Enables or disables property management buttons based on the player's ownership.</summary>
    /// <param name="b">If true, check player's ownership; if false, disable all buttons.</param>
    public void CanManageProperty(bool b)
    {
        // if input false, deavtivate all buttons, else, check if player owned any property
        bool active = b && Controller.CurPlayer.OwnedAnyProperty();
        SellPropertyButton.interactable = active;
        ReviewPropertyButton.interactable = active;
    }       

    /// <summary>Called when any UI is enabled.</summary>
    public void OnAnyUIEnabled()
    {
        if (gameObject.IsDestroyed() || Controller == null) return;
        SetFinishRoundButtonEnable();
    }

    /// <summary>Called when any UI is disabled.</summary>
    public void OnAnyUIDisabled()
    {
        if (gameObject.IsDestroyed() || Controller == null) return;
        SetFinishRoundButtonEnable();
    }

    /// <summary>Sets the finish round button enable state based on UI activity and player state.</summary>
    public void SetFinishRoundButtonEnable()
    {
        if (gameObject.IsDestroyed() || Controller == null) return;
        if (FinishRoundButton.gameObject != null && !Controller.gameObject.IsDestroyed())
            FinishRoundButton.gameObject.SetActive(!IsAnyUIActive() && Controller.EnteredTile);
    }

    /// <summary>Disables all buttons and the finish round button.</summary>
    public void SetAllButtonsDisable()
    {
        CanManageProperty(false);
        FinishRoundButton.gameObject.SetActive(false);
        DiceButton.gameObject.SetActive(false);
    }

    /// <summary>Checks if any UI window is currently active.</summary>
    /// <returns>True if any UI window is active, false otherwise.</returns>
    bool IsAnyUIActive()
    {
        if (AllUIObjects.Count == 0 || AllUIObjects == null) return false;
        foreach (var keyPair in AllUIObjects)
        {
            if (keyPair.Key != UIType.HUD && keyPair.Value.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Displays the property review UI.</summary>
    void ShowReviewPropertyUI()
    {
        DisableAllWindows();
        PropertyReviewUI.SetActive(true);
    }
    /// <summary>Displays the sell property UI.</summary>
    void ShowSellPropertyUI()
    {
        DisableAllWindows();
        SellPropertyUI.SetActive(true);
    }

    UI_Manager UIManager;
    public GameController Controller { get { return GameController.Instance; } }
    public GameObject BoardDetailUI { get { return AllUIObjects[UIType.BoardDetailUI]; } }
    public GameObject DiceButton { get { return AllUIObjects[UIType.DiceButton]; } }
    public GameObject CardUI { get { return AllUIObjects[UIType.CardUI]; } }
    public GameObject PropertyUI { get { return AllUIObjects[UIType.PropertyUI]; } }
    public GameObject BiddingUI { get { return AllUIObjects[UIType.BiddingUI]; } }
    public GameObject HUD { get { return AllUIObjects[UIType.HUD]; } }
    public GameObject NotifyWindow { get { return AllUIObjects[UIType.NotifyWindow]; } }
    public GameObject UtilitesUI { get { return AllUIObjects[UIType.UtilitesUI]; } }
    public GameObject SellPropertyUI { get { return AllUIObjects[UIType.SellPropertyUI]; } }
    public GameObject PauseMenu { get { return AllUIObjects[UIType.SellPropertyUI]; } }
    public GameObject PropertyReviewUI { get { return AllUIObjects[UIType.PropertyReviewUI]; } }

    public Dictionary<UIType, GameObject> AllUIObjects = new Dictionary<UIType, GameObject>();
    public enum UIType
    {
        DiceButton, CardUI, PropertyUI, BiddingUI, HUD, BoardDetailUI, NotifyWindow, UtilitesUI, SellPropertyUI,
        PauseMenu, PropertyReviewUI
    }

    public PropertyUIControl PropertyUIScript { get; private set; }
    public ShowCardDetail CardUIScript { get; private set; }
    public HUD_Manager HudScript { get; private set; }
    public BoardDetailUI BoardUIScript { get; private set; }
    public BiddingUIControl BiddingUIScript { get; private set; }
    public NotifyPopupWindow NotifyWindowScript { get; private set; }
    public UtilitesUIController UtilitesUIScript { get; private set; }
    public SellingUIController SellingUIScript { get; private set; }
    public PauseMenu PauseMenuScript { get; private set; }
    public PropertyReviewController PropertyReviewUIScript { get; private set; }

    public Button FinishRoundButton { get { return UIManager.FinishRoundButton; } }
    public Button ReviewPropertyButton { get { return UIManager.ReviewPropertyButton; } }
    public Button SellPropertyButton { get { return UIManager.SellPropertyButton; } }
}
