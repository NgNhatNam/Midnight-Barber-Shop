using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SleepUI : MonoBehaviour, IInteractable
{
    public static SleepUI Instance { get; private set; }

    [Header("UI Windows")]
    public GameObject TabUi;           
    public GameObject InteractPrompt;   
    public GameObject SleepWindow;     

    [Header("Scroll Config")]
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject sleepSlotPrefab;

    private TimeChanger timeChanger;
    private SaveController saveController;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        timeChanger = FindFirstObjectByType<TimeChanger>();
        saveController = FindFirstObjectByType<SaveController>();

        if (InteractPrompt) InteractPrompt.SetActive(false);
        if (SleepWindow) SleepWindow.SetActive(false);
    }

    // ---  INTERFACE IINTERACTABLE ---
    public bool CanInteract()
    {
        return !TabUi.activeSelf && !SleepWindow.activeSelf;
    }

    public void Interact()
    {
        OpenSleepWindow();
    }
    // ---------------------------------------

    public void OpenSleepWindow()
    {
        SleepWindow.SetActive(true);
        InteractPrompt.SetActive(false);
        Time.timeScale = 0f; 

        // Làm mới danh sách trong ScrollView
        foreach (Transform child in scrollContent) Destroy(child.gameObject);

        for (int i = 1; i <= 12; i++)
        {
            GameObject slotGO = Instantiate(sleepSlotPrefab, scrollContent);
            var slotScript = slotGO.GetComponent<SleepSlot>();
            if (slotScript != null)
            {
                slotScript.Setup($"Ngủ {i} giờ", i, i * 10);
            }
        }
    }

    public async void ExecuteSleep(int hours, int mana)
    {
        SleepWindow.SetActive(false);

        await ScreenFader.Instance.FadeOut();
        
        timeChanger.SleepWithDuration(hours, mana);
        saveController.SaveGame();
        await Task.Delay(500);

        await ScreenFader.Instance.FadeIn();
        Time.timeScale = 1f;
    }

    public void CloseSleepUI()
    {
        if (saveController) saveController.SaveGame();
        SleepWindow.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (TabUi == null || SleepWindow == null) return;

        if (TabUi.activeSelf && SleepWindow.activeSelf)
        {
            CloseSleepUI();
        }

        if (SleepWindow.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSleepUI();
        }
    }
    
}