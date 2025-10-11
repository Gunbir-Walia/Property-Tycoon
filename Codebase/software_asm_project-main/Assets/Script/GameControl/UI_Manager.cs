using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Button DiceButton;
    public GameObject BoardDetailUI;
    public GameObject PropertyUI;
    public GameObject CardUI;
    public GameObject HUD;
    public GameObject BiddingUI;
    public GameObject NotifyWindow;
    public GameObject UtilitesUI;
    public GameObject SellingUI;
    public GameObject PauseMenu;
    public GameObject PropertyReviewUI;
    [Space(10)]
    public Button PauseButton;
    public Button FinishRoundButton;
    public Button ReviewPropertyButton;
    public Button SellPropertyButton;
    GameMonitor Monitor { get { return GameMonitor.Instance; } }
    public GameController Controller { get { return GameController.Instance; } }

    private void Start()
    {
        PauseButton.onClick.AddListener(OnPauseClick);
        FinishRoundButton.onClick.AddListener(OnFinsihRoundClick);
    }

    public void OnPauseClick()
    {
        PauseMenu.SetActive(true);
        Time.timeScale = 0;
        //Monitor.PauseGame(true);
    }

    public void OnFinsihRoundClick()
    {
        Controller.FinishRound();
        FinishRoundButton.gameObject.SetActive(false);
    }
}
