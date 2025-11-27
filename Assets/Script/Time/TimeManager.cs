using System;
using UnityEngine;
using UnityEngine.Events;

namespace DPUtils.System.DateTime
{
    public class TimeManager : MonoBehaviour
    {
        [Header("Date & Time Setting")]
        [Range(1, 28)]
        public int dateInMonth;
        [Range(1, 4)]
        public int season;
        [Range(1, 99)]
        public int year;
        [Range(0, 24)]
        public int hour;
        [Range(0, 6)]
        public int minutes;

        private DateTime DateTime;

        [Header("Tick Setting")]
        public int TickMinutesIncreased = 10;
        public float TimeBetweenTicks = 1;
        private float currentTimeBetweenTicks = 0;

        public static UnityAction<DateTime> OnDateTimeChanged;

        public static object CurrentDateTime { get; internal set; }

        private void Awake()
        {
            DateTime = new DateTime(dateInMonth, season - 1, year, hour, minutes * 10);

            Debug.Log($"New Year Day: {DateTime.NewYearsDay(2)}");
            Debug.Log($"Summer Solstice: {DateTime.SummerSolstice(4)}");
            Debug.Log($"Pimpkin Harvest: {DateTime.PumpkinHarvest(10)}");
            Debug.Log($"Start of a Season: {DateTime.StartOfSeason(1, 3)}");
            Debug.Log($"Start of Winter: {DateTime.StartOfWinter(3)}");
        }

        private void Start()
        {
            OnDateTimeChanged?.Invoke(DateTime);
        }

        private void Update()
        {
            currentTimeBetweenTicks += Time.deltaTime;
            if (currentTimeBetweenTicks >= TimeBetweenTicks)
            {
                currentTimeBetweenTicks = 0;
                Tick();
            }
        }
        //-------------------------------------------

        public DPUtils.System.DateTime.DateTime GetCurrentDateTime()
        {
            return DateTime;
        }

        //-------------------------------------------

        void Tick()
        {
            AdvanceTime();
        }

        void AdvanceTime()
        {
            DateTime.AdvanceMinutes(TickMinutesIncreased);
            OnDateTimeChanged?.Invoke(DateTime);
        }
    }

    [Serializable]
    public struct DateTime
    {
        #region Fields
        private Days day;
        private int date;
        private int year;

        private int hour;
        private int minutes;

        private Season season;

        private int totalNumDays;
        private int totalNumWeeks;

        public DateTime SetHour(int newHour)
        {
            this.hour = newHour;
            return this;
        }

        public DateTime SetMinutes(int newMinutes)
        {
            this.minutes = newMinutes;
            return this;
        }

        #endregion

        #region Properties
        public Days Day => day;
        public int Date => date;
        public int Hour => hour;
        public int Minutes => minutes;
        public  Season Season => season;
        public int Year => year;
        public int TotalNumDays => totalNumDays;
        public int TotalNumWeeks => totalNumWeeks;
        public int CurrentWeek   => totalNumWeeks % 16 == 0 ? 16 : totalNumWeeks % 16 ;
        #endregion

        #region Constructors

        public DateTime(int date, int season, int year, int hour, int minutes)
        {
            this.day = (Days)(date % 7);
            if (day == 0) day = (Days)7;
            this.date = date;
            this.season = (Season)season;
            this.year = year;

            this.hour = hour; 
            this.minutes = minutes;

            totalNumDays = (int)this.season > 0 ? date + (28 * (int)this.season) : date;
            totalNumDays = year > 1 ? totalNumDays + (112 * (year - 1 )) : totalNumDays;

            totalNumWeeks = 1 + totalNumDays / 7 ;

        }
        #endregion

        #region Sleep Advanced
        public DateTime AddDays(int daysToAdd)
        {
            DateTime result = this;

            for (int i = 0; i < daysToAdd; i++)
            {
                // Tăng ngày
                result.date++;

                // Tăng thứ
                if (result.day + 1 > (Days)7)
                {
                    result.day = (Days)1;
                    result.totalNumWeeks++;
                }
                else
                {
                    result.day++;
                }

                // Nếu quá 28 ngày thì sang mùa
                if (result.date > 28)
                {
                    result.date = 1;

                    if (result.season == Season.Winter)
                    {
                        result.season = Season.Spring;
                        result.year++;
                    }
                    else
                    {
                        result.season++;
                    }
                }

                result.totalNumDays++;
            }

            return result;
        }

