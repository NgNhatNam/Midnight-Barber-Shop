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

    public void SleepWithDuration(int hours, int manaRecovery)
    {
        // 1. Hồi phục Mana
        health.HealMN(manaRecovery);

        // 2. Nhảy thời gian
        var current = timeManager.GetCurrentDateTime();

        int newHour = current.Hour + hours;
        int daysToAdd = 0;

        // Nếu tổng số giờ vượt quá 24, tính toán số ngày cần thêm
        if (newHour >= 24)
        {
            daysToAdd = newHour / 24;
            newHour = newHour % 24;
        }

        // Tạo bản sao thời gian mới
        var targetTime = current;

        // Nếu có nhảy ngày (ví dụ ngủ 12h từ lúc 20h tối -> 8h sáng hôm sau)
        if (daysToAdd > 0)
        {
            targetTime = targetTime.AddDays(daysToAdd);
        }

        // Gán lại giờ mới (đã nằm trong khoảng 0-23)
        targetTime = targetTime.SetHour(newHour);

        // Áp dụng thời gian mới vào hệ thống
        Apply(targetTime);

        Debug.Log($"Đã ngủ {hours} giờ. Ngày mới: {targetTime.Date}, Giờ mới: {targetTime.Hour}");
    }

    public void Sleep()
    {
        health.HealMN(20);
        SetTime(6, 0); 
    }

    public void SetTime(int targetHour, int targetMinute)
    {
        var current = timeManager.GetCurrentDateTime();
        var targetToday = current;

        targetToday = targetToday.SetHour(targetHour).SetMinutes(targetMinute);

        /*/ Không trừ máu ngày đầu tiên
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
        }*/


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
