using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Farming/Crop")]
public class CropData : ScriptableObject
{
    public string cropName;
    public int daysToGrow; // Tổng số ngày lớn
    public Sprite[] growthStageSprites; // Các giai đoạn hình ảnh (Hạt -> Cây con -> Chín)
    public GameObject harvestResult; // Vật phẩm nhận được
}