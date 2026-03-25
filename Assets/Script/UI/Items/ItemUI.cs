using TMPro;
using UnityEngine;

public class ItemUI : MonoBehaviour
{
    public static ItemUI Instance;

    public GameObject TabUi;
    public GameObject panel;
    private Item currentItem;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    private void Update()
    {
        /*
        if (!TabUi.activeSelf)
        {
            Hide();
        }*/
    }

    public void Show(Item item, Vector2 position)
    {
        currentItem = item;
        panel.SetActive(true);

        // Nếu là Mobile, cho panel hiện cao lên 100 pixel để ngón tay không che mất
        Vector2 offset = new Vector2(0, 150f);
        panel.transform.position = position + offset;
    }
    public void Hide()
    {
        panel.SetActive(false);
        currentItem = null;
    }

    public void OnUseButton()
    {
        if (currentItem != null)
            currentItem.UseItem();

        Hide();
    }

    public void OnSellButton()
    {
        if (currentItem != null)
            currentItem.SellItem();

        Hide();
    }

    public void OnSplitButtonClicked()
    {
        if (currentItem != null && currentItem.quantity > 1)
        {
            // Gọi hàm SplitStack từ ItemDragHandler hoặc logic tách của bạn
            ItemDragHandler dragHandler = currentItem.GetComponent<ItemDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.SplitStack();
            }
            Hide(); // Đóng menu sau khi tách
        }
    }

}
