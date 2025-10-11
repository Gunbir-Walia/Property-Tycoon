using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBiddingData : MonoBehaviour
{
    [HideInInspector] public PlayerInfo playerInfo;
    public Image playerAvatar;
    public TMP_Text playerName;


    public void SetupPlayer(PlayerInfo playerInfo, Sprite avatar)
    {
        this.playerInfo = playerInfo;
        playerAvatar.sprite = avatar;
        playerName.text = playerInfo.playerName;
    }
}
