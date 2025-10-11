using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardControl : MonoBehaviour
{
    public BoardPlaceData boardData;
    public boardAction action;
    public GameObject[] houses = new GameObject[5];
    int localHouseNum = 0;
    [HideInInspector]public int ownerID;
    GameController controller;

    private void Awake()
    {
        controller = GameController.Instance;
        boardData.housesObject = houses;
    }

    private void Update()
    {
        if(localHouseNum != boardData.house_num)
        {
            localHouseNum = boardData.house_num;
            boardData.OnHousesChange();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        GameObject collider = collision.gameObject;
        if (collider.CompareTag("Player") && collider.GetComponent<PlayerInfo>() == controller.CurPlayer)
        {
            action.TriggerAction(boardData);
        }
    }
}
