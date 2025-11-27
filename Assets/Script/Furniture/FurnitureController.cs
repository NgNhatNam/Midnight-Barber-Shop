using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    public Transform sitPoint;   // Điểm mà player sẽ ngồi
    private bool playerInRange = false;
    private PlayerController player;   // Luôn là PlayerController

    private bool isSitting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.GetComponent<PlayerController>(); // ✅ Lấy PlayerController
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isSitting)
                SitDown();
            else
                StandUp();
        }
    }

    void SitDown()
    {
        if (player != null)
        {
            // Đặt player vào đúng vị trí ngồi
            player.transform.position = sitPoint.position;

            // Khóa script PlayerController để không di chuyển
            player.enabled = false;

            isSitting = true;
            Debug.Log("Player is sitting.");
        }
    }

    void StandUp()
    {
        if (player != null)
        {
            // Bật lại di chuyển
            player.enabled = true;

            isSitting = false;
            Debug.Log("Player stood up.");
        }
    }
}
