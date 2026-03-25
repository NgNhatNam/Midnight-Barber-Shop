using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int rows = 5;
    [SerializeField]
    private int cols = 8;
    [SerializeField]
    private float tileSize = 1;

    [Header("Spacing Settings")]
    [Range(0, 100f)]
    [SerializeField] private float spacingX = 0.1f; // Khoảng cách giữa các cột
    [Range(0, 100f)]
    [SerializeField] private float spacingY = 0.1f; // Khoảng cách giữa các hàng

    public List<FarmTile> allTiles = new List<FarmTile>(); // Để quản lý tất cả các ô đất

    public GameObject farmGround;

     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGrid();
    }
    private void GenerateGrid()
    {
        if (farmGround == null) return;

        // 1. Tính toán bước nhảy (tổng kích thước ô + khoảng cách)
        float stepX = tileSize + spacingX;
        float stepY = tileSize + spacingY;

        // 2. Tính toán tổng chiều rộng và chiều cao thực tế của Grid để căn giữa
        float gridWidth = (cols - 1) * stepX;
        float gridHeight = (rows - 1) * stepY;

        // 3. Tính Offset để đưa tâm về (0,0) local
        float offsetX = -gridWidth / 2f;
        float offsetY = gridHeight / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                GameObject tile = Instantiate(farmGround, transform);

                // 4. Tính vị trí dựa trên bước nhảy (step) thay vì chỉ tileSize
                float posX = (col * stepX) + offsetX;
                float posY = (row * -stepY) + offsetY;

                tile.transform.localPosition = new Vector2(posX, posY);
                tile.name = $"Tile_{col}_{row}";

                // Tự động thêm script quản lý ô đất nếu chưa có
                FarmTile tileScript = tile.GetComponent<FarmTile>();
                if (tileScript == null)
                {
                    tileScript = tile.AddComponent<FarmTile>();
                }
                allTiles.Add(tileScript); // Lưu vào danh sách quản lý
            }
        }
    }
    /*
    private void GenerateGrid()
    {
        //GameObject referenceTile = (GameObject)Instantiate(Resources.Load("Tile Assets_3166"));
        GameObject referenceTile = (GameObject)Instantiate(farmGround);
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++) 
            { 
                GameObject tile = (GameObject)Instantiate(referenceTile, transform);

                float posX = col * tileSize;
                float posY = row * -tileSize;

                tile.transform.position = new Vector2(posX, posY);
            }
        }  

        Destroy(referenceTile );

        float gridW = cols * tileSize;
        float gridH = rows * tileSize;
        transform.position = new Vector2( -gridW/2 + tileSize/2, gridH/2 - tileSize/2);
    }
    */

    // Update is called once per frame
    void Update()
    {
        
    }
}
