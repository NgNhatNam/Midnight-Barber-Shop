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

    private Item itemData; // dữ liệu item thực sự
    private InventoryController inventory;
    private Health playerHealth;

    public void Setup(Item[] itemPrefabs)
    {

        itemData = Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Length)]);
        //itemData = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        if (itemData != null) 
        {
            Debug.Log("Chưa thấy vấn đề");
        }

        inventory = FindFirstObjectByType<InventoryController>();
        playerHealth = FindAnyObjectByType<Health>();

        // Load UI
        nameTxt.text = itemData.itemName;
        priceTxt.text = itemData.price + " G";
        hpTxt.text = itemData.amountHP.ToString();
        mnTxt.text = itemData.amountMN.ToString();
        stTxt.text = itemData.amountST.ToString();

        if (itemData.icon != null)
            iconImg.sprite = itemData.icon;

    }

    // Chuột phải mở menu Shop
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ItemPriceUI.Instance.Show(this, eventData.position);
        }
    }

    // 
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

    public Item GetItemData()
    {
        return itemData;
    }
}
