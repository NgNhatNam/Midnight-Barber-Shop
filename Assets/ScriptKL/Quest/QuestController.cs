using System.Collections.Generic;
using UnityEngine;


public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    public List<QuestProgress> activateQuests = new();

    private QuestUI questUI;

    public List<string> handinQuestIDs = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        questUI = FindObjectOfType<QuestUI>();
        InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
    }

    public bool IsQuestActive(string questID)
    {
        // 1. Kiểm tra list có tồn tại không
        if (activateQuests == null) return false;
        // 2. Kiểm tra an toàn từng phần tử q và q.quest
        return activateQuests.Exists(q => q != null && q.quest != null && q.QuestID == questID);
    }

    public void AcceptQuest(Quest quest, int startTime = 0, int duration = 24)
    {
        // Kiểm tra đầu vào để tránh add quest null vào list
        if (quest == null)
        {
            // Đảm bảo UpdateUI luôn chạy trên Main Thread
            questUI.UpdateQuestUI();
            Debug.LogWarning("NPC đang cố giao một Quest bị null!");
            return;
        }

        if (IsQuestActive(quest.questID)) return;

        activateQuests.Add(new QuestProgress(quest, startTime, duration));

        CheckInventoryForQuests();

        // Kiểm tra questUI trước khi gọi để tránh lỗi Null thứ 2
        if (questUI != null)
        {
            questUI.UpdateQuestUI();
        }
        else
        {
            questUI = FindFirstObjectByType<QuestUI>(); // Tìm lại nếu chưa gán
            if (questUI != null) questUI.UpdateQuestUI();
        }
    }

    public void CheckInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();

        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if (questObjective.type != ObjectiveType.CollectItem) continue;
                if (!int.TryParse(questObjective.objectiveID, out int itemId)) continue;

                int newAmount = itemCounts.TryGetValue(itemId, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : 0;

                if (questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                }
            }
        }

        questUI.UpdateQuestUI();
    }

    public bool IsQuestCompleted(string questID)
    {
        // Kiểm tra danh sách và ID đầu vào
        if (activateQuests == null || string.IsNullOrEmpty(questID)) return false;

        // Tìm Quest với điều kiện bảo vệ Null
        QuestProgress quest = activateQuests.Find(q => q != null && q.quest != null && q.QuestID == questID);

        return quest != null && quest.objectives != null && quest.objectives.TrueForAll(o => o != null && o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        if (string.IsNullOrEmpty(questID) || activateQuests == null) return;

        QuestProgress targetQuest = null;

        // Sử dụng vòng lặp thay vì Find để kiểm soát Null từng bước
        for (int i = 0; i < activateQuests.Count; i++)
        {
            var qp = activateQuests[i];
            if (qp == null) continue;

            // KIỂM TRA QUAN TRỌNG: Phải chắc chắn qp.quest không null trước khi lấy QuestID
            if (qp.quest != null && qp.QuestID == questID)
            {
                targetQuest = qp;
                break;
            }
        }

        if (targetQuest != null)
        {
            // Thực hiện xóa đồ
            if (ExecuteItemRemoval(targetQuest))
            {
                if (!handinQuestIDs.Contains(questID))
                    handinQuestIDs.Add(questID);

                activateQuests.Remove(targetQuest);

                if (questUI != null) questUI.UpdateQuestUI();

                Debug.Log($"[Success] Quest {questID} completed.");
            }
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy Quest ID: {questID} trong danh sách Active.");
        }
    }

    public void OnCustomerServed(string customerType = "")
    {
        foreach (QuestProgress progress in activateQuests)
        {
            if (progress.IsCompleted) continue;

            foreach (QuestObjective objective in progress.objectives)
            {
                if (objective.type == ObjectiveType.ServeCustomer)
                {
                    // KIỂM TRA ID Ở ĐÂY:
                    // Nếu ID trong Quest để trống -> Tính cho tất cả khách
                    // Nếu ID có giá trị -> Phải trùng với loại khách vừa cắt (customerType)
                    if (string.IsNullOrEmpty(objective.objectiveID) || objective.objectiveID == customerType)
                    {
                        if (objective.currentAmount < objective.requiredAmount)
                        {
                            objective.currentAmount++;
                        }
                    }
                }
            }
        }
        questUI.UpdateQuestUI();
    }

    private bool ExecuteItemRemoval(QuestProgress quest)
    {
        if (quest == null || quest.objectives == null) return false;

        Dictionary<int, int> itemsToRemove = new Dictionary<int, int>();

        foreach (var obj in quest.objectives)
        {
            if (obj == null) continue;

            if (obj.type == ObjectiveType.CollectItem && int.TryParse(obj.objectiveID, out int id))
            {
                itemsToRemove[id] = obj.requiredAmount;
            }
        }

        // Thực hiện xóa thực tế
        foreach (var item in itemsToRemove)
        {
            if (InventoryController.Instance != null)
            {
                InventoryController.Instance.RemoveItemsFromInventory(item.Key, item.Value);
            }
        }
        return true;
    }

    public bool IsQuestHandedIn(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }

    public bool RemoveRequiredItemsFromInventory(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return false;

        QuestProgress quest = activateQuests.Find(q => q != null && q.quest != null && q.QuestID == questID);

        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();

        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective != null && objective.type == ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objective.requiredAmount;
            }
        }

        // Kiểm tra số lượng thực tế trong kho
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        foreach (var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key) < item.Value) return false;
        }

        // Thực hiện xóa
        foreach (var itemRequirement in requiredItems)
        {
            InventoryController.Instance.RemoveItemsFromInventory(itemRequirement.Key, itemRequirement.Value);
        }
        return true;
    }

     public void LoadQuestProgress(List<QuestProgress> saveQuests)
    {
        activateQuests = saveQuests ?? new();
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }

}

