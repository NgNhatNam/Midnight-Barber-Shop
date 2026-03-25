using UnityEngine;

public class FarmTile : MonoBehaviour
{
    private bool isPlayerInside = false;
    private bool isPlanted = false;
    public GameObject cropPrefab; // Kéo prefab cây muốn trồng vào đây (trong Inspector của FarmGround)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            // Highlight ô đất để người chơi biết đang đứng ở đây (tùy chọn)
            GetComponent<SpriteRenderer>().color = Color.green;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void Update()
    {
        // Nếu người chơi đứng trong ô đất và nhấn phím (ví dụ phím E hoặc nút trồng trên Mobile)
        if (isPlayerInside && !isPlanted)
        {
            if (Input.GetKeyDown(KeyCode.E)) // Bạn có thể thay bằng logic Mobile Tap
            {
                Plant();
            }
        }
    }

    public void Plant()
    {
        if (cropPrefab != null)
        {
            Instantiate(cropPrefab, transform.position, Quaternion.identity, transform);
            isPlanted = true;
            Debug.Log("Đã trồng cây tại " + gameObject.name);
        }
    }

}