        #endregion
        #region Time Advancement
        public void AdvanceMinutes(int SecondsToAdvenceBy)
        {
            if(minutes + SecondsToAdvenceBy >= 60)
            {
                minutes = (minutes + SecondsToAdvenceBy) % 60;
                AdvanceHour();
            }
            else
            {
                minutes += SecondsToAdvenceBy;
            }
        }
        private void AdvanceHour()
        {
            if((hour + 1) == 24)
            {
                hour = 0;
                AdvanceDay();
            }
            else
            {
                hour++;
            }
        }

        private void AdvanceDay()
        {
            if(day + 1 > (Days)7)
            {
                day = (Days)1;
                totalNumWeeks++;
            }
            else
            {
                day++;
            }

            date++;

            if(date % 29 == 0)
            {
                AdvanceSeason();
                date = 1;
            }
            totalNumDays++;
        }

        private void AdvanceSeason()
        {
            if (Season == Season.Winter)
            {
                season = Season.Spring;
                AdvanceYear();
            }
            else season++;
        }

        private void AdvanceYear()
        {
            date = 1;
            year++;
        }

        #endregion

        #region Bool Checks

        public bool DaySummary()
        {
            return hour >= 6 || hour  <= 6.30f;
        }

        public bool TimeToOpen()
        {
            return hour >= 21 || hour < 6;
        }

        public bool SoulTime()
        {
            return hour >= 1 && hour < 6;

        }
        public bool IsNight()
        {
            return hour > 18 || hour < 6;
        }
        public bool IsMorning()
        {
            return hour > 6 || hour <= 12;
        }
        public bool IsAfterNoon()
        {
            return hour > 12 || hour < 18;
        }
        public bool IsWeekend()
        {
            return day > Days.CN ? true : false;
        }

        public bool IsParticularDay(Days _day)
        {
            return day == _day;
        }

        #endregion

        #region Key Dates

        public DateTime NewYearsDay(int year)
        {
            if (year == 0) year = 1;
            return new DateTime(1, 0, year, 6, 0);
        }

        public DateTime SummerSolstice(int year)
        {
            if (year == 0) year = 1;
            return new DateTime(28, 1, year, 6, 0);
        }

        public DateTime PumpkinHarvest(int year)
        {
            if (year == 0) year = 1;
            return new DateTime(28, 2, year, 6, 0);
        }

        #endregion

        #region Start Of Season
        public DateTime StartOfSeason(int season, int year)
        {
                
            return new DateTime(1, season, year, 6, 0);
        }

        public DateTime StartOfSpring( int year)
        {
            return StartOfSeason(0, year);
        }

        public DateTime StartOfSummer(int year)
        {
            return StartOfSeason(1, year);
        }

        public DateTime StartOfAutumn(int year)
        {
            return StartOfSeason(2, year);
        }

        public DateTime StartOfWinter(int year)
        {
            return StartOfSeason(3, year);
        }


        #endregion

        #region To Strings

        public override string ToString()
        {
            return $"Date: {DateToString()} Season: {season} Time: {TimeToString()}" +
                $"\nTotal Days: {totalNumDays} | Total Week: {totalNumWeeks}";
        }

        public string DateToString()
        {
            return $"{Day} {Date} | Năm {Year.ToString("D2")}";
        }

        public string TimeToString()
        {
            int adjustedHour = 0;

            if (hour == 0)
            {
                adjustedHour = 12;
            }
            /*else if (hour == 24)
            {
                adjustedHour = 12;
            }*/
            else if (hour >= 13)
            {
                adjustedHour = hour - 12;
            }
            else
            {
                adjustedHour = hour;
            }

            string AmPm = hour == 0 || hour < 12 ? "AM" : "PM";

            return $"{adjustedHour.ToString("D2")}:{minutes.ToString("D2")} {AmPm}";

        }


        #endregion
    }
    [Serializable]
    public enum Days
    {
        NULL = 0,
        T2 = 1,
        T3 = 2,
        T4 = 3,
        T5 = 4,
        T6 = 5,
        T7 = 6,
        CN = 7
    }
    [Serializable]
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }

    /*
    [Serializable]
    public enum Days
    {
        NULL = 0,
        Mon = 1,
        Tue = 2,    
        Wed = 3,
        Thu = 4,
        Fri = 5,
        Sat = 6,
        Sun = 7
    }
    [Serializable]
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
    */
}

