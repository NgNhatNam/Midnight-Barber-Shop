using DPUtils.System.DateTime;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.IO;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
public class SaveController : MonoBehaviour
{
    private string saveLocation;

    private InventoryController inventoryController;

    private TimeManager timeManager; 

    private Light2D globalLight;

    private Health playerHealth;

    public static bool IsLoadingGame = false;

    public static bool PendingReset = false;

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
        timeManager = FindAnyObjectByType<TimeManager>(); //Add
        globalLight = GameObject.FindGameObjectWithTag("GlobalLight").GetComponent<Light2D>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();

        // Reset sau khi scene load xong
        if (PendingReset)
        {
            PendingReset = false;
            PerformResetAfterLoad();
            return;
        }

        // New Game
        if (GameStartupMode.IsNewGame)
        {
            GameStartupMode.IsNewGame = false;
            ResetGame();
            return;
        }

        // Load Game
        if (GameStartupMode.IsLoadGame)
        {
            GameStartupMode.IsLoadGame = false;
            LoadGame();
            return;
        }

        // Default (Editor Play)
        if (File.Exists(saveLocation))  
            LoadGame();
        else
            ResetGame();
    }

    public void SaveGame()
    {
        
        var currentTime = GetCurrentGameTime(); // Lấy thời gian hiện tại trong game

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,

            mapBoundary = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,

            inventorySaveData = inventoryController.GetInventoryItems(),
            

            globalLightIntensity = globalLight != null ? globalLight.intensity : 1f, //  Light Save

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
            Stress = playerHealth.Stress
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));

        Debug.Log($"Saved: {currentTime.DateToString()} {currentTime.TimeToString()} | Light={globalLight.intensity} " +
            $"| HP={playerHealth.HP}, Gold={playerHealth.Gold} | MN={playerHealth.MN}, Stress={playerHealth.Stress}");
 
    }

    public void LoadGame()
    {

        IsLoadingGame = true;

        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));


            // Player Position
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;

            // Player Camera

            FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<BoxCollider2D>();

            // Player Inventory
            inventoryController.SetInventoryItems(saveData.inventorySaveData);
   

            // Player Health
            if (playerHealth != null && saveData != null)
            {
                playerHealth.Adjust(saveData.HP);
                playerHealth.AdjustMN(saveData.MN);
                playerHealth.SetGold(saveData.Gold);
                playerHealth.SetStress(saveData.Stress);

                Debug.Log($"❤️ Restored HP={playerHealth.HP}, MN={playerHealth.MN}, Gold={playerHealth.Gold}, Stress={playerHealth.Stress}");
            }


            // Tạo lại thời gian từ dữ liệu đã lưu
            var loadedTime = new DPUtils.System.DateTime.DateTime(
                saveData.date,
                saveData.season,
                saveData.year,
                saveData.hour,
                saveData.minutes
            );

            // Cập nhật lại vào TimeManager
            SetCurrentGameTime(loadedTime);
            Debug.Log($"⏰ Loaded Time: {loadedTime.DateToString()} {loadedTime.TimeToString()}");

            // Light
            
            globalLight.intensity = saveData.globalLightIntensity;
            Debug.Log($"☀️ Restored Light Intensity: {saveData.globalLightIntensity}");
            }
        else
        {
            SaveGame();
        }

        IsLoadingGame = false;
    }


    public void LoadGameButton()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            StartCoroutine(LoadAfterStart(saveData));
        }
        else
        {
            SaveGame();
        }
    }

    private IEnumerator LoadAfterStart(SaveData saveData)
    {
        yield return null;  // ⬅ delay 1 frame để InventoryController.Start() chạy xong

        // Player Position
        GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;

        // Player Camera
        FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D =
            GameObject.Find(saveData.mapBoundary).GetComponent<BoxCollider2D>();

        // Player Inventory (bây giờ itemDictionary đã được gán)
        inventoryController.SetLoadInventoryItems(saveData.inventorySaveData);

        // Player Health
        if (playerHealth != null)
        {
            playerHealth.Adjust(saveData.HP);
            playerHealth.AdjustMN(saveData.MN);
            playerHealth.SetGold(saveData.Gold);
            playerHealth.SetStress(saveData.Stress);
        }

        // Time
        var loadedTime = new DPUtils.System.DateTime.DateTime(
            saveData.date, saveData.season, saveData.year, saveData.hour, saveData.minutes);

        SetCurrentGameTime(loadedTime);

        // Light
        globalLight.intensity = saveData.globalLightIntensity;
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
        playerHealth.SetGold(0);
        playerHealth.SetStress(playerHealth.MaxStress);


        inventoryController.ClearInventory();

        SaveGame();
        Debug.Log("Reset hoàn tất sau khi load scene!");
    }

    public void ResetGame()
    {
        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Save deleted!");
        }

        PendingReset = true;  

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void timeScale()
    {
        Time.timeScale = 1f;
    }

    public void timeStop()
    {
        Time.timeScale = 0f;
    }
    /*
  public void ResetGame()
  {
      // Xóa file save
      if (File.Exists(saveLocation))
      {
          File.Delete(saveLocation);
          Debug.Log("Save data has been deleted!");
      }

      var confiner = FindAnyObjectByType<Unity.Cinemachine.CinemachineConfiner2D>();
      confiner.BoundingShape2D = GameObject.Find("Village_City").GetComponent<BoxCollider2D>();

      // Đảm bảo game không bị pause
      Time.timeScale = 1f;

      // Reload lại scene hiện tại
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);

      // Đặt lại thời gian game về mặc định
      var resetTime = new DPUtils.System.DateTime.DateTime(
          date: 1,
          season: 0,
          year: 1,
          hour: 6,
          minutes: 0
      );
      SetCurrentGameTime(resetTime);
      Debug.Log("Game sẽ quay reset vào Ngày 1, Mùa Xuân, Năm 1, 06:00 AM");
      SaveGame();
      Debug.Log("Game reset: reloading scene...");
  }


  public void ResetGame()
  {
      // Kiểm tra file lưu
      if (File.Exists(saveLocation))
      {
          File.Delete(saveLocation);
          Debug.Log("🧹 Save data has been deleted!");
      }

      // Đưa player về vị trí mặc định (tuỳ bạn chỉnh)
      GameObject player = GameObject.FindGameObjectWithTag("Player");
      if (player != null)
      {
          player.transform.position = new Vector3(-1f, 6f, 0f);
      }


      var confiner = FindAnyObjectByType<Unity.Cinemachine.CinemachineConfiner2D>();
      if (confiner != null)
          confiner.BoundingShape2D = GameObject.Find("City_Shop").GetComponent<BoxCollider2D>();



      // Xoá dữ liệu inventory
      if (inventoryController != null)
      {
          inventoryController.ClearInventory(); // ⚠️ Cần có hàm này trong InventoryController
      } 

      // Đặt lại ánh sáng mặc định
      if (globalLight != null)
      {
          globalLight.intensity = 1f;
      }

      // Đặt lại thời gian game về mặc định
      var resetTime = new DPUtils.System.DateTime.DateTime(
          date: 1,
          season: 0,
          year: 1,
          hour: 6,
          minutes: 0
      );
      SetCurrentGameTime(resetTime);
      Debug.Log("⏰ Game time reset to Day 1, Spring, Year 1, 06:00 AM");

      // Đặt lại Player Health
      if (playerHealth != null)
      {
          playerHealth.HealFull();
          playerHealth.HealFullMN();
          playerHealth.SetGold(0);
          playerHealth.SetStress(playerHealth.MaxStress);
      }

      // 💾 6. Lưu lại để tạo file mới từ đầu
      SaveGame();

      Debug.Log("✅ Game reset complete!");
  }
*/

}
