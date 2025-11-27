using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [Header("Item Properties")]
    public int ID;
    public string itemName;

    [Header("Stats")]
    public int price;
    public int amountHP;
    public int amountMN;
    public int amountST;

    [Header("Auto Icon")]
    public Sprite icon;

    private void Awake()
    {
        // Tìm Image trong prefab để auto lấy icon
        Image img = GetComponentInChildren<Image>();
        if (img != null)
        {
            icon = img.sprite;
        }
        else
        {
            Debug.LogWarning($"{name}: Prefab không chứa Image để lấy icon!");
        }
    }
}
