using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    private ItemDictionary itemDictionary;

    public GameObject toolbarPanel; 
    public GameObject inventoryPanel;
    public GameObject slotPrefab;       
    public int slotCount;           
    
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        itemDictionary = ItemDictionary.Instance;
        RebuildItemCounts();

    }


    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if(item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }
        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public bool AddItem(GameObject itemObj)
    {
        Item itemToAdd = itemObj.GetComponent<Item>();
        if (itemToAdd == null) return false;

        int amountToAdd = itemToAdd.quantity;

        // Tìm để Stack
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item existingItem = slot.currentItem.GetComponent<Item>();
                if (existingItem != null && existingItem.ID == itemToAdd.ID)
                {
                    existingItem.AddToStack(amountToAdd);
                    Destroy(itemObj);
                    RebuildItemCounts();
                    return true;
                }
            }
        }

        // Tìm ô trống và TỰ ĐỘNG lấy Prefab chuẩn từ Dictionary
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject correctPrefab = itemDictionary.GetItemPrefab(itemToAdd.ID);

                if (correctPrefab != null)
                {
                    GameObject newItem = Instantiate(correctPrefab, slot.transform);
                    newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = newItem;
                   
                    Item newItemComponent = newItem.GetComponent<Item>();
                    newItemComponent.quantity = amountToAdd;
                    newItemComponent.UpdateQuantityDisplay();

                    Destroy(itemObj);
                    RebuildItemCounts();
                    return true;
                }
            }
        }
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { 
                    itemID = item.ID, 
                    slotIndex = slotTransform.GetSiblingIndex(), 
                    quantity = item.quantity 
                });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        // cleat inventory panel - avoid duplicates
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        //Create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        //Populate slots with save items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null && data.quantity > 1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;
                }
            }

        }

        RebuildItemCounts();
    }

    public void SetLoadInventoryItems(List<InventorySaveData> inventorySaveData)
    {   
 
        //Populate slots with save items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();

                // Clear item trong slot trước khi spawn lại
                if (slot.transform.childCount > 0)
                {
                    Destroy(slot.transform.GetChild(0).gameObject);
                }

                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null && data.quantity > 1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;
                }
            }

        }
    }

    //=================================================================================

    // Hàm bổ trợ để quét bất kỳ Panel nào 
    public List<InventorySaveData> GetItemsFromPanel(GameObject panel)
    {
        List<InventorySaveData> data = new List<InventorySaveData>();
        if (panel == null) return data;

        foreach (Transform slotTransform in panel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                data.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }
        return data;
    }

    // Hàm Load cho Toolbar
    public void SetToolbarItems(List<InventorySaveData> data)
    {
        PopulatePanel(toolbarPanel, data);
    }

    // Hàm bổ trợ để đổ dữ liệu vào Panel
    private void PopulatePanel(GameObject panel, List<InventorySaveData> data)
    {
        if (panel == null || data == null) return;

        // Xóa item cũ trong các slot hiện có (không xóa bản thân Slot)
        foreach (Transform slotTransform in panel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.transform.childCount > 0)
            {
                foreach (Transform child in slot.transform) Destroy(child.gameObject);
                slot.currentItem = null;
            }
        }

        // Spawn item mới
        foreach (var itemData in data)
        {
            if (itemData.slotIndex < panel.transform.childCount)
            {
                Slot slot = panel.transform.GetChild(itemData.slotIndex).GetComponent<Slot>();
                GameObject prefab = itemDictionary.GetItemPrefab(itemData.itemID);
                if (prefab != null)
                {
                    GameObject newItem = Instantiate(prefab, slot.transform);
                    newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    Item itemComp = newItem.GetComponent<Item>();
                    itemComp.quantity = itemData.quantity;
                    itemComp.UpdateQuantityDisplay();
                    slot.currentItem = newItem;
                }
            }
        }
    }

    //=================================================================================
    public void ClearInventory()
    {
        // Xoá toàn bộ item và slot cũ
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Tạo lại slot trống như ban đầu
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        Debug.Log("Inventory has been cleared!");
    }

    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        // Tạo một danh sách các Panel cần quét để xóa đồ
        List<GameObject> panelsToSearch = new List<GameObject> { inventoryPanel, toolbarPanel };

        foreach (GameObject panel in panelsToSearch)
        {
            if (panel == null) continue;

            foreach (Transform slotTransform in panel.transform)
            {
                if (amountToRemove <= 0) break;

                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot != null && slot.currentItem != null)
                {
                    Item item = slot.currentItem.GetComponent<Item>();
                    if (item != null && item.ID == itemID)
                    {
                        int canRemove = Mathf.Min(amountToRemove, item.quantity);

                        item.RemoveFromStack(canRemove);
                        amountToRemove -= canRemove;

                        if (item.quantity <= 0)
                        {
                            Destroy(slot.currentItem);
                            slot.currentItem = null;
                        }
                    }
                }
            }
        }

        // Cập nhật lại Cache số lượng để QuestController nhận dữ liệu mới
        RebuildItemCounts();
    }
    
}
