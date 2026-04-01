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

    /*
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
    */

    async Task NewGame()
    {
        await LoadSceneWithFade("New");
    }

    async Task LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            loadGameUI.SetActive(true);
            return;
        }

        await LoadSceneWithFade("Load");
    }
    async Task LoadSceneWithFade(string mode)
    {
        // 1. Chạy hiệu ứng Fade Out (Màn hình tối dần)
        if (ScreenFader.Instance != null)
        {
            await ScreenFader.Instance.FadeOut();
        }

        // 2. Lưu mode vào PlayerPrefs
        PlayerPrefs.SetString("GameMode", mode);

        // 3. Load Scene ngầm (Async)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("World");

        // Ngăn không cho Scene mới hiện ra ngay lập tức nếu chưa load xong
        asyncLoad.allowSceneActivation = false;

        // Đợi cho đến khi Scene mới load được 90% (mức sẵn sàng của Unity)
        while (asyncLoad.progress < 0.9f)
        {
            await Task.Yield();
        }

        // 4. Cho phép kích hoạt Scene mới
        asyncLoad.allowSceneActivation = true;

        // Lưu ý: Ở Scene "World", bạn cần gọi ScreenFader.Instance.FadeIn() trong hàm Start 
        // để màn hình sáng trở lại mượt mà.
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

