using System.Linq;
using TMPro;
using UnityEngine;

public class TestMethods : MonoBehaviour
{
    CardManager CardManager;
    CameraControl _camController;

    public TMP_InputField[] inputs = new TMP_InputField[9];
    public GameController Controller { get { return GameController.Instance; } }
    public GameMethods gameMethods { get { return GameMethods.Instance; } }
    public UI_Controller UIcontroller { get { return UI_Controller.Instance; } }


    private void Awake()
    {
        CardManager = CardManager.Instance;
        _camController = GameObject.FindGameObjectWithTag("Director").GetComponent<CameraControl>();
    }

    private void Start()
    {
        foreach (TMP_InputField input in inputs)
        {
            input.onValueChanged.AddListener((value) => OnValueChanged(input, value));
        }
        inputs[0].onEndEdit.AddListener((value) => LimitNumberBelowMaxValue(inputs[0], value, 39));
        inputs[1].onEndEdit.AddListener((value) => LimitNumberBelowMaxValue(inputs[1], value, 16));
        inputs[2].onEndEdit.AddListener((value) => LimitNumberBelowMaxValue(inputs[2], value, 15));
        inputs[3].onEndEdit.AddListener((value) => LimitNumberBelowMaxValue(inputs[3], value, 39));
        inputs[4].onEndEdit.AddListener((value) => LimitNumberBelowMaxValue(inputs[4], value, 39));
        inputs[6].onEndEdit.AddListener((value) => LimitNumberBetweenValues(inputs[6], value, 1, 6));
        inputs[7].onEndEdit.AddListener((value) => LimitNumberBetweenValues(inputs[7], value, 1, 6));
    }

    void OnValueChanged(TMP_InputField input, string value)
    {
        if (!int.TryParse(value, out int result))
            // remove char where is not numbers
            input.text = new string(input.text.Where(char.IsDigit).ToArray());
    }

    void LimitNumberBelowMaxValue(TMP_InputField input, string value, int Max)
    {
        if (!int.TryParse(value, out int result))
            return;
        if(result > Max)
            input.text = Max.ToString();
    }

    void LimitNumberBetweenValues(TMP_InputField input, string value, int Min, int Max)
    {
        if (!int.TryParse(value, out int result))
            return;
        if (result < Min)
            input.text = Min.ToString();
        else if (result > Max)
            input.text = Max.ToString();
    }
    public void GoToTile()
    {
        if (inputs[0].text.Trim() == "") return;
        int targetTile = (int.TryParse(inputs[0].text, out int result)) ? result : 0;
        gameMethods.GoToTile(targetTile);
    }
    public void GoToPotLuck()
    {
        gameMethods.GoToTile(2);
    }
    public void BackwardGoToTile()
    {
        if (inputs[0].text.Trim() == "") return;
        int targetTile = (int.TryParse(inputs[0].text, out int result)) ? result : 0;
        gameMethods.BackwardMoveToTile(targetTile);
    }

    public void PopPotluckCardOfNo()
    {
        if (inputs[1].text.Trim() == "") return;
        int PopLuckCardNo = (int.TryParse(inputs[1].text, out int result)) ? result : 0;
        CardManager.PopCardToTop(
            CardManager.GetCardListOf(CardListType.PotLuck).cardList[PopLuckCardNo], 
            CardListType.PotLuck
        );
    }

    public void PopOppKnockCardOfNo()
    {
        if (inputs[2].text.Trim() == "") return;
        int OPK_CardNo = (int.TryParse(inputs[2].text, out int result)) ? result : 0;
        CardManager.PopCardToTop(
            CardManager.GetCardListOf(CardListType.OpportunityKnocks).cardList[OPK_CardNo], 
            CardListType.OpportunityKnocks
        );
    }

    public void AllPlayerRoundsIncrease()
    {
        foreach(PlayerInfo player in Controller.Players)
        {
            player.finishRounds++;
        }
    }

    public void BuyProperty()
    {
        if (inputs[3].text.Trim() == "") return;
        int buyPropertyAt = (int.TryParse(inputs[3].text, out int result)) ? result : 0;
        BoardPlaceData data = Controller.BoardDatas.boardDataList[buyPropertyAt];
        if (data.boardType == BoardType.Property ||
            data.boardType == BoardType.Station ||
            data.boardType == BoardType.Utility)
        {
            if(data.ownerID == Controller.CurPlayer.playerID)
            {
                UIcontroller.PopWindow($"this property({data.boardName}) is already yours");
            }
            else
            {
                UIcontroller.PopWindow($"You owned {data.boardName}");
                data.ownerID = Controller.CurPlayer.playerID;
                UIcontroller.CanManageProperty(true);
                Controller.CurPlayer.ownedProperties.Add(data);
            }
        } else
        {
            UIcontroller.PopWindow($"You can't buy this property ({data.boardName}), it is not a property, station or utitity.");
        }
    }

    public void UpgradeProperty()
    {
        if (inputs[4].text.Trim() == "") return;
        int upgradePropertyAt = (int.TryParse(inputs[4].text, out int result)) ? result : 0;
        BoardPlaceData data = Controller.BoardDatas.boardDataList[upgradePropertyAt];
        if (data.boardType == BoardType.Property)
        {
            if (data.ownerID == Controller.CurPlayer.playerID)
            {
                data.house_num++;
                UIcontroller.PopWindow($"property({data.boardName}) is upgraded to {data.house_num} houses");
            }
            else
            {
                UIcontroller.PopWindow($"You cannot upgrade {data.boardName} because you don't owned it.");
                //data.ownerID = Controller.CurPlayer.playerID;
            }
        }
        else
        {
            UIcontroller.PopWindow($"You can't upgrade property ({data.boardName}), it is not a property.");
        }
    }

    public void SetPlayerMoney()
    {
        if (inputs[5].text.Trim() == "") return;
        Controller.CurPlayer.money = (int.TryParse(inputs[5].text, out int result)) ? result : 0;
        UIcontroller.HudScript._playerMoney.text = "£ " + result;
    }

    public void EndGameImmidiately()
    {
        Controller.EndGame();
    }

    public void SetAndThrowDice()
    {
        if (inputs[6].text.Trim() == "" || !int.TryParse(inputs[6].text, out int result1)) return;
        if (inputs[7].text.Trim() == "" || !int.TryParse(inputs[7].text, out int result2)) return;
        int[] dicePoints = new int[2] { result1, result2 };
        Throwdice diceScript = UIcontroller.DiceButton.GetComponent<Throwdice>();
        diceScript.guiDice1text.text = inputs[6].text;
        diceScript.guiDice2text.text = inputs[7].text;
        Controller.OnDiceThrow(dicePoints);
    }

    public void SetTime()
    {
        if (inputs[8].text.Trim() == "" || !int.TryParse(inputs[8].text, out int result)) return;
        GameMonitor.Instance.GameTimeInSecond = result;
    }
}
