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

   // Kiểm tra xem shop này có kinh doanh loại item này không
    public bool WillBuyItem(ItemType type)
    {
        return typesToSell.Contains(type);
    }
}

