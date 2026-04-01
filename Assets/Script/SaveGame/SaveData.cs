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

    // Stable 
    public List<StableSaveData> allStablesData = new List<StableSaveData>();

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

    public int currentLevel;
    public int currentEXP;
    public int expToNextLevel;
    public int customersServed;
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

[System.Serializable]
public class AnimalSaveData
{
    public string animalPrefabName; // Để biết con này là Heo hay Gà khi load
    public Vector3 position;
    public int birthDayTotal; // Lưu ngày sinh để tính giá bán sau này
    public int lastHarvestTotalDays; // Lưu ngày thu hoạch cuối cùng
}

[System.Serializable]
public class StableSaveData
{
    public string stableID; // Tên hoặc ID của chuồng
    public List<AnimalSaveData> animals = new List<AnimalSaveData>();
}
