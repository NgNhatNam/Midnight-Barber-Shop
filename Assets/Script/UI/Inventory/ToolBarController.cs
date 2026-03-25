using UnityEngine;

public class ToolbarController : MonoBehaviour
{
    public static ToolbarController Instance; // Thêm dòng này

    public Slot[] slots;
    private int selectedIndex = 0;

    private void Awake()
    {
        Instance = this; // Gán Instance khi game bắt đầu
    }

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        // Giữ nguyên logic lăn chuột cho PC
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0) selectedIndex--;
            else selectedIndex++;

            selectedIndex = Mathf.Clamp(selectedIndex, 0, slots.Length - 1);
            UpdateSelection();
        }
    }

    // Hàm này để các Slot/Item gọi khi được chạm vào
    public void SelectSlot(Slot targetSlot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == targetSlot)
            {
                selectedIndex = i;
                UpdateSelection(); // Cập nhật màu sắc highlight
                break;
            }
        }
    }

    // HÀM MỚI: Dành cho Mobile/Click chọn ô
    public void SelectSlotByPointer(Slot clickedSlot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == clickedSlot)
            {
                // Nếu bấm vào ô đã được chọn rồi -> Thực hiện hành động (Dùng Item)
                if (selectedIndex == i)
                {
                    ExecuteItemAction();
                }
                else // Nếu bấm vào ô khác -> Chỉ chuyển Highlight qua đó
                {
                    selectedIndex = i;
                    UpdateSelection();
                }
                break;
            }
        }
    }

    // Hàm lấy Item đang được chọn trên Toolbar
    public Item GetSelectedItem()
    {
        Slot currentSlot = slots[selectedIndex];
        if (currentSlot.currentItem != null)
        {
            return currentSlot.currentItem.GetComponent<Item>();
        }
        return null;
    }

    void UpdateSelection()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetHighlight(i == selectedIndex);
        }
    }

    void ExecuteItemAction()
    {
        Item item = slots[selectedIndex].GetItemComponent();
        if (item != null) item.UseItem();
    }
}