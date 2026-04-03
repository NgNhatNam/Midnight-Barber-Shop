using DPUtils.System.DateTime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Map Navigation")]
    public LayerMask mapBoundLayer;

    [Header("Schedule Settings")]
    public List<NPCSchedule> schedules;
    private NPCSchedule currentSchedule;
    private int currentWaypointIndex = 0;

    [Header("Settings")]
    public float updateRate = 0.2f;
    private NavMeshAgent agent;
    private TimeManager timeManager;
    private Health playerHealth;

    [Header("Movement Speeds")]
    private bool isExitDelay = false; // Trạng thái chờ 1s sau khi hội thoại
    public float moveSpeed = 2f;

    // Animation
    private Animator animator;
    private string currentAnimation;
    // Animation States 
    const string NPC_IDLE = "Idle";
    const string NPC_WALK = "Walking";
    const string NPC_Sit = "Sit";

    [Header("Interaction Settings")]
    public float waitTime = 2f; // Thời gian dừng lại khi đụng Player
    private bool isWaiting = false;

    [Header("Dialogue NPC")]
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;
    private Quest activeQuestInConversation; // Biến tạm để lưu quest của cuộc đối thoại này

    // Bug ==========================================
    private float lastDialogueEndTime = -999f;
    public float interactionCooldown = 1.5f;
    // Biến static giúp tất cả NPC biết được có ai đó đang nói chuyện không
    private static bool IsAnyNPCSpeaking = false;
    private bool isThisNPCSpeaking = false; // Trạng thái riêng của NPC này
    private Coroutine typingCoroutine;
    //=============================================

    private List<DialogueLine> activeLines;
    private int dialogueIndex;
    private bool isTyping;

    // Quest
    private enum QuestState { NotStarted, InProgress, Completed}
    private QuestState questState = QuestState.NotStarted;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        timeManager = FindAnyObjectByType<TimeManager>();
        playerHealth = FindAnyObjectByType<Health>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = moveSpeed;
        agent.acceleration = 50f;   
    }

    void Update()
    {
        // Thêm isExitDelay vào điều kiện dừng
        if (isThisNPCSpeaking || isWaiting || isExitDelay)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector2.zero;
            }
            ChangeAnimationState(NPC_IDLE);
            return;
        }

        //HandleFastTravel();
        CheckSchedule();
        MoveToWaypoint();
        FlipSprite();
    }

    #region Movement & Schedule

    private void FacePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float direction = player.transform.position.x - transform.position.x;

        if (direction > 0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else if (direction < -0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
    }

    void CheckSchedule()
    {
        int currentHour = timeManager.GetCurrentDateTime().Hour;
        NPCSchedule foundSchedule = null;

        foreach (var schedule in schedules)
        {
            if (currentHour >= schedule.hour)
            {
                foundSchedule = schedule;
            }
        }

        // Nếu lịch trình thay đổi, reset chỉ số Waypoint
        if (foundSchedule != currentSchedule)
        {
            currentSchedule = foundSchedule;
            currentWaypointIndex = 0;
        }
    }

    void MoveToWaypoint()
    {
        if (currentSchedule == null || currentSchedule.waypoints == null || currentSchedule.waypoints.Count == 0) return;

        // 1. Kiểm tra va chạm biên (Giữ nguyên để Teleport)
        Collider2D hit = Physics2D.OverlapPoint(transform.position, mapBoundLayer);
        if (hit == null)
        {
            TeleportToWaypoint(currentSchedule.waypoints[currentWaypointIndex].position);
            return;
        }

        // 2. Lấy vị trí đích hiện tại
        Vector3 targetPos = currentSchedule.waypoints[currentWaypointIndex].position;
        float distanceToDestination = Vector2.Distance(transform.position, targetPos);

        if (distanceToDestination > agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPos);
            ChangeAnimationState(NPC_WALK);
        }
        else
        {
            // --- LOGIC SỬA TẠI ĐÂY ---
            // Kiểm tra xem đã đến Element cuối cùng của danh sách chưa
            if (currentWaypointIndex < currentSchedule.waypoints.Count - 1)
            {
                // Nếu chưa phải cuối danh sách, tăng index để đi tiếp điểm tiếp theo
                currentWaypointIndex++;
            }
            else
            {
                // ĐÃ ĐẾN ĐÍCH (Element cuối cùng): Dừng Agent hoàn toàn
                agent.isStopped = true;
                agent.velocity = Vector2.zero;

                // Chuyển sang Action Animation (ngồi, đứng chơi...) nếu có, không thì Idle
                string actionAnim = string.IsNullOrEmpty(currentSchedule.actionAnim) ? NPC_IDLE : currentSchedule.actionAnim;
                ChangeAnimationState(actionAnim);
            }
        }
    }

    void TeleportToWaypoint(Vector3 targetPos)
    {
        if (agent == null) return;

        agent.enabled = false;
        transform.position = targetPos;
        agent.enabled = true;

        if (agent.isActiveAndEnabled)
        {
            agent.Warp(targetPos);
            agent.ResetPath();
        }

        Debug.Log("NPC đã ra khỏi biên và Teleport đến Waypoint đầu tiên của lịch trình mới.");
    }

    public void OnMapTeleported()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.ResetPath();

            // Kiểm tra xem lịch trình hiện tại có danh sách Waypoints không
            if (currentSchedule != null && currentSchedule.waypoints != null && currentSchedule.waypoints.Count > 0)
            {
                Vector3 targetPos = currentSchedule.waypoints[currentWaypointIndex].position;

                agent.SetDestination(targetPos);
                Debug.Log($"NPC đã tính lại đường đi tới Waypoint index {currentWaypointIndex} tại Map mới.");
            }
        }
    }

    #endregion

    #region Interaction & Dialogue

    public bool CanInteract()
    {
        return !IsAnyNPCSpeaking && (Time.time - lastDialogueEndTime > interactionCooldown);
    }

    public void Interact()
    {
        if (dialogueData == null) return;

        if (isThisNPCSpeaking)
        {
            NextLine();
            return;
        }

        // Nếu người khác đang nói hoặc chưa hết cooldown thì bỏ qua
        if (IsAnyNPCSpeaking || Time.time - lastDialogueEndTime < interactionCooldown) return;

        StartDialogue();
    }

    // Sửa
    void StartDialogue()
    {
        // Tìm quest đã hoàn thành
        Quest progressQuest = null;
        foreach (var group in dialogueData.conditionalGroups)
        {
            if (group.quest != null && QuestController.Instance.IsQuestActive(group.quest.questID)
                && QuestController.Instance.IsQuestCompleted(group.quest.questID))
            {
                progressQuest = group.quest;
                break; 
            }
        }

        // Nếu không có quest nào xong, thì mới tìm quest đang làm dở hoặc quest mới
        if (progressQuest == null)
        {
            activeLines = GetCurrentDialogueLines(); //lấy theo logic ưu tiên thông thường
        }
        else
        {
            // Nếu có quest xong
            activeQuestInConversation = progressQuest;
            activeLines = GetCurrentDialogueLines();
        }

        if (activeLines == null) { EndDialogue(); return; }

        SyncQuestState();

        // Trả thưởng nếu đã hoàn thành
        if (activeQuestInConversation != null)
        {
            string qID = activeQuestInConversation.questID;
            if (QuestController.Instance.IsQuestCompleted(qID) && !QuestController.Instance.IsQuestHandedIn(qID))
            {
                HandleQuestCompletion(activeQuestInConversation);
                QuestController.Instance.HandInQuest(qID);
                questState = QuestState.Completed;

                activeLines = GetCurrentDialogueLines();
            }
        }
        
        IsAnyNPCSpeaking = true; 
        isThisNPCSpeaking = true;


        dialogueIndex = GetStartingDialogueIndex();
        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;
        dialoguePanel.SetActive(true);

        FacePlayer();
        //Time.timeScale = 0f;
        DisplayCurrentLine();
    }

    // Thêm
    private int GetStartingDialogueIndex()
    {
        if (activeLines == null || activeLines.Count == 0) return 0;

        for (int i = 0; i < activeLines.Count; i++)
        {
            if (questState == QuestState.InProgress && activeLines[i].isInProgressLine)
            {
                return i;
            }
            if (questState == QuestState.Completed && activeLines[i].isCompletedLine)
            {
                return i;
            }
        }

        return 0;
    }

    private void SyncQuestState()
    {
        if (activeQuestInConversation == null || QuestController.Instance == null)
        {
            questState = QuestState.NotStarted;
            return;
        }

        string qID = activeQuestInConversation.questID;

        // Đã hoàn thành (đủ điều kiện) nhưng chưa trả hoặc đã trả rồi
        if (QuestController.Instance.IsQuestCompleted(qID) || QuestController.Instance.IsQuestHandedIn(qID))
        {
            questState = QuestState.Completed;
        }
        // Đang trong danh sách nhiệm vụ nhưng chưa xong
        else if (QuestController.Instance.IsQuestActive(qID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }


    private List<DialogueLine> GetCurrentDialogueLines()
    {
        if (dialogueData == null || timeManager == null) return null;

        DateTime now = timeManager.GetCurrentDateTime();
        int currentLevel = playerHealth != null ? playerHealth.currentLevel : 1;

        activeQuestInConversation = null;

        // --- BƯỚC 1: ƯU TIÊN QUEST ĐANG LÀM (IN PROGRESS) ---
        // Duyệt danh sách để xem Player có đang giữ Quest nào của NPC này không.
        // Nếu có, tập trung vào Quest đó, không quan tâm các Quest mới khác.
        foreach (var group in dialogueData.conditionalGroups)
        {
            if (group.quest != null && group.IsValid(now, currentLevel))
            {
                if (QuestController.Instance.IsQuestActive(group.quest.questID))
                {
                    activeQuestInConversation = group.quest;
                    return group.dialogueLines; // Trả về ngay lập tức
                }
            }
        }

        // --- BƯỚC 2: TÌM QUEST MỚI (CHỈ CHẠY KHI KHÔNG CÓ QUEST ĐANG LÀM) ---
        // Vòng lặp foreach sẽ tự động chạy từ trên xuống dưới (từ Element 0).
        // Quest nào thỏa mãn IsValid đầu tiên sẽ được chọn -> Đúng ý đồ ưu tiên Element trước.
        foreach (var group in dialogueData.conditionalGroups)
        {
            if (group.quest != null && group.IsValid(now, currentLevel))
            {
                string qID = group.quest.questID;
                // Điều kiện: Chưa nhận (Active) và chưa hoàn thành trả thưởng xong (HandedIn)
                if (!QuestController.Instance.IsQuestActive(qID) && !QuestController.Instance.IsQuestHandedIn(qID))
                {
                    activeQuestInConversation = group.quest;
                    return group.dialogueLines; // Lấy Quest đầu tiên tìm thấy và thoát hàm
                }
            }
        }

        // --- BƯỚC 3: HỘI THOẠI THÔNG THƯỜNG ---
        // Nếu không có Quest nào (cũ hay mới), NPC mới nói chuyện phiếm theo điều kiện.
        foreach (var group in dialogueData.conditionalGroups)
        {
            if (group.quest == null && group.IsValid(now, currentLevel))
            {
                return group.dialogueLines;
            }
        }

        // --- BƯỚC 4: DỰ PHÒNG (FALLBACK) ---
        // Nếu tất cả các điều kiện trên đều không khớp, lấy Element đầu tiên làm mặc định.
        if (dialogueData.conditionalGroups.Count > 0)
        {
            return dialogueData.conditionalGroups[0].dialogueLines;
        }

        return null;
    }   

    void DisplayCurrentLine()
    {
        ClearChoices(); // Xóa các nút cũ trước khi hiện câu mới

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine());
       
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in activeLines[dialogueIndex].text)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }
        isTyping = false;

        // Sau khi chạy chữ xong, kiểm tra xem câu này có Choice không
        CheckAndDisplayChoices();

        // Nếu KHÔNG có Choice và có AutoProgress thì mới tự qua câu
        if (choiceContainer.childCount == 0 && activeLines[dialogueIndex].autoProgress)
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoPorgressDelay);
            NextLine();
        }
    }

    //Sửa
    void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.SetText(activeLines[dialogueIndex].text);
            isTyping = false;
            CheckAndDisplayChoices();
            return;
        }

        if (choiceContainer.childCount > 0) return;

        dialogueIndex++;

        if (dialogueIndex < activeLines.Count)
        {
            if (questState == QuestState.InProgress && activeLines[dialogueIndex].isCompletedLine)
            {
                EndDialogue();
                return;
            }

            if (questState == QuestState.NotStarted && activeLines[dialogueIndex].isInProgressLine)
            {
                EndDialogue();
                return;
            }

            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    // Choice 
    private void CheckAndDisplayChoices()
    {
        // 1. Dọn dẹp các nút cũ
        ClearChoices();

        // 2. Lấy dữ liệu câu thoại hiện tại
        DialogueLine currentLine = activeLines[dialogueIndex];

        // 3. Nếu câu thoại này có chứa danh sách các lựa chọn
        if (currentLine.branchChoice != null && currentLine.branchChoice.Count > 0)
        {
            foreach (DialogueChoice choice in currentLine.branchChoice)
            {
                // Tạo nút bấm cho mỗi lựa chọn
                CreateChoiceButoon(choice.choices, () => OnChoiceClicked(choice));
            }
        }
    }

    private void OnChoiceClicked(DialogueChoice selectedChoice)
    {
        if (selectedChoice.giveQuest && activeQuestInConversation != null)
        {
            // Kiểm tra xem quest đã được nhận chưa để tránh nhận đè
            if (!QuestController.Instance.IsQuestActive(activeQuestInConversation.questID))
            {
                int duration = 24;
                if (dialogueData.conditionalGroups != null)
                {
                    foreach (var group in dialogueData.conditionalGroups)
                    {
                        if (group.quest == activeQuestInConversation)
                        {
                            duration = group.questDurationHours;
                            break;
                        }
                    }
                }

                var time = timeManager.GetCurrentDateTime();
                QuestController.Instance.AcceptQuest(activeQuestInConversation, (int)time.TotalNumHours, duration);

                // Cập nhật lại trạng thái Quest 
                SyncQuestState();
            }
        }

        // Chuyển sang các dòng thoại tiếp theo của Choice
        if (selectedChoice.nextLines != null && selectedChoice.nextLines.Count > 0)
        {
            activeLines = selectedChoice.nextLines;
            dialogueIndex = 0;
            ClearChoices();
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {

        //StopAllCoroutines();
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        isThisNPCSpeaking = false;
        IsAnyNPCSpeaking = false;
        isWaiting = false;
        isExitDelay = false;

        //dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        //Time.timeScale = 1f;
        activeLines = null;

        lastDialogueEndTime = Time.time;

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            // Ép NPC quay lại hành trình cũ ngay lập tức
            if (currentSchedule != null && currentSchedule.waypoints.Count > 0)
            {
                Vector3 targetPos = currentSchedule.waypoints[currentWaypointIndex].position;
                agent.SetDestination(targetPos);
            }
        }

        
        // Bắt đầu quá trình chờ 1 giây trước khi cho phép di chuyển lại
        StartCoroutine(ReactivateMovementAfterDelay(1f));
    }

    void HandleQuestCompletion(Quest quest)
    {
        RewardsController.Instance.GiveQuestReward(quest);
        //QuestController.Instance.HandInQuest(quest.questID);
    }

    #endregion

    #region UI Helpers

    
    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
    }

    public GameObject CreateChoiceButoon(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
        return choiceButton;
    }

    #endregion

    #region Another
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.CompareTag("PlayerInteractRange") || collision.CompareTag("Player"))
        {
            if (!isWaiting && !isThisNPCSpeaking)
            {
                StartCoroutine(WaitAfterCollision());
            }
        }
    }

    IEnumerator WaitAfterCollision()
    {
        isWaiting = true;
        agent.isStopped = true;
        agent.velocity = Vector2.zero; // Triệt tiêu lực quán tính ngay lập tức

        // Chờ 1 đoạn thời gian
        yield return new WaitForSeconds(waitTime);

        isWaiting = false;
        agent.isStopped = false;
    }
   
    IEnumerator ReactivateMovementAfterDelay(float delay)
    {
        isExitDelay = true; // Bật trạng thái chờ

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector2.zero;
        }

        yield return new WaitForSeconds(delay); // Đợi đúng 1 giây

        isExitDelay = false; // Tắt trạng thái chờ để Update cho phép di chuyển lại

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;

            // Cập nhật lại đích đến dựa trên List Waypoints và Index hiện tại
            if (currentSchedule != null && currentSchedule.waypoints != null && currentSchedule.waypoints.Count > 0)
            {
                // Lấy vị trí waypoint mà NPC đang đi dở trước khi hội thoại
                Vector3 targetPos = currentSchedule.waypoints[currentWaypointIndex].position;
                agent.SetDestination(targetPos);
            }
        }
    }

    //---------------------------------------------------------------------------
    void FlipSprite()
    {
        if (Mathf.Abs(agent.velocity.x) < 0.1f) return;

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
    #endregion

    #region reset if UI Dialogue NPC disable
    void OnEnable()
    {
        // Đăng ký: Khi có ai đó báo hiệu UI tắt, tôi sẽ chạy hàm EndDialogue
        // Bạn có thể tạo một static event ở một script UI Manager nào đó
        DialogueEvents.OnDialogueUIClosed += ForceResetNPC;
    }

    void OnDisable()
    {
        DialogueEvents.OnDialogueUIClosed -= ForceResetNPC;
    }

    void ForceResetNPC()
    {
        if (isThisNPCSpeaking)
        {
            EndDialogue();
        }
    }
    #endregion
}
