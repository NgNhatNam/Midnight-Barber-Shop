using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class PowerBarController : MonoBehaviour
{
    [Header("References")]
    public RectTransform marker;       
    public RectTransform bar;          

    
    private CustomerManager customerManager;

    [Header("UI Control")]
    public GameObject UI;

    [Header("Settings")]
    public float speed = 1000f;         
    public KeyCode stopKey = KeyCode.Space;
    public KeyCode resetKey = KeyCode.Z;

    private bool movingUp = true;
    private bool isStopped = false;
    private float currentY;

    [Header("Perfect Zone")]
    public float centerY = 0f;   
    public float barHeight = 100f;  

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
        currentY = -bar.rect.height / 2f;
        marker.anchoredPosition = new Vector2(0, currentY);
    }

    private void Update()
    {

        if (UI == null || !UI.activeSelf)
        {
            ResetBar();
            return;
        }

        if (isStopped) return;

        if (Input.GetKeyDown(resetKey))
        {
            ResetBar();
        }

        MoveMarker();

        if (Input.GetKeyDown(stopKey))
        {
            CutButton(); 
        }

    }


    public void CutButton()
    {
        if (!isStopped)
        {
            isStopped = true;
            CheckResult();
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
            speed *= 1.5f;  
        else
            speed *= 1f;
    }

    public void CheckResult()
    {
        Health h = FindAnyObjectByType<Health>();

        int score = 0;
        int moneyFromSkill = 0;
        float distance = Mathf.Abs(currentY - centerY);

        // Tính tiền dựa trên vùng cắt 
        if (distance <= score_10) { score = 10; moneyFromSkill = 200; h.AddExperience(50); h.Tired(4); }
        else if (distance <= score_9) { score = 9; moneyFromSkill = 100; h.AddExperience(20); h.Tired(7); }
        else if (distance <= score_7) { score = 7; moneyFromSkill = 60; h.AddExperience(15); h.Tired(10); }
        else if (distance <= score_5) { score = 5; moneyFromSkill = 30; h.AddExperience(10); h.Tired(14); }
        else { score = 0; moneyFromSkill = 0; h.AddExperience(0); h.Tired(20); }

        isStopped = true;
        customerManager.FinishHaircut(score, moneyFromSkill);


        // tăng số lượng người đã cắt 
        if (h != null)
        {
            h.AddExperience(10);
        }

        this.enabled = false;
        this.gameObject.SetActive(false);
    }

    public void ResetBar()
    {
        isStopped = false;
        movingUp = true;

        if (bar != null)
        {
            currentY = -bar.rect.height / 2f;
            marker.anchoredPosition = new Vector2(0, currentY);
        }

        this.enabled = true; 
    }
}
