using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    private Animator animator;
    private string currentAnimation;

    // Animation States 
    const string BOSS_IDLE = "B_Idle";
    const string BOSS_DIE = "B_Die";
    const string BOSS_ATTACK1 = "B_Attack1";
    const string BOSS_ATTACK2 = "B_Attack2";
    const string BOSS_ATTACK3 = "B_Attack3";
    const string BOSS_HIT = "B_Hit";

    [Header("Attack Points")]
    public Transform[] firePoints;       // Nhiều điểm bắn đạn

    [Header("Attack Settings")]
    public float attackCooldown = 2f;    // Thời gian nghỉ giữa các đợt tấn công
    private Pool pool;                   // Pool quản lý đạn

    [Header("Map Boundaries")]
    public BoxCollider2D mapBound;   // gán BoxCollider2D của map vào
    private PlayerCombats playerCombats;
    private CinemachineConfiner2D confiner;
    private AudioController audioController;

    private Health playerHealth;
    private bool isCanAttack = true;
    private bool isDie = false;
    private bool isPlayingBossMusic = false;

    void Start()
    {
        playerHealth = FindFirstObjectByType<Health>();
        playerCombats = FindAnyObjectByType<PlayerCombats>();
        confiner = FindAnyObjectByType<CinemachineConfiner2D>();

        pool = GetComponent<Pool>();
        animator = GetComponent<Animator>();
        audioController = FindAnyObjectByType<AudioController>();

    }

    void Update()
    {
        if (isDie) return;
        if (playerCombats == null) return;

        // Player chưa "vào map" → boss đứng yên
        if (!playerCombats.enabled) return;

        // Trong cooldown → không tấn công
        if (!isCanAttack) return;

        // Random attack
        StartCoroutine(PerformAttack(Random.Range(1, 4)));


    }

    IEnumerator PerformAttack(int type)
    {
        isCanAttack = false;

        switch (type)
        {
            case 1:
                yield return Attack_SpreadBullets();
                break;
            case 2:
                yield return Attack_MultiPointBurst();
                break;
            case 3:
                yield return Attack_AllDirections();
                break;
        }

        yield return new WaitForSeconds(attackCooldown);
        isCanAttack = true;
    }

    // --- Attack 1: Bắn 5 viên toả ra từ 1 điểm ---
    IEnumerator Attack_SpreadBullets()
    {
        animator.Play(BOSS_ATTACK2);

        Transform firePoint = firePoints[Random.Range(0, firePoints.Length)];
        int bulletCount = 5;
        float spreadAngle = 30f;

        for (int i = 0; i < bulletCount; i++)
        {
            GameObject bullet = pool.GetObject("Boss_Bullet");
            bullet.transform.position = firePoint.position;

            float angle = -spreadAngle * 0.5f + spreadAngle * (i / (float)(bulletCount - 1));
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        yield return new WaitForSeconds(0.5f);
    }

    // --- Attack 2: Bắn đồng loạt từ tất cả firePoints ---
    IEnumerator Attack_MultiPointBurst()
    {
        animator.Play(BOSS_ATTACK2);

        foreach (Transform point in firePoints)
        {
            GameObject bullet = pool.GetObject("Boss_Bullet");
            bullet.transform.position = point.position;
            bullet.transform.rotation = point.rotation;
        }

        yield return new WaitForSeconds(0.5f);
    }

    // --- Attack 3: Bắn vòng tròn từ mỗi firePoint ---
    IEnumerator Attack_AllDirections()
    {
        animator.Play(BOSS_ATTACK2);

        int bulletCount = 12;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * (360f / bulletCount);

            foreach (Transform point in firePoints)
            {
                GameObject bullet = pool.GetObject("Boss_Bullet");
                bullet.transform.position = point.position;
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        yield return new WaitForSeconds(1f);
    }

    // ========== HIT & DIE ==========
    public void Bosshit()
    {
        ChangeAnimationState(BOSS_HIT);
        StartCoroutine(ReturnIdle());
    }

    IEnumerator ReturnIdle()
    {
        yield return new WaitForSeconds(0.2f);
        ChangeAnimationState(BOSS_IDLE);
    }

    public void BossDie()
    {
        isDie = true;
        isCanAttack = false;
        ChangeAnimationState(BOSS_DIE);
    }

    // ========== RESET BOSS ==========
    public void ResetEnemy()
    {
        isDie = false;

        StopAllCoroutines();

        // Tắt tấn công ngay
        isCanAttack = false;

        // Player chưa vào map → boss chưa đánh
        if (playerCombats != null)
            playerCombats.enabled = false;

        ChangeAnimationState(BOSS_IDLE);

        // Boss chờ 2s rồi mới cho phép attack (khi player vào map)
        StartCoroutine(EnableAttackWhenCombatOn());
    }

    IEnumerator EnableAttackWhenCombatOn()
    {
        yield return new WaitForSeconds(2f);

        while (!playerCombats.enabled)
            yield return null;

        isCanAttack = true;
    }

    // ========== MUSIC BOSS FIGHT ==========
    void PlayBossMusic()
    {
        if (audioController == null) return;
        if (isPlayingBossMusic) return;   // Không bị bật nhiều lần

        audioController.PlayMusic(audioController.bossFightMusic, true);
        isPlayingBossMusic = true;
    }

    void StopBossMusic()
    {
        if (audioController == null) return;
        if (!isPlayingBossMusic) return;

        audioController.StopMusic();
        isPlayingBossMusic = false;
    }

    // ========== SAVE GAME ==========
    public void saveGame()
    {
        Time.timeScale = 1f;

        playerCombats.enabled = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3(-40f, 2f, 0f);

        if (confiner != null)
            confiner.BoundingShape2D = GameObject.Find("BarBerHouse_City").GetComponent<BoxCollider2D>();

        playerHealth.HealFull();
        playerHealth.HealFullMN();
        playerHealth.ResetStress();
        playerHealth.AddGold(20000);



        FindAnyObjectByType<SaveController>().SaveGame();
    }


    // ========== ANIMATION ==========
    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;

        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
