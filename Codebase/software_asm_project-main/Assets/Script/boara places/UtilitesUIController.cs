using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UtilitesUIController : MonoBehaviour
{
    [Header("Property Data")]
    public GameObject stationDataParent;
    public GameObject utilityDataParent;
    public GameObject optionButtonParent;
    public GameObject ownerParent;

    GameObject utilityRentDescParent;
    GameObject utilityPayRentParent;
    [Header("Images")]
    public Image BG;
    public Sprite[] BGSprites = new Sprite[3];
    TMP_Text[] _stationPrices = new TMP_Text[5];
    TMP_Text[] _utilityRentDesc = new TMP_Text[2];

    [Header("Property Text")]
    public TMP_Text propertyName;
    public TMP_Text propertyOwner;
    TMP_Text utilityRentText;
    [Header("Buttons")]
    public GameObject PayButtonObj;
    Button PayButton;
    public Button ExitButton;

    BoardPlaceData curProperty;
    int propertytRent;

    GameController Controller { get { return GameController.Instance; } }
    GameMethods gameMethods { get { return GameMethods.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    private void OnDisable()
    {
        if (UIcontroller.gameObject != null)
            UIcontroller.OnAnyUIDisabled();
    }

    private void Awake()
    {
        utilityRentDescParent = utilityDataParent.transform.GetChild(0).gameObject;
        utilityPayRentParent = utilityDataParent.transform.GetChild(1).gameObject;
        utilityRentText = utilityPayRentParent.transform.GetChild(1).GetComponent<TMP_Text>();
        for (int i = 0; i < 5; i++)
        {
            if(i < 2) _utilityRentDesc[i] = utilityRentDescParent.transform.GetChild(i).GetComponent<TMP_Text>();
            _stationPrices[i] = stationDataParent.transform.GetChild(1).GetChild(i).GetComponent<TMP_Text>();
        }
        PayButton = PayButtonObj.GetComponent<Button>();
    }

    /// <summary>
    /// Sets the property detail by the given board data.
    /// </summary>
    /// <param name="data">The BoardPlaceData object containing the property information.</param>
    public void SetPropertyDetail(BoardPlaceData data)
    {
        transform.localPosition = Vector3.zero;
        curProperty = data;

        if (data.boardType == BoardType.Station)
        {
            SetStationData();
            propertyOwner.transform.parent.localPosition = new Vector3(0, 40, 0);
        }
        if (data.boardType == BoardType.Utility)
        {
            SetUtilitesData();
            propertyOwner.transform.parent.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Sets the station data for the UI.
    /// </summary>
    void SetStationData()
    {
        propertyName.text = curProperty.boardName;
        BG.sprite = BGSprites[0];
        stationDataParent.SetActive(true);
        utilityDataParent.SetActive(false);
        if (curProperty.ownerID == -1)
        { // property has no owner
            _stationPrices[0].color = Color.green;
            propertyOwner.text = "No Owner";
            SetupsIfNoOneOwnedProperty();
        }
        else if (curProperty.ownerID == Controller.CurPlayer.playerID)
        { // property is owned by current player
            SetStaionRentHighlight(Color.green);
            propertyOwner.text = "You";
            SetupsIfOwnedProperty();
        }
        else
        { // property is not owned by current player
            propertytRent = gameMethods.GetStationRent(curProperty.ownerID);
            propertyOwner.text = Controller.Players[curProperty.ownerID].playerName;
            SetStaionRentHighlight(Color.red);
            SetButtonsPayRent();
        }
    }

    /// <summary>
    /// Highlights the station rent text based on the color provided.
    /// </summary>
    /// <param name="color">The color to highlight the rent text.</param>
    void SetStaionRentHighlight(Color color)
    {
        foreach(TMP_Text text in _stationPrices) text.color = Color.white;
        _stationPrices[gameMethods.CountOwnedProperties(
            BoardType.Station, curProperty.ownerID)].color = color;
    }

    /// <summary>
    /// Sets the utilities data for the UI.
    /// </summary>
    void SetUtilitesData()
    {
        propertyName.text = ""; // name is already shown in BG
        stationDataParent.SetActive(false);
        utilityDataParent.SetActive(true);
        utilityPayRentParent.SetActive(
            curProperty.ownerID >= 0 &&
            curProperty.ownerID != Controller.CurPlayer.playerID );
        BG.sprite = (curProperty.tileID == 12) ? BGSprites[1] : BGSprites[2]; //tile 12 is Tesla
        if (curProperty.ownerID == -1)
        { // property has no owner
            _stationPrices[0].color = Color.green;
            propertyOwner.text = "No Owner";
            SetupsIfNoOneOwnedProperty();
        }
        else if (curProperty.ownerID == Controller.CurPlayer.playerID)
        { // property is owned by current player
            SetUtilityRentHighlight(Color.green);
            propertyOwner.text = "You";
            SetupsIfOwnedProperty();
        }
        else
        { // property is not owned by current player
            propertytRent = gameMethods.GetUtilitesRent(curProperty.ownerID);
            propertyOwner.text = Controller.Players[curProperty.ownerID].playerName;
            utilityRentText.text = $"£{propertytRent}";
            SetUtilityRentHighlight(Color.red);
            SetButtonsPayRent();
        }
    }

    /// <summary>
    /// Sets up the pay rent button based on the current player and landlord's information.
    /// </summary>
    void SetButtonsPayRent()
    {
        PlayerInfo curPlayer = Controller.CurPlayer;
        PlayerInfo landlord = Controller.Players[curProperty.ownerID];
        PayButton.SetupButton("Pay Rent", () => { OnPayRent(); },
            curPlayer.money >= propertytRent,
            landlord.CanReceiveRent());
        PayButton.SetButtonPadding(20f);
        ExitButton.gameObject.SetActive(!landlord.CanReceiveRent());
    }

    /// <summary>
    /// Highlights the utilities rent description text based on the color provided.
    /// </summary>
    /// <param name="color">The color to highlight the rent description text.</param>
    void SetUtilityRentHighlight(Color color)
    {
        foreach (TMP_Text text in _utilityRentDesc) text.color = Color.white;
        _utilityRentDesc[gameMethods.CountOwnedProperties(BoardType.Utility, curProperty.ownerID) - 1].color = color;
    }

    /// <summary>
    /// Sets up the UI when the property is owned by the current player.
    /// </summary>
    void SetupsIfOwnedProperty()
    {
        PayButton.gameObject.SetActive(false);
        ExitButton.SetupButton("Exit", () => { OnExitUI(); });
        ExitButton.SetButtonPadding(20f);
    }

    /// <summary>
    /// Sets up the UI when the property is not owned by anyone.
    /// </summary>
    void SetupsIfNoOneOwnedProperty()
    {
        propertyOwner.text = "No Owner";

        PayButton.SetupButton("Buy it", () => { OnBuyProperty(); }, 
            Controller.CurPlayer.money >= curProperty.propertyPrice,
            Controller.CurPlayer.finishRounds > 0);
        PayButton.SetButtonPadding(20f);
        // start bidding when not buying the property
        ExitButton.AddSingleEventToOnClick(OnGiveupBuyingProperty);
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
        _stationPrices[0].color = Color.white;
        propertyOwner.text = curPlayer.playerName;
        if(curProperty.boardType == BoardType.Station)
        {
            SetStaionRentHighlight(Color.green);
        }
        if (curProperty.boardType == BoardType.Utility)
        {
            SetUtilityRentHighlight(Color.green);
        }
        SetupsIfOwnedProperty();
        UIcontroller.CanManageProperty(true);
    }

    /// <summary>
    /// Handles the payment of rent.
    /// </summary>
    void OnPayRent()
    {
        Controller.CurPlayer.PayToPlayer(Controller.Players[curProperty.ownerID], propertytRent);
        OnExitUI();
    }

    /// <summary>
    /// Handles the scenario when the player gives up buying a property.
    /// </summary>
    void OnGiveupBuyingProperty()
    {
        int numPlayersCanBid = Controller.Players.Count(player => player.finishRounds > 0);
        if (numPlayersCanBid > 1)
        {
            UI_Controller uiContr = UI_Controller.Instance;
            uiContr.BiddingUI.SetActive(true);
            transform.localPosition = new Vector3(-520, 0, 0);
            optionButtonParent.SetActive(false);
            uiContr.BiddingUIScript.StartBidding(curProperty);
        }
        else
        {
            OnExitUI();
        }
    }

    /// <summary>
    /// Exit the UI and allow player to finish the round.
    /// </summary>
    public void OnExitUI()
    {
        Controller.CanFinishRound();
        gameObject.SetActive(false);
    }
}
