using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour
{
    public TMP_Text _playerName;
    public TMP_Text _playerMoney;
    public TMP_Text _moneyChange;
    public TMP_Text _roundsCount;
    public TMP_Text _timeCountdown;
    public GameObject _timerParent;

    public TMP_Text[] _timerTexts = new TMP_Text[5];
    Color _timeColor;

    public float timeTextFlashInterval = 1f;
    public float timeTextFlashTime = 0.3f;
    float _flashTimer = 0f;
    bool _isFlashing = false;

    public float _MoneyTextSpeedY = 5f;
    public float _MoneyTextFadingSpeed = 100f;

    public Button SellProperty;

    Vector2 _originTextPos;

    PlayerInfo _player;

    GameController Controller { get { return GameController.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }

    void Start()
    {
        _originTextPos = new Vector2 (_moneyChange.rectTransform.anchoredPosition.x, _moneyChange.rectTransform.anchoredPosition.y);
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

    void Update()
    {
        if (_moneyChange.gameObject.activeInHierarchy)
        {
            if (_moneyChange.color.a > 0)
            {
                Color textColor = _moneyChange.color;
                textColor.a -= _MoneyTextFadingSpeed * Time.deltaTime;
                _moneyChange.color = textColor;
                //Debug.Log("_moneyChange.alpha: " + _moneyChange.color.a);
                Vector2 textPos = _moneyChange.rectTransform.anchoredPosition;
                textPos.y += _MoneyTextSpeedY * Time.deltaTime;
                _moneyChange.rectTransform.anchoredPosition = textPos;
            }
            else
            {
                _moneyChange.gameObject.SetActive(false);
            }
        }
        HandleColonFlashing();
    }

    /// <summary> Handles the flashing of the colons in the timer UI.</summary>
    private void HandleColonFlashing()
    {
        _flashTimer += Time.deltaTime;

        if (_isFlashing == true)
        {
            if (_flashTimer >= timeTextFlashTime)
            {
                _isFlashing = !_isFlashing;
                _flashTimer = 0f;
            }
        }
        else
        {
            if (_flashTimer >= timeTextFlashInterval)
            {
                _isFlashing = !_isFlashing;
                _flashTimer = 0f;
            }
        }

        // flash colons
        _timerTexts[1].color = _isFlashing ? Color.clear : _timeColor;
        _timerTexts[3].color = _isFlashing ? Color.clear : _timeColor;
    }

    /// <summary>
    /// Updates the timer UI with the given hour, minute, second, and color by setting the text and colors of corresponding text component
    /// </summary>
    /// <param name="hour">The hour value to display.</param>
    /// <param name="minute">The minute value to display.</param>
    /// <param name="second">The second value to display.</param>
    /// <param name="color">The color to apply to the timer text.</param>
    public void UpdateTimerUI(int hour, int minute, int second, Color color)
    {
        _timerTexts[0].text = hour.ToString();
        _timerTexts[2].text = (minute < 10) ? "0" + minute : minute.ToString();
        _timerTexts[4].text = (second < 10) ? "0" + second : second.ToString();
        _timerTexts[0].color = color;
        _timerTexts[2].color = color;
        _timerTexts[4].color = color;
        _timeColor = color;
    }

    /// <summary>
    /// Sets the player information and updates the corresponding UI elements.<br/>
    /// Updates the player name, money, and rounds count displayed in the HUD.
    /// </summary>
    /// <param name="player">The PlayerInfo object containing the player's data.</param>
    public void SetPlayer(PlayerInfo player)
    { 
        _player = player;
        _playerName.text = player.playerName;
        _playerMoney.text = "£ " + player.money;
        _roundsCount.text = Controller.finishedPlayerRound.ToString();
    }

    /// <summary>
    /// Updates the money change text.<br/>
    /// - Will displays the money change amount with appropriate color and animates a fade out effect.
    /// </summary>
    /// <param name="money">The amount of money change to display.</param>
    public void MoneyChange(int money)
    {
        if(money > 0)
        {
            _moneyChange.text = "+£" + Mathf.Abs(money);
            _moneyChange.color = Color.green;
        } else
        {
            _moneyChange.text = "-£" + Mathf.Abs(money);
            _moneyChange.color = Color.red;
        }
         
        _moneyChange.rectTransform.anchoredPosition = _originTextPos;
        _moneyChange.gameObject.SetActive(true);
        //Debug.Log("moneyChange text enabled: " + _moneyChange.enabled);
        _playerMoney.text = "£ " + _player.money;
    }
}
