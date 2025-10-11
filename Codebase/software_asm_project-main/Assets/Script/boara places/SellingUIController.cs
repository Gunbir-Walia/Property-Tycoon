using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SellingUIController : MonoBehaviour
{
    [Header("GameObject Settings")]
    public GameObject optionParent;
    public GameObject optionPerfab;
    [Header("Other Settings")]
    public TMP_Text costText;
    public Button sellButton;
    public Toggle toggleSetAll;

    HashSet<SellPropertyOptions> _properties = new HashSet<SellPropertyOptions>();
    readonly float optionheight = 60f;
    public GameController Controller { get { return GameController.Instance; } }
    public GameMethods gameMethods { get { return GameMethods.Instance; } }
    public UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    private void OnDisable()
    {
        if (UIcontroller.gameObject != null && !UIcontroller.gameObject.IsDestroyed())
        {
            UIcontroller.OnAnyUIDisabled();
        }

        ResetUI();
    }
    private void OnEnable()
    {
        Initialize();
        if (UIcontroller != null)
        {
            UIcontroller.OnAnyUIEnabled();
        }
    }

    /// <summary>
    /// Initializes the Selling UI by creating options for each property owned by the current player.
    /// </summary>
    public void Initialize()
    {
        if (Controller.CurPlayer == null) return;
        PlayerInfo curPlayer = Controller.CurPlayer;
        int curPlayerIndex = curPlayer.playerID;
        float perferredHeight = 0;
        foreach (BoardPlaceData property in Controller.CurPlayer.ownedProperties)
        {
            if (property.ownerID == curPlayerIndex)
            {
                CreateOption(property);
                perferredHeight += optionheight;
            }
        }
        optionParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, perferredHeight + 10);
        costText.text = "Cost: £0";
        sellButton.interactable = false;
        toggleSetAll.isOn = false;
    }

    /// <summary>
    /// Creates a UI option for a given property.
    /// </summary>
    /// <param name="property">The property for which the option is to be created.</param>
    void CreateOption(BoardPlaceData property)
    {
        SellPropertyOptions optionScript = Instantiate(optionPerfab, optionParent.transform).GetComponent<SellPropertyOptions>();
        _properties.Add(optionScript);
        optionScript.InitializeOption(property);
        foreach (Toggle toggle in optionScript.toggles)
        {
            if (toggle.gameObject.activeSelf)
            { //the underline is to ignore parameter
                toggle.onValueChanged.AddListener(_ => UpdateCost());
                toggle.onValueChanged.AddListener(value => ResetToggleSetAll(value));
            }
        }
    }

    /// <summary>
    /// Updates the cost text and the interactability of the sell button based on the total cost.
    /// </summary>
    public void UpdateCost()
    {
        int costTotal = GetTotalCost();
        costText.text = "Cost: £" + costTotal;
        sellButton.interactable = (costTotal > 0);
    }

    /// <summary>
    /// Resets the toggleSetAll if any toggle is deselected.
    /// </summary>
    /// <param name="value">The new value of the toggle that triggered this method.</param>
    void ResetToggleSetAll(bool value)
    {
        if (value == false)
        {
            toggleSetAll.isOn = false;
            return;
        }
        // check all of the toggles in the table, if all of them on, then set on to the toggleSetAll
        if (IsAnyToggleValueIs(false)) return;
        Debug.Log("all toggles are on");
        toggleSetAll.isOn = true;
    }

    /// <summary>
    /// Calculates the total cost of the selected properties.
    /// </summary>
    /// <returns>The total cost of the selected properties.</returns>
    int GetTotalCost()
    {
        int costTotal = 0;
        foreach (SellPropertyOptions option in _properties)
        {
            costTotal += option.CalculateCost();
        }
        return costTotal;
    }

    /// <summary>
    /// Sells the selected properties and updates the UI and player's money.
    /// </summary>
    void SellProperty()
    {
        int costTotal = GetTotalCost();
        float newHeight = optionParent.GetComponent<RectTransform>().sizeDelta.y;
        HashSet<SellPropertyOptions> removeList = new HashSet<SellPropertyOptions>();
        foreach (SellPropertyOptions option in _properties)
        {
            List<Toggle> toggles = option.toggles;
            if (option.toggles[0].isOn)
            {
                option.Property.SoldProperty();
                removeList.Add(option);
                Destroy(option.gameObject);
                newHeight -= optionheight;
                continue;
            }
            for (int i = 1; i < toggles.Count; i++)
            {
                if (toggles[i].isOn)
                {
                    toggles[i].isOn = false;
                    toggles[i].interactable = false;
                    toggles[i].gameObject.SetActive(false);
                }
            }
            int originHouseNum = option.Property.house_num;
            option.Property.house_num = option.CheckActiveToggles();
            if(originHouseNum == 5)
            {
                Controller.CurPlayer.ownedHotels--;
                originHouseNum--;
            }
            Controller.CurPlayer.ownedHouses -= (originHouseNum - option.Property.house_num);
        }
        _properties.ExceptWith(removeList);
        optionParent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, newHeight);
        costText.text = "Cost: £0";
        sellButton.interactable = false;
        Controller.CurPlayer.PlayerMoneyChange(costTotal);
        UIcontroller.CanManageProperty(true);
        if (!Controller.CurPlayer.OwnedAnyProperty())
        { // nothing to sell, close the window;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Resets the UI by destroying all property options.
    /// </summary>
    public void ResetUI()
    {
        if (_properties.Count == 0) return;
        foreach (SellPropertyOptions property in _properties)
        {
            Destroy(property.gameObject);
        }
        _properties.Clear();
    }

    /// <summary>
    /// Deselects all toggles and resets the UI elements.
    /// </summary>
    void DeselectAllToggles()
    {
        //Debug.Log("Deselect all toggles");
        foreach (SellPropertyOptions option in _properties)
        {
            foreach (Toggle toggle in option.toggles)
            {
                if (toggle.gameObject.activeSelf)
                {
                    toggle.isOn = false;
                    toggle.interactable = false;
                }
            }
            option.CheckActiveToggles();
        }
        costText.text = "Cost: £0";
        sellButton.interactable = false;
    }

    /// <summary>
    /// Handles the select all functionality.
    /// </summary>
    /// <param name="value">True if all toggles should be selected, false otherwise.</param>
    public void OnSelectAll(bool value)
    {
        if (value == false)
        {
            // check all of the toggles in the table, if all of them on, then set all the toggles off
            if (IsAnyToggleValueIs(false)) return;
            DeselectAllToggles();
        }
        else
        {
            foreach (SellPropertyOptions option in _properties)
            {
                foreach (Toggle toggle in option.toggles)
                {
                    if (toggle.gameObject.activeSelf)
                    {
                        toggle.isOn = true;
                        toggle.interactable = true;
                    }
                }
            }
            UpdateCost();
        }
    }

    /// <summary>
    /// Checks if any toggle has the specified value.
    /// </summary>
    /// <param name="b">The value to check for.</param>
    /// <returns>True if any toggle has the specified value, false otherwise.</returns>
    bool IsAnyToggleValueIs(bool b)
    {
        foreach (SellPropertyOptions option in _properties)
        {
            foreach (Toggle toggle in option.toggles)
            {
                if (toggle.gameObject.activeSelf && toggle.isOn == b)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
