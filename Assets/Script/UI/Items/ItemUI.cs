using UnityEngine;

public class ItemUI : MonoBehaviour
{
    public static ItemUI Instance;

    public GameObject TabUi;
    public GameObject panel;
    private ItemDragHandler currentItem;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    private void Update()
    {
        if (!TabUi.activeSelf)
        {
            Hide();
        }
    }

    public void Show(ItemDragHandler item, Vector2 position)
    {
        currentItem = item;
        panel.SetActive(true);
        panel.transform.position = position;
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
}
