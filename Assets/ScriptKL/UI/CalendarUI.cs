using System.Collections.Generic; // Để dùng List
using UnityEngine;
using DPUtils.System.DateTime;
using TMPro;

public class CalendarUI : MonoBehaviour
{
    public TMP_Text monthYearText;
    public Transform gridContainer;
    public GameObject daySlotPrefab;

    // Lưu danh sách các ô đã tạo để tái sử dụng, không cần Destroy/Instantiate lại
    private List<DaySlot> allSlots = new List<DaySlot>();

    void Awake()
    {
        InitializeGrid();
    }
    private void Start()
    {
        // Cập nhật dữ liệu ngay lập tức khi khởi tạo
        TimeManager tm = FindFirstObjectByType<TimeManager>();
        if (tm != null)
        {
            UpdateCalendar(tm.GetCurrentDateTime());
        }
    }

    void InitializeGrid()
    {
        // Xóa sạch container trước khi tạo
        foreach (Transform child in gridContainer) Destroy(child.gameObject);
        allSlots.Clear();

        for (int i = 1; i <= 28; i++)
        {
            GameObject obj = Instantiate(daySlotPrefab, gridContainer);
            DaySlot slot = obj.GetComponent<DaySlot>();

            slot.Setup(i); // Ghi chữ "Ngày i"
            allSlots.Add(slot);
        }
    }

    public void UpdateCalendar(DPUtils.System.DateTime.DateTime dt)
    {
        if (monthYearText != null)
        {
            string tenMuaViet = VietNameseName(dt.Season);

            monthYearText.text = $"{tenMuaViet} - Năm {dt.Year:D2}";
        }

        if (allSlots == null || allSlots.Count < 28) return;

        for (int i = 0; i < allSlots.Count; i++)
        {
            // Kiểm tra xem slot có tồn tại không trước khi gọi hàm
            if (allSlots[i] != null)
            {
                int dayNumber = i + 1;
                allSlots[i].SetHighlight(dayNumber == dt.Date, Color.red, Color.black);
            }
        }
    }

    private string VietNameseName(Season s)
    {
        return s switch
        {
            Season.Spring => "Mùa Xuân",
            Season.Summer => "Mùa Hạ",
            Season.Autumn => "Mùa Thu",
            Season.Winter => "Mùa Đông",
            _ => "Không xác định"
        };
    }

    private void OnEnable()
    {
        TimeManager.OnDateTimeChanged += UpdateCalendar;
    }

    private void OnDisable()
    {
        TimeManager.OnDateTimeChanged -= UpdateCalendar;
    }
}