using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel; // Panel Ui chứa slot
    public GameObject slotPrefab;       
    public int slotCount;               
    public GameObject[] itemPrefabs;  // Danh sách chứa item sẽ spawn vào các slot

    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();

    }

    public bool AddItem(GameObject itemPrefab)
    {
        //Look for empty slot
        foreach (Transform slotTranform in inventoryPanel.transform) 
        { 
            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Invetory is full");
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
                invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
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
                    slot.currentItem = item;
                }
            }

        }
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
                    slot.currentItem = item;
                }
            }

        }
    }

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
}
