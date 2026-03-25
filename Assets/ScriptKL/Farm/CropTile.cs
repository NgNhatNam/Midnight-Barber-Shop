using UnityEngine;

[System.Serializable]
public class CropTile
{
    public Vector3Int position;
    public CropData plantedCrop;
    public int plantDay; // Ngày bắt đầu trồng
    public bool isWatered;
    public int growthStage = 0;

    public CropTile(Vector3Int pos)
    {
        position = pos;
    }
}
