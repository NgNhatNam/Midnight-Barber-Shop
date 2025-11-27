using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class PowerBarController : MonoBehaviour
{
    [Header("References")]
    public RectTransform marker;       // Thanh chạy qua lại (dọc)
    public RectTransform bar;          // Thanh màu (cha)

    
    private CustomerManager customerManager;
    private Health health;

    [Header("UI Control")]
    public GameObject UI;

    [Header("Settings")]
    public float speed = 1000f;         // Tốc độ di chuyển
    public KeyCode stopKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.Z;

    private bool movingUp = true;
    private bool isStopped = false;
    private float currentY;

    [Header("Perfect Zone")]
    public float centerY = 0f;   // vị trí trung tâm
    public float barHeight = 100f;  // Chiều cao thanh power

    [Header("Distance Zone")]
    public float score_10 = 10;
    public float score_9 = 30;
    public float score_7 = 50;
    public float score_5 = 60;
    public float score_3 = 100;

    private bool isSoulCustomer = false;


    private void Start()
    {
        customerManager = FindAnyObjectByType<CustomerManager>();  
        health = FindAnyObjectByType<Health>();
        currentY = -bar.rect.height / 2f;
        marker.anchoredPosition = new Vector2(0, currentY);
    }

    private void Update()
    {
        // Chỉ hoạt động nếu menu đang bật
        if (UI == null || !UI.activeSelf) 
        {
            ResetBar();
            return;
        }

        if (Input.GetKeyDown(resetKey))
        {
            ResetBar();
        }

        if (isStopped) return;

        MoveMarker();

        if (Input.GetKeyDown(stopKey))
        {
            CheckResult();
            isStopped = true;

        }

    }


    public void CutButton()
    {
        if (!isStopped)
        {
            CheckResult();
            isStopped = true;
        }
    }

    void MoveMarker()
    {
        float move = speed * Time.deltaTime * (movingUp ? 1 : -1);
        currentY += move;

        float halfHeight = bar.rect.height  / 2f;
        if (currentY >= halfHeight)
        {
            currentY = halfHeight;
            movingUp = false;
        }
        else if (currentY <= -halfHeight)
        {
            currentY = -halfHeight;
            movingUp = true;
        }

        marker.anchoredPosition = new Vector2(0, currentY);
    }

    public void SetCustomerType(bool isSoul)
    {
        isSoulCustomer = isSoul;

        if (isSoulCustomer)
            speed *= 1.5f;   // soul nhanh hơn 50% (tuỳ chỉnh)
        else
            speed *= 1f;
    }

    public void CheckResult()
    {
        int score = 0;
        float distance = Mathf.Abs(currentY - centerY);

        if (distance <= score_10)
        {
            score = 10;
            if (isSoulCustomer)
            {
                health.AddGold(300);
                health.Tired(0);
                Debug.Log("Soul Coin And Tired");
            }
            else
            {
                health.AddGold(200);
                health.Tired(1);
                Debug.Log("Human Coin And Tired");
            }
        }
        else if (distance <= score_9)
        {
            score = 9;
            if (isSoulCustomer)
            {
                health.AddGold(150);
                health.Tired(2);
                Debug.Log("Soul Coin And Tired");
            }
            else
            {
                health.AddGold(100);
                health.Tired(3);
                Debug.Log("Human Coin And Tired");
            }
        }
        else if (distance <= score_7)
        {
            score = 7;
            if (isSoulCustomer)
            {
                health.AddGold(80);
                health.Tired(2);
                Debug.Log("Soul Coin And Tired");
            }
            else
            {
                health.AddGold(60);
                health.Tired(3);
                Debug.Log("Human Coin And Tired");
            }
        }
        else if (distance <= score_5)
        {
            score = 5;
            if (isSoulCustomer)
            {
                health.AddGold(40);
                health.Tired(4);
                Debug.Log("Soul Coin And Tired");
            }
            else
            {
                health.AddGold(30);
                health.Tired(7);
                Debug.Log("Human Coin And Tired");
            }
        }
        else if (distance <= score_3)
        {
            score = 3;
            if (isSoulCustomer)
            {
                
                health.Damage(10);
                health.Tired(15);
                health.DecreaseStress(20);
                Debug.Log("⚠️ Soul bị điểm 3 → Trừ Stress!");
            }
            else
            {
                health.AddGold(5);
                health.Tired(10);
                Debug.Log("Human Coin And Tired");
            }
        }
        else
        {
            score = 0;

            if (isSoulCustomer)
            {
                // Fail hoàn toàn với Soul = trừ stress mạnh
                health.DecreaseStress(25);
                Debug.Log("❌ Cắt Soul FAILED → Trừ Stress mạnh!");
            }
        }
        /*
        float distance = Mathf.Abs(currentY - centerY);

        int score = 0;

        if (distance <= score_10)
        {
            score = 10;
            health.Tired(1);
            health.AddGold(200);
        }
        else if (distance <= score_9)
        {
            score = 9;
            health.Tired(2);
            health.AddGold(100);
        }
        else if (distance <= score_7)
        {
            score = 7;
            health.Tired(3);
            health.AddGold(60);
        }
        else if (distance <= score_5)
        {
            score = 5;
            health.Tired(7);
            health.AddGold(30);
        }
        else if (distance <= score_3)
        {
            score = 3;
            health.DecreaseStress(100);
            health.Tired(10);
            health.AddGold(5);
        }
        else
            score = 0;
        */
        Debug.Log($"🎯 Trúng! Vùng: {distance:F1} | Điểm: {score}" );

        // Gửi kết quả sang CustomerManager
        if (customerManager != null)
        {
            customerManager.ReactToHaircut(score);
        }

        isStopped = true;
    }

    public void ResetBar()
    {
        isStopped = false;
        movingUp = true;
        currentY = -bar.rect.height / 2f;
        marker.anchoredPosition = new Vector2(0, currentY);
    }
}
