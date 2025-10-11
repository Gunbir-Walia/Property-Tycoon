using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum GameMode
{
    FullGame, AbridgedGame
}
public class StartMenuManager : MonoBehaviour
{
    public PlayerSlots Players;
    GameController Controller;
    HashSet<GameObject> tempPlayers = new HashSet<GameObject>();
    GameMode gameMode;

    int timeLimit = 0;
    private void Start()
    {
        Controller = GameController.Instance;
    }
    
    /// <summary>
    /// Called when the game start button is clicked.
    /// </summary>
    public void OnClickGameStart()
    {
        StartCoroutine(LoadScene());
    }

    /// <summary>
    /// Asynchronously loads a new scene.
    /// </summary>
    private IEnumerator LoadScene()
    {
        // load new scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        Destroy(FindAnyObjectByType<EventSystem>().gameObject);
        asyncLoad.allowSceneActivation = true;
        int i = 0;
        foreach (PlayerConfigurations player in Players.PlayerList)
        { // create player and dontdestroy them on load
            GameObject playerObj = Controller.CreatePlayer(i, player.PlayerName.text, player.selectedTokenIndex, player.isAI);
            DontDestroyOnLoad(playerObj);
            tempPlayers.Add(playerObj);
            i++;
        }
        GameMonitor.Instance.SetGame(gameMode, timeLimit);
        // wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return new WaitForFixedUpdate(); //delay for a fixed update frame;
        }
        StartCoroutine(MovePlayers());
        StartCoroutine(UnloadScene());
    }

    /// <summary>
    /// Moves the game controller and initializes the game after a new scene is loaded.
    /// </summary>
    private IEnumerator MovePlayers()
    {
        Debug.LogWarning("Load Scene success");
        yield return new WaitForSeconds(0.1f);
        // temp move the controller to new scene so that the created object by it is in new scene

        Scene newScene = SceneManager.GetSceneByBuildIndex(1);
        SceneManager.MoveGameObjectToScene(Controller.gameObject, newScene);
        Controller.GameStartingInitialization();
    }

    /// <summary>
    /// Sets the game mode to FullGame.
    /// </summary>
    public void OnSelectFullGame()
    {
        gameMode = GameMode.FullGame;
    }

    /// <summary>
    /// Sets the game mode to AbridgedGame and assigns a time limit.
    /// </summary>
    /// <param name="timeLimit">The time limit for the abridged game.</param>
    public void OnSelectAbridgedGame(int timeLimit)
    {
        gameMode = GameMode.AbridgedGame;
        this.timeLimit = timeLimit;
    }

    /// <summary>
    /// Unloads the current scene after a delay and moves all players to the new scene.
    /// </summary>
    IEnumerator UnloadScene()
    {
        yield return new WaitForSeconds(0.2f);
        // dump every player into new scene
        Scene newScene = SceneManager.GetSceneByBuildIndex(1);
        foreach (GameObject player in tempPlayers)
        {
            SceneManager.MoveGameObjectToScene(player, newScene);
        }
        DontDestroyOnLoad(Controller.gameObject); // dont destroy controller on load
        SceneManager.UnloadSceneAsync(gameObject.scene); // unload old scene
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnClickQuit()
    {
        Application.Quit();
    }
}
