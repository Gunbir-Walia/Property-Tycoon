using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    GameMonitor Monitor { get { return GameMonitor.Instance; } }
    UI_Controller UIcontroller { get { return UI_Controller.Instance; } }
    public Button QuitButton;

    private void Start()
    {
        gameObject.SetActive(false);
        QuitButton.onClick.AddListener(OnClickQuitGame);
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

    public void OnClickContinue()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        //Monitor.PauseGame(false);
    }

    public void OnClickQuitGame()
    {
        Time.timeScale = 1;
        GameController.Instance.ReturnToMainMenu();
        //Monitor.PauseGame(false);
        //Monitor.QuitGame();
    }
}
