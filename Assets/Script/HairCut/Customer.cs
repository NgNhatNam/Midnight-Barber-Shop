using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Customer : MonoBehaviour
{
    [Header("References")]
    public Image customerImage;
    public TMP_Text dialogueText;

    [HideInInspector] public CustomerData data;

    public void Init(CustomerData newData)
    {
        data = newData;

        // Random sprite và thoại
        customerImage.sprite = data.sprites[Random.Range(0, data.sprites.Count)];
        dialogueText.text = data.dialogues[Random.Range(0, data.dialogues.Count)];
    }
}
