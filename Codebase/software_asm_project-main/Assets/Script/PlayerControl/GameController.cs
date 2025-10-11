using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public static class GameConstants
{
    /// <summary>
    /// Stores the rent amounts for different stations on the board.<br/>
    ///  - The count of station starts from 1.
    /// </summary>
    public readonly static int[] StationRent = { 0, 25, 50, 100, 200 };

    /// <summary> Stores the rent multipliers for different utilities on the board.</summary>
    public readonly static int[] UtilitesRentMultiplier = { 0, 4, 10 };

    /// <summary>The tile number of the jail on the board.</summary>
    public readonly static int JailTileNo = 10;

    /// <summary>The amount of money required to pay to release a player from jail.</summary>
    public readonly static int MoneyPaidToReleaseJail = 50;

    /// <summary>The number of rounds if a player choose to stay in jail.</summary>
    public readonly static int RoundsStayInJail = 2;
}

public class GameController : Singleton<GameController>
{
    private void Start()
    {
        InitialzeVar();
        Debug.Log($"current scene: {SceneManager.GetActiveScene().name}");
        if (SceneManager.GetActiveScene().name.Equals("SampleScene") && GameController.Instance == this)
        {
            Debug.Log("auto starting game ini");
            CreatePlayer(0, "new Player 1", 3, false);
            CreatePlayer(1, "new Player 2", 1, false);
            GameStartingInitialization();
        }
    }

    /// <summary> Initializes game variables. </summary>
    void InitialzeVar()
    {
        cardMan = CardManager.Instance;
        UIcontr = UI_Controller.Instance;
        gameMethods = GameMethods.Instance;
        CardActMan = new CardActionManager();
        StateMan = new GameStateManager();
        DicePoints = new int[2];
        CurPlayerTileGoal = -1; //prevent game taking action on start
        finishedPlayerRound = 0;
        EnteredTile = false;
        Tiles = new List<GameObject>();
        CurState = StateMan.Diceing();
    }

