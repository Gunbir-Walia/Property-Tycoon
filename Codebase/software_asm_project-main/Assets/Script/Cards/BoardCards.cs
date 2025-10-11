using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Game Functions/Board Cards")]
public class BoardCards : ScriptableObject
{
    [SerializeField] private int _cardID;
    [SerializeField] private string _cardDesc;

    [SerializeField] private CardType _cardType;
    [SerializeField] private bool _isMoveFixStep = false;
    [SerializeField] private int _moveSteps;
    [SerializeField] private bool _isPassGo = true;
    [SerializeField] private PlayerMoveType _moveType;

    //[Header("money paid")]
    [SerializeField] private BoardPlaceData _moveToBoard;
    [SerializeField] private int _moneyPaid;

    [SerializeField] private PayTarget _payTo;
    [SerializeField] private PayTarget _payFrom;
    [SerializeField] private Sprite _cardSprite;

    [SerializeField] private int _houseRepairPrice;
    [SerializeField] private int _hotelRepairPrice;

    public int CardID => _cardID;
    public string CardDesc => _cardDesc;
    public bool IsMoveFixStep => _isMoveFixStep;
    public int MoveSteps => _moveSteps;
    public bool IsPassGo => _isPassGo;
    public BoardPlaceData MoveToBoard => _moveToBoard;
    public PlayerMoveType MoveType => _moveType;
    public int MoneyPaid => _moneyPaid;
    public PayTarget PayTo => _payTo;
    public PayTarget PayFrom => _payFrom;
    public CardType CardType => _cardType;
    public Sprite CardSprite => _cardSprite;

    public int HouseRepairPrice => _houseRepairPrice;
    public int HotelRepairPrice => _hotelRepairPrice;

    public CardListType BelongedCardList;
}


public enum PayTarget { BANK, PARK, PLAYER, ALLPLAYER };
public enum CardType { MOVE, PAY, JAILFREE, REPAIR, TakeOtherCard }
public enum PlayerMoveType { FORWARD, BACKWARD, TELEPORT };
