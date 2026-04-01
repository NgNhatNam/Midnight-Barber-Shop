using UnityEngine;

public class DialogueCloseButton : MonoBehaviour
{
    public void OnClickClose()
    {
        // Phát tín hiệu cho toàn bộ NPC
        DialogueEvents.OnDialogueUIClosed?.Invoke();

        // Hoặc nếu bạn dùng DialogueManager như cách trước:
        // DialogueManager.Instance.currentNPC.EndDialogue();
    }
}

// Một class trung gian nhỏ để truyền tín hiệu
public static class DialogueEvents
{
    public static System.Action OnDialogueUIClosed;
}