    /// <summary> Initializes game starting settings. </summary>
    public void GameStartingInitialization()
    {
        FindObjectsInScene();
        gameMethods.Initialize();
        UIcontr.FindObjectsInScene();
        cardMan.SuffleAllCards();
        for (int i = 0; i < _boardListObj.transform.childCount; i++)
        {
            GameObject tileObj = _boardListObj.transform.GetChild(i).gameObject;
            Tiles.Add(tileObj);
        }
        for (int i = 0; i < Tiles[0].transform.childCount; i++)
        {
            _spawn_position.Add(Tiles[0].transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < _players.Count; i++)
        {
            Vector3 spawnPos = _spawn_position[i].transform.position;
            Quaternion front = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            _players[i].gameObject.transform.rotation = front;
            _players[i].gameObject.transform.position = spawnPos;
            Debug.Log(_players[i].playerName + " created, playerID:" + i);
            //test only
        }
        for (int i = 0; i < _boardDatas.boardDataList.Count; i++)
        {
            _boardDatas.boardDataList[i].tileID = i;
        }
        ChangeCurPlayer(0);
        UIcontr.DiceButton.SetActive(true);
        UIcontr.FinishRoundButton.gameObject.SetActive(false);
        UIcontr.CanManageProperty(false);
    }

    /// <summary>
    /// Creates a new player and sets up their initial properties.
    /// </summary>
    /// <param name="id">The ID of the player.</param>
    /// <param name="name">The name of the player.</param>
    /// <param name="tokenIndex">The index of the player's token.</param>
    /// <param name="isAI">Indicates whether the player is AI controlled.</param>
    /// <returns>The newly created player game object.</returns>
    public GameObject CreatePlayer(int id, string name, int tokenIndex, bool isAI)
    {
        GameObject newPlayer = Instantiate(_playerPrefab[tokenIndex]);
        PlayerInfo newPlayerInfo = newPlayer.AddComponent<PlayerInfo>();
        newPlayerInfo.SetupPlayer(id, tokenIndex, name, isAI);
        _players.Add(newPlayerInfo);
        return newPlayer;
    }

    /// <summary>
    /// Execute the action when a player is allowed to finished a round.
    /// </summary>
    public void CanFinishRound()
    {
        UIcontr.CanManageProperty(true);
    }

    /// <summary>
    /// Finishes the current player's round and sets up the next player, and preform these actions: <br/>
    ///  - if a player throws a double, he will continue to next round.<br/>
    ///  - if a player is in jail, he will stay in jail and deduct the rounds that he left in jail.<br/>
    ///  - if the player changes, performs a transition of camera from current player to next player.<br/>
    ///  - closes all the windows that currently displays, and disable the buttons to open or sell any property.
    /// </summary>
    public void FinishRound()
    {
        Debug.Log($"{CurPlayer.playerName} round finish");
        UIcontr.DisableAllWindows();
        UIcontr.FinishRoundButton.gameObject.SetActive(false);
        UIcontr.DiceButton.SetActive(true);
        UIcontr.CanManageProperty(false);
        CurPlayerSentToJail = false;
        CurState.SwitchState(StateMan.Diceing());
        if (RepeatRound)
        {
            return; //continue with same player
        }
        int newPlayerID = CurPlayer.playerID;
        while (true)
        {
            newPlayerID = (newPlayerID + 1) % _players.Count;
            if (newPlayerID == 0) OnAllPlayerFinishRound();
            if (!_players[newPlayerID].gameObject.activeSelf) continue;
            if (_players[newPlayerID].IsInJail())
            {// if player in jail 
                _players[newPlayerID].roundsLeftInJail--;
            }
            else break;
        }
        ChangeCurPlayer(newPlayerID);
        gameMethods.RoundFinishCamTransition(); // move camera to other player
    }

    /// <summary> Changes the current player to the player with the specified ID. </summary>
    /// <param name="playerID">The ID of the player to set as current.</param>
    void ChangeCurPlayer(int playerID)
    {
        CurPlayer = _players[playerID];
        CurPlayerAgent = _players[playerID].Agent;
        CurPlayerTileGoal = CurPlayer.curTile;
        CurCard = null;
        UIcontr.HudScript.SetPlayer(CurPlayer);
        UIcontr.CanManageProperty(true);
        UIcontr.SellingUIScript.ResetUI();
    }

    //test only
    /// <summary> Moves the current player to a location when the mouse is clicked. </summary>
    void MoveToLoc()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                CurPlayerAgent.SetDestination(hit.point);
            }
        }
    }
    void Update()
    {
        //MoveToLoc();
    }

    /// <summary> Handles actions when the player enters a board tile. </summary>
    /// <param name="data">The data of the board tile entered.</param>
    public void OnEnteringBoard(BoardPlaceData data)
    {
        if (EnteredTile && !CurPlayerSentToJail)
        {
            BoardType enteredBoardType = data.boardType;
            Debug.Log($"[{CurPlayer.playerName}] entered {data.boardName}: Block type: {enteredBoardType}, CurPlayerTile:  {CurPlayerTileGoal} , tileID:  {data.tileID} , EnteredTile: {EnteredTile}");
            SetPlayerGoalTile(data.tileID);
            //_throwDicesUI.SetActive(false);
            if (enteredBoardType == BoardType.GO)
            {
                CanFinishRound();
                UIcontr.FinishRoundButton.gameObject.SetActive(true);
                return;
            }
            GameBaseStates newState = StateMan.GetGameStateByBoardType(enteredBoardType);


            if (enteredBoardType == BoardType.ToJail && !CurPlayer.HasJailFreeCard())
            {
                UIcontr.BoardUIScript.SetTitleAndDesc("Jail", GameLocalization.Instance.JailMessages[0]);
                //UIcontr.BoardUIScript.ShowJailOptions();
            }
            if (newState != null)
            {
                CurState.SwitchState(newState);
                CurState.RaiseStateEvents();
                // card action needs to wait for after player picks card
            }
            if (!(enteredBoardType == BoardType.GO || enteredBoardType == BoardType.ToJail))
            {
                //UIcontr.BoardUIScript.ShowBoardDetail(data);
            }
            UIcontr.SetFinishRoundButtonEnable();
        }
    }
    #region Trigger Actions
    /// <summary> Triggers actions when the dice is thrown. </summary>
    /// <param name="points">The points rolled on the dice.</param>
    public void OnDiceThrow(int[] points)
    {
        EnteredTile = false;
        DicePoints = points;
        CurState.SwitchState(StateMan.Diceing());
        CurState.RaiseStateEvents();
        StartCoroutine(HideDiceUI(1f));
        //_curState.SwitchState()
    }
    /// <summary> Handles actions when the player enters the "GO" tile. </summary>
    public void OnEnteringGo()
    {
        if (CurPlayer.passingGO) // passingGo should only sets to true when going to new tile
        {
            Debug.Log($"{CurPlayer.playerName} passes go");
            CurPlayer.PlayerMoneyChange(200);
            CurPlayer.passingGO = false;
            CurPlayer.finishRounds++;
        }
    }
    /// <summary>Triggers actions when the player picks a card.</summary>
    public void OnPickCard()
    {
        CurState.SwitchState(StateMan.CARD());
        CurState.RaiseStateEvents();
    }
    /// <summary>Send the player to Jail and execute any action in JailState.</summary>
    public void EnterJail()
    {
        CurState.SwitchState(StateMan.JAIL());
        CurState.RaiseStateEvents();
    }
    /// <summary>Raises the card action for the current card.</summary>
    public void RaiseCardAction()
    {
        CardAction.ExecuteCardActions(CurCard);
    }

    /// <summary> Pays to get out of jail. </summary>
    public void PayToGetOutJail()
    {
        CurPlayer.PayToFacility(PayTarget.PARK, GameConstants.MoneyPaidToReleaseJail);
        CanFinishRound();
    }

    /// <summary> Stays in jail for a specified number of rounds. </summary>
    public void StayInJail()
    {
        CurPlayer.StayJailForRounds(GameConstants.RoundsStayInJail);
        FinishRound(); // Change to next player directly because in jail
    }
    #endregion
    /// <summary> Sets the destination for the current player's agent. </summary>
    /// <param name="goalID">The ID of the goal tile.</param>
    public void AgentSetDestination(int goalID)
    {
        //CurPlayer.Agent.isStopped = false;
        if (CurPlayer.Agent != null)
        {
            CurPlayer.Agent.SetDestination(Tiles[goalID].transform.position);
        }
    }
    /// <summary> Sets the current player's goal tile. </summary>
    /// <param name="tile">The ID of the tile to set as the goal.</param>
    public void SetPlayerGoalTile(int tile)
    {
        CurPlayerTileGoal = tile;
        CurPlayer.curTile = tile;
    }

    #region Pay Money Fuctions
    /// <summary> Transfers money from a facility to a player. </summary>
    /// <param name="payer">The facility paying the money.</param>
    /// <param name="receiver">The player receiving the money.</param>
    /// <param name="amount">The amount of money to transfer.</param>
    public void FacilityPayToPlayer(PayTarget payer, PlayerInfo receiver, int amount)
    {
        receiver.PlayerMoneyChange(amount);
        switch (payer)
        {
            case PayTarget.PARK:
                ParkMoney -= amount;
                return;
            case PayTarget.BANK:
                //BankMoney -= amount; // bank has infinate money
                return;
        }
    }
    /// <summary>
    /// Changes the player's money by a specified amount.<br/>
    ///  - if the player don't have enough money to pay, a window will pop up to tell the player to sell property to pay the money.<br/>
    ///  - if the player overall deposit (counting in all the price of the property of this player owns) cannot affort the payment, <br/>
    ///  the player will be bankruptcy and lose the game.
    /// </summary>
    /// <param name="player">The player whose money is being changed.</param>
    /// <param name="amount">The amount to change the player's money by.</param>
    public void PlayerMoneyChange(PlayerInfo player, int amount)
    {
        if (amount == 0) return;
        if (amount > 0 || (amount < 0 && player.money - Mathf.Abs(amount) > 0))
        {
            player.money += amount;
            // show money change animation if the current player is the player that pays/receive money
            if (CurPlayer == player)
                UIcontr.HudScript.MoneyChange(amount);
        }
        else
        {
            if (player.HasEnoughMoney(Mathf.Abs(amount))){
                Debug.Log("player don't have enough money to pay");
                UIcontr.PopWindow(
                    $"You don't have enough money to pay £{amount}, sell or mortgage a property",
                    () => { UIcontr.SellPropertyUI.SetActive(true); },
                    "Sell", null,
                    () => { PlayerMoneyChange(player, amount); },
                    "Pay", () => { return player.money >= amount; });
            } else
            {

                PlayerLose(player);
            }
        }
    }

    /// <summary>Handles actions when all players finish a round.</summary>
    void OnAllPlayerFinishRound()
    {
        finishedPlayerRound++;
        if (GameMonitor.Instance.IsGameOver())
        {
            EndGame();
        }
    }
    #endregion

    /// <summary>
    /// Ends the game and determines the winner(s).
    /// </summary>
    public void EndGame()
    {
        UIcontr.DisableAllWindows();
        HashSet<PlayerInfo> winners = new HashSet<PlayerInfo>();
        int maxOverallDeposit = int.MinValue;
        foreach (PlayerInfo player in _players)
        {
            if (player.gameObject.activeSelf)
            {
                int playerOverallDeposit = player.GetOverallDeposit();
                if (playerOverallDeposit > maxOverallDeposit)
                {
                    maxOverallDeposit = playerOverallDeposit;
                    winners.Clear(); 
                    winners.Add(player);
                }
                else if (playerOverallDeposit == maxOverallDeposit)
                { // if two players have same overall deposit, they are both winner
                    winners.Add(player); 
                }
            }
        }
        StringBuilder winnerNames = new StringBuilder();
        foreach (PlayerInfo player in winners)
        {
            winnerNames.Append(player.playerName).Append(", ");
        }
        winnerNames.Remove(winnerNames.Length - 2, 2);
        UIcontr.PopWindow($"Game Over, {winnerNames.ToString()} wins the game!",
            () => { ReturnToMainMenu(); });
    }

    /// <summary>
    /// Handles a player when he losing the game.<br/>
    ///  - if there's only one player left, declear the winner and finish the game, otherwise pass the round to next player.<br/>
    ///  - if the player has any property and cards left, clear them all and return the cards to the suffled list.<br/>
    /// </summary>
    /// <param name="player">The player who lost the game.</param>
    public void PlayerLose(PlayerInfo player)
    {
        UIcontr.DisableAllWindows();
        player.gameObject.SetActive(false);
        if (player.ownedProperties.Count > 0)
        {
            foreach (BoardPlaceData properties in player.ownedProperties)
            { // sell all of the properties
                properties.ownerID = -1;
                properties.house_num = 0;
            }
        }
        if (player.RetainCards.Count > 0)
        {
            foreach (BoardCards card in player.RetainCards)
            {
                if (card.BelongedCardList == CardListType.PotLuck)
                {
                    cardMan.AddCardToPotLuckCards(card);
                }
                else if (card.BelongedCardList == CardListType.OpportunityKnocks)
                {
                    cardMan.AddCardToOpportunityKnocksCards(card);
                }
            }
        }
        if (_players.Count(player => player.gameObject.activeSelf) > 1)
        {
            if (player = CurPlayer)
            {
                FinishRound();
            }
            UIcontr.PopWindow($"{player.playerName} is bankruptcy and cannot continue the game");
        }
        else
        {
            UIcontr.PopWindow($"Game Over, {_players[0].playerName} wins the game!", 
                () => { ReturnToMainMenu(); });
        }
    }

    /// <summary> Returns to the main menu and resets game variables.</summary>
    public void ReturnToMainMenu()
    {
        _players.Clear();
        Tiles.Clear();
        _spawn_position.Clear();
        SceneManager.LoadScene(0);
    }

    /// <summary> Hides the dice UI after a specified time. </summary>
    /// <param name="time">The time in seconds to wait before hiding the dice UI.</param>
    /// <returns>An IEnumerator to run the coroutine.</returns>
    IEnumerator HideDiceUI(float time)
    {
        yield return new WaitForSeconds(time);
        UIcontr.DiceButton.SetActive(false);
    }

    #region Variables
    public BoardList _boardDatas;
    public int _player_start_money;
    //public int BankMoney;
    GameObject _boardListObj;

    public List<GameObject> _playerPrefab = new List<GameObject>();
    List<PlayerInfo> _players = new List<PlayerInfo>();
    List<GameObject> _spawn_position = new List<GameObject>();

    Camera _cam;
    NavMeshAgent CurPlayerAgent;

    GameMethods gameMethods;
    CardManager cardMan;
    UI_Controller UIcontr;
    GameStateManager StateMan;

    [HideInInspector] public bool CurPlayerSentToJail = false;
    [HideInInspector] public int ParkMoney;
    public int finishedPlayerRound { get; private set; }

    private void FindObjectsInScene()
    {
        _boardListObj = GameObject.FindGameObjectWithTag("BoardObjects");
        _cam = Camera.main;
    }
    // getter and setters
    public int[] DicePoints { get; private set; }
    public BoardList BoardDatas => _boardDatas;
    public List<GameObject> Tiles { get; private set; }
    public List<PlayerInfo> Players => _players; 

    public bool RepeatRound { get; set; }
    public PlayerInfo CurPlayer { get; private set; }
    public int CurPlayerTileGoal { get; set; }
    public bool EnteredTile { get; set; }

    public GameBaseStates CurState { get; set; }
    public CardActionManager CardActMan { get; private set; }
    public CardActions CardAction { get; set; }
    public BoardCards CurCard { get; set; }
    public PlayableDirector Director { get; private set; }
    #endregion
}
