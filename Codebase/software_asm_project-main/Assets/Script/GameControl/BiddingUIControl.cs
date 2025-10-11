using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BiddingUIControl : MonoBehaviour
{
    [Header("Settings")]
    public TMP_InputField bidPriceInput;
    public GameObject playerSlots;
    public GameObject playerBidPrefab;
    public GameObject curBidHighlighter;
    public Button SubmitBidButton;

    [Space(5), Header("Bid Info Text")]
    public TMP_Text highestBidAmount;

    PlayerBiddingData curBidPlayer;
    int curBidderIndex = 0;
    PlayerBiddingData leadingBidder;
    public int curHighestBid;

    [Space(5), Header("Other Settings")]
    [Tooltip("Minimum increase of bid amount")]
    public int MinBidIncrease;
    [Tooltip("The maximum bid allowed in this bid will be property price * this multiplier")]
    public int MaxBidMultiplier = 10;
    int MaxBidMoney;

    public List<Sprite> tokenAvatar;
    List<PlayerBiddingData> biddingPlayers = new List<PlayerBiddingData>();
    BoardPlaceData biddingProperty;

    GameController Controller { get { return GameController.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }
    void Start()
    {
        bidPriceInput.onValueChanged.AddListener(OnValueChanged);
        bidPriceInput.onEndEdit.AddListener(OnEndEdit);
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
    }

    void ResetHighlighter()
    {
        SetHighlighterToPlayer(curBidHighlighter, gameObject);
    }

    /// <summary>
    /// This method is called when the value of the bidPriceInput has changed.
    /// </summary>
    /// <param name="value">The new value of the bidPriceInput as a string.</param>
    void OnValueChanged(string value)
    {
        //int maxBiddingAmount = 1000;
        if (!int.TryParse(value, out int result))
        {
            // remove char where is not numbers
            bidPriceInput.text = new string(bidPriceInput.text.Where(char.IsDigit).ToArray());
            bidPriceInput.caretPosition = bidPriceInput.text.Length; // move to end of input field
        }
    }

    /// <summary>
    /// This method is called when the editing of the bidPriceInput has ended.
    /// </summary>
    /// <param name="value">The value of the bidPriceInput as a string at the end of editing.</param>
    void OnEndEdit(string value)
    {
        if (!int.TryParse(value, out int result))
            return;
        if (!IsBitAmountIllegal(result))
        {
            ResetBid();
        }
    }

    /// <summary>
    /// This method initializes the bidding process for a given property.
    /// </summary>
    /// <param name="biddingProperty">The BoardPlaceData object representing the property up for bid.</param>
    public void StartBidding(BoardPlaceData biddingProperty)
    {
        ResetHighlighter();
        biddingPlayers.Clear();
        curBidderIndex = 0;
        foreach (Transform child in playerSlots.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (PlayerInfo player in Controller.Players)
        {
            if (player.finishRounds > 0) // can bid only if finish at least 1 round
            {
                GameObject newplayer = Instantiate(playerBidPrefab, playerSlots.transform);
                PlayerBiddingData script = newplayer.GetComponent<PlayerBiddingData>();
                script.SetupPlayer(player, tokenAvatar[player.tokenModel]);
                biddingPlayers.Add(script);
            }
        }
        curBidPlayer = biddingPlayers[0];
        SetHighlighterToPlayer(curBidHighlighter, biddingPlayers[0].gameObject);
        this.biddingProperty = biddingProperty;
        SetCurrentbid(0);
        MaxBidMoney = biddingProperty.propertyPrice * MaxBidMultiplier;
        ResetBid();
        if(biddingPlayers.Count > 5)
        {
            playerSlots.GetComponent<RectTransform>().sizeDelta = new Vector2(980, 220);
        }
        else
        {
            playerSlots.GetComponent<RectTransform>().sizeDelta = new Vector2(780, 220);
        }
    }

    /// <summary>
    /// Submit the bit and move to the next player.
    /// </summary>
    public void OnSubmitBid()
    {
        if(int.TryParse(bidPriceInput.text, out int result))
        {
            if (IsBitAmountIllegal(result))
            {
                SetCurrentbid(result);
                NextBidPlayer();
            }
        }
    }

    /// <summary>
    /// This method is called when the current player gives up on bidding.
    /// </summary>
    public void OnPlayerGiveUp()
    {
        biddingPlayers.Remove(curBidPlayer);
        ResetHighlighter();// prevent destroying the hightlighter object
        Destroy(curBidPlayer.gameObject);
        if (biddingPlayers.Count == 1)
            FinishBidding();
        else
            NextBidPlayer();
    }

    /// <summary>
    /// This method finalizes the bidding process and assigns the property to the winner.
    ///     - will pop out a window to tell who wins the bid.
    /// </summary>
    void FinishBidding()
    {
        UI_Controller UIController = UI_Controller.Instance;
        PlayerInfo bidWinner = biddingPlayers[0].playerInfo;
        if (bidWinner.money < curHighestBid)
        {
            RestartBid();
            UIController.NotifyWindowScript.PopWindow(
                $"{bidWinner.playerName} Cannot Pay the bid price of {curHighestBid}, restart bidding.");
            return;
        }
        bidWinner.PlayerMoneyChange(-curHighestBid);
        biddingProperty.ownerID = bidWinner.playerID;
        bidWinner.ownedProperties.Add(biddingProperty);
        Controller.CanFinishRound();
        UIController.DisableAllWindows();
        UIController.NotifyWindowScript.PopWindow(
            $"{bidWinner.playerName} wins the bid. He owns the property {biddingProperty.boardName}.", 
            () => { Controller.CanFinishRound(); }
        );
        if(bidWinner == Controller.CurPlayer)
            UIcontroller.CanManageProperty(true);
    }

    /// <summary>
    /// This method restarts the bidding process for the same property.
    /// </summary>
    void RestartBid()
    {
        StartBidding(biddingProperty);
    }

    /// <summary>
    /// This method increases the bid amount by 100 and updates the bidPriceInput.
    /// </summary>
    public void OnClickIncreaseBid()
    {
        if (int.TryParse(bidPriceInput.text, out int result))
        {
            int newBid = result + 100;
            SetNewBidInput(newBid);
        }
    }

    /// <summary>
    /// This method resets the bid to the next valid bid amount based on the current highest bid and minimum bid increase.
    /// </summary>
    public void ResetBid()
    {
        int newBid = curHighestBid + MinBidIncrease;
        SetNewBidInput(newBid);
    }

    /// <summary>
    /// This method sets the new bid amount in the bidPriceInput and checks if the new bid is valid.
    /// </summary>
    /// <param name="bidAmount">The new bid amount to be set.</param>
    void SetNewBidInput(int bidAmount)
    {
        if (!IsBitAmountIllegal(bidAmount))
        { // if bid amount still less than player owned money
            SubmitBidButton.interactable = false;
        }
        else
        {
            bidPriceInput.text = bidAmount.ToString();
            bidPriceInput.caretPosition = bidPriceInput.text.Length; // move to end of input field
        }
    }

    /// <summary>
    /// This method advances to the next player in the bidding queue.
    /// </summary>
    void NextBidPlayer()
    {
        curBidderIndex = (curBidderIndex + 1) % biddingPlayers.Count;
        curBidPlayer = biddingPlayers[curBidderIndex];
        
        SetHighlighterToPlayer(curBidHighlighter, curBidPlayer.gameObject);
        SetNewBidInput(curHighestBid + MinBidIncrease);
        if (curBidderIndex == 5)
        {
            playerSlots.transform.localPosition = new Vector3(-130, 0, 0);
        }
        else
        {
            playerSlots.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Sets the current highest bid and updates the highestBidAmount text.
    /// </summary>
    /// <param name="bid">The bid amount to be set as the current highest bid.</param>
    void SetCurrentbid(int bid)
    {
        curHighestBid = bid;
        highestBidAmount.text = curHighestBid.ToString();
    }

    /// <summary>
    /// Attaches the highlighter to a specified player's game object.
    /// </summary>
    /// <param name="highlighter">The game object representing the highlighter.</param>
    /// <param name="player">The game object representing the player.</param>
    void SetHighlighterToPlayer(GameObject highlighter, GameObject player)
    {
        highlighter.transform.SetParent(player.transform);
        highlighter.transform.SetAsFirstSibling();
        highlighter.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// This method checks if a given bid amount is within the legal bidding range.
    /// </summary>
    /// <param name="bidAmount">The bid amount to be checked.</param>
    /// <returns>A boolean indicating whether the bid amount is illegal.</returns>
    bool IsBitAmountIllegal(int bidAmount)
    {
        return bidAmount >= curHighestBid + MinBidIncrease && bidAmount <= Mathf.Max(MaxBidMoney, 9999);
    }
}
