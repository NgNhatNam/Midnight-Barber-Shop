using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIDisplay : MonoBehaviour
{
    public static ShopUIDisplay Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private Image shopIconImage;
    [SerializeField] private Transform itemGrid;
    [SerializeField] private GameObject itemPricePrefab;
    [SerializeField] private GameObject shopUIContainer;
    [SerializeField] private GameObject interactButtonUI;


    [Header("Status Visuals (Chỉ là UI trong bảng này)")]
    [SerializeField] private GameObject openUI;
    [SerializeField] private GameObject closeUI;
    [SerializeField] private GameObject closeWall;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        shopUIContainer.SetActive(false);
    }

    private void Update()
    {
        //if (shopUIContainer.activeSelf)
        //{
          //  interactButtonUI.SetActive(false);
       // }

        if (shopUIContainer.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    // Hàm để cập nhật trạng thái hiển thị của các icon Open/Close trên UI
    public void UpdateStatusUI(bool isOpen)
    {
        if (openUI) openUI.SetActive(isOpen);
        if (closeUI) closeUI.SetActive(!isOpen);
        if (closeWall) closeWall.SetActive(!isOpen);
    }

    public bool IsUIActive()
    {
        return shopUIContainer.activeSelf;
    }

    public void OpenShop(string name, Sprite icon, List<ItemStockData> stock, bool isOpenNow)
    {
        shopUIContainer.SetActive(true);
        if (shopNameText) shopNameText.text = name;
        if (shopIconImage) shopIconImage.sprite = icon;

        // Cập nhật trạng thái icon Open/Close trên UI
        UpdateStatusUI(isOpenNow);

        // Xóa đồ cũ
        foreach (Transform child in itemGrid) Destroy(child.gameObject);

        // Tạo đồ mới
        foreach (var data in stock)
        {
            if (data.stock <= 0) continue;
            Item item = ItemDictionary.Instance.itemPrefabs.Find(i => i.itemName == data.itemName);
            if (item != null)
            {
                GameObject slot = Instantiate(itemPricePrefab, itemGrid);
                slot.GetComponent<ItemPrice>().Setup(item, data.stock);
            }
        }
    }

    public void CloseShop() => shopUIContainer.SetActive(false);
}