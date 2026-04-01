using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DPUtils.System.DateTime;
using Unity.Cinemachine;

public class CustomerManager : MonoBehaviour
{

    // --- KHAI BÁO BỔ SUNG ---

    public GameObject stickButton;
    public GameObject interactButton;

    private Sprite selectedHairSprite; // Thêm biến này để lưu ảnh tóc đã chọn
    private bool isProcessingCustomer = false;

    [Header("New UI References")]
    public Image targetHairRequestUI; // Cái UI hiện cái tóc mà khách MUỐN (để người chơi nhìn theo)
    public GameObject cutButton;      // Nút "Cắt" của thanh PowerBar
    public GameObject powerBarUI;


    [Header("New Selection System")]
    public HairData hairDatabase; // Chỉ cần kéo 1 file duy nhất vào đây
    public HairSelectorUI hairSelector;

    private HairEntry targetHairRequest; // Đổi kiểu từ HairData thành HairEntry
    private bool isCorrectStyleSelected = false;

    [Header("Shop State")]
    public bool isShopOpen = false; // Trạng thái đóng/mở tiệm

    [Header("Seat Management")]
    public Transform[] seats; // Kéo các vị trí ghế vào đây trong Inspector
    private bool[] isSeatOccupied;

    // Biến lưu trữ khách hàng hiện tại đang tương tác
    private Customer currentServingCustomer;

    [Header("Spawn Settings")]
    public GameObject customerPrefab; // Kéo Prefab Customer vào đây
    public Transform entrancePoint;   // Điểm xuất hiện (Waypoint đầu tiên)
    public float spawnInterval = 10f; // Khoảng thời gian giữa mỗi lần khách đến
    private float spawnTimer;

    //___________________________________________________

    [Header("Customer Data Sources")]
    public CustomerData normalData;
    public CustomerData soulData;
    public CustomerData bossData;

    [Header("Player UI References")]
    public GameObject player;
    public GameObject customerPanel;
    public GameObject buttonIcon;
    public GameObject hairBefore;

    
    public GameObject closeUI;
    public GameObject openUI;
    public GameObject manaOut;


    public Image customerImage;
    public TMP_Text dialogueText;

    [Header("Boss UI References")]
    public GameObject bossFightUI;
    public GameObject bossHealthUI;
    public GameObject bossWayPoint;

    private PlayerController playerController;
    private PlayerCombats playerCombats;
    private PowerBarController powerBar;
    private TimeManager timeManager;
    private Health health;
    private EnemyHealth enemyHealth;


    [Header("Hair Result UI")]
    public GameObject hairResultPanel;
    public Image hairResultImage;
    public TMP_Text scoreText;
    public TMP_Text hairResultText;

    [Header("System Settings")]
    public float bossThreshold = 0f; 
    public float soulChance = 0.30f;

    [Header("Stop Cutting Confirmation UI")]
    public GameObject stopConfirmPanel;

    [Header("Daily Summary")]
    public GameObject daySummaryPanel;
    public TMP_Text days;
    public TMP_Text totalCustomerText;
    public TMP_Text totalMoneyText;

    private bool hasShownSummaryToday = false;
    private bool hasResetToday = false;

    private int customersToday = 0;
    private int moneyToday = 0;
    private int goldBeforeCut = 0;

    private bool isCuttingHair = false;
    private bool isActive = false;

    private CustomerData currentCustomerData;
    private AudioController audioController;
    private AudioClip currentMusic = null;


