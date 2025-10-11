using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerConfigurations : MonoBehaviour
{
    public Button AvatarButton;
    public TMP_Text PlayerName;
    RawImage Image;
    [HideInInspector] public int selectedTokenIndex = -1;
    [HideInInspector] public bool isAI = false;

    private void OnEnable()
    {
        if (AvatarButton != null) AvatarButton.onClick.AddListener(SelectAvatar);
        Image = GetComponentInChildren<RawImage>();
    }

    private void OnDisable()
    {
        if (AvatarButton != null) AvatarButton.onClick.RemoveListener(SelectAvatar);
    }

    /// <summary>
    /// Sets the currently selected player to this instance if it is not already selected.
    /// </summary>
    public void SelectAvatar()
    {
        if (PlayerConfigurator.CurSelectPlayer != null && PlayerConfigurator.CurSelectPlayer != this)
        {
            PlayerConfigurator.CurSelectPlayer = this;
        }
    }

    /// <summary>
    /// Sets the texture of the RawImage to the texture of the provided Sprite.
    /// </summary>
    /// <param name="sprite">The Sprite whose texture will be applied to the RawImage.</param>
    public void SetImage(Sprite sprite)
    {
        Image.texture = sprite.texture;
    }  

    /// <summary>
    /// Sets the text of the PlayerName TMP_Text component to the provided name.
    /// </summary>
    /// <param name="name">The name to set for the player.</param>
    public void SetPlayerName(string name)
    {
        PlayerName.text = name;
    }
}
