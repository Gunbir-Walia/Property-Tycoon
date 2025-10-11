using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PropertyUIControl : MonoBehaviour
{
    [Header("Property Data")]
    public GameObject propertyDataParent;
    public GameObject optionButtonParent;
    public Image BG;
    public Sprite[] BGSprites = new Sprite[8];
    TMP_Text[] _propertyPrices = new TMP_Text[7];
    TMP_Text[] _upgradePrices = new TMP_Text[2];

    [Header("Property Text")]
    public TMP_Text propertyName;
    public TMP_Text propertyOwner;
    public TMP_Text builtHouses;
    [Header("Buttons")]
    public GameObject PayButtonObj;
    Button PayButton;
    public Button ExitButton;

    BoardPlaceData curProperty;
    Func<bool> PayButtonConditon;

    GameController Controller { get { return GameController.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    private void Awake()
    {
        for (int i = 0; i < 7; i++)
        {
            if (i < 2) _upgradePrices[i] = propertyDataParent.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>();
            _propertyPrices[i] = propertyDataParent.transform.GetChild(0).GetChild(i).GetComponent<TMP_Text>();
        }
        PayButton = PayButtonObj.GetComponent<Button>();
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
        for (int i = 0; i < 6; i++)
        { // restore price color
            if (i < 2) _upgradePrices[0].color = Color.white;
            _propertyPrices[i].color = Color.white;
        }
        optionButtonParent.SetActive(true);
        PayButton.gameObject.SetActive(true);
        PayButton.onClick.RemoveAllListeners();
        ExitButton.gameObject.SetActive(true);
        ExitButton.onClick.RemoveAllListeners();
        PayButtonConditon = null;
    }

    private void Update()
    {
        if (PayButton.gameObject.activeSelf && PayButtonConditon != null) {
            PayButton.interactable = PayButtonConditon();
        }
    }

    /// <summary>
    /// Sets the property details in the UI.
    /// </summary>
    /// <param name="data">The property data to set in the UI.</param>
    public void SetPropertyDetail(BoardPlaceData data)
    {
        transform.localPosition = Vector3.zero;
        BG.sprite = BGSprites[(int)data.propertyColor];
        curProperty = data;
        propertyName.text = data.boardName;

        // showing property price data
        _propertyPrices[0].text = $"£ {data.propertyPrice}";
        for (int i = 0; i < 6; i++)
            _propertyPrices[i + 1].text = $"£ {data.rentPrice[i]}";
        // showing upgrade price
        _upgradePrices[0].text = $"£ {data.house_price}";
        _upgradePrices[1].text = $"£ {data.house_price * 5}";

        if (data.ownerID == -1)
        { // property has no owner
            _propertyPrices[0].color = Color.green;
            propertyOwner.text = "No Owner";
            PayButtonConditon = () => { return Controller.CurPlayer.money >= data.propertyPrice; };
            PayButton.SetupButton("Buy it", () => { OnBuyProperty(); },
                PayButtonConditon(),
                Controller.CurPlayer.finishRounds > 0);
            // start bidding when not buying the property
            ExitButton.AddSingleEventToOnClick(OnGiveupBuyingProperty);
        }
        else if (data.ownerID == Controller.CurPlayer.playerID)
        { // property is owned by current player
            setUpgradeDetail();
        }
        else
        { // property is not owned by current player
            _propertyPrices[data.house_num + 1].color = Color.red;// highlight related price
            int rentPrice = data.GetRentPrice();
            if (data.house_num == 0 && data.Landlord().IsPlayerOwnsAllPropertyInColor(data.propertyColor))
            {
                _propertyPrices[1].text += " x2";
                rentPrice *= 2;
            }
            propertyOwner.text = data.Landlord().playerName;
            if (Controller.Players[curProperty.ownerID].CanReceiveRent())
            {
                PayButtonConditon = () => { return Controller.CurPlayer.money >= rentPrice; };
                PayButton.SetupButton("Pay Rent", () => { OnPayRent(rentPrice); },
                    PayButtonConditon(),
                    true);
            }
            else
            {
                PayButton.SetupButton("Pay Rent", () => { }, false);
            }
            if (!curProperty.Landlord().CanReceiveRent())
            {
                ExitButton.gameObject.SetActive(true);
                ExitButton.AddSingleEventToOnClick(OnExitUI);
            }
        }
        SetBuiltHouseText();
    }

    /// <summary>
    /// Sets the UI details for upgrading a property.
    /// </summary>
    void setUpgradeDetail()
    {
        propertyOwner.text = "You";
        PayButton.SetButtonTitle("Upgrade");
        // hightlight related price
        _upgradePrices[0].color = (curProperty.house_num == 4) ? Color.white : Color.green;
        _upgradePrices[1].color = (curProperty.house_num == 4) ? Color.green : Color.white;
        foreach(TMP_Text price in _propertyPrices) price.color = Color.white;
        _propertyPrices[curProperty.house_num+1].color = Color.green;
        SetBuiltHouseText();
        ExitButton.AddSingleEventToOnClick(OnExitUI);
        if (Controller.CurPlayer.IsPlayerOwnsAllPropertyInColor(curProperty.propertyColor) &&
            curProperty.CanPropertyImproveHouse())
        {
            PayButton.AddSingleEventToOnClick(OnUpgradeyProperty);
            PayButtonConditon = () => { return Controller.CurPlayer.money >= curProperty.GetUpgradePrice(); };
            PayButton.interactable = PayButtonConditon();
            // a property can build hotel only if there is no other property in the same color set has hotel 
            PayButton.gameObject.SetActive(curProperty.house_num < 5 && !curProperty.IsAnyPropertyHasHotelInSet());
        } else
        {
            PayButton.gameObject.SetActive(true);
            PayButton.interactable = false;
            PayButtonConditon = null;
        }
    }

    /// <summary>
    /// Sets the text for the number of built houses on the property.
    /// </summary>
    void SetBuiltHouseText()
    {
        switch (curProperty.house_num)
        {
            case 0:
                builtHouses.text = GameLocalization.Instance.PropertyUIMessages[3];
                break;
            case 1:
                builtHouses.text = GameLocalization.Instance.PropertyUIMessages[4];
                break;
            case 5:
                builtHouses.text = GameLocalization.Instance.PropertyUIMessages[5];
                break;
            default:
                builtHouses.text = curProperty.house_num + " Houses";
                break;
        }
    }

    /// <summary>
    /// Handles the buying of a property.
    /// </summary>
    void OnBuyProperty()
    {
        PlayerInfo curPlayer = Controller.CurPlayer;
        curProperty.ownerID = curPlayer.playerID;
        curPlayer.ownedProperties.Add(curProperty);
        curPlayer.PlayerMoneyChange(-curProperty.propertyPrice);
        _propertyPrices[0].color = Color.white;
        UIcontroller.CanManageProperty(true);
        setUpgradeDetail();
    }

    /// <summary>
    /// Handles the upgrading of a property.
    /// </summary>
    void OnUpgradeyProperty()
    {
        curProperty.house_num++;
        if (curProperty.house_num == 5)
        {
            Controller.CurPlayer.ownedHotels++;
        }
        else {
            Controller.CurPlayer.ownedHouses++;
        }
        Controller.CurPlayer.PlayerMoneyChange(-curProperty.GetUpgradePrice());
        setUpgradeDetail();
    }

    /// <summary>
    /// Handles the payment of rent for a property.
    /// </summary>
    /// <param name="rent">The amount of rent to pay.</param>
    void OnPayRent(int rent)
    {
        Controller.CurPlayer.PayToPlayer(curProperty.Landlord(), rent);
        OnExitUI();
    }

    /// <summary>
    /// Handles the action of giving up on buying a property and starts bidding if possible.
    /// </summary>
    void OnGiveupBuyingProperty()
    {
        int numPlayersCanBid = Controller.Players.Count(player => player.finishRounds > 0);
        if (numPlayersCanBid > 1)
        {
            UI_Controller uiContr = UI_Controller.Instance;
            uiContr.BiddingUI.SetActive(true);
            transform.localPosition = new Vector3(-500, 0, 0);
            optionButtonParent.SetActive(false);
            uiContr.BiddingUIScript.StartBidding(curProperty);
        }
        else
        {
            OnExitUI();
        }
    }

    /// <summary>
    /// Exits the UI and allows the player to finish their round.
    /// </summary>
    public void OnExitUI()
    {
        Controller.CanFinishRound();
        gameObject.SetActive(false);
    }
}
