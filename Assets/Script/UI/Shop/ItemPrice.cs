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

    private Item itemData; // dữ liệu item 
    private InventoryController inventory;
    private Health playerHealth;

    public void Setup(Item selectedItem, int amount)
    {
        // Gán trực tiếp data từ Manager truyền qua, không cần Instantiate ngẫu nhiên ở đây nữa
        itemData = selectedItem;

        inventory = FindFirstObjectByType<InventoryController>();
        playerHealth = FindAnyObjectByType<Health>();

        // Trong ItemPrice.cs, hàm Setup
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

    // Chuột phải mở menu Shop
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
        // Quan trọng: Phải Instantiate một bản sao item để đưa vào túi đồ
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

    /*
    public void BuyItem()
    {
        Debug.Log("Mua item: " + itemData.itemName + " giá " + itemData.price);

        if (playerHealth == null || inventory == null)
        {
            Debug.LogError("Không tìm thấy PlayerMoney hoặc InventoryController!");
            return;
        }

        // Kiểm tra đủ tiền
        if (playerHealth.Gold < itemData.price)
        {
            Debug.Log("Không đủ vàng!");
            return;
        }

        playerHealth.SpendGold(itemData.price);


        // Add vào inventory
        if (inventory.AddItem(itemData.gameObject))
        {
            Debug.Log("Đã mua: " + itemData.itemName);
            Destroy(gameObject); // Xoá item khỏi shop
        }
        else
        {
            Debug.Log("Inventory đầy!");
        }
    }
    */

    public Item GetItemData()
    {
        return itemData;
    }
}
