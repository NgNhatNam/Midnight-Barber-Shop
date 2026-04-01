using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    public Quest testQuest;
    public int testQuestAmount;
    private List<QuestProgress> testQuests = new();

     void Start()
    {
        for(int i = 0; i < testQuestAmount; i++)
        {
            testQuests.Add(new QuestProgress(testQuest));
        }

        UpdateQuestUI();
    }
   
    public void UpdateQuestUI()
    {
        if (questListContent == null) return;

        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var questProgress in QuestController.Instance.activateQuests)
        {
            if (questProgress == null || questProgress.quest == null) continue;

            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            TMP_Text questNameText = entry.transform.Find("QuestName")?.GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            // Kiểm tra xem đã kéo đúng tên trong Prefab chưa
            if (questNameText != null)
            {
                // Nên dùng questName (biến bạn đặt trong class Quest) thay vì .name (tên file SO)
                questNameText.text = questProgress.quest.questName;
            }
            else
            {
                Debug.LogError("Không tìm thấy GameObject tên 'QuestName' có TMP_Text trong QuestEntryPrefab!");
            }

            if (objectiveList != null)
            {
                foreach (var objective in questProgress.objectives)
                {
                    GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                    TMP_Text objText = objTextGO.GetComponent<TMP_Text>();

                    if (objText != null)
                    {
                        objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
                    }
                }
            }
            else
            {
                Debug.LogError("Không tìm thấy GameObject tên 'ObjectiveList' trong QuestEntryPrefab!");
            }
        }
    }
    /*
    public void UpdateQuestUI()
    {
        //Destroy existing quest entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }
        
        //Build quest entries
        foreach(var quest in QuestController.Instance.activateQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = quest.quest.name;

            foreach(var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
    */
}