    void Start()
    {
        // Đảm bảo khi bắt đầu (hoặc load lại scene), tất cả ghế đều trống
        if (seats != null)
        {
            isSeatOccupied = new bool[seats.Length];
            for (int i = 0; i < isSeatOccupied.Length; i++)
            {
                isSeatOccupied[i] = false;
            }
        }

        // Reset các biến trạng thái phục vụ
        isProcessingCustomer = false;
        isCuttingHair = false;
        spawnTimer = 2f; // Khách đầu tiên sẽ đến sau 2 giây khi mở tiệm

        customerPanel.SetActive(false);
        hairBefore.SetActive(false);
        hairResultPanel.SetActive(false);
        buttonIcon.SetActive(false);
        bossFightUI.SetActive(false);
        bossHealthUI.SetActive(false);

        /*
        if (health == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                health = playerObj.GetComponent<Health>();
            else
                Debug.LogError("Không tìm thấy Player có tag 'Player'!");
        }*/

        all();
        audioController.PlayMusic(audioController.morning, true);

    }

    private void all()
    {
        audioController = FindAnyObjectByType<AudioController>();
        enemyHealth = FindAnyObjectByType<EnemyHealth>();
        playerCombats = FindAnyObjectByType<PlayerCombats>();
        playerController = FindAnyObjectByType<PlayerController>();
        powerBar = FindAnyObjectByType<PowerBarController>();
        timeManager = FindAnyObjectByType<TimeManager>();
        health = FindAnyObjectByType<Health>();
    }

