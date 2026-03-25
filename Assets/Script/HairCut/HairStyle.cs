using UnityEngine;

[CreateAssetMenu(fileName = "HairStyle", menuName = "Game/Hair Style")]
public class HairStyle : ScriptableObject
{
    public string hairID; 
    public Sprite requirementSprite; 

    [Header("Kết quả hiển thị sau khi cắt")]
    public Sprite perfectHair; // Tóc rất đẹp (Score 10)
    public Sprite goodHair;    // Tóc đẹp (Score 9)
    public Sprite normalHair;  // Tóc trung bình (Score 7)
    public Sprite badHair;     // Tóc xấu (Score 5)
    public Sprite veryBadHair; // Tóc rất tệ (Score 3)
}