using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject toolBar;
    public GameObject toolBarCombats;
    public GameObject hairCut;
    public GameObject toolIcon;
    //public GameObject cutHairButton;
    public GameObject stickButton;
    public GameObject useItemUI;
    public GameObject shopUI;
    
    public GameObject interactButton;
    private bool wasInteractActiveBeforeMenu = false;

    public GameObject exitUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hairCut.SetActive(false);

        menuCanvas.SetActive(false);
        
        toolBarCombats.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }

        toolIcon.SetActive(!menuCanvas.activeSelf && !hairCut.activeSelf);

        if (Input.GetKeyDown(KeyCode.C))
        {
            bool isCombatActive = !toolBarCombats.activeSelf;
            toolBarCombats.SetActive(isCombatActive);
            toolBar.SetActive(!isCombatActive);
        }
    }

    public void ToggleMenu()
    {
        bool isOpening = !menuCanvas.activeSelf;

        if (isOpening)
        {
            // --- KHI MỞ MENU ---
            // Ghi nhớ trạng thái hiện tại của nút Interact trước khi tắt nó
            wasInteractActiveBeforeMenu = interactButton.activeSelf;

            menuCanvas.SetActive(true);
            stickButton.SetActive(false);
            interactButton.SetActive(false); // Luôn tắt khi mở Menu
            shopUI.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            // --- KHI ĐÓNG MENU ---
            menuCanvas.SetActive(false);
            stickButton.SetActive(true); // Move button luôn hiện lại khi đóng menu
            useItemUI.SetActive(false);
            // CHỈ hiện lại nút Interact nếu trước đó nó đang hiện (đang đứng gần NPC/Rương)
            interactButton.SetActive(wasInteractActiveBeforeMenu);

            Time.timeScale = 1f;
        }
    }
    public void TabExitButton()
    {

        if (menuCanvas.activeSelf) ToggleMenu();
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }

}