    void Update()
    {

        if (playerCombats.enabled == true)
        {
            SwitchMusic(audioController.bossFightMusic);
        }
        else
        {
            SwitchMusic(audioController.mainMenuMusic2);
        }

        var time = timeManager.GetCurrentDateTime();
        /*
        if (time.Hour == 6)
        {
            if (!hasShownSummaryToday)
            {
                ShowDailySummary();
                hasShownSummaryToday = true;
            }
        }
        else
        {
            hasShownSummaryToday = false;
        }
        */

        if (time.TimeToOpen())
        {
            if (!hasResetToday)
            {
                customersToday = 0;
                moneyToday = 0;
                hasResetToday = true;
            }
        }
        else
        {
            hasResetToday = false;
        }

        //-----------------------------------------------------
        if (isActive) // Nếu Player đang đứng ở tiệm
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleShop();
            }
        }

        if (isShopOpen && health != null && health.MN <= 0)
        {
            isShopOpen = false;
            if (isActive)
            {
                openUI.SetActive(false);
                closeUI.SetActive(false);
            }
            StartCoroutine(PlayerIsTired()); // Hiện thông báo mệt mỏi
            ClearAllCustomers();
        }

        // Chỉ tự động Spawn khách khi tiệm đang Mở 
        if (isShopOpen)
        {
            HandleAutoSpawn();
        }

        if (!timeManager.GetCurrentDateTime().TimeToOpen() && isShopOpen)
        {
            isShopOpen = false;
            if (isActive) { openUI.SetActive(false); closeUI.SetActive(true); }
        }
    }
    
    //==============  MOBILE BUTTON ====================================================================

    public void ToggleShop()
    {
        // Nếu người chơi không đứng ở tiệm thì không cho bấm 
        if (!isActive) return;

        if (health != null && health.MN <= 0)
        {
            openUI.SetActive(false);
            closeUI.SetActive(false);
            StartCoroutine(PlayerIsTired());
            return;
        }

        var time = timeManager.GetCurrentDateTime();

        if (time.TimeToOpen())
        {
            isShopOpen = !isShopOpen;

            if (isShopOpen)
            {
                Debug.Log("Tiệm mở cửa!");
                openUI.SetActive(true);
                closeUI.SetActive(false);
                spawnTimer = 1f;
            }
            else
            {
                Debug.Log("Tiệm đóng cửa!");
                ClearAllCustomers();
                openUI.SetActive(false);
                closeUI.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Chưa đến giờ mở cửa!");
            isShopOpen = false;
            openUI.SetActive(false);
            closeUI.SetActive(true);
        }
    }

    IEnumerator PlayerIsTired()
    {
        manaOut.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        manaOut.SetActive(false);
    }

    //==================================================================================

    // SPAWN CUSTOMER
    private void HandleAutoSpawn()
    {
        // Đếm ngược thời gian
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            TrySpawnCustomer();
            spawnTimer = spawnInterval; // Reset thời gian chờ khách tiếp theo
        }
    }

    private void TrySpawnCustomer()
    {
        // 1. Tìm ghế còn trống
        int emptySeatIndex = -1;
        for (int i = 0; i < isSeatOccupied.Length; i++)
        {
            if (!isSeatOccupied[i])
            {
                emptySeatIndex = i;
                break;
            }
        }

        // Nếu hết ghế thì không tạo thêm khách
        if (emptySeatIndex == -1) return;

        // 2. Lấy dữ liệu khách (Linh hồn/Người) từ hàm PickCustomerData có sẵn của bạn
        CustomerData data = PickCustomerData();

        // 3. Tạo khách tại điểm Entrance và điều hướng vào ghế
        GameObject newCustomer = Instantiate(customerPrefab, entrancePoint.position, Quaternion.identity);
        Customer customerScript = newCustomer.GetComponent<Customer>();

        if (customerScript != null)
        {
            isSeatOccupied[emptySeatIndex] = true;
            // Gọi hàm Init để khách tự đi vào ghế (Waypoint logic)
            customerScript.Init(data, seats[emptySeatIndex]);
        }
    }

    // Hàm này được gọi từ Customer.cs khi khách rời đi (hết giờ hoặc xong việc)
    public void OnCustomerLeave(Transform seat)
    {
        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] == seat)
            {
                isSeatOccupied[i] = false;
                break;
            }
        }
    }
    //==================================================================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isActive = true;
            buttonIcon.SetActive(true);

            if (health != null && health.MN <= 0)
            {
                openUI.SetActive(false);
                closeUI.SetActive(false);

                StartCoroutine(PlayerIsTired());
            }
            else
            {
                if (isShopOpen)
                {
                    openUI.SetActive(true);
                    closeUI.SetActive(false);
                }
                else
                {
                    openUI.SetActive(false);
                    closeUI.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isActive = false;
            buttonIcon.SetActive(false);
            openUI.SetActive(false);
            closeUI.SetActive(false);
        }
    }

    //------------Stop music------------------------------------------------------------

    private void SwitchMusic(AudioClip newClip)
    {
        if (currentMusic == newClip) return; 

        audioController.StopMusic();
        audioController.PlayMusic(newClip, true);
        currentMusic = newClip;
    }
 
    //------------StopCutting------------------------------------------------------------
    public void ShowStopCuttingPopup()
    {
        stopConfirmPanel.SetActive(true);

        powerBar.enabled = false;

        Time.timeScale = 0f;
    }

    public void OnStopConfirmNo()
    {
        Time.timeScale = 1f;
        stopConfirmPanel.SetActive(false);
        powerBar.enabled = true;
    }

    public void OnStopConfirmYes()
    {
        Time.timeScale = 1f;
        stopConfirmPanel.SetActive(false);
        HideCustomer();
    }

    //-----------------------------------------------------------------------------------

    public void BossFightButton()
    {
        playerController.enabled = true;

        if (player == null || bossWayPoint == null)
        {
            Debug.LogWarning("Player hoặc Boss WayPoint chưa được gán trong Inspector!");
            return;
        }


        // Đóng quán ngay lập tức để HandleAutoSpawn không tạo thêm khách mới
        isShopOpen = false;

        // Cập nhật UI thông báo đóng cửa 
        if (isActive)
        {
            openUI.SetActive(false);
            closeUI.SetActive(true);
        }

        ClearAllCustomers();

        // ------------------------------------------------

        if (stickButton != null) stickButton.SetActive(true);

        player.transform.position = bossWayPoint.transform.position;

        // Camera Confiner logic
        Collider2D collider = Physics2D.OverlapPoint(player.transform.position);
        if (collider != null && collider.GetComponent<BoxCollider2D>() != null)
        {
            var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
            if (confiner != null)
                confiner.BoundingShape2D = collider.GetComponent<BoxCollider2D>();
        }

        bossFightUI.SetActive(false);
        bossHealthUI.SetActive(true);

        if (customerPanel.activeSelf) customerPanel.SetActive(false);
        buttonIcon.SetActive(false);

        StartCoroutine(BossFightDelay());
    }

    IEnumerator BossFightDelay()
    {
        yield return new WaitForSeconds(1.5f);
        playerCombats.enabled = true;
    }
   
    public void hideBossUI()
    {
        enemyHealth.EnemyHeal(500);
        playerCombats.enabled = false;
        bossHealthUI.SetActive(false);
        if (interactButton != null) interactButton.SetActive(false);
    }

    //============================================================================

    private void ClearAllCustomers()
    {
        Customer[] allCustomers = FindObjectsByType<Customer>(FindObjectsSortMode.None);

        foreach (Customer c in allCustomers)
        {
            // Nếu khách đang ngồi trên ghế và CHƯA được phục vụ
            if (c.CanInteract())
            {
                // Gọi hàm rời đi kèm hình phạt (ví dụ phạt 15 điểm Stress cơ bản)
                c.LeaveUnserved(5);
            }
            else
            {
                // Nếu khách đang đi vào hoặc đã phục vụ xong thì về bình thường
                c.FinishAndLeave();
            }

            if (c.mySeat != null)
            {
                OnCustomerLeave(c.mySeat);
            }
        }

        isProcessingCustomer = false;
        isCuttingHair = false;
        if (customerPanel.activeSelf) customerPanel.SetActive(false);
    }

    public void ShowCustomer(Customer customer)
    {
        if (isProcessingCustomer) return;

        isProcessingCustomer = true;
        currentServingCustomer = customer;
        isCuttingHair = true;

        if (stickButton != null) stickButton.SetActive(false);
        if (interactButton != null) interactButton.SetActive(false);
        // Lấy dữ liệu khách hàng
        currentCustomerData = PickCustomerData();

        
        if (currentCustomerData == soulData)
        {
            powerBar.speed = 2000f;
        }
        else if (currentCustomerData == bossData)
        {
            powerBar.speed = 2500f; 
        }
        else
        {
            powerBar.speed = 1500f; 
        }

        // Hiển thị UI ngoại hình
        customerImage.sprite = currentCustomerData.sprites[Random.Range(0, currentCustomerData.sprites.Count)];
        dialogueText.text = currentCustomerData.dialogues[Random.Range(0, currentCustomerData.dialogues.Count)];

        // Random mẫu tóc yêu cầu
        targetHairRequest = hairDatabase.GetRandomHair();
        if (targetHairRequest != null)
        {
            targetHairRequestUI.sprite = targetHairRequest.hairSprite;
        }

        customerPanel.SetActive(true);
        hairBefore.SetActive(true);
        hairSelector.gameObject.SetActive(true);
        hairSelector.Setup(hairDatabase.allHairs);

        powerBarUI.SetActive(false);
        if (cutButton != null) cutButton.SetActive(false);

        playerController.enabled = false;

        // Báo cho Customer biết là đã bắt đầu phục vụ để dừng đếm ngược bỏ về
        customer.StartBeingServiced();
    }

    public void StartCuttingMinigame()
    {
        // Lưu thông tin tóc đã chọn
        HairEntry selected = hairSelector.GetSelectedHair();
        isCorrectStyleSelected = (selected.styleID == targetHairRequest.styleID);
        selectedHairSprite = selected.hairSprite; // Lưu lại Sprite để hiện ở bảng kết quả

        // Chuyển đổi UI
        
        hairSelector.gameObject.SetActive(false);
        powerBarUI.SetActive(true);
        
        powerBar.enabled = true;
        powerBar.gameObject.SetActive(true);
        // Reset PowerBar 
        powerBar.ResetBar();

        if (cutButton != null) cutButton.SetActive(true);

    }

    public void FinishHaircut(int skillScore, int skillMoney)
    {
        int finalMoney = 0;
        string feedbackMessage = "";

        if (hairResultImage != null)
        {
            hairResultImage.sprite = selectedHairSprite;
        }

        // Tính tiền và phản hồi
        if (isCorrectStyleSelected)
        {
            int bonusMatch = 100;
            finalMoney = skillMoney + bonusMatch;
            feedbackMessage = $"Đúng mẫu (+{bonusMatch}) & Kỹ thuật {skillScore}đ!";
        }
        else
        {
            int penaltyMismatch = -150;
            finalMoney = skillMoney + penaltyMismatch;
            health.DecreaseStress(10);
            feedbackMessage = $"Sai mẫu rồi (-{Mathf.Abs(penaltyMismatch)})! Kỹ thuật {skillScore}đ.";
        }

        // Cập nhật hệ thống tiền tệ và số lượng khách
        health.AddGold(finalMoney);
        moneyToday += finalMoney;
        customersToday++;

        // Dọn dẹp trạng thái PowerBar để khách sau không bị lỗi
        powerBar.enabled = false; // Tắt script để dừng Update bên trong PowerBar
        if (cutButton != null) cutButton.SetActive(false);

        // Cập nhật thống kê vào Health
        if (health != null) health.AddCustomer();

        if (QuestController.Instance != null && currentCustomerData != null)
        {
            QuestController.Instance.OnCustomerServed(currentCustomerData.customerType);
        }

        // Hiển thị Panel kết quả
        DisplayResult(skillScore, finalMoney, feedbackMessage);


    }

    private void DisplayResult(int score, int money, string msg)
    {
        hairBefore.SetActive(false);
        hairResultPanel.SetActive(true);
        scoreText.text = "Điểm kỹ năng: " + score;
        hairResultText.text = msg;

        hairResultText.color = (money >= 0) ? Color.green : Color.red;

        StartCoroutine(ChangeCustomerRoutine());
    }

    public void HideCustomer()
    {
        if (currentServingCustomer != null)
        {
            OnCustomerLeave(currentServingCustomer.mySeat);
            currentServingCustomer.FinishAndLeave();
            currentServingCustomer = null;
        }

        customerPanel.SetActive(false);
        playerController.enabled = true;

        if (isActive)
        {
            if (stickButton != null) stickButton.SetActive(true);
        }

        // QUAN TRỌNG: Chỉ mở khóa tương tác khi khách cũ đã thực sự rời đi hoàn toàn
        isProcessingCustomer = false;
        isCuttingHair = false;
    }

    CustomerData PickCustomerData()
    {

        if (health.Stress <= bossThreshold)
        {
            //powerBar.enabled = false;
            bossFightUI.SetActive(true);
            return bossData;
        }


        var currentDateTime = timeManager.GetCurrentDateTime();


        if (currentDateTime.SoulTime())
        {
            float roll = Random.value;
            if (roll < soulChance) 
            {
                audioController.PlaySFX(audioController.soul, false);
                return soulData;
            }
            else 
            {
                audioController.PlaySFX(audioController.customer, false);
                return normalData; 
            }
               
        }
        audioController.PlaySFX(audioController.customer, false);
        return normalData;
    }

    public IEnumerator ChangeCustomerRoutine()
    {
        yield return new WaitForSeconds(1.4f);
        hairResultPanel.SetActive(false);
        HideCustomer();
    }

 
    public void damageOffDailyUI() 
    {
        daySummaryPanel.SetActive(false);
        int firstDay = timeManager.GetCurrentDateTime().TotalNumDays;
        if(firstDay <= 1)
        {
            Debug.Log("Không bị mất máu");
        }
        else
        {
            //health.Damage(10);
            //health.DecreaseStress(10);
        }
    }
}

