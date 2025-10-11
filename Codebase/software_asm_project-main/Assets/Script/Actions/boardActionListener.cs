using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class boardActionListener : MonoBehaviour
{
    public boardAction action;
    public UnityEvent<BoardPlaceData> eventTriggered;

    void OnEnable()
    {
        action.AddListener(this);
    }
    void OnDisable()
    {
        action.RemoveListener(this);
    }

    public void OnEventTriggered(BoardPlaceData data)
    {
        eventTriggered.Invoke(data);
    }
}
