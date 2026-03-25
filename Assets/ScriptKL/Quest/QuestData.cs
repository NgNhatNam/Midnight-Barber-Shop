using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;                                // Tên Quest
    [TextArea(3, 100)] public string description;           // Nhiệm vụ

    [Header("--- ĐIỀU KIỆN XUẤT HIỆN ---")]
    public bool useTimeLimit;                               // Bật tắt thời gian kích hoạt nv
    public int startHour, startDay, startSeason;             // Season: 0-Xuân, 1-Hạ...

    [Header("--- ĐIỀU KIỆN HẾT HẠN (Biến mất) ---")]
    public bool useExpiry;                                  // bật tắt thời gian kích hoạt nv
    public int endHour, endDay, endSeason;

    [Header("--- GIỚI HẠN THỜI GIAN LÀM (Sau khi nhận) ---")]
    public bool hasTimeLimitAfterAccept;                    // bật tắt Thời gian nhiệm vụ hết
    public int limitHours;                                  // Ví dụ: 24 (phải làm xong trong 1 ngày)

    [Header("--- LƯU TRỮ NỘI BỘ (Không chỉnh ở Inspector) ---")]
    public long acceptedTotalHours;                         // Mốc thời gian (tổng giờ) lúc người chơi bấm Accept

    public bool useExpLimit;                                // Bật tắt kinh nghiệm kích hoạt nhiệm vụ
    public int requiredLevel;
    public int requiredExp;

    [Header("--- CHUỖI NHIỆM VỤ (A -> B -> C) ---")]
    public List<QuestStep> questSteps;                       // Chuỗi nhiệm vụ mà người chơi nói chuyện với NPC

    [Header("--- PHẦN THƯỞNG ---")]
    public bool giveReward;
    public int expReward;
    public int goldReward;
    public List<GameObject> itemPrefabs; // List các Prefab v?t ph?m/quà t?ng

    [Header("--- TRẠNG THÁI ---")]
    public bool isAvailable;    // Nhiệm vụ đang hiện trong bảng tin/Quest Log
    public bool isCompleted;    // Nhiệm vụ hoàn thành
    public bool isAccepted;   // Người chơi đã đến nói chuyện và bấm "Đồng ý"
    public int currentStepIndex = 0;

    public void ResetQuest()
    {
        isAvailable = false;
        isAccepted = false;
        isCompleted = false;
        currentStepIndex = 0;

        foreach (var step in questSteps)
        {
            step.isCompleted = false;
        }
    }
}

[System.Serializable]
public class QuestStep
{
    public string stepName;         // Tên nhiệm vụ con, nói chuyện với NPC
    public string targetNPCName;    // tên của NPC
    public bool isCompleted;


    [Header("Hội thoại nhiệm vụ (Viết trực tiếp tại đây)")]
    public List<DialogueLine> dialogueLines;    //Hội thoại mà NPC đó nói 

    public bool requiresChoice; // Nếu tích, sau khi hết thoại thì sẽ hoàn thành nhiệm vụ
}