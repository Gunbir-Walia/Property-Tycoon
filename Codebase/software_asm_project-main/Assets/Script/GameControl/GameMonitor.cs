using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMonitor : Singleton<GameMonitor>
{
    public GameMode curGameMode;

    public float GameTimeInSecond = 5000f;

    public float remindingTime = 300f;
    public float warningTime = 600f;

    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }
    readonly Color orange = new Color(1, 0.5f, 0);
    private void Start()
    {
    }
    public void SetGame(GameMode mode, int time)
    {
        curGameMode = mode;
        if (mode == GameMode.AbridgedGame)
        {
            GameTimeInSecond = time * 60;
        }
    }

    private void FixedUpdate()
    {
        if (curGameMode == GameMode.AbridgedGame && GameTimeInSecond > 0)
        {
            if (GameTimeInSecond <= 0 || UIcontroller.HUD == null|| UIcontroller.HUD.gameObject.IsDestroyed()) return;
            GameTimeInSecond -= Time.fixedDeltaTime;
            if (GameTimeInSecond <= 0)
            {
                UIcontroller.HudScript.UpdateTimerUI(0, 0, 0, Color.red);
                return;
            }
            int timeLimitHour = Mathf.FloorToInt(GameTimeInSecond / 3600);
            int timeLimitMinute = Mathf.FloorToInt((GameTimeInSecond % 3600) / 60);
            int timeLimitSecond = Mathf.FloorToInt(GameTimeInSecond % 60);
            UIcontroller.HudScript.UpdateTimerUI(timeLimitHour, timeLimitMinute, timeLimitSecond, HandleTimerColor());

            if (GameTimeInSecond <= 0)
            {
                // TODO: End game
            }
        }
        //calculate color based on ratio of time limit and passed time
        Color HandleTimerColor()
        {
            Color timerColor;
            if (GameTimeInSecond <= warningTime)
            {
                // when 5 min left, start turning to red
                float timeRatio = GameTimeInSecond / warningTime;
                timerColor = Color.Lerp(Color.red, orange, timeRatio);
            }
            else if (GameTimeInSecond <= remindingTime)
            {
                // when 5-10 min left, start turning to orange
                float timeRatio = (GameTimeInSecond - warningTime) / (warningTime);
                timerColor = Color.Lerp(orange, Color.white, timeRatio);
            }
            else
            {
                // when 10 or more min left, color is white
                timerColor = Color.white;
            }
            return timerColor;
        }
    }

    public bool IsGameOver()
    {
        return GameTimeInSecond <= 0;
    }

    public void PauseGame(bool isPaused) { Time.timeScale = isPaused ? 0 : 1; }

    public bool IsGamePause() { return Time.timeScale == 0; }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
