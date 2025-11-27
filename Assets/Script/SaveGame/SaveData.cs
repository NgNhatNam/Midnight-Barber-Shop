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


    // Inventory
    public List<InventorySaveData> inventorySaveData;

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
