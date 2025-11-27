using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [Header("HealthBar")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;
    [SerializeField] public TMP_Text hpText;

    private float maxTopMask;
    private float initialTopMask;

    private float maxRightMask;
    private float initialRightMask;

    [Header("ManaBar")]
    [SerializeField] private RectTransform barRectMana;
    [SerializeField] private RectMask2D maskMana;
    [SerializeField] public TMP_Text mnText;

    private float maxTopMaskMana;
    private float initialTopMaskMana;

    [Header("Gold Text")]
    [SerializeField] public TMP_Text goldText;

    [Header("Stress Text")]
    [SerializeField] public TMP_Text StressText;



    private void Start()
    {
        // x = left, w = top, y = bottom, z = right
        maxTopMask = barRect.rect.height - mask.padding.y - mask.padding.w;
        maxRightMask = barRect.rect.width - mask.padding.x - mask.padding.z;
        maxTopMaskMana = barRectMana.rect.height - maskMana.padding.y - maskMana.padding.w;

        hpText.SetText($"{health.HP}/{health.MaxHP}");
        mnText.SetText($"{health.MN}/{health.MaxMN}");
        goldText.SetText($"{health.Gold}");
        SetValueStress(health.Stress);

        initialRightMask = mask.padding.z;
        initialTopMask = mask.padding.w;
        initialTopMaskMana = maskMana.padding.w;

    }

    public void SetValueHP(int newValue)
    {
        var targetHeight = newValue * maxTopMask / health.MaxHP;
        var newTopMask = maxTopMask + initialTopMask - targetHeight;

        var padding = mask.padding;
        padding.w = newTopMask ;
        mask.padding = padding * 4.5f;

        hpText.SetText($"{newValue}/{health.MaxHP}");
    }

    public void SetValueRightHP(int newValue)
    {
        var targetWidth = newValue * maxRightMask / health.MaxHP;
        var newRightMask = maxRightMask + initialRightMask - targetWidth;

        var padding = mask.padding;
        padding.w = maxRightMask;
        mask.padding = padding * 4.5f;

        hpText.SetText($"{newValue}/{health.MaxHP}");
    }

    public void SetValueMana(int newValueMn)
    {
        var targetHeightMana = newValueMn * maxTopMaskMana / health.MaxMN;
        var newTopMaskMana = maxTopMaskMana + initialTopMaskMana - targetHeightMana;

        var paddingMana = maskMana.padding;
        paddingMana.w = newTopMaskMana;
        maskMana.padding = paddingMana * 4.5f;

        mnText.SetText($"{newValueMn}/{health.MaxMN}");
    }

    public void SetValueGold(int newValueGold)
    {
        if (newValueGold < 0)
            newValueGold = 0;

        // Cập nhật text hiển thị
        goldText.SetText($"{newValueGold:N0}".Replace(",", ".")); //Hiển thị dấu chấm cách hàng nghìn
    }

    public void SetValueStress(int newValueStress)
    {
        Color color;
        newValueStress = Mathf.Clamp(newValueStress, 0, 100);

        string status;

        if (newValueStress == 0)
        {
            status = "ASNAT";
            color = new Color(0.6f, 0f, 0f); // đỏ đậm
        }
        else if (newValueStress <= 20)
        {
            status = "Rất mệt";
            color = new Color(0.8f, 0f, 0f); // đỏ tươi
        }
        else if (newValueStress <= 40)
        {
            status = "Mệt mỏi";
            color = new Color(1f, 0.5f, 0f); // cam
        }
        else if (newValueStress <= 60)
        {
            status = "Bình thường";
            color = Color.yellow;
        }
        else if (newValueStress <= 80)
        {
            status = "Tốt";
            color = new Color(0.4f, 0.8f, 0.4f); // xanh nhạt
        }
        else // 81 - 100
        {
            status = "Rất tốt";
            color = Color.green;
        }

        StressText.SetText($"{status} ({newValueStress})");
        StressText.color = color;
    }

}
