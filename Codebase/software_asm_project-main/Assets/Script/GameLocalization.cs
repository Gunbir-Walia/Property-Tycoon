using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Localization/Messages")]
public class GameLocalization : SingletonScriptObj<GameLocalization>
{
    [Header("Jail Messages")]
    [Space(5)]
    [TextArea] public List<string> JailMessages;
    public List<string> PropertyUIMessages;
    [TextArea] public List<string> BoardUIMessages;

}
