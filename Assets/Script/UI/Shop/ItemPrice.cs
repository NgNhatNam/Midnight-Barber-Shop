using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemPrice : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public TMP_Text nameTxt;
    public TMP_Text priceTxt;
    public TMP_Text hpTxt;
    public TMP_Text mnTxt;
    public TMP_Text stTxt;
    public Image iconImg;

    public int currentStock;
    public TMP_Text stockText;

    private Item itemData;
    private InventoryController inventory;
    private Health playerHealth;

    public void Setup(Item selectedItem, int amount)
    {
        itemData = selectedItem;

        inventory = FindFirstObjectByType<InventoryController>();
        playerHealth = FindAnyObjectByType<Health>();

        if (itemData.icon != null)
        {
            iconImg.sprite = itemData.icon;
        }
        else
        {
            // Nếu icon bị null, thử lấy trực tiếp từ Component Image của Prefab
            Image prefabImg = itemData.GetComponentInChildren<Image>();
            if (prefabImg != null)
            {
                iconImg.sprite = prefabImg.sprite;
            }
        }

        // Load UI
        nameTxt.text = itemData.itemName;
        priceTxt.text = itemData.price + " G";

        // Hiển thị chỉ số 
        if (hpTxt) hpTxt.text = itemData.amountHP.ToString();
        if (mnTxt) mnTxt.text = itemData.amountMN.ToString();
        if (stTxt) stTxt.text = itemData.amountST.ToString();

        currentStock = amount;
        UpdateStockUI();
    }

    private void UpdateStockUI()
    {
        if (stockText != null) stockText.text = "" + currentStock;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Chuột phải mở menu. Mobile: Chạm nhẹ (Tap) mở menu.
        bool isRightClick = eventData.button == PointerEventData.InputButton.Right;
        bool isMobileTap = !eventData.dragging;

        if (isRightClick || isMobileTap)
        {
            ItemPriceUI.Instance.Show(this, eventData.position);
        }
    }

    public void BuyItem()
    {
        if (playerHealth == null || inventory == null) return;

        // Kiểm tra đủ tiền
        if (playerHealth.Gold < itemData.price)
        {
            Debug.Log("Không đủ vàng!");
            return;
        }

        // Kiểm tra kho (Stock)
        if (currentStock <= 0)
        {
            Debug.Log("Hết hàng!");
            return;
        }

        // Thực hiện mua
        GameObject purchasedItem = Instantiate(itemData.gameObject);

        if (inventory.AddItem(purchasedItem))
        {
            playerHealth.SpendGold(itemData.price);
            currentStock--; // Trừ số lượng trong shop
            UpdateStockUI();

            Debug.Log($"Đã mua {itemData.itemName}. Còn lại: {currentStock}");

            // Nếu hết sạch hàng trong kho thì mới xóa Slot này
            if (currentStock <= 0)
            {
                if (ItemPriceUI.Instance != null) ItemPriceUI.Instance.Hide();
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(purchasedItem); // Inventory đầy thì xóa bản sao vừa tạo
            Debug.Log("Inventory đầy!");
        }
    }


    public Item GetItemData()
    {
        return itemData;
    }
}
