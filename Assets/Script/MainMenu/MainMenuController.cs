using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    public GameObject loadGameUI;
    public GameObject settingGameUI;

    private string saveLocation;

    private void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    public void SettingSound()
    {
        settingGameUI.SetActive(true);
    }

    public void NewGame()
    {
        PlayerPrefs.SetString("GameMode", "New");
        SceneManager.LoadScene("World");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            loadGameUI.SetActive(true);
            return;
        }

        PlayerPrefs.SetString("GameMode", "Load");
        SceneManager.LoadScene("World");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

