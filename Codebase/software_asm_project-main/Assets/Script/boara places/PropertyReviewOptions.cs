using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PropertyReviewOptions : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text _propertyType;
    public TMP_Text _propertyName;
    [Header("Others")]
    public Button _reviewButton;
    [HideInInspector] public BoardPlaceData Property;

    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    /// <summary>
    /// Initializes the option with the given property data.
    /// </summary>
    /// <param name="data">The property data to initialize the option with.</param>
    public void InitializeOption(BoardPlaceData data)
    {
        Property = data;
        _propertyType.text = data.boardType.ToString();
        _propertyName.text = data.boardName;
        _reviewButton.onClick.AddListener(ReviewProperty);
    }

    /// <summary>
    /// Display the property or utility details UI of this option.
    /// </summary>
    public void ReviewProperty()
    {
        if (Property.boardType == BoardType.Property)
        {
            UIcontroller.ShowPropertyUI(Property);
            UIcontroller.PropertyUI.transform.localPosition = new Vector3(-500, 0, 0);
            // prevent it takes any action if exit
            UIcontroller.PropertyUIScript.ExitButton.AddSingleEventToOnClick(
                () => { UIcontroller.PropertyUI.SetActive(false); });
        }
        else
        {
            UIcontroller.UtilitesUI.SetActive(true);
            UIcontroller.UtilitesUIScript.SetPropertyDetail(Property);
            UIcontroller.UtilitesUI.transform.localPosition = new Vector3(-500, 0, 0);
        }
    }
}
