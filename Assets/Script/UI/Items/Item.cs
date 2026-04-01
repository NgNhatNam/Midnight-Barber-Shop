using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour  //IPointerClickHandler
{
    // thêm 
    public ItemType itemType; // Chọn loại item ở đây
    [Header("Tool/Seed Properties")]
    public string specialAction; // Ví dụ: "Trồng lúa", "Cắt tóc"


    [Header("Stacking")]
    public int quantity = 1;
    public TMP_Text quantityText; // Kéo Text hiển thị số lượng vào đây (trong Prefab)

    //==================================

    [Header("Item Properties")]
    public int ID;
    public string itemName;

    [Header("Stats")]
    public int price;
    public int amountHP;
    public int amountMN;
    public int amountST;

    [Header("Auto Icon")]
    public Sprite icon;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();

        // Tìm Image trong prefab để auto lấy icon
        Image img = GetComponentInChildren<Image>();
        if (img != null)
        {
            icon = img.sprite;
        }
        else
        {
            Debug.LogWarning($"{name}: Prefab không chứa Image để lấy icon!");
        }

        UpdateQuantityDisplay();

    }

    public virtual void ShowPopUp()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;
        if (ItemPickupUIController.Instance != null) 
        { 
            ItemPickupUIController.Instance.ShowItemPickup(itemName, icon);
        }
    }

    // Stack Item
    public void UpdateQuantityDisplay()
    {
        if(quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : " ";
        }   
        
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public void RemoveFromStack(int amount = 1)
    {
        /*int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
        */
        Consume(amount);
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();    
        cloneItem.quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();
        return clone;
    }

    // Use Item
    public void UseItem()
    {
        switch (itemType)
        {
            case ItemType.Food: EatFood(); break;
            case ItemType.Fish: EatFood(); break;
            case ItemType.Drink: EatFood(); break;
            case ItemType.Seed: PlantSeed(); break;
            case ItemType.Tool: EquipTool(); break;
        }

        if (itemType != ItemType.Tool)
        {
            Consume(1);
        }
        // Xóa vật phẩm sau khi dùng (nếu là đồ ăn/hạt giống)
        //if (itemType != ItemType.Tool) Destroy(gameObject);
        //if (itemType != ItemType.Seed) Destroy(gameObject);

    }

    // Hàm dùng chung để trừ vật phẩm và dọn dẹp bộ nhớ
    private void Consume(int amount)
    {
        quantity -= amount;

        if (quantity <= 0)
        {
            // Báo cho Slot biết là nó đã trống
            Slot parentSlot = GetComponentInParent<Slot>();
            if (parentSlot != null) parentSlot.currentItem = null;

            // Ẩn menu UI nếu đang mở
            if (ItemUI.Instance != null) ItemUI.Instance.Hide();

            Destroy(gameObject);
        }
        else
        {
            UpdateQuantityDisplay();
        }

        // CỰC KỲ QUAN TRỌNG: Cập nhật lại "sổ tay" Inventory để Quest nhận diện được
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.RebuildItemCounts();
        }
    }

    public void SellItem()
    {
        Health player = FindAnyObjectByType<Health>();
        if (player != null)
        {
            player.AddGold(price); // Giả sử bạn có hàm AddGold trong Health
            Debug.Log($"Đã bán {itemName} lấy {price} vàng");
            //Destroy(gameObject);
            Consume(1);
        }
    }

    private void EatFood()
    {
        Health player = FindAnyObjectByType<Health>();
        if (player != null)
        {
            player.Heal(amountHP);
            player.HealMN(amountMN);
            player.IncreaseStress(amountST);
            Debug.Log($"Đã ăn {itemName}, hồi {amountHP} HP");
        }
    }

    private void PlantSeed()
    {
        Debug.Log($"Đã gieo hạt: {itemName}. Hành động: {specialAction}");
        // Logic gieo hạt của bạn ở đây
    }

    private void EquipTool()
    {
        Debug.Log($"Đang sử dụng công cụ: {itemName}");
        // Logic cầm kéo, cầm lược ở đây
    }

}
public enum ItemType
{
    Food,       // Đồ ăn: Hồi HP, MN, Stress
    Seed,       // Hạt giống: Dùng để trồng trọt
    Tool,       // Công cụ: Kéo, lược, máy sấy...
    Furniture,   // Nội thất: Trang trí tiệm tóc
    Drink,
    Fish,
    Meat,
    Egg
}