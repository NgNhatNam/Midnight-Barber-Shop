using UnityEngine;

public class GameStartupMode : MonoBehaviour
{
    public static bool IsNewGame;
    public static bool IsLoadGame;

    void Awake()
    {
        string mode = PlayerPrefs.GetString("GameMode", "New");

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
    }
}
