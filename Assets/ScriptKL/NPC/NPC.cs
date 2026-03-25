using DPUtils.System.DateTime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{

    [Header("Schedule Settings")]
    public List<NPCSchedule> schedules;
    private NPCSchedule currentSchedule;

    [Header("Settings")]
    public float updateRate = 0.2f;
    private NavMeshAgent agent;
    private TimeManager timeManager;
    private Health playerHealth;

    [Header("Movement Speeds")]
    private bool isExitDelay = false; // Trạng thái chờ 1s sau khi hội thoại
    public float normalSpeed = 2f;
    public float fastSpeed = 50f;
    public LayerMask mapBoundLayer;

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

    private List<DialogueLine> activeLines;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

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
    }

    void Update()
    {
        // Thêm isExitDelay vào điều kiện dừng
        if (isDialogueActive || isWaiting || isExitDelay)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector2.zero;
            }
            ChangeAnimationState(NPC_IDLE);
            return;
        }

        HandleFastTravel();
        CheckSchedule();
        MoveToWaypoint();
        FlipSprite();
    }

    #region Movement & Schedule

    void CheckSchedule()
    {
        int currentHour = timeManager.GetCurrentDateTime().Hour;

        // Tìm lịch trình phù hợp nhất với giờ hiện tại
        foreach (var schedule in schedules)
        {
            if (currentHour >= schedule.hour)
            {
                currentSchedule = schedule;
            }
        }
    }

    void MoveToWaypoint()
    {
        if (currentSchedule == null || currentSchedule.waypoint == null) return;

        // Tính khoảng cách đến đích của lịch trình
        float distanceToDestination = Vector2.Distance(transform.position, currentSchedule.waypoint.position);

        //  Nếu còn xa đích hơn khoảng cách dừng 
        if (distanceToDestination > agent.stoppingDistance)
        {
            // Đang ở xa -> Phải đi
            agent.isStopped = false;
            agent.SetDestination(currentSchedule.waypoint.position);
            ChangeAnimationState(NPC_WALK);
        }
        else
        {
            // Đã đến rất gần đích -> Dừng lại và thực hiện Action
            agent.isStopped = true;
            agent.velocity = Vector2.zero;

            // Nếu không có anim hành động thì về Idle
            string actionAnim = string.IsNullOrEmpty(currentSchedule.actionAnim) ? NPC_IDLE : currentSchedule.actionAnim;
            ChangeAnimationState(actionAnim);
        }
    }

    void HandleFastTravel()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position, mapBoundLayer);

        if (hit != null)
        {
            // --- TRẠNG THÁI TRONG MAP (ĐI CHẬM) ---
            Debug.Log($"NPC đang ở TRONG: {hit.gameObject.name}");
            // Nếu trước đó NPC đang chạy nhanh (vừa bước vào map)
            if (agent.speed > normalSpeed)
            {
                // Ép vận tốc hiện tại về tốc độ bình thường ngay lập tức để không bị "trôi"
                agent.velocity = agent.velocity.normalized * normalSpeed;
            }

            agent.speed = normalSpeed;
            agent.acceleration = 50f; // Tăng gia tốc cao để nó "bám" đường tốt hơn, không bị trượt

            if (animator != null && !animator.enabled) animator.enabled = true;
        }
        else
        {
            Debug.Log("NPC đang ở NGOÀI MapBound");
            // --- TRẠNG THÁI NGOÀI MAP (CHẠY NHANH) ---
            agent.speed = fastSpeed;
            agent.acceleration = 100f;

            if (animator != null && animator.enabled) animator.enabled = false;
        }
    }


    #endregion

    #region Interaction & Dialogue

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            return;
        }

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            // Ép dừng Agent để tránh lỗi Mobile bị trôi
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector2.zero;
            }

            StartDialogue();
        }
    }

    void StartDialogue()
    {
        // 1. Lấy thoại trước để xác định activeQuestInConversation
        List<DialogueLine> rawLines = GetCurrentDialogueLines();
        if (rawLines == null) { EndDialogue(); return; }

        // 2. Sau khi đã có activeQuestInConversation, mới đồng bộ State
        SyncQuestState();

        // 3. Logic tự động trả nhiệm vụ nếu đã hoàn thành (Dùng biến tạm)
        if (activeQuestInConversation != null)
        {
            string qID = activeQuestInConversation.questID;
            if (QuestController.Instance.IsQuestActive(qID) && QuestController.Instance.IsQuestCompleted(qID))
            {
                HandleQuestCompletion(activeQuestInConversation);

                QuestController.Instance.HandInQuest(qID);
                questState = QuestState.Completed; // Cập nhật ngay để lọc câu thoại bên dưới
            }
        }

        // 4. Lọc câu thoại (Giữ nguyên logic lọc InProgress/Completed của bạn)
        activeLines = new List<DialogueLine>();
        foreach (var line in rawLines)
        {
            if (questState == QuestState.Completed && line.isCompletedLine) activeLines.Add(line);
            else if (questState == QuestState.InProgress && line.isInProgressLine) activeLines.Add(line);
            else if (questState == QuestState.NotStarted && !line.isInProgressLine && !line.isCompletedLine) activeLines.Add(line);
        }

        if (activeLines.Count == 0) activeLines = rawLines;

        // 5. Mở UI
        isDialogueActive = true;
        dialogueIndex = 0;
        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;
        dialoguePanel.SetActive(true);

        Time.timeScale = 0f;
        DisplayCurrentLine();
    }
   
    private void SyncQuestState()
    {
        /*/ Kiểm tra an toàn toàn diện trước khi chạy logic
        if (dialogueData == null || dialogueData.quest == null || QuestController.Instance == null)
        {
            questState = QuestState.NotStarted;
            return;
        }

        string questID = dialogueData.quest.questID;
        */

        // Kiểm tra activeQuestInConversation thay vì dialogueData.quest
        if (activeQuestInConversation == null || QuestController.Instance == null)
        {
            questState = QuestState.NotStarted;
            return;
        }

        string questID = activeQuestInConversation.questID;
        // 1. Kiểm tra đã trả chưa
        if (QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        // 2. Kiểm tra ĐÃ XONG (đủ đồ) nhưng CHƯA TRẢ
        else if (QuestController.Instance.IsQuestCompleted(questID))
        {
            questState = QuestState.Completed;
        }
        // 3. Kiểm tra ĐANG LÀM nhưng CHƯA XONG
        else if (QuestController.Instance.IsQuestActive(questID))
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

        if (dialogueData.conditionalGroups != null)
        {
            foreach (var group in dialogueData.conditionalGroups)
            {
                // 1. Kiểm tra điều kiện thời gian/level cơ bản
                if (group.IsValid(now, currentLevel))
                {
                    // 2. Nếu Group này không có Quest, nó là thoại ưu tiên bình thường -> Lấy luôn
                    if (group.quest == null) return group.dialogueLines;

                    // 3. Nếu Group có Quest, kiểm tra xem Quest này đã TRẢ (Handed In) chưa
                    bool isQuestAlreadyDone = QuestController.Instance.IsQuestHandedIn(group.quest.questID);

                    if (!isQuestAlreadyDone)
                    {
                        // Nếu chưa trả xong, đây chính là Group chúng ta cần (đang làm hoặc chưa nhận)
                        activeQuestInConversation = group.quest;
                        return group.dialogueLines;
                    }
                    // Nếu isQuestAlreadyDone = true, vòng lặp sẽ TIẾP TỤC sang Group tiếp theo trong List
                    Debug.Log($"Quest {group.quest.questID} đã xong, đang tìm Group tiếp theo...");
                }
            }
        }

        // 4. Nếu tất cả Conditional Groups đều đã xong hoặc không thỏa mãn, về mặc định
        if (dialogueData.defaultDialogueGroups != null && dialogueData.defaultDialogueGroups.Count > 0)
        {
            int randomIndex = Random.Range(0, dialogueData.defaultDialogueGroups.Count);
            return dialogueData.defaultDialogueGroups[randomIndex].dialogueLines;
        }

        return null;
    }
    
    void DisplayCurrentLine()
    {
        ClearChoices(); // Xóa các nút cũ trước khi hiện câu mới
        StopAllCoroutines();
        StartCoroutine(TypeLine());
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

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(activeLines[dialogueIndex].text);
            isTyping = false;
            CheckAndDisplayChoices(); // Hiện Choice ngay khi skip chữ
            return;
        }

        // Nếu đang có các nút lựa chọn trên màn hình, KHÔNG cho phép bấm Next tiếp
        if (choiceContainer.childCount > 0) return;

        dialogueIndex++;

        if (dialogueIndex < activeLines.Count)
        {
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
            int duration = 24; // Mặc định

            // Tìm duration từ group tương ứng với activeQuestInConversation
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
            // SỬA TẠI ĐÂY: Dùng activeQuestInConversation và ép kiểu chuẩn
            QuestController.Instance.AcceptQuest(activeQuestInConversation, (int)time.TotalNumHours, duration);

            SyncQuestState();
        }

        // Chuyển sang các dòng thoại tiếp theo trong Choice (nếu có)
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
    
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText("");
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f;

        if (agent != null)
        {
            agent.isStopped = false;
        }

        isWaiting = false;
        // Bắt đầu quá trình chờ 1 giây trước khi cho phép di chuyển lại
        //StartCoroutine(ReactivateMovementAfterDelay(1f));
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
            if (!isWaiting && !isDialogueActive)
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
            // Cập nhật lại đích đến ngay lập tức để NPC không bị khựng
            if (currentSchedule != null && currentSchedule.waypoint != null)
            {
                agent.SetDestination(currentSchedule.waypoint.position);
            }
        }
    }

    //---------------------------------------------------------------------------
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
    #endregion
}


