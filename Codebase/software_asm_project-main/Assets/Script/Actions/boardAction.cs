using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Functions/Board Action")]
public class boardAction : ScriptableObject
{
    List<boardActionListener> listeners = new List<boardActionListener>();

    public void TriggerAction(BoardPlaceData data)
    {
        foreach (boardActionListener listener in listeners)
        {
            listener.OnEventTriggered(data);
        }
    }

    public void AddListener(boardActionListener listener)
    {
        listeners.Add(listener);
    }

    public void RemoveListener(boardActionListener listener)
    {
        listeners.Remove(listener);
    }
}
