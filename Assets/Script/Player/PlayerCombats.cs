using System.Collections;
using UnityEngine;

public class PlayerCombats : MonoBehaviour
{
    [SerializeField] public Transform firePoint;
    [SerializeField] public float bulletSpeed = 2f;
    [SerializeField] private float attackDelay = 0.2f;
    public GameObject shootUI;

    private Pool bulletPool;
    private bool isAttacking;
    public bool IsAttacking => isAttacking;

    private Animator animator;
    private PlayerController playerController;
    private AudioController audioController;

    const string PLAYER_ATTACK = "Attack";

    private void Start()
    {
        audioController = FindAnyObjectByType<AudioController>();
        bulletPool = GetComponent<Pool>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();  
    }

    public void Shoot(bool facingRight)
    {

        audioController.PlaySFX(audioController.playerShoot);
        if (isAttacking || bulletPool == null || firePoint == null) return;

        // Gọi animation bắn qua PlayerController
        playerController?.PlayAttackAnimation();

        // Lấy đạn từ Pool
        GameObject bullet = bulletPool.GetObject("Bullet");
        bullet.transform.position = firePoint.position;

        // Thiết lập vận tốc đạn
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = (facingRight ? Vector2.right : Vector2.left) * bulletSpeed;
        

        // Bỏ qua va chạm giữa đạn và các trigger map
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol != null)
        {
            var mapTriggers = FindObjectsByType<MapTransation>(FindObjectsSortMode.None);
            foreach (var trigger in mapTriggers)
            {
                var mapCollider = trigger.GetComponent<Collider2D>();
                if (mapCollider != null)
                    Physics2D.IgnoreCollision(bulletCol, mapCollider);
            }
        }

        // Bắt đầu hồi chiêu tấn công
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }

}
