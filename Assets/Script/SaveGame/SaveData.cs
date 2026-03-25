using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable] 
public class SaveData 
{
    // Player Position
    public Vector3 playerPosition;
    public string mapBoundary;

    // NPC Position
    public List<NPCSaveData> npcSaveData = new List<NPCSaveData>();


    // Inventory
    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> toolbarSaveData;
    public List<ChestSaveData> chestSaveData;
    public List<QuestProgress> questProgressData;

    // Shop Data
    public List<ShopItemSaveData> allShopsData = new List<ShopItemSaveData>();

    // Light
    public float globalLightIntensity;


    // Time
    public int date;
    public int season;
    public int year;
    public int hour;
    public int minutes;

    // Player Health
    public int HP;
    public int MaxHP;
    public int MN;
    public int MaxMN;
    public int Gold;
    public int Stress;
}



[System.Serializable]
public class NPCSaveData
{
    public string npcName;
    public Vector3 position;
}


[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}

[System.Serializable]
public class ShopItemSaveData
{
    public string shopID; // Để phân biệt tiệm cá, tiệm hoa...
    public List<ItemStockData> items = new List<ItemStockData>();
    public int lastResetDay;
}

[System.Serializable]
public class ItemStockData
{
    public string itemName;
    public int stock;
}
