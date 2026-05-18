using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HairEntry
{
    public string styleName;
    public Sprite hairSprite;
    public int styleID;
}

[CreateAssetMenu(fileName = "HairDatabase", menuName = "Game/Hair Database")]
public class HairData : ScriptableObject
{
    [Header("Danh sách tất cả các kiểu tóc trong Game")]
    public List<HairEntry> allHairs = new List<HairEntry>();

    public HairEntry GetRandomHair()
    {
        if (allHairs.Count == 0) return null;
        return allHairs[Random.Range(0, allHairs.Count)];
    }
}