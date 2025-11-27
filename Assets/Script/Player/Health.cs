using DPUtils.System.DateTime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Health : MonoBehaviour
{

    [Header("Player UI")]
    public GameObject dieUI;
    public TMP_Text dayPlayedText;
    public TMP_Text moneyTextOnDie;

    [SerializeField] private int maxHP = 100;
    private int hp;

    [SerializeField] private int maxMN = 100;
    private int mn;

    [SerializeField] private int maxStress = 100;
    private int stress;

    [SerializeField] private int gold = 0;

    private AudioController audioController;

    public int MaxHP => maxHP; 
    public int MaxMN => maxMN;
    public int MaxStress => maxStress;  
    public int Gold => gold;

    public int Money
    {
        get => gold; 
        private set
        {
            var isDecrease = value < gold;
            gold = Mathf.Max(0, value);
            if (isDecrease)
            {
                audioController.PlaySFX(audioController.moneyPayOut, false);
                GoldDecreased?.Invoke(gold);
            }
            else 
            {
                audioController.PlaySFX(audioController.money, false);
                GoldIncreased?.Invoke(gold);
            }
        }
    }
    public int Stress
    {
        get => stress;
        private set
        {
            var isIncrease = value > stress;
            stress = Mathf.Clamp(value, 0, maxStress);

            if (isIncrease)
            {
                StressIncreased?.Invoke(stress);
            }
            else
            {
                StressDecreased?.Invoke(stress);
            }

            if (stress >= maxStress)
            {
                StressOverload?.Invoke(stress);
            }
        }
    }

    public int HP
    {
        get => hp;
        private set
        {
            /*
            if (SaveController.IsLoadingGame)
            {
                hp = Mathf.Clamp(value, 0, maxHP);
                Healed?.Invoke(hp);
                return;
            }*/

            var isDamage = value < hp;
            hp = Mathf.Clamp(value, 0, maxHP);
            if (isDamage) 
            {
                Damaged?.Invoke(hp);
            }
            else
            {
                Healed?.Invoke(hp);
            }

            if (hp <= 0)
            {
                audioController.PlaySFX(audioController.playerDie, false);

                Time.timeScale = 0f;

                Died?.Invoke(hp);

                moneyTextOnDie.text = $"Tiền hiện có: {gold}";

                var timeManager = FindAnyObjectByType<TimeManager>();
                
                int daysPlayed = timeManager.GetCurrentDateTime().TotalNumDays;
                dayPlayedText.text = $"Số ngày đã sống: {daysPlayed}";

                dieUI.SetActive(true);
            }

        }
    }

    public int MN
    {
        get => mn;
        private set
        {
            var isTired = value < mn;
            mn = Mathf.Clamp(value, 0, maxMN);
            if (isTired)
            {
                TiredMN?.Invoke(mn);
            }
            else
            {
                HealedMana?.Invoke(mn);
            }

            if (mn <= 0)
            {
                
                ManaOut?.Invoke(mn);
                DecreaseStress(50);
            }

        }
    }


    // === EVENTS ===
    [Header("Health Events")]
    public UnityEvent<int> Healed;
    public UnityEvent<int> Damaged;
    public UnityEvent<int> Died;

    [Header("Mana Events")]
    public UnityEvent<int> HealedMana;
    public UnityEvent<int> TiredMN;
    public UnityEvent<int> ManaOut;

    [Header("Gold Events")]
    public UnityEvent<int> GoldIncreased;
    public UnityEvent<int> GoldDecreased;

    [Header("Stress Events")]
    public UnityEvent<int> StressIncreased;
    public UnityEvent<int> StressDecreased;
    public UnityEvent<int> StressOverload;

    private void Awake()
    {
        audioController = FindAnyObjectByType<AudioController>();
        hp  = maxHP;
        mn = maxMN;
        stress = maxStress;
    }
    
    
    //====Health=====
    public void Damage(int amount) => HP -= amount;  
    public void Heal(int amount) => HP += amount;
    public void HealFull() => HP = maxHP;
    public void Kill() => HP = 0;
    public void Adjust(int value) => HP = value;
    
    //====Mana=====
    public void Tired(int amount) => MN -= amount;
    public void HealMN(int amount) => MN += amount;
    public void HealFullMN() => MN = maxMN;
    public void KillMN() => MN = 0;
    public void AdjustMN(int value) => MN = value;

    // ==== Gold =====
    public void AddGold(int amount) => Money += amount;
    public void SpendGold(int amount) => Money -= amount;
    public void SetGold(int value) => Money = value;

    // ==== Stress =====
    public void IncreaseStress(int amount) => Stress += amount;
    public void DecreaseStress(int amount) => Stress -= amount;
    public void SetStress(int value) => Stress = value;
    public void ResetStress() => Stress = 100;

}
