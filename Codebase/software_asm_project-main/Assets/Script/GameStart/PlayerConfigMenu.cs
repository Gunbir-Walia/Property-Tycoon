using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfigMenu : MonoBehaviour
{
    public GameObject MenuButtons; // Button Group in the menu
    public GameObject SelectorMenu; // Selector Menu
    public GameObject MainMenuButtons;

    PlayerConfigurator _configurator;
    PlayerSlots _playerSlotScript;

    private void Awake()
    {
        _configurator = SelectorMenu.GetComponentInChildren<PlayerConfigurator>();
        _playerSlotScript = GetComponent<PlayerSlots>();
        _playerSlotScript.Initialize(_configurator);
    }
    public void SetSelectorMenuActive(bool b)
    {
        MenuButtons.SetActive(!b);
        SelectorMenu.SetActive(b);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickExit();
        }
    }

    public void OnClickExit()
    {
        MainMenuButtons.SetActive(true);
        gameObject.SetActive(false);
    }
}
