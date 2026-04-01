using UnityEngine;
using System.Collections;
using DPUtils.System.DateTime;

public class Animal : MonoBehaviour, IInteractable
{
    [Header("Animal Settings")]
    public string animalName;    
    public int birthDayTotal = -1;

    [Header("Harvest Settings")]
    public int produceItemID;
    public int produceAmount = 1;
    public int daysToRegrow = 1;


    [Header("Slaughter Settings")]
    public int meatItemID;    // ID Thịt
    public int baseMeatAmount = 1;

    [Header("Movement Settings")]
    public Collider2D moveArea;
    public float moveSpeed = 1.0f;
    public float idleTime = 3f;
    public float wanderRadius = 4f;

    // Animation States - Giống hệt cách bạn làm với NPC
    const string ANIMAL_EAT = "Eat";
    const string TOP_WALK = "TopWalk";
    const string BOTTOM_WALK = "BottomWalk";
    const string LEFT_WALK = "LeftWalk";
    const string RIGHT_WALK = "RightWalk";
    const string TOP_IDLE = "TopIdle";
    const string BOTTOM_IDLE = "BottomIdle";
    const string LEFT_IDLE = "LeftIdle";
    const string RIGHT_IDLE = "RightIdle";

    private string currentAnimation;
    private Animator animator;
    private Vector2 lastDirection = Vector2.down;
    private bool isMoving = false;
    private bool isEating = false;

    // Biến lưu trữ ngày thu hoạch cuối cùng
    public int lastHarvestTotalDays = -999;

    // Biến tạm để lưu thông số ngẫu nhiên riêng cho mỗi con
    private float individualSpeed;
    private float individualIdleTime;

    [Header("Economy Settings")]
    public int basePrice = 100; // Giá mua ban đầu
    public float priceMultiplierPerDay = 1.1f; // Mỗi ngày tăng 10% giá trị

    void Start()
    {
        animator = GetComponent<Animator>();

        if (birthDayTotal <= 0)
        {
            if (TimeManager.Instance != null)
            {
                birthDayTotal = TimeManager.Instance.GetCurrentDateTime().TotalNumDays;
            }
            else
            {
                // Phòng hờ nếu TimeManager chưa kịp Init
                birthDayTotal = 1;
            }
        }

        // Tạo sự khác biệt về tốc độ (lệch khoảng 20%)
        individualSpeed = moveSpeed * Random.Range(0.8f, 1.2f);
        // Tạo sự khác biệt về thời gian đứng yên
        individualIdleTime = idleTime * Random.Range(0.7f, 1.5f);


        StartCoroutine(WanderRoutine());
    }

    //  Tính thịt khi mổ 
    public int GetSlaughterMeatAmount()
    {
        if (TimeManager.Instance == null) return baseMeatAmount;
        int daysAlive = TimeManager.Instance.GetCurrentDateTime().TotalNumDays - birthDayTotal;

        // Thịt gốc + (số ngày sống chia 3)
        return baseMeatAmount + (daysAlive / 3);
    }

    public int GetSellPrice()
    {
        if (TimeManager.Instance == null) return basePrice;

        int currentDay = TimeManager.Instance.GetCurrentDateTime().TotalNumDays;
        int daysAlive = currentDay - birthDayTotal;

        // Công thức: Giá gốc * (Tỉ lệ ^ số ngày)
        return Mathf.RoundToInt(basePrice * Mathf.Pow(priceMultiplierPerDay, daysAlive));
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Tỉ lệ ăn ngẫu nhiên
            if (Random.value < 0.2f)
            {
                isEating = true;
                ChangeAnimationState(ANIMAL_EAT);
                yield return new WaitForSeconds(Random.Range(2f, 3.5f)); // Ăn lâu mau khác nhau
                isEating = false;
            }

            // --- LOGIC CHỌN ĐIỂM ĐẾN MỚI ---
            Vector2 targetPos = GetRandomValidPos();

            isMoving = true;
            while (Vector2.Distance(transform.position, targetPos) > 0.1f)
            {
                Vector2 dir = (targetPos - (Vector2)transform.position).normalized;

                // Sử dụng tốc độ riêng của mỗi con
                transform.position = Vector2.MoveTowards(transform.position, targetPos, individualSpeed * Time.deltaTime);

                lastDirection = dir;
                UpdateAnimationState(dir, true);
                yield return null;
            }

            isMoving = false;
            UpdateAnimationState(lastDirection, false);

            // Nghỉ ngơi theo thời gian riêng của mỗi con
            yield return new WaitForSeconds(individualIdleTime);
        }
    }

    // Hàm bổ trợ để tìm điểm đến không bao giờ trùng center
    Vector2 GetRandomValidPos()
    {
        Vector2 potentialPos;
        int attempts = 0;

        do
        {
            Vector2 randomStep = Random.insideUnitCircle * wanderRadius;
            potentialPos = (Vector2)transform.position + randomStep;
            attempts++;

            // Nếu thử quá 10 lần không tìm được điểm trong vùng, thì lấy đại một điểm ngẫu nhiên TRONG bounds
            // chứ không lấy ngay Center
            if (attempts > 10 && moveArea != null)
            {
                return new Vector2(
                    Random.Range(moveArea.bounds.min.x, moveArea.bounds.max.x),
                    Random.Range(moveArea.bounds.min.y, moveArea.bounds.max.y)
                );
            }

        } while (moveArea != null && !moveArea.OverlapPoint(potentialPos));

        return potentialPos;
    }

    // Logic chọn string animation dựa trên hướng
    void UpdateAnimationState(Vector2 dir, bool moving)
    {
        if (isEating) return;

        if (moving)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                ChangeAnimationState(dir.x > 0 ? RIGHT_WALK : LEFT_WALK);
            else
                ChangeAnimationState(dir.y > 0 ? TOP_WALK : BOTTOM_WALK);
        }
        else
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                ChangeAnimationState(dir.x > 0 ? RIGHT_IDLE : LEFT_IDLE);
            else
                ChangeAnimationState(dir.y > 0 ? TOP_IDLE : BOTTOM_IDLE);
        }
    }
    // Hàm đổi Anim chuẩn của bạn
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;
        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }

    public bool CanInteract()
    {
        if (TimeManager.Instance == null) return true;

        int currentTotalDays = TimeManager.Instance.GetCurrentDateTime().TotalNumDays;
        int daysAlive = currentTotalDays - birthDayTotal;

        // Đủ 5 ngày tuổi VÀ đã hồi sản phẩm
        bool isMature = daysAlive >= 5;
        bool isRegrown = (currentTotalDays - lastHarvestTotalDays) >= daysToRegrow;

        return isMature && isRegrown;
    }
    public void Interact()
    {
        if (TimeManager.Instance == null) return;

        // Gọi hàm kiểm tra duy nhất
        if (CanInteract())
        {
            if (RewardsController.Instance != null)
            {
                RewardsController.Instance.GiveItemReward(produceItemID, produceAmount);
                lastHarvestTotalDays = TimeManager.Instance.GetCurrentDateTime().TotalNumDays;
                Debug.Log($"Đã thu hoạch sản phẩm từ {animalName}.");
            }
        }
        else
        {
            // Thông báo chung hoặc bạn có thể chi tiết hơn nếu muốn
            int daysAlive = TimeManager.Instance.GetCurrentDateTime().TotalNumDays - birthDayTotal;
            if (daysAlive < 5)
                Debug.Log($"{animalName} chưa đủ 5 ngày tuổi để tạo sản phẩm!");
            else
                Debug.Log($"{animalName} chưa hồi sản phẩm mới!");
        }
    }
}