using DPUtils.System.DateTime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Boss Die UI")]
    public GameObject dieUI;
    public TMP_Text dayPlayedText;
    public TMP_Text moneyTextOnDie;

    private Health health;
    private AudioController audioController;


    [SerializeField] private int maxHP = 500;


    private int hp;
    public int MaxHP => maxHP;

    private bool isDead = false;
    public int HP
    {
        get => hp;
        private set
        {
           
            var isDamage = value < hp;
            hp = Mathf.Clamp(value, 0, maxHP);
            if (isDamage)
            {
                audioController.PlaySFX(audioController.bossHit, false);
                Damaged?.Invoke(hp);
            }
            else
            {
                Healed?.Invoke(hp);
            }

            if (hp <= 0 && !isDead)
            {
                audioController.PlaySFX(audioController.bossLose, false);

                isDead = true;
                Time.timeScale = 0f;
                Died?.Invoke(hp);
                moneyTextOnDie.text = $"Tiền hiện có: {health.Gold}";

                var timeManager = FindAnyObjectByType<TimeManager>();

                int daysPlayed = timeManager.GetCurrentDateTime().TotalNumDays;

                dayPlayedText.text = $"Số ngày đã sống: {daysPlayed}";

                dieUI.SetActive(true);
            }
        }
    }

 
    // === EVENTS ===
    [Header("Health Events")]
    public UnityEvent<int> Healed;
    public UnityEvent<int> Damaged;
    public UnityEvent<int> Died;


    private void Awake()
    {
        audioController = FindAnyObjectByType<AudioController>();
        health = FindAnyObjectByType<Health>();
        hp = maxHP;
    }

    public void ResetBoss()
    {
        isDead = false;
        dieUI.SetActive(false);
        EnemyHeal(500);
    }

    //====Health=====
    public void EnemyDamage(int amount) => HP -= amount;
    public void EnemyHeal(int amount) => HP += amount;
    public void EnemyHealFull() => HP = maxHP;
    public void EnemyKill() => HP = 0;
    public void EnemyAdjust(int value) => HP = value;



}
