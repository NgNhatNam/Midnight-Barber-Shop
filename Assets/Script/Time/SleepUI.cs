using DPUtils.System.DateTime;
using System.Collections;
using UnityEngine;

public class SleepUI : MonoBehaviour
{
    public GameObject TabUi;
    public GameObject SleepPanel;
    void Update()
    {

        if (TabUi.activeSelf)
        {
            SleepPanel.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Dính");
            SleepPanel.SetActive(true);
            //StartCoroutine(timeStop());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Ko Dính");
            SleepPanel.SetActive(false);
            //Time.timeScale = 1f;
        }
    }

    IEnumerator timeStop()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0f;
    }

}