/*  using DPUtils.System.DateTime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class NPC : MonoBehaviour, IInteractable
{

    [Header("Schedule Settings")]
    public List<NPCSchedule> schedules;
    private NPCSchedule currentSchedule;

    [Header("Settings")]
    public float updateRate = 0.2f;
    private NavMeshAgent agent;
    private TimeManager timeManager;
    private Health playerHealth;

    [Header("Movement Speeds")]
    private bool isExitDelay = false; // Trạng thái chờ 1s sau khi hội thoại
    public float normalSpeed = 2f;
    public float fastSpeed = 50f;
    public LayerMask mapBoundLayer;

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
    //public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    private List<DialogueLine> activeLines;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

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
    }

    void Update()
    {
        // Thêm isExitDelay vào điều kiện dừng
        if (isDialogueActive || isWaiting || isExitDelay)
        {
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector2.zero;
            }
            ChangeAnimationState(NPC_IDLE);
            return;
        }

        HandleFastTravel();
        CheckSchedule();
        MoveToWaypoint();
        FlipSprite();
    }

    #region Movement & Schedule

    void CheckSchedule()
    {
        int currentHour = timeManager.GetCurrentDateTime().Hour;

        // Tìm lịch trình phù hợp nhất với giờ hiện tại
        foreach (var schedule in schedules)
        {
            if (currentHour >= schedule.hour)
            {
                currentSchedule = schedule;
            }
        }
    }

    void MoveToWaypoint()
    {
        if (currentSchedule == null || currentSchedule.waypoint == null) return;

        // Tính khoảng cách đến đích của lịch trình
        float distanceToDestination = Vector2.Distance(transform.position, currentSchedule.waypoint.position);

        //  Nếu còn xa đích hơn khoảng cách dừng 
        if (distanceToDestination > agent.stoppingDistance)
        {
            // Đang ở xa -> Phải đi
            agent.isStopped = false;
            agent.SetDestination(currentSchedule.waypoint.position);
            ChangeAnimationState(NPC_WALK);
        }
        else
        {
            // Đã đến rất gần đích -> Dừng lại và thực hiện Action
            agent.isStopped = true;
            agent.velocity = Vector2.zero;

            // Nếu không có anim hành động thì về Idle
            string actionAnim = string.IsNullOrEmpty(currentSchedule.actionAnim) ? NPC_IDLE : currentSchedule.actionAnim;
            ChangeAnimationState(actionAnim);
        }
    }

    void HandleFastTravel()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position, mapBoundLayer);

        if (hit != null)
        {
            // --- TRẠNG THÁI TRONG MAP (ĐI CHẬM) ---
            Debug.Log($"NPC đang ở TRONG: {hit.gameObject.name}");
            // Nếu trước đó NPC đang chạy nhanh (vừa bước vào map)
            if (agent.speed > normalSpeed)
            {
                // Ép vận tốc hiện tại về tốc độ bình thường ngay lập tức để không bị "trôi"
                agent.velocity = agent.velocity.normalized * normalSpeed;
            }

            agent.speed = normalSpeed;
            agent.acceleration = 50f; // Tăng gia tốc cao để nó "bám" đường tốt hơn, không bị trượt

            if (animator != null && !animator.enabled) animator.enabled = true;
        }
        else
        {
            Debug.Log("NPC đang ở NGOÀI MapBound");
            // --- TRẠNG THÁI NGOÀI MAP (CHẠY NHANH) ---
            agent.speed = fastSpeed;
            agent.acceleration = 100f;

            if (animator != null && animator.enabled) animator.enabled = false;
        }
    }


    #endregion

    #region Interaction & Dialogue

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            return;
        }

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            // Ép dừng Agent để tránh lỗi Mobile bị trôi
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector2.zero;
            }

            StartDialogue();
        }
    }

    
void StartDialogue()
{
    // 1. Đồng bộ trạng thái ban đầu
    SyncQuestState();

    if (dialogueData != null && dialogueData.quest != null)
    {
        string qID = dialogueData.quest.questID;

        // Nếu đủ điều kiện trả đồ
        if (QuestController.Instance.IsQuestActive(qID) && QuestController.Instance.IsQuestCompleted(qID))
        {
            QuestController.Instance.HandInQuest(qID);
            // Sau khi trả, trạng thái chắc chắn là Completed
            questState = QuestState.Completed;
        }
    }

    // 2. Lấy danh sách câu thoại (Đảm bảo không bị Null)
    List<DialogueLine> rawLines = GetCurrentDialogueLines();
    if (rawLines == null) { EndDialogue(); return; }

    activeLines = new List<DialogueLine>();

    // 3. Lọc câu thoại dựa trên trạng thái MỚI NHẤT
    foreach (var line in rawLines)
    {
        if (questState == QuestState.Completed)
        {
            if (line.isCompletedLine) activeLines.Add(line);
        }
        else if (questState == QuestState.InProgress)
        {
            if (line.isInProgressLine) activeLines.Add(line);
        }
        else
        {
            if (!line.isInProgressLine && !line.isCompletedLine) activeLines.Add(line);
        }
    }

    // Nếu không có câu thoại đặc biệt nào, dùng toàn bộ câu thoại mặc định
    if (activeLines.Count == 0) activeLines = rawLines;

    // 4. Hiển thị
    isDialogueActive = true;
    dialogueIndex = 0;
    nameText.SetText(dialogueData.npcName);
    portraitImage.sprite = dialogueData.npcPortrait;
    dialoguePanel.SetActive(true);

    Time.timeScale = 0f;
    DisplayCurrentLine();
}

private void SyncQuestState()
{
    // Kiểm tra an toàn toàn diện trước khi chạy logic
    if (dialogueData == null || dialogueData.quest == null || QuestController.Instance == null)
    {
        questState = QuestState.NotStarted;
        return;
    }

    string questID = dialogueData.quest.questID;

    // 1. Kiểm tra đã trả chưa
    if (QuestController.Instance.IsQuestHandedIn(questID))
    {
        questState = QuestState.Completed;
    }
    // 2. Kiểm tra ĐÃ XONG (đủ đồ) nhưng CHƯA TRẢ
    else if (QuestController.Instance.IsQuestCompleted(questID))
    {
        questState = QuestState.Completed;
    }
    // 3. Kiểm tra ĐANG LÀM nhưng CHƯA XONG
    else if (QuestController.Instance.IsQuestActive(questID))
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
    int currentHour = timeManager.GetCurrentDateTime().Hour;

    // Ưu tiên kiểm tra thoại theo giờ trước
    foreach (var timeDialogue in dialogueData.timeBasedDialogues)
    {
        if (currentHour >= timeDialogue.startHour && currentHour < timeDialogue.endHour)
            return timeDialogue.dialogueLines;
    }

    // Nếu không khớp giờ, chọn ngẫu nhiên 1 nhóm trong Default Dialogue Groups
    if (dialogueData.defaultDialogueGroups != null && dialogueData.defaultDialogueGroups.Count > 0)
    {
        // Chọn ngẫu nhiên một nhóm thoại mặc định để tăng tính đa dạng
        int randomIndex = Random.Range(0, dialogueData.defaultDialogueGroups.Count);
        return dialogueData.defaultDialogueGroups[randomIndex].dialogueLines;
    }

    return null;
}

void DisplayCurrentLine()
{
    ClearChoices(); // Xóa các nút cũ trước khi hiện câu mới
    StopAllCoroutines();
    StartCoroutine(TypeLine());
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

void NextLine()
{
    if (isTyping)
    {
        StopAllCoroutines();
        dialogueText.SetText(activeLines[dialogueIndex].text);
        isTyping = false;
        CheckAndDisplayChoices(); // Hiện Choice ngay khi skip chữ
        return;
    }

    // Nếu đang có các nút lựa chọn trên màn hình, KHÔNG cho phép bấm Next tiếp
    if (choiceContainer.childCount > 0) return;

    dialogueIndex++;

    if (dialogueIndex < activeLines.Count)
    {
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
    // 1. Xử lý Give Quest (Dùng object Quest thay vì string ID)
    if (selectedChoice.giveQuest && dialogueData.quest != null)
    {
        // Truyền cả object Quest vào đây
        QuestController.Instance.AcceptQuest(dialogueData.quest);
        SyncQuestState();
    }

    // 2. Chuyển sang mạch thoại tiếp theo (Branching)
    if (selectedChoice.nextLines != null && selectedChoice.nextLines.Count > 0)
    {
        // Thay đổi danh sách câu thoại đang chạy thành danh sách mới từ Choice
        activeLines = selectedChoice.nextLines;
        dialogueIndex = 0; // Reset về câu đầu tiên của nhánh mới

        ClearChoices(); // Xóa các nút sau khi đã chọn
        DisplayCurrentLine(); // Bắt đầu hiển thị mạch mới
    }
    else
    {
        // Nếu không có câu thoại tiếp theo, kết thúc hội thoại
        EndDialogue();
    }
}

public void EndDialogue()
{
  
    StopAllCoroutines();
    isDialogueActive = false;
    dialogueText.SetText("");
    dialoguePanel.SetActive(false);
    Time.timeScale = 1f;

    if (agent != null)
    {
        agent.isStopped = false;
    }

    isWaiting = false;
    // Bắt đầu quá trình chờ 1 giây trước khi cho phép di chuyển lại
    //StartCoroutine(ReactivateMovementAfterDelay(1f));
}

void HandleQuestCompletion(Quest quest)
{
    QuestController.Instance.HandInQuest(quest.questID);
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
        if (!isWaiting && !isDialogueActive)
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
        // Cập nhật lại đích đến ngay lập tức để NPC không bị khựng
        if (currentSchedule != null && currentSchedule.waypoint != null)
        {
            agent.SetDestination(currentSchedule.waypoint.position);
        }
    }
}

//---------------------------------------------------------------------------
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
    #endregion
}
 */