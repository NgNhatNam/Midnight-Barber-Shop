using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class Customer : MonoBehaviour, IInteractable
{
    private NavMeshAgent agent;
    private bool isSeated = false;
    private bool isBeingServiced = false;

    public Slider patienceSlider;
    public float maxWaitTime = 40f;
    private float currentPatience;

    [HideInInspector] public CustomerData currentData;
    [HideInInspector] public Transform mySeat;

    // Animation
    private Animator animator;
    private string currentAnimation;
    // Animation States 
    const string NPC_IDLE = "Idle";
    const string NPC_WALK = "Walking";
    const string NPC_Sit = "Sit";

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public void Init(CustomerData data, Transform seat)
    {
        currentData = data;
        mySeat = seat;
        currentPatience = maxWaitTime;

        if (patienceSlider)
        {
            patienceSlider.maxValue = maxWaitTime;
            patienceSlider.gameObject.SetActive(false);
        }

        agent.SetDestination(seat.position);
        StartCoroutine(WalkToSeat());
    }

    IEnumerator WalkToSeat()
    {
        // Bắt đầu đi bộ
        agent.isStopped = false;
        ChangeAnimationState(NPC_WALK);

        while (Vector2.Distance(transform.position, mySeat.position) > 0.2f)
        {
            FlipSprite(); // Quay mặt theo hướng di chuyển
            yield return null;
        }

        // Đã tới ghế
        isSeated = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Chuyển sang animation ngồi và xoay mặt theo hướng của ghế
        transform.rotation = mySeat.rotation;
        ChangeAnimationState(NPC_Sit);

        if (patienceSlider)
        {
            patienceSlider.value = maxWaitTime;
            patienceSlider.gameObject.SetActive(true);
        }
    }

    // --- INTERFACE IINTERACTABLE ---

    // Hàm này được gọi khi Player nhấn phím tương tác (E hoặc phím bạn đã cài)
    public void Interact()
    {
        if (CanInteract())
        {
            // Gọi sang CustomerManager để bắt đầu cắt tóc
            CustomerManager manager = FindAnyObjectByType<CustomerManager>();
            if (manager != null)
            {
                manager.ShowCustomer(this);
            }
        }
    }

    // Điều kiện để hiện icon tương tác và cho phép nhấn phím
    public bool CanInteract()
    {
        // Chỉ cho phép tương tác khi khách đã ngồi vào ghế và chưa được phục vụ
        return isSeated && !isBeingServiced;
    }

    // ------------------------------------------

    public void StartBeingServiced()
    {
        isBeingServiced = true;
        if (patienceSlider) patienceSlider.gameObject.SetActive(false);
    }

    public void FinishAndLeave()
    {
        // Chặn mọi Coroutine cũ trước khi đi về
        StopAllCoroutines();
        StartCoroutine(LeaveRoutine());
    }

    IEnumerator LeaveRoutine()
    {
        isBeingServiced = true;
        isSeated = false;

        // Đứng dậy và đi bộ
        agent.isStopped = false;
        ChangeAnimationState(NPC_WALK);

        CustomerManager manager = FindAnyObjectByType<CustomerManager>();
        if (manager != null && manager.entrancePoint != null)
        {
            agent.SetDestination(manager.entrancePoint.position);

            while (Vector2.Distance(transform.position, manager.entrancePoint.position) > 0.5f)
            {
                FlipSprite(); // Quay mặt theo hướng di chuyển
                yield return null;
            }
        }

        // Chuyển về Idle một nhịp trước khi biến mất (tùy chọn)
        ChangeAnimationState(NPC_IDLE);
        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }

    void LeaveEarly()
    {
        CustomerManager manager = FindAnyObjectByType<CustomerManager>();
        if (manager != null) manager.OnCustomerLeave(mySeat);

        FinishAndLeave();
    }

    void Update()
    {
        if (isSeated && !isBeingServiced)
        {
            currentPatience -= Time.deltaTime;
            if (patienceSlider) patienceSlider.value = currentPatience;
            if (currentPatience <= 0) LeaveEarly();
        }
    }
    //__________ANIMATION_________________________________________________________
    void FlipSprite()
    {
        if (agent.velocity.x < 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else if (agent.velocity.x > -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;
        animator.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}

/*using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class Customer : MonoBehaviour
{

    [Header("References")]
    private Image customerImage;
    private TMP_Text dialogueText;

    [Header("Settings")]
    public float maxWaitTime = 40f;


    [HideInInspector] public CustomerData data;
    
    public void Init(CustomerData newData)
    {
        data = newData;

        // Random sprite và thoại
        customerImage.sprite = data.sprites[Random.Range(0, data.sprites.Count)];
        dialogueText.text = data.dialogues[Random.Range(0, data.dialogues.Count)];

    }
   
   
}*/
