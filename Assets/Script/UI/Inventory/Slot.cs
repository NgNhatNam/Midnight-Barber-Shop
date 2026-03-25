using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public GameObject currentItem;
    private Image slotImage;
    private ToolbarController toolbarController;

    [Header("Colors")]
    public Color normalColor = new Color(1, 1, 1, 0.5f); // Trắng mờ
    public Color selectedColor = Color.white;           // Trắng sáng

    private void Awake()
    {
        slotImage = GetComponent<Image>();
        toolbarController = FindAnyObjectByType<ToolbarController>();
    }

    // Hàm đổi màu nền
    public void SetHighlight(bool isSelected)
    {
        slotImage.color = isSelected ? selectedColor : normalColor;
        // Nếu được chọn thì hơi to ra một chút, không thì về bình thường
        transform.localScale = isSelected ? new Vector3(1.1f, 1.1f, 1f) : Vector3.one;
    }

    // Xử lý khi BẤM vào ô (Dành cho Mobile và Mouse Click)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Thông báo cho ToolbarController biết ô này vừa được chọn
        if (toolbarController != null)
        {
            toolbarController.SelectSlotByPointer(this);
        }
    }

    public Item GetItemComponent()
    {
        if (currentItem == null) return null;
        return currentItem.GetComponentInChildren<Item>();
    }
}
