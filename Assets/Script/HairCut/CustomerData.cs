using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "Game/Customer Data")]
public class CustomerData : ScriptableObject
{
    public string customerType; // "Normal", "Soul", "Boss"
    public List<Sprite> sprites; // Các sprite khác nhau của loại khách này
    [TextArea(2, 6)] public List<string> dialogues; // Các câu nói ngẫu nhiên
}