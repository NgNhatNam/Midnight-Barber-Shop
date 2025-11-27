using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HairData", menuName = "Game/Hair Data")]
public class HairData : ScriptableObject
{
    [Header("Tên loại tóc (Đẹp, Trung bình, Xấu)")]
    public string styleName;

    [Header("Danh sách tóc có thể xuất hiện")]
    public List<Sprite> hairSprites;
}
