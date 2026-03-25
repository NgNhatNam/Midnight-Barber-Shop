using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjective> objectives;

    public List<QuestReward> questRewards;

    // Called when scriptable obj is created
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }

    }

}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;

    public bool IsCompleted => currentAmount >= requiredAmount;
}

public enum ObjectiveType
{
    CollectItem,
    DefeatEnemy,
    ReachLocation,
    talkNPC,
    Custom
}

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;

    public int startTotalHours;
    public int durationHours;
    public QuestProgress(Quest quest, int startTime = 0, int duration = 24)
    {
        this.quest = quest;
        this.startTotalHours = startTime;
        this.durationHours = duration;

        objectives = new List<QuestObjective>();

        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.questID;

    // Kiểm tra xem nhiệm vụ đã hết hạn chưa
    public bool IsExpired(int currentTotalHours)
    {
        return currentTotalHours > (startTotalHours + durationHours);
    }
}

[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public int rewardID;
    public int amount = 1;
}

public enum RewardType
{
    Item, Gold, Experience, Custom
}