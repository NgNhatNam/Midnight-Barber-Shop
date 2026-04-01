using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SleepSlot : MonoBehaviour
{
    public TextMeshProUGUI infoText; // Hiện: "Ngủ 5 giờ (+75 MN)"
    public Button sleepButton;
    

    public void Setup(string label, int hours, int mana)
    {
        if (infoText) infoText.text = $"{label} <color=blue>+{mana} MN</color>";

        sleepButton.onClick.RemoveAllListeners();
        sleepButton.onClick.AddListener(() => {
            SleepUI.Instance.ExecuteSleep(hours, mana);
        });
    }
}