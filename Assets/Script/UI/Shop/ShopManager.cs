using DPUtils.System.DateTime;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop UI")]
    public Transform itemGrid;          // Nơi chứa các slot
    public GameObject itemPricePrefab;  // Prefab Slot (ItemPrice)
    public int slotCount = 6;           // Số lượng Slot
    public GameObject closeShopUI;
    public GameObject closeWallUI;
    public GameObject openShopUI;

    [Header("Item List")]
    public Item[] itemPrefabs;          // Prefab Item

    private TimeManager timeManager;
    private bool wasOpen = false;

    private void Start()
    {
        timeManager = FindAnyObjectByType<TimeManager>();

        GenerateSlots();
    }
    public void Update()
    {
        var currentTime = timeManager.GetCurrentDateTime();
        bool isOpen = !currentTime.TimeToOpen();

        if (isOpen)
        {
            openShopUI.SetActive(true);
            closeShopUI.SetActive(false);
            closeWallUI.SetActive(false);

            // Chỉ tạo lại shop duy nhất 1 lần khi shop vừa mở
            if (!wasOpen)
            {
                GenerateSlots();
                wasOpen = true;
            }
        }
        else
        {
            openShopUI.SetActive(false);
            closeShopUI.SetActive(true);
            closeWallUI.SetActive(true);

            // Đánh dấu shop đã đóng → hôm sau mở lại sẽ tạo mới
            wasOpen = false;
        }

    }
    void GenerateSlots()
    {
        // Xóa slot cũ
        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        // Tạo slot mới
        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = Instantiate(itemPricePrefab, itemGrid);
            ItemPrice slot = slotObj.GetComponent<ItemPrice>();

            if (slot != null)
                slot.Setup(itemPrefabs);
        }

        Debug.Log("Shop đã tạo " + slotCount + " ItemPrice slot!");
    }
}
