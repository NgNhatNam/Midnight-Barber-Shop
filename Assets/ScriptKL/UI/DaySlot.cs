using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DaySlot : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Image backgroundImage;
    private Image myImage;

    public void Awake()
    {
        myImage = GetComponent<Image>();
    }

    public void Setup(int dayNumber)
    {
        // Ghi rõ "Ngày 1", "Ngày 2"... hoặc chỉ số "1", "2" tùy bạn
        dayText.text = "" + dayNumber.ToString();
    }

    public void SetHighlight(bool isToday, Color todayColor, Color defaultColor)
    {
        if (myImage == null)
        {
            myImage = GetComponent<Image>();
        }

        if (myImage != null)
        {
            myImage.color = isToday ? todayColor : defaultColor;
        }

        dayText.color = isToday ? todayColor : defaultColor;
        //transform.localScale = isToday ? new Vector3(1f, 1f, 1f) : Vector3.one;
    }
}