using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;
    [Header("HealthBar")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;
    [SerializeField] public TMP_Text hpText;

    private float maxRightMask;
    private float initialRightMask;




    private void Start()
    {
        // x = left, w = top, y = bottom, z = right
        maxRightMask = barRect.rect.width - mask.padding.x - mask.padding.z;

        hpText.SetText($"{health.HP}/{health.MaxHP}");

        initialRightMask = mask.padding.z;
    }

    public void SetValueRightHP(int newValue)
    {
        var targetWidth = newValue * maxRightMask / health.MaxHP;
        var newRightMask = maxRightMask + initialRightMask - targetWidth;

        var padding = mask.padding;
        padding.z = newRightMask;
        mask.padding = padding * 4.5f;

        hpText.SetText($"{newValue}/{health.MaxHP}");
    }


}
