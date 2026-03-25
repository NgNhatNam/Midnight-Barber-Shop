using DPUtils.System.DateTime;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newNPCDialogue", menuName = "NPC Dialogue System")]
public class NPCDialogue : ScriptableObject
{
    [Header("NPC Identity")]
    public string npcName;
    public Sprite npcPortrait;

    [Header("Voice & Typing Settings")]
    public float typingSpeed = 0.05f;
    public AudioClip voidSound;
    public float voicePitch = 1f;
    public float autoPorgressDelay = 1.5f;

    [Header("Default Dialogues")]
    public List<DialogueGroup> defaultDialogueGroups;

    [Header("Priority Dialogues")]
    public List<ConditionalDialogueGroup> conditionalGroups;
}   

[System.Serializable]
public class DialogueGroup
{
    public string groupName; 
    public List<DialogueLine> dialogueLines; 
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string text;
    public bool autoProgress;

    [Header("Quest State Markers")]
    public bool isInProgressLine; 
    public bool isCompletedLine;

    
    public List<DialogueChoice> branchChoice;
}

[System.Serializable]
public class DialogueChoice
{
    public string choices; //Player response options

    public List<DialogueLine> nextLines;

    public bool giveQuest; //If choise gives quest
}

[System.Serializable]
public class ConditionalDialogueGroup
{
    public string description; 

    [Header("Conditions (Điều kiện)")]
    public int minLevel = 0;
    public Season requiredSeason;
    public int startHour = 0;
    public int endHour = 23;
    public int startDate = 1; // Ngày bắt đầu trong tháng (1-28)

    [Header("Quest Content")]
    public Quest quest; 
    public int questDurationHours = 24; // Thời hạn hoàn thành

    [Header("Dialogue Content")]
    public List<DialogueLine> dialogueLines;

    
    public bool IsValid(DateTime now, int currentLevel)
    {
        // Nếu minLevel = 0 hoặc 1 thì người chơi luôn thỏa mãn
        if (currentLevel < minLevel) return false;

        // if (requiredSeason != Season.Any && now.Season != requiredSeason) return false;
        if (now.Season != requiredSeason) return false;

        // Kiểm tra ngày: Nếu startDate = 0, coi như không giới hạn ngày bắt đầu
        if (startDate > 0 && now.Date < startDate) return false;

        // Kiểm tra giờ: 
        // Nếu để startHour = 0 và endHour = 23, nó sẽ bao phủ toàn bộ 24h.
        if (now.Hour < startHour || now.Hour > endHour) return false;

        return true;
    }
}
