using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Dùng để lọc List dễ dàng hơn

public class FishingMiniGame : MonoBehaviour
{
    [Header("Current Fish Logic")]
    private Item currentFishData;
    private int currentFishID;

    [Header("Settings & UI")]
    public RectTransform areaRect;
    public RectTransform fishRect;
    public RectTransform bobberRect;
    public Slider progressBar;

    [Header("Physics (Base)")]
    public float gravity = 500f;
    public float liftForce = 600f;
    private float fishSpeed; 

    private float bobberPosition;
    private float bobberVelocity;
    private float fishPosition;
    private float fishDestination;
    private float fishTimer;
    private float progress = 0f;

    private Health health;
    private int expReward;
    private int staminaCost;

    private void Awake()
    {
        health = FindAnyObjectByType<Health>();
    }

    // --- RANDOM CÁ VÀ TÍNH ĐỘ KHÓ ---
    void OnEnable()
    {
        SetupRandomFish();

        // Reset trạng thái UI
        progress = 25f; // Cho sẵn một ít tiến trình
        bobberPosition = 0f;
        fishPosition = 50f;
    }


    void SetupRandomFish()
    {
        if (ItemDictionary.Instance == null) return;

        List<GameObject> allPrefabs = ItemDictionary.Instance.GetAllPrefabs();
        List<Item> fishList = allPrefabs
            .Select(go => go.GetComponent<Item>())
            .Where(item => item != null && item.itemType == ItemType.Fish)
            .ToList();

        if (fishList.Count == 0) return;

        currentFishData = fishList[Random.Range(0, fishList.Count)];
        currentFishID = currentFishData.ID;

        float price = currentFishData.price;

        
        int tier = Mathf.FloorToInt(price / 100f);
        tier = Mathf.Clamp(tier, 1, 9); 

        // TÍNH ĐỘ DÀI BOBBER 
        float baseHeight = 0f;
        float minHeightInTier = 0f;
        
        switch (tier)
        {
            case 1: baseHeight = 160f; minHeightInTier = 140f; break; 
            case 2: baseHeight = 140f; minHeightInTier = 120f; break; 
            case 3: baseHeight = 120f; minHeightInTier = 100f; break;
            case 4: baseHeight = 100f; minHeightInTier = 90f; break;
            case 5: baseHeight = 90f; minHeightInTier = 80f; break;
            case 6: baseHeight = 85f; minHeightInTier = 75f; break;
            case 7: baseHeight = 80f; minHeightInTier = 70f; break;
            case 8: baseHeight = 75f; minHeightInTier = 65f; break;
            case 9: baseHeight = 70f; minHeightInTier = 60f; break;
            default: baseHeight = 60f; minHeightInTier = 50f; break;
        }

        // Tính toán độ giảm của vùng câu 
        float tInTier = (price % 100) / 99f;
        float targetHeight = Mathf.Lerp(baseHeight, minHeightInTier, tInTier);

        // Cập nhật size
        bobberRect.sizeDelta = new Vector2(bobberRect.sizeDelta.x, targetHeight);

        // TÍNH TỐC ĐỘ CÁ 
        fishSpeed = 1.5f + (tier * 0.4f) + (tInTier * 0.2f);
        fishSpeed = Mathf.Clamp(fishSpeed, 1.8f, 6.0f);

        // TẦN SUẤT ĐỔI HƯỚNG
        tempMinWait = Mathf.Max(0.2f, 1.0f - (tier * 0.1f));
        tempMaxWait = Mathf.Max(0.5f, 2.0f - (tier * 0.15f));

        
        expReward = tier * 15;
        staminaCost = 3 + (tier * 2);
        Debug.Log($"[Tier {tier}] Cá: {currentFishData.itemName} | Giá: {price} | Bobber Height: {targetHeight:F1}");
    }

    //  biến tạm để lưu range đổi hướng
    private float tempMinWait = 0.5f;
    private float tempMaxWait = 2f;

    void Update()
    {
        HandleBobberMovement();
        HandleFishMovement();
        CheckSuccess();
    }


    void HandleBobberMovement()
    {

        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            bobberVelocity += liftForce * Time.deltaTime;
        }
        else
        {
            bobberVelocity -= gravity * Time.deltaTime;
        }

        bobberPosition += bobberVelocity * Time.deltaTime;

        // Giới hạn thanh xanh trong Area
        float maxPos = areaRect.rect.height - bobberRect.rect.height;
        if (bobberPosition < 0) { bobberPosition = 0; bobberVelocity = 0; }
        if (bobberPosition > maxPos) { bobberPosition = maxPos; bobberVelocity = 0; }

        bobberRect.anchoredPosition = new Vector2(0, bobberPosition);
    }

    void HandleFishMovement()
    {
        fishTimer -= Time.deltaTime;
        if (fishTimer <= 0)
        {
            // Sử dụng tempMinWait và tempMaxWait đã tính theo giá tiền
            fishTimer = Random.Range(tempMinWait, tempMaxWait);

            fishDestination = Random.Range(0, areaRect.rect.height - fishRect.rect.height);
        }

        fishPosition = Mathf.Lerp(fishPosition, fishDestination, Time.deltaTime * fishSpeed);
        fishRect.anchoredPosition = new Vector2(0, fishPosition);
    }

    void CheckSuccess()
    {
        float bobberBottom = bobberPosition;
        float bobberTop = bobberPosition + bobberRect.rect.height;
        float fishBottom = fishPosition;
        float fishTop = fishPosition + fishRect.rect.height;

        if (fishTop > bobberBottom && fishBottom < bobberTop)
            progress += 20f * Time.deltaTime;
        else
            progress -= 15f * Time.deltaTime;

        progress = Mathf.Clamp(progress, 0, 100);
        progressBar.value = progress;

        Debug.Log("Tiến trình hiện tại: " + progress);

        if (progress >= 100) Win();
        if (progress <= 0) Lose();
    }

    void Win()
    {
        Debug.Log($"Thành công! Đã câu được {currentFishData.itemName}");

        health.Tired(staminaCost);
        health.AddExperience(expReward);

        if (RewardsController.Instance != null)
        {
            RewardsController.Instance.GiveItemReward(currentFishID, 1);
        }

        EndGame();
    }

    void Lose()
    {
        health.Tired(staminaCost);
        Debug.Log("Cá sổng mất rồi...");
        EndGame();
    }

    void EndGame()
    {
        gameObject.SetActive(false);
        FishingSpot spot = FindAnyObjectByType<FishingSpot>();
        if (spot != null) spot.EndFishing();
    }
}


