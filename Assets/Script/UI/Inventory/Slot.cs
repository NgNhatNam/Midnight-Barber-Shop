using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public GameObject currentItem;
    private Image slotImage;
    private ToolbarController toolbarController;

    [Header("Colors")]
    public Color normalColor = new Color(1, 1, 1, 0.5f); 
    public Color selectedColor = Color.white;           

    private void Awake()
    {
        slotImage = GetComponent<Image>();
        toolbarController = FindAnyObjectByType<ToolbarController>();
    }

    // Hàm đổi màu nền
    public void SetHighlight(bool isSelected)
    {
        slotImage.color = isSelected ? selectedColor : normalColor;
        transform.localScale = isSelected ? new Vector3(1.1f, 1.1f, 1f) : Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
