using DPUtils.System.DateTime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour, IInteractable
{
    [Header("Shop Identity")]
    public string shopID;
    [SerializeField] private string shopDisplayName;
    [SerializeField] private Sprite shopIcon;
    //[SerializeField] private ItemType typeToSell;
    [SerializeField] private List<ItemType> typesToSell = new List<ItemType>();

    [Header("Visuals")]
    public GameObject openVisual;
    public GameObject closeVisual;

    private List<ItemStockData> currentStockList = new List<ItemStockData>();
    private int lastResetDay = -1;
    private TimeManager timeManager;
    private bool isCurrentlyOpen = false;

    private void Start()
    {
        if (string.IsNullOrEmpty(shopID)) shopID = GlobalHelper.GenerateUniqueID(gameObject);
        timeManager = FindAnyObjectByType<TimeManager>();

        if (timeManager != null)
        {
            isCurrentlyOpen = timeManager.GetCurrentDateTime().TimeToAllShopOpen();
            UpdateShopVisuals(isCurrentlyOpen);
        }
    }

    private void Update()
    {
        if (timeManager == null) return;

        // Cập nhật trạng thái đóng mở ngay lập tức theo thời gian thực
        bool isOpenNow = timeManager.GetCurrentDateTime().TimeToAllShopOpen();

        if (isOpenNow != isCurrentlyOpen)
        {
            isCurrentlyOpen = isOpenNow;
            UpdateShopVisuals(isCurrentlyOpen);

            // Đuổi khách nếu tiệm đóng cửa khi đang mở UI
            if (!isOpenNow && ShopUIDisplay.Instance != null && ShopUIDisplay.Instance.IsUIActive())
            {
                ShopUIDisplay.Instance.CloseShop();
            }
        }
    }

    private void UpdateShopVisuals(bool isOpen)
    {
        // Bật tắt GameObject biển hiệu
        if (openVisual) openVisual.SetActive(isOpen);
        if (closeVisual) closeVisual.SetActive(!isOpen);
    }

    private void OnDestroy() => TimeManager.OnDateTimeChanged -= CheckShopStatus;

    public bool CanInteract() => isCurrentlyOpen;

    public void Interact()
    {
        
        if (!CanInteract()) return;

        if (ShopUIDisplay.Instance.IsUIActive())
        {
            ShopUIDisplay.Instance.CloseShop();
            return; // Dừng lại ở đây, không chạy xuống phần mở Shop bên dưới
        }

        CheckAndRefreshStock();

        // Gửi thêm biến isCurrentlyOpen để UI biết bật đèn xanh hay đỏ
        ShopUIDisplay.Instance.OpenShop(shopDisplayName, shopIcon, currentStockList, isCurrentlyOpen);
    }

    private void CheckShopStatus(DPUtils.System.DateTime.DateTime currentTime)
    {
        bool isOpenNow = currentTime.TimeToAllShopOpen();
        if (isOpenNow != isCurrentlyOpen)
        {
            isCurrentlyOpen = isOpenNow;
    
            // Nếu đang mở bảng UI mà tiệm đóng cửa 
            if (!isOpenNow && ShopUIDisplay.Instance != null)
            {
                ShopUIDisplay.Instance.CloseShop();
            }
        }
    }

    private void CheckAndRefreshStock()
    {
        var currentTime = timeManager.GetCurrentDateTime();
        if (currentTime.TotalNumDays != lastResetDay)
        {
            RefreshStock();
            lastResetDay = currentTime.TotalNumDays;
        }
    }

  
    private void RefreshStock()
    {
        currentStockList.Clear();

        // Lọc các item có trong từ điển mà thuộc bất kỳ loại nào trong danh sách typesToSell
        var validItems = ItemDictionary.Instance.itemPrefabs.Where(i => typesToSell.Contains(i.itemType));

        foreach (var i in validItems)
        {
            currentStockList.Add(new ItemStockData
            {
                itemName = i.itemName,
                stock = Random.Range(5, 15)
            });
        }
    }

    public ShopItemSaveData GetShopSaveData() => new ShopItemSaveData { shopID = shopID, items = currentStockList, lastResetDay = lastResetDay };
    public void LoadShopSaveData(ShopItemSaveData data)
    {
        if (data == null) return;
        currentStockList = data.items;
        lastResetDay = data.lastResetDay;
    }
}

