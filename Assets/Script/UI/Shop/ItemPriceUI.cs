using UnityEngine;

public class ItemPriceUI : MonoBehaviour
{
    public static ItemPriceUI Instance;

    public GameObject TabUi;
    public GameObject panel;
    private ItemPrice currentPriceItem;

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
    public void Show(ItemPrice item, Vector2 pos)
    {
        currentPriceItem = item;

        panel.SetActive(true);
        panel.transform.position = pos;
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentPriceItem = null;
    }

    public void OnBuyButton()
    {
        if (currentPriceItem != null)
            currentPriceItem.BuyItem();

        Hide();
    }

    public void OnCloseButton()
    {
        Hide();
    }
}
