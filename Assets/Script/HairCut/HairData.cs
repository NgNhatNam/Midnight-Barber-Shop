using System.Collections.Generic;
using UnityEngine;

// Class này dùng để định nghĩa từng kiểu tóc lẻ
[System.Serializable]
public class HairEntry
{
    public string styleName;
    public Sprite hairSprite;
    public int styleID;
}

// ScriptableObject chính chứa danh sách tất cả kiểu tóc
[CreateAssetMenu(fileName = "HairDatabase", menuName = "Game/Hair Database")]
public class HairData : ScriptableObject
{
    [Header("Danh sách tất cả các kiểu tóc trong Game")]
    public List<HairEntry> allHairs = new List<HairEntry>();

    // Hàm hỗ trợ lấy nhanh một kiểu tóc ngẫu nhiên
    public HairEntry GetRandomHair()
    {
        if (allHairs.Count == 0) return null;
        return allHairs[Random.Range(0, allHairs.Count)];
    }
}