/*
public class ShopManager : MonoBehaviour
{
    [Header("Save Settings")]
    public string shopID; // Đặt tên trong Inspector (ví dụ: "FishShop_01")
    private List<ItemStockData> currentStockList = new List<ItemStockData>();
    private int lastResetDay = -1;


    [Header("Shop Settings")]
    [SerializeField] private ItemType typeToSell;
    [SerializeField] private int slotCount = 6;


    [Header("Shop UI")]
    public Transform itemGrid;          // Nơi chứa các slot
    public GameObject itemPricePrefab;  // Prefab Slot (ItemPrice)
    public GameObject closeShopUI;
    public GameObject closeWallUI;
    public GameObject openShopUI;

    private TimeManager timeManager;
    private bool isCurrentlyOpen = false;

    private void Start()
    {
        timeManager = FindAnyObjectByType<TimeManager>();

        // Kiểm tra script quản lý thời gian có tồn tại trong Scene không
        if (timeManager == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy TimeManager trong Scene!");
            return;
        }

        TimeManager.OnDateTimeChanged += CheckShopStatus;

        // Lấy thời gian hiện tại
        var initialTime = timeManager.GetCurrentDateTime();

        // Vì DateTime là struct, nó luôn có giá trị mặc định (thường là 00:00:00)
        // nên ta gọi luôn hàm check status
        CheckShopStatus(initialTime);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký để tránh lỗi leak memory
        TimeManager.OnDateTimeChanged -= CheckShopStatus;
    }

    // Thay vì Update, hàm này chỉ chạy khi thời gian trong game nhảy số
    private void CheckShopStatus(DPUtils.System.DateTime.DateTime currentTime)
    {
        bool isOpenNow = currentTime.TimeToOpen();

        // Chỉ xử lý nếu trạng thái Đóng/Mở có sự thay đổi
        if (isOpenNow != isCurrentlyOpen)
        {
            isCurrentlyOpen = isOpenNow;
            UpdateShopVisuals(isOpenNow);

            if (isOpenNow)
            {
                GenerateShopItems();
            }
        }
    }

    private void GenerateShopItems()
    {
        foreach (Transform child in itemGrid) Destroy(child.gameObject);

        var currentTime = timeManager.GetCurrentDateTime();

        //  Nếu ngày hiện tại khác ngày reset cuối cùng thì làm mới dữ liệu
        if (currentTime.TotalNumDays != lastResetDay)
        {
            RefreshStock();
            lastResetDay = currentTime.TotalNumDays;
        }

        // Tạo UI từ danh sách currentStockList
        foreach (var stockData in currentStockList)
        {
            if (stockData.stock <= 0) continue;

            Item item = ItemDictionary.Instance.itemPrefabs.Find(i => i.itemName == stockData.itemName);
            if (item != null)
            {
                GameObject slotObj = Instantiate(itemPricePrefab, itemGrid);
                ItemPrice slotScript = slotObj.GetComponent<ItemPrice>();
                slotScript.Setup(item, stockData.stock);

                // Cập nhật lại số lượng khi người chơi mua (bạn cần thêm event hoặc callback)
            }
        }
    }

    private void RefreshStock()
    {
        currentStockList.Clear();
        var validItems = ItemDictionary.Instance.itemPrefabs.Where(i => i.itemType == typeToSell);
        foreach (var i in validItems)
        {
            currentStockList.Add(new ItemStockData { itemName = i.itemName, stock = Random.Range(5, 15) });
        }
    }

    private void UpdateShopVisuals(bool isOpen)
    {
        if (openShopUI) openShopUI.SetActive(isOpen);
        if (closeShopUI) closeShopUI.SetActive(!isOpen);
        if (closeWallUI) closeWallUI.SetActive(!isOpen);
    }

  
    // Get Save Data

    // Hàm để SaveController gọi lấy dữ liệu
    public ShopItemSaveData GetShopSaveData()
    {
        return new ShopItemSaveData
        {
            shopID = this.shopID,
            items = currentStockList,
            lastResetDay = this.lastResetDay
        };
    }

    // Hàm để SaveController đổ dữ liệu vào khi Load
    public void LoadShopSaveData(ShopItemSaveData data)
    {
        if (data == null) return;
        this.currentStockList = data.items;
        this.lastResetDay = data.lastResetDay;

        // Nếu đã load xong mà shop đang mở, vẽ lại UI
        if (isCurrentlyOpen) GenerateShopItems();
    }

}
*/