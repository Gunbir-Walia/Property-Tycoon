using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PropertyReviewController : MonoBehaviour
{
    [Header("GameObject Settings")]
    public GameObject optionParent;
    public GameObject optionPerfab;
    [Header("Other Settings")]

    HashSet<PropertyReviewOptions> _properties = new HashSet<PropertyReviewOptions>();
    HashSet<BoardPlaceData> _shownProperties = new HashSet<BoardPlaceData>();
    readonly float optionheight = 60f;
    public GameController Controller { get { return GameController.Instance; } }
    public GameMethods gameMethods { get { return GameMethods.Instance; } }
    public UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    void Start()
    {
    }

    private void OnDisable()
    {
        if (UIcontroller.gameObject != null && !UIcontroller.gameObject.IsDestroyed())
            UIcontroller.OnAnyUIDisabled();
        ResetUI();
    }

    private void OnEnable()
    {
        Initialize();
        if (UIcontroller != null)
            UIcontroller.OnAnyUIEnabled();
    }

    /// <summary>
    /// Initializes the UI with the properties owned by the current player.
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
    }

    /// <summary>
    /// Creates an option for the given property in the UI.
    /// </summary>
    /// <param name="property">The property data to create an option for.</param>
    void CreateOption(BoardPlaceData property)
    {
        PropertyReviewOptions optionScript = Instantiate(optionPerfab, optionParent.transform).GetComponent<PropertyReviewOptions>();
        _properties.Add(optionScript);
        _shownProperties.Add(property);
        optionScript.InitializeOption(property);
    }

    /// <summary>
    /// Resets the UI by destroying all property options.
    /// </summary>
    public void ResetUI()
    {
        if (_properties.Count == 0) return;
        foreach (PropertyReviewOptions property in _properties)
        {
            Destroy(property.gameObject);
        }
        _properties.Clear();
        _shownProperties.Clear();
    }
}
