using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Functions/Board List")]
public class BoardList : ScriptableObject
{
    public List<BoardPlaceData> boardDataList;
}
