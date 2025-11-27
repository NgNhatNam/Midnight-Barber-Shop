using UnityEngine;
using DPUtils.System.DateTime;

public class TimeChanger : MonoBehaviour
{
    private TimeManager timeManager;
    private Health health;

    private void Awake()
    {
        health = FindAnyObjectByType<Health>();
        timeManager = FindAnyObjectByType<TimeManager>();
    }

    public void Sleep()
    {
        health.HealMN(20);
        SetTime(20, 0); 
    }

    public void SetTime(int targetHour, int targetMinute)
    {
        var current = timeManager.GetCurrentDateTime();
        var targetToday = current;

        targetToday = targetToday.SetHour(targetHour).SetMinutes(targetMinute);

        // Không trừ máu ngày đầu tiên
        if (timeManager.GetCurrentDateTime().TotalNumDays != 1)
        {
            if (current.Hour < 6 && targetHour >= 6)
                health.Damage(10);
        }

        // Nếu target đã qua → chuyển sang ngày sau
        if (IsBefore(targetToday, current))
        {
            targetToday = targetToday.AddDays(1);
            health.Damage(10);
        }


        Apply(targetToday);
    }
    private bool IsBefore(DPUtils.System.DateTime.DateTime a, DPUtils.System.DateTime.DateTime b)
    {
        if (a.Year != b.Year) return a.Year < b.Year;
        if ((int)a.Season != (int)b.Season) return (int)a.Season < (int)b.Season;
        if (a.Date != b.Date) return a.Date < b.Date;
        if (a.Hour != b.Hour) return a.Hour < b.Hour;
        return a.Minutes < b.Minutes;
    }

    private void Apply(DPUtils.System.DateTime.DateTime newTime)
    {
        var field = typeof(TimeManager).GetField("DateTime",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        field.SetValue(timeManager, newTime);

        TimeManager.OnDateTimeChanged?.Invoke(newTime);
    }
}
