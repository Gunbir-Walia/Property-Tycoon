using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Throwdice : MonoBehaviour
{
    public TMP_Text guiDice1text;
    public TMP_Text guiDice2text;
    int[] dicePoints = new int[2] {1, 1};
    GameController Controller { get { return GameController.Instance; } }

    /// <summary>Method to throw two dice and update the UI with the result.<br/>
    ///  - will calls the GameController with the dice throw result of an array of 2 integers.</summary>
    public void ThrowDice()
    {
        dicePoints[0] = Random.Range(1, 6);
        dicePoints[1] = Random.Range(1, 6);

        guiDice1text.text = "" + dicePoints[0];
        guiDice2text.text = "" + dicePoints[1];

        Controller.OnDiceThrow(dicePoints);
    }
}
