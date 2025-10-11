using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomNames", menuName = "StartMenuAssets/RandomNames")]
public class RandomNames : ScriptableObject
{
    public List<string> FirstNames;
    public List<string> LastNames;

    /// <summary>
    /// Generates a random full name by selecting a random first name and a random last name from the respective lists.
    /// </summary>
    /// <returns>A string representing the random full name.</returns>
    public string GetRandomName()
    {
        string firstName = FirstNames[Random.Range(0, FirstNames.Count)];
        string lastName = LastNames[Random.Range(0, LastNames.Count)];
        return $"{firstName} {lastName}";
    }
}
