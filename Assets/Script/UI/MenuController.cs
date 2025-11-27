using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject toolBar;
    public GameObject toolBarCombats;
    public GameObject hairCut;
    public GameObject toolIcon;

    public GameObject exitUI;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hairCut.SetActive(false);

        menuCanvas.SetActive(false);
        
        toolBarCombats.SetActive(false); ;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isMenuActive = !menuCanvas.activeSelf;
            menuCanvas.SetActive(!menuCanvas.activeSelf);

            
            Time.timeScale = isMenuActive ? 0f : 1f;
        }

        
        toolIcon.SetActive(!menuCanvas.activeSelf && !hairCut.activeSelf); 


        if (Input.GetKeyDown(KeyCode.C))
        {
            toolBarCombats.SetActive(!toolBarCombats.activeSelf);
            toolBar.SetActive(false); ;
        }
        else
        {
            toolBar.SetActive(true);
        }
    }

    public void TabExitButton()
    {

        menuCanvas.SetActive(false);

        Time.timeScale =1f;
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }
}
