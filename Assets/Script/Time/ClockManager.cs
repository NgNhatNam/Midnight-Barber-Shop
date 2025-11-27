using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DPUtils.System.DateTime;
using UnityEngine.Rendering.Universal;


public class ClockManager : MonoBehaviour
{

    //public RectTransform ClockFace;
    public TextMeshProUGUI Date, Time, Season, Week;

    public Image weatherSprite;
    public Sprite[] weartherSprites;

    private float startingRotation;

    public Light2D sunlight;
    public float nightIntensity;
    public float dayIntensity;
    public AnimationCurve dayNightCurve;


    private void Start()
    {


        //startingRotation = ClockFace.localEulerAngles.z;
    }

    private void OnEnable()
    {
        TimeManager.OnDateTimeChanged += UpdateDateTime;
    }

    private void OnDisable()
    {
        TimeManager.OnDateTimeChanged -= UpdateDateTime;
    }
    
    private void UpdateDateTime(DateTime dateTime)
    {
        Date.text = dateTime.DateToString();
        Time.text = dateTime.TimeToString();
        Season.text = dateTime.Season.ToString();
        Week.text = $"Tuần: {dateTime.CurrentWeek}";
        //weatherSprite.sprite = weatherSprites[(int)WeatherManager.currentWeather];
    
        float t = (float)dateTime.Hour /24f ;

        //float newRotation = Mathf.Lerp(0, 360, t);
        //ClockFace.localEulerAngles = new Vector3 (0, 0, newRotation + startingRotation);

        

        float dayNightT = dayNightCurve.Evaluate(t);

        sunlight.intensity = Mathf.Lerp(dayIntensity, nightIntensity,dayNightT);

    }
}
