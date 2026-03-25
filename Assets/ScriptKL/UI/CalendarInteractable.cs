using UnityEngine;

public class CalendarInteractable : MonoBehaviour, IInteractable
{
    [Header("UI Reference")]
    [SerializeField] private GameObject calendarPanel; // Kéo CalendarUI GameObject vào đây

    public bool CanInteract()
    {
        // Luôn có thể tương tác để mở/đóng lịch
        return true;
    }

    public void Interact()
    {
        if (calendarPanel == null)
        {
            Debug.LogError("CalendarInteractable: Chưa gán Calendar Panel vào Inspector!");
            return;
        }

        bool isActive = calendarPanel.activeSelf;
        calendarPanel.SetActive(!isActive);

        // Nếu bạn muốn khi mở lịch thì game tạm dừng
        // Time.timeScale = !isActive ? 0 : 1;
    }
}