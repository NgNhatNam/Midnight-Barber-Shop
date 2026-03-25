using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickupUIController : MonoBehaviour
{
    public static ItemPickupUIController Instance { get; private set; }

    public GameObject popupPrefab;
    public int maxPopups = 4;
    public float popupDuration;

    private readonly Queue<GameObject> activePopups = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple ItemPickupUIManager instance detected! Destroying the extra one");
            Destroy(gameObject);
        }
    }

    public void ShowItemPickup(string itemName, Sprite itemIcon)
    {
        if (popupPrefab == null)
        {
            Debug.LogError("Chưa kéo popupPrefab vào ItemPickupUIController!");
            return;
        }

        GameObject newPopup = Instantiate(popupPrefab, transform);

        // Tìm Text an toàn hơn (tìm cả ở các con)
        TMP_Text textComp = newPopup.GetComponentInChildren<TMP_Text>();
        if (textComp != null)
        {
            textComp.text = itemName;
        }
        else
        {
            Debug.LogWarning("Prefab Popup thiếu component TMP_Text!");
        }

        // Tìm Icon an toàn hơn
        Transform iconTransform = newPopup.transform.Find("ItemIcon");
        if (iconTransform != null)
        {
            Image itemImage = iconTransform.GetComponent<Image>();
            if (itemImage != null) itemImage.sprite = itemIcon;
        }

        activePopups.Enqueue(newPopup);
        if (activePopups.Count > maxPopups)
        {
            GameObject oldPopup = activePopups.Dequeue();
            if (oldPopup != null) Destroy(oldPopup);
        }

        StartCoroutine(FadeOutAndDestroy(newPopup));
    }

    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popupDuration);
        if (popup == null) yield break;

        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        for (float timePassed = 0f; timePassed < 1f; timePassed += Time.deltaTime) 
        { 
            if(popup == null)  yield break;
            canvasGroup.alpha = 1f - timePassed;
            yield return null;
        }

        Destroy(popup);
    }
}
