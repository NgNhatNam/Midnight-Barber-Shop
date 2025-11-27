using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DPUtils.System.DateTime;
using Unity.Cinemachine;

public class CustomerManager : MonoBehaviour
{
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
    public float bossThreshold = 0f; // stress < 20 => gặp Boss
    public float soulChance = 0.30f;

    [Header("Haircut")]
    public HairData veryGoodHair;
    public HairData goodHair;
    public HairData normalHair;
    public HairData notBadHair;
    public HairData veryBadHair;

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
        audioController = FindAnyObjectByType<AudioController>();
        enemyHealth = FindAnyObjectByType<EnemyHealth>();
        playerCombats = FindAnyObjectByType<PlayerCombats>();
        playerController = FindAnyObjectByType<PlayerController>();
        powerBar = FindAnyObjectByType<PowerBarController>();
        timeManager = FindAnyObjectByType<TimeManager>();
        health = FindAnyObjectByType<Health>();


        audioController.PlayMusic(audioController.morning, true);

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
        if (isActive)
        {
            var currentTime = time.TimeToOpen();

            openUI.SetActive(currentTime);
            closeUI.SetActive(!currentTime);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!customerPanel.activeSelf)
                    ShowCustomer();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (customerPanel.activeSelf)
                    ShowStopCuttingPopup();
            }
        }



    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isActive = true;
            buttonIcon.SetActive(true);

            var currentTime = timeManager.GetCurrentDateTime();

            if (currentTime.TimeToOpen()) 
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

        // Dịch chuyển player
        player.transform.position = bossWayPoint.transform.position;
        Debug.Log($"Dịch chuyển đến Boss WayPoint tại {bossWayPoint.transform.position}");


        // Tự động tìm BoxCollider2D (map boundary) mà player đang ở trong đó
        Collider2D collider = Physics2D.OverlapPoint(player.transform.position);

        if (collider != null && collider.GetComponent<BoxCollider2D>() != null)
        {
            var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
            confiner.BoundingShape2D = collider.GetComponent<BoxCollider2D>();
            Debug.Log($"Camera confiner đã chuyển sang {collider.name}");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy BoxCollider2D map boundary tại vị trí player!");
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
    }
    
    public void ShowCustomer()
    {
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

    public void HideCustomer()
    {

        var currentDateTime = timeManager.GetCurrentDateTime();

        if (isCuttingHair && customerPanel.activeSelf)
        {
            health.Damage(50);
            health.DecreaseStress(10);
            health.Tired(50);

            Debug.Log("Bạn đã bỏ giữa chừng! -10 HP");
        }

        customerPanel.SetActive(false);
        playerController.enabled = true;
    }

    CustomerData PickCustomerData()
    {

        if (health.Stress <= bossThreshold)
        {
            powerBar.enabled = false;
            bossFightUI.SetActive(true);
            return bossData;
        }


        var currentDateTime = timeManager.GetCurrentDateTime();


        if (currentDateTime.SoulTime())
        {
            float roll = Random.value;
            if (roll < soulChance) // soulChance = 0.20f => 20% là linh hồn
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
        /*// Lấy sprite ngẫu nhiên trong bộ tóc
         * HairData hairSet = null;
        if (hairSet != null && hairSet.hairSprites.Count > 0)
        {
            Sprite chosenHair = hairSet.hairSprites[Random.Range(0, hairSet.hairSprites.Count)];
            hairResultImage.sprite = chosenHair;
        }
        else
        {
            Debug.LogWarning("❗Không có sprite tóc nào trong HairData này!");
        }*/

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

    public IEnumerator ChangeCustomerRoutine()
    {
        yield return new WaitForSeconds(1.4f);
        hairResultPanel.SetActive(false);

        var time = timeManager.GetCurrentDateTime();
        
        if (time.TimeToOpen())
        {
            ShowCustomer();
            powerBar.ResetBar();
        }
    }
    
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
            health.Damage(10);

        }
    }
}
