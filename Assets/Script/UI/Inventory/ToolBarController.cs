using UnityEngine;

public class ToolbarController : MonoBehaviour
{
    public static ToolbarController Instance; 

    public Slot[] slots;
    private int selectedIndex = 0;

    private void Awake()
    {
        Instance = this; 
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

    public void SelectSlotByPointer(Slot clickedSlot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == clickedSlot)
            {
                
                if (selectedIndex == i)
                {
                    ExecuteItemAction();
                }
                else 
                {
                    selectedIndex = i;
                    UpdateSelection();
                }
                break;
            }
        }
    }

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