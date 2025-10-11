using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SellPropertyOptions : MonoBehaviour
{
    public GameObject _propertyOptionsParent;
    [Header("Text")]
    public TMP_Text _propertyType;
    public TMP_Text _propertyName;
    [Header("Toggles")]
    public List<Toggle> toggles = new List<Toggle>(6);

    public BoardPlaceData Property;

    /// <summary>
    /// Initializes the option with the given property data.
    /// </summary>
    /// <param name="data">The property data to initialize the option with.</param>
    public void InitializeOption(BoardPlaceData data)
    {
        Property = data;
        _propertyType.text = data.boardType.ToString();
        _propertyName.text = data.boardName;
        toggles[0].isOn = false;
        _propertyOptionsParent.SetActive(data.boardType == BoardType.Property);
        if (data.boardType == BoardType.Property)
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                Toggle toggle = toggles[i];
                if (i > 0)
                {
                    toggle.onValueChanged.AddListener((value) => OnToggleSelect(toggle, value));
                    toggle.gameObject.SetActive(data.house_num > (i - 1));
                }
                toggle.isOn = false;
                toggle.interactable = false;
            }
            toggles[data.house_num].interactable = true;
        }
    }

    /// <summary>
    /// Refreshes the toggles based on the current property data.
    /// </summary>
    public void RefreshToggles()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            Toggle toggle = toggles[i];
            toggle.gameObject.SetActive(Property.house_num > (i - 1));
            toggle.isOn = false;
            toggle.interactable = false;
        }
        CheckActiveToggles();
    }

    /// <summary>
    /// Checks which toggles are active and returns the index of the last active toggle.
    /// </summary>
    /// <returns>The index of the last active toggle.</returns>
    public int CheckActiveToggles()
    {
        if (Property.boardType != BoardType.Property)
        {
            toggles[0].interactable = true;
            return 0;
        }
        int lastActiveToggle = 0;
        for (int i = toggles.Count - 1; i > 0; i--)
        {
            if (toggles[i].gameObject.activeSelf)
            {
                lastActiveToggle = i;
                break;
            }
        }
        Debug.Log("Last active toggle is " + lastActiveToggle);
        toggles[lastActiveToggle].interactable = true;
        return lastActiveToggle;
    }

    /// <summary>
    /// Handles the toggle selection event.
    /// </summary>
    /// <param name="toggle">The toggle that was selected.</param>
    /// <param name="isOn">The new value of the toggle (true if selected, false otherwise).</param>
    public void OnToggleSelect(Toggle toggle, bool isOn)
    {
        int index = toggles.IndexOf(toggle);
        Debug.Log("Toggle " + index + " is on");
        if (isOn)
        {
            if (index > 0 && toggles[index - 1].gameObject.activeSelf)
            {
                Debug.Log($"turning toggle {index-1} on");
                toggles[index - 1].interactable = true;
            }
        }
        else
        {
            if (index > 0)
            {
                for (int i = index-1; i >= 0; i--)
                {
                    if (toggles[i].gameObject.activeSelf)
                    {
                        toggles[index - 1].interactable = false;
                        toggles[index - 1].isOn = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates the total cost based on the selected toggles.
    /// </summary>
    /// <returns>The total cost calculated.</returns>
    public int CalculateCost()
    {
        int cost = (toggles[0].isOn) ? Property.propertyPrice : 0;
        if (Property.boardType != BoardType.Property) return cost;
        for(int i = 1; i < 5; i++)
        {
            cost += (toggles[i].isOn && toggles[i].gameObject.activeSelf) ? Property.house_price : 0;
        }
        cost += (toggles[5].isOn && toggles[5].gameObject.activeSelf) ? Property.house_price * 5 : 0;
        return cost;
    }
}
