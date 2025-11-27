using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitions : MonoBehaviour
{
    [SerializeField]
    private Animator transitionAnim;
    [SerializeField]
    private string sceneName;


    public GameObject sceneTransitions;

    void Start()
    {

        sceneTransitions.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space)) 
        {
            sceneTransitions.SetActive(true);
            StartCoroutine(LoadScene());
        }     
    }

    IEnumerator LoadScene()
    {
        transitionAnim.SetTrigger("end");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(sceneName);  
    }
}
