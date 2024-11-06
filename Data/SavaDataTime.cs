using System;

namespace _Development.Scripts.BeginnersFest.Data
{
    [Serializable]
    public class SavaDataTime
    {
        public int Day;
        public int Month;
        public int Year;

        public SavaDataTime(int day, int month, int year)
        {
            Day = day;
            Month = month;
            Year = year;
        }
    }
}