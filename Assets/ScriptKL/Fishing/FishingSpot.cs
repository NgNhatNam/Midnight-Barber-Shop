using UnityEngine;

public class FishingSpot : MonoBehaviour, IInteractable
{
    [Header("UI Settings")]
    public GameObject fishingMinigameUI; 

    private bool isFishing = false;

    public bool CanInteract()
    {
        
        Health playerHealth = GameObject.FindWithTag("Player").GetComponent<Health>();

        return !isFishing && playerHealth.MN > 0;
    }

    public void Interact()
    {
        Debug.Log("Hàm Interact đã được gọi!");
        if (!isFishing)
        {
            if (fishingMinigameUI == null)
            {
                Debug.LogError("Chưa kéo Panel UI vào ô fishingMinigameUI trong Inspector!");
                return;
            }

            fishingMinigameUI.SetActive(true);
            Debug.Log("Trạng thái UI sau khi SetActive: " + fishingMinigameUI.activeSelf);
        }
    }

    void StartFishing()
    {
        isFishing = true;

        if (fishingMinigameUI != null)
        {
            fishingMinigameUI.SetActive(true);
        }

        // Time.timeScale = 0f; 

    }

    public void EndFishing()
    {
        isFishing = false;
        if (fishingMinigameUI != null)
        {
            fishingMinigameUI.SetActive(false);
        }
        // Time.timeScale = 1f;
    }
}