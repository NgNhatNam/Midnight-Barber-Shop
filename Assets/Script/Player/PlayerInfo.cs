using UnityEngine;
using TMPro;
using DPUtils.System.DateTime;

public class PlayerInfo : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text levelText;
    public TMP_Text expText;
    public TMP_Text moneyText;
    public TMP_Text daysText;
    public TMP_Text customersText;
    public GameObject infoPanel;

    private Health playerHealth;
    private TimeManager timeManager;

    private void Start()
    {
        playerHealth = FindAnyObjectByType<Health>();
        timeManager = FindAnyObjectByType<TimeManager>();

        // Mặc định ẩn bảng khi bắt đầu
        //if (infoPanel != null) infoPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (playerHealth == null) return;

        // Cập nhật các chỉ số
        if (levelText) levelText.text = "Cấp độ: " + playerHealth.currentLevel;
        if (expText) expText.text = $"Kinh nghiệm: {playerHealth.currentEXP}/{playerHealth.expToNextLevel}";
        if (moneyText) moneyText.text = "Tiền: " + playerHealth.Gold + " $";
        if (customersText) customersText.text = "Đã phục vụ: " + playerHealth.customersServed + " người";

        // Lấy số ngày từ TimeManager
        if (timeManager != null && daysText != null)
        {
            int daysPlayed = timeManager.GetCurrentDateTime().TotalNumDays;
            daysText.text = "Ngày thứ: " + daysPlayed;
        }
    }
}