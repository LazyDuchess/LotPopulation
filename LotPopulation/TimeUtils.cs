using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace.Enums;

namespace LazyDuchess.LotPopulation
{
    public static class TimeUtils
    {
        public static DaysOfTheWeek Tomorrow => SimClock.DayAfter(SimClock.CurrentDayOfWeek);
        public static DaysOfTheWeek Today => SimClock.CurrentDayOfWeek;
        public static bool OnHoliday
        {
            get
            {
                if (HolidayManager.Instance != null)
                {
                    if (!HolidayManager.Instance.IsThisDayAHoliday(SimClock.CurrentTime()))
                        return true;
                }
                return false;
            }
        }
        public static bool IsTodayWeekendOrHoliday
        {
            get
            {
                if (IsWeekend(Today))
                    return true;
                if (OnHoliday)
                    return true;
                return false;
            }
        }
        public static bool IsTomorrowWeekendOrHoliday
        {
            get
            {
                if (IsWeekend(Tomorrow))
                    return true;
                if (HolidayManager.Instance != null)
                {
                    if (HolidayManager.Instance.HowManyDaysUntilAHoliday() == 1)
                        return true;
                }
                return false;
            }
        }
        public static bool IsNight => SimClock.IsTimeBetweenTimes(21, 5);
        public static bool IsMorning => SimClock.IsTimeBetweenTimes(6, 8);
        public static bool IsEarlyMorning => SimClock.IsTimeBetweenTimes(0, 6);
        public static bool IsGettingLate => SimClock.IsTimeBetweenTimes(19, 23);
        public static bool IsSummer => SeasonsManager.CurrentSeason == Season.Summer;
        public static bool IsWinter => SeasonsManager.CurrentSeason == Season.Winter;
        public static bool SnowOnGround
        {
            get
            {
                if (SeasonsManager.CurrentSnowLevel >= SnowLevel.Low)
                    return true;
                return false;
            }
        }
        
        public static bool IsTherePrecipitation
        {
            get
            {
                var cw = SeasonsManager.CurrentWeather;
                if (cw == Weather.BewitchingRain || cw == Weather.Rain || cw == Weather.Snow || cw == Weather.Hail)
                    return true;
                return false;
            }
        }

        public static bool Foggy { get
            {
                var cw = SeasonsManager.CurrentWeather;
                if (cw == Weather.Fog)
                    return true;
                return false;
            } }
        public static bool IsWeekend(DaysOfTheWeek day)
        {
            return day == DaysOfTheWeek.Saturday || day == DaysOfTheWeek.Sunday;
        }
    }
}