/*
    private void ShowDailySummary()
    {


        int firstDay = timeManager.GetCurrentDateTime().TotalNumDays;
        if (firstDay <= 1)
        {
            daySummaryPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            daySummaryPanel.SetActive(true);
            Time.timeScale = 0f;

        }

        //daySummaryPanel.SetActive(true);
        playerController.enabled = true;
        customerPanel.SetActive(false);

        int daySummary = timeManager.GetCurrentDateTime().TotalNumDays;
        days.text = $"Số ngày đã sống: {daySummary}";

        totalCustomerText.text = "Khách hôm nay: " + customersToday;
        totalMoneyText.text = "Tiền kiếm được: " + moneyToday;

        Debug.Log("Summary ngày đã hiển thị");
    }
 
  public void ShowCustomer(Customer customer)
    {
        currentServingCustomer = customer; 
        isCuttingHair = true;

        // Nếu tiệm đóng, không cho mở panel
        if (!timeManager.GetCurrentDateTime().TimeToOpen())
        {
            closeUI.SetActive(true);
            Debug.Log("Tiệm đóng cửa. Không có khách!");

            if (powerBar != null)
                powerBar.enabled = false;

            playerController.enabled = true;
            return;
        }

        customer.StartBeingServiced(); // GỌI ĐỂ KHÁCH DỪNG KIÊN NHẪN

        goldBeforeCut = health.Gold;

        powerBar.enabled = true;
        customerPanel.SetActive(true);
        playerController.enabled = false;

        hairBefore.SetActive(true);

        //CustomerData dataToUse = PickCustomerData();
        // Xác định khách là linh hồn hay người
        currentCustomerData = PickCustomerData();

        bool isSoul = (currentCustomerData == soulData);
        powerBar.SetCustomerType(isSoul);

        // tốc độ cho linh hồn
        powerBar.speed = (currentCustomerData == soulData) ? 2000f : 1500f;
        // Random sprite và lời nói
        Sprite randomSprite = currentCustomerData.sprites[Random.Range(0, currentCustomerData.sprites.Count)];
        string randomDialogue = currentCustomerData.dialogues[Random.Range(0, currentCustomerData.dialogues.Count)];

        // Gán lên UI
        customerImage.sprite = randomSprite;
        dialogueText.text = randomDialogue;

        // reset PowerBar
        powerBar.ResetBar();
    }

  


    public void ReactToHaircut(int score)
    {
        hairBefore.SetActive(false);
        hairResultPanel.SetActive(true);

        scoreText.text = "Điểm: " + score;

        HairData set =
            (score >= 10) ? veryGoodHair :
            (score >= 9) ? goodHair :
            (score >= 7) ? normalHair :
            (score >= 5) ? notBadHair :
                            veryBadHair;
        
        hairResultImage.sprite = set.hairSprites[Random.Range(0, set.hairSprites.Count)];
        // Lời thoại phản ứng
        string message;
        if (score >= 10)
            message = "Tuyệt vời! Tôi thích kiểu này!";
        else if (score >= 9)
            message = "Cũng được đấy, nhìn ổn!";
        else if (score >= 7)
            message = "Ờm... cũng tạm thôi.";
        else if (score >= 5)
            message = "Thảm họa đó!";
        else
            message = "Quá thất vọng!";

        hairResultText.text = message;

        //----------------Đếm số lượng khách và tiền trong ngày------------------
        customersToday++;

        int goldAfterCut = health.Gold;
        int moneyEarned = goldAfterCut - goldBeforeCut;  

        if (moneyEarned < 0) moneyEarned = 0; 

        moneyToday += moneyEarned;

        Debug.Log("Tiền kiếm được: " + moneyEarned);

        //health.SetGold(health.Gold + moneyEarned);

        // Sau vài giây, khách rời đi và xuất hiện khách mới
        StartCoroutine(ChangeCustomerRoutine());
    }

 */

