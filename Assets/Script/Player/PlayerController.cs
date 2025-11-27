using System.Collections;                         
using System.Collections.Generic;                  
using UnityEngine;                                 
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 6f;

    //private MapTransation mapTransation;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb2D;
    private PlayerCombats playerCombats;
    private AudioController audioController;

    private string currentAnimation;
    private bool isRunningPressed;
    private bool isHit = false;

    [SerializeField] private float attackDelay = 0.2f;

    // Animation States 
    const string PLAYER_IDLE = "Idle";
    const string PLAYER_WALK = "Walk";
    const string PLAYER_RUN = "Running";
    const string PLAYER_SITTING = "Sit";
    const string PLAYER_ATTACK = "Attack";
    const string PLAYER_HIT = "Hit";
    
    void Start()
    {
        audioController = FindAnyObjectByType<AudioController>();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCombats = GetComponent<PlayerCombats>();


        //mapTransation = FindAnyObjectByType<MapTransation>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && !isHit && playerCombats.enabled == true 
            || Input.GetKeyDown(KeyCode.Mouse1) && !isHit && playerCombats.enabled == true)
        {
            playerCombats?.Shoot(spriteRenderer.flipX); // Gọi qua combat
        }
  

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            isRunningPressed = true;
        } 
        if (Keyboard.current.leftShiftKey.wasReleasedThisFrame)
        {
            isRunningPressed = false;
        }
                                   
    }

    private void FixedUpdate()
    {

        if (isHit || (playerCombats != null && playerCombats.IsAttacking))
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }


        float currentSpeed = isRunningPressed ? runSpeed : walkSpeed;
        rb2D.linearVelocity = moveInput * currentSpeed;


        if (moveInput != Vector2.zero)
        {
            ChangeAnimationState(isRunningPressed ? PLAYER_RUN : PLAYER_WALK);
        }
        else
        {
            ChangeAnimationState(PLAYER_IDLE);
        }
    }

    public void Flip()
    {

        if (moveInput.x >= 0.01f) // đi sang phải
        {
            spriteRenderer.flipX = true;
        }
        else if (moveInput.x <= -0.01f) // đi sang trái
        {
            spriteRenderer.flipX = false;
        }

    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Flip();
    }

    public void Hit()
    {
        if (isHit) return; // tránh lặp lại hit nhiều lần

        isHit = true;
        audioController.PlaySFX(audioController.playerHit,false);

        rb2D.linearVelocity = Vector2.zero;
        ChangeAnimationState(PLAYER_HIT);
        StartCoroutine(HitRecovery());
    }

    private IEnumerator HitRecovery()
    {
        rb2D.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);

        isHit = false;
    }

  
    public void PlayAttackAnimation()
    {
        ChangeAnimationState(PLAYER_ATTACK);
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;
        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }

}
    /*
    public void Shoot()
    {
        GameObject bullet = bulletPool.GetObject();
        bullet.transform.position = firePoint.position;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = (spriteRenderer.flipX ? Vector2.right : Vector2.left) * bulletSpeed;
        // Bỏ qua va chạm giữa đạn và trigger map
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (bulletCol != null)
        {
            var mapTriggers = FindObjectsByType<MapTransation>(FindObjectsSortMode.None);
            foreach (var trigger in mapTriggers)
            {
                var mapCollider = trigger.GetComponent<Collider2D>();
                if (mapCollider != null)
                {
                    Physics2D.IgnoreCollision(bulletCol, mapCollider);
                }
            }
        }

        // Có thể phát animation tấn công
        ChangeAnimationState(PLAYER_ATTACK);
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }
*/
