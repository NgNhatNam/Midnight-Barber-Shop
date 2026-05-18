using UnityEngine;

public class GameStartupMode : MonoBehaviour
{
    public static bool IsNewGame;
    public static bool IsLoadGame;

    void Awake()
    {
        string mode = PlayerPrefs.GetString("GameMode", "None");

        if (mode == "New")
        {
            IsNewGame = true;
            IsLoadGame = false;
        }
        else if (mode == "Load")
        {
            IsNewGame = false;
            IsLoadGame = true;
        }

        // Xóa tránh chạy lại sau Reload scene
        PlayerPrefs.DeleteKey("GameMode");

        // Force black screen during transition
        if (mode != "None")
        {
            // Find ScreenFader explicitly in case it hasn't Awoken yet
            ScreenFader fader = FindAnyObjectByType<ScreenFader>();
            if (fader != null)
            {
                fader.SetAlpha(1f);
            }
        }
    }
}
