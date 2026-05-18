using DPUtils.System.DateTime;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    private InventoryController inventoryController;

    private Chest[] chests;

    private TimeManager timeManager; 

    private Light2D globalLight;

    private Health playerHealth;

    public static bool IsLoadingGame = false;

    public static bool PendingReset = false;


    async void Start()
    {
        InitializeComponents();

        // Reset sau khi scene load xong
        if (PendingReset)
        {
            PendingReset = false;
            IsLoadingGame = true;
            PerformResetAfterLoad();
            IsLoadingGame = false;

            await Task.Yield();

            if (ScreenFader.Instance != null)
                await ScreenFader.Instance.FadeIn();
            return;
        }

        // New Game
        if (GameStartupMode.IsNewGame)
        {
            GameStartupMode.IsNewGame = false;
            await ResetGame(false);
            return;
        }

        // Load Game
        if (GameStartupMode.IsLoadGame)
        {
            GameStartupMode.IsLoadGame = false;
            await LoadGame();
            return;
        }

        // Default (Editor Play)
        if (File.Exists(saveLocation))
            await LoadGame();
        else
            await ResetGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
        chests = FindObjectsOfType<Chest>();
        timeManager = FindAnyObjectByType<TimeManager>(); //Add
        globalLight = GameObject.FindGameObjectWithTag("GlobalLight").GetComponent<Light2D>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
    }
   
    public void SaveGame()
    {
        
        var currentTime = GetCurrentGameTime(); // Lấy thời gian hiện tại trong game

        // Stable
        List<StableSaveData> stables = new List<StableSaveData>();
        AnimalStable[] allStables = FindObjectsOfType<AnimalStable>();
        foreach (var stable in allStables)
        {
            stables.Add(stable.GetStableSaveData());
        }
        //  NPC 
        List<NPCSaveData> npcs = new List<NPCSaveData>();
        NPC[] allNPCs = FindObjectsOfType<NPC>(); // Tìm tất cả NPC trong Scene

        foreach (NPC npc in allNPCs)
        {
            npcs.Add(new NPCSaveData
            {
                npcName = npc.gameObject.name,
                position = npc.transform.position
            });
        }

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,

            npcSaveData = npcs,

            mapBoundary = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,

            inventorySaveData = inventoryController.GetInventoryItems(),

            toolbarSaveData = inventoryController.GetItemsFromPanel(inventoryController.toolbarPanel), // Thêm dòng này

            chestSaveData = GetChestsState(),

            questProgressData = QuestController.Instance.activateQuests,

            globalLightIntensity = globalLight != null ? globalLight.intensity : 1f, //  Light Save

            allStablesData = stables, // Stable Save

            // Time Save
            date = currentTime.Date,
            season = (int)currentTime.Season,
            year = currentTime.Year,
            hour = currentTime.Hour,
            minutes = currentTime.Minutes,


            HP = playerHealth.HP,
            MaxHP = playerHealth.MaxHP,
            MN = playerHealth.MN,
            MaxMN = playerHealth.MaxMN,
            Gold = playerHealth.Gold,
            Stress = playerHealth.Stress,
            currentLevel = playerHealth.currentLevel,
            currentEXP = playerHealth.currentEXP,
            expToNextLevel = playerHealth.expToNextLevel,
            customersServed = playerHealth.customersServed,
        };

        // Tìm tất cả các shop trong scene và lưu lại
        List<ShopItemSaveData> shops = new List<ShopItemSaveData>();
        ShopManager[] allShops = FindObjectsOfType<ShopManager>();
        foreach (var shop in allShops)
        {
            shops.Add(shop.GetShopSaveData());
        }

        // Gán vào saveData
        saveData.allShopsData = shops;

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));

        Debug.Log($"Saved: {currentTime.DateToString()} {currentTime.TimeToString()} | Light={globalLight.intensity} " +
            $"| HP={playerHealth.HP}, Gold={playerHealth.Gold} | MN={playerHealth.MN}, Stress={playerHealth.Stress}" + "Game & NPCs Saved!");
 
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened,
            };
            chestStates.Add(chestSaveData);
        }
        return chestStates;
    }

    public async Task LoadGame()
    {

        IsLoadingGame = true;

        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeOut();

        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            ExecuteLoad(saveData);

            await Task.Yield();

            // Ép Cinemachine cập nhật vị trí ngay lập tức để tránh bị khựng 
            var vcam = FindAnyObjectByType<CinemachineCamera>(); 
            if (vcam != null)
            {
                vcam.ForceCameraPosition(saveData.playerPosition, Quaternion.identity);
            }
        }
        else
        {
            SaveGame();
        }

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeIn();

        IsLoadingGame = false;
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);
            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }


    public async void LoadGameButton()
    {
        await LoadGame();
    }
    private void ExecuteLoad(SaveData saveData)
    {

        // Tìm Confiner và Player
        var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        var player = GameObject.FindGameObjectWithTag("Player");
        var vcam = FindAnyObjectByType<CinemachineCamera>();

        // Tìm đúng Map Bound theo tên và Layer (Gọn hơn)
        GameObject actualMap = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .FirstOrDefault(obj => obj.name == saveData.mapBoundary && obj.layer == LayerMask.NameToLayer("MapBound"));

        if (actualMap != null && confiner != null)
        {
            confiner.BoundingShape2D = actualMap.GetComponent<BoxCollider2D>();
            confiner.InvalidateBoundingShapeCache();
        }

        // Dịch chuyển Player & Warp Camera
        if (player != null)
        {
            player.transform.position = saveData.playerPosition;

            if (vcam != null)
                vcam.OnTargetObjectWarped(player.transform, saveData.playerPosition - vcam.transform.position);
        }

        // Player Position
        // GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
        // Player Camera
        //FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<BoxCollider2D>();

        // Player Inventory
        inventoryController.SetInventoryItems(saveData.inventorySaveData);

        // Load Toolbar 
        inventoryController.SetToolbarItems(saveData.toolbarSaveData);

        // Load Quest Inventory
        QuestController.Instance.LoadQuestProgress(saveData.questProgressData);

        // Player Health
        if (playerHealth != null && saveData != null)
        {
            playerHealth.Adjust(saveData.HP);
            playerHealth.AdjustMN(saveData.MN);
            playerHealth.SetGold(saveData.Gold);
            playerHealth.SetStress(saveData.Stress);
            playerHealth.currentLevel = saveData.currentLevel;
            playerHealth.currentEXP = saveData.currentEXP;
            playerHealth.expToNextLevel = saveData.expToNextLevel;
            playerHealth.customersServed = saveData.customersServed;

            Debug.Log($" Restored HP={playerHealth.HP}, MN={playerHealth.MN}, Gold={playerHealth.Gold}, Stress={playerHealth.Stress}");
        }


        if (saveData.npcSaveData != null)
        {
            foreach (var npcData in saveData.npcSaveData)
            {
                GameObject npcObj = GameObject.Find(npcData.npcName);
                if (npcObj != null)
                {
                    NPC npcScript = npcObj.GetComponent<NPC>();
                    UnityEngine.AI.NavMeshAgent agent = npcObj.GetComponent<UnityEngine.AI.NavMeshAgent>();

                    if (agent != null)
                    {
                        agent.Warp(npcData.position);
                    }
                    else
                    {
                        npcObj.transform.position = npcData.position;
                    }
                }
            }
        }

        // Tạo lại thời gian từ dữ liệu đã lưu
        var loadedTime = new DPUtils.System.DateTime.DateTime(
        saveData.date,
        saveData.season,
        saveData.year,
        saveData.hour,
            saveData.minutes
        );

        // Load Shop Data
        if (saveData.allShopsData != null)
        {
            ShopManager[] allShops = FindObjectsOfType<ShopManager>();
            foreach (var shop in allShops)
            {
                var data = saveData.allShopsData.Find(s => s.shopID == shop.shopID);
                if (data != null)
                {
                    shop.LoadShopSaveData(data);
                }
            }
        }

        // Load Save Stable
        if (saveData.allStablesData != null)
        {
            AnimalStable[] allStables = FindObjectsOfType<AnimalStable>();
            foreach (var stable in allStables)
            {
                var data = saveData.allStablesData.Find(s => s.stableID == stable.stableID || s.stableID == stable.gameObject.name);
                if (data != null)
                {
                    stable.LoadStableSaveData(data);
                }
            }
        }

        //Load ChestStates
        LoadChestStates(saveData.chestSaveData);
        // Cập nhật lại vào TimeManager
        SetCurrentGameTime(loadedTime);
        Debug.Log($"⏰ Loaded Time: {loadedTime.DateToString()} {loadedTime.TimeToString()}");

        // Light

        globalLight.intensity = saveData.globalLightIntensity;
        Debug.Log($"☀️ Restored Light Intensity: {saveData.globalLightIntensity}");
    }

    // Hàm phụ để lấy và set thời gian an toàn
    private DPUtils.System.DateTime.DateTime GetCurrentGameTime()
    {
        var timeField = typeof(TimeManager).GetField("DateTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (DPUtils.System.DateTime.DateTime)timeField.GetValue(timeManager);
    }

    private void SetCurrentGameTime(DPUtils.System.DateTime.DateTime dateTime)
    {
        var timeField = typeof(TimeManager).GetField("DateTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        timeField.SetValue(timeManager, dateTime);

        // Gửi sự kiện để ClockManager cập nhật lại
        TimeManager.OnDateTimeChanged?.Invoke(dateTime);
    }


    //-----------------------------------------------------------------------------------------------------
    private void PerformResetAfterLoad()
    {
        var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        confiner.BoundingShape2D = GameObject.Find("Village_City").GetComponent<BoxCollider2D>();

        // Reset time
        var resetTime = new DPUtils.System.DateTime.DateTime(
            1, 0, 1, 6, 0
        );
        SetCurrentGameTime(resetTime);
        // Reset health
        playerHealth.HealFull();
        playerHealth.HealFullMN();
        playerHealth.SetGold(1000);
        playerHealth.SetStress(playerHealth.MaxStress);


        inventoryController.ClearInventory();

        SaveGame();
        Debug.Log("Reset hoàn tất sau khi load scene!");
    }

    async Task ResetGame(bool reloadScene = true)
    {
        if (reloadScene && ScreenFader.Instance != null) await ScreenFader.Instance.FadeOut();

        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Save deleted!");
        }

        Time.timeScale = 1f;

        if (reloadScene)
        {
            PendingReset = true;  
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            IsLoadingGame = true;
            PerformResetAfterLoad();
            IsLoadingGame = false;
            await Task.Yield();
            if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeIn();
        }
    }

    public void OnClickResetButton()
    {
        // Sử dụng "_" để nhận biết là Task chạy độc lập
        _ = ResetGame();
    }

    public void timeScale()
    {
        Time.timeScale = 1f;
    }

    public void timeStop()
    {
        Time.timeScale = 0f;
    }
   
}
