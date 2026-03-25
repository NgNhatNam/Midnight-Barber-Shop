using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading.Tasks;

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

    async Task NewGame()
    {
        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeOut();
        PlayerPrefs.SetString("GameMode", "New");
        SceneManager.LoadScene("World");
    }

    async Task LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            loadGameUI.SetActive(true);
            return;
        }
        
        if (ScreenFader.Instance != null)
            await ScreenFader.Instance.FadeOut();

        PlayerPrefs.SetString("GameMode", "Load");
        SceneManager.LoadScene("World");
    }

    public void ButtonNewGame()
    {
        _ = NewGame();
    }

    public void ButtonLoadGame()
    {
        _ = LoadGame();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

