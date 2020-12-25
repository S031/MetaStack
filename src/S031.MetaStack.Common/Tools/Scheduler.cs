using System;

namespace S031.MetaStack.Common
{
    public enum UnitOfQt
    {
        Year = -2,
        Month = -1,
        Day = 86400,
        Hour = 3600,
        Minute = 60,
        Second = 1
    }
    public class Scheduler
    {
        const string dayFormat = "yyyy-MM-dd";
        const string timeFormat = "HH:mm:ss";
		string _dayStartTime = "00:00:00";
        string _dayEndTime = "23:59:00";
		readonly int _qt;
		readonly UnitOfQt _unitOfQt;
        string _weekDayMask = "1,2,3,4,5,6,7";

		public string DayStartTime
		{
			get => _dayStartTime;

			set
			{
				if (DateTime.TryParse(value, out DateTime d))
					_dayStartTime = d.ToString(timeFormat);
			}
		}

		public string DayEndTime
		{
			get => _dayEndTime;

			set
			{
				if (DateTime.TryParse(value, out DateTime d))
					_dayEndTime = d.ToString(timeFormat);
			}
		}

		public string WeekDayMask
		{
			get => _weekDayMask;

			set
			{
				if (!value.IsEmpty())
					_weekDayMask = value;
			}
		}

		public DateTime CreateTime { get; set; } = vbo.Date();

		public DateTime LastStartTime { get; private set; } = DateTime.MinValue;

		public DateTime NextStartTime { get; private set; } = DateTime.MinValue;

		public Scheduler(int qt, UnitOfQt unitOfQt)
        {
            _qt = qt;
            _unitOfQt = unitOfQt;
        }

        public DateTime GetNextStartTime()
        {
            return GetNextStartTime(DateTime.Now);
        }

        public DateTime GetNextStartTime(DateTime startTime)
        {

            DateTime dayStartTime = DateTime.Parse(startTime.ToString(dayFormat) + " " + _dayStartTime);
            DateTime dayEndTime = DateTime.Parse(startTime.ToString(dayFormat) + " " + _dayEndTime);
            DateTime nextStartTime = dayStartTime > CreateTime ? dayStartTime : CreateTime;

            for (; nextStartTime <= startTime;)
            {
                if (!inDay())
                    nextStartTime = increment(dayStartTime);
                else
                {
                    nextStartTime = increment(nextStartTime);
                    if (nextStartTime > dayEndTime)
                        nextStartTime = dayStartTime.AddDays(1);
                }
            }
            if (!_weekDayMask.Contains(((int)nextStartTime.DayOfWeek + 1).ToString()))
            {
                nextStartTime = dayStartTime.AddDays(1);
                for (; !_weekDayMask.Contains(((int)nextStartTime.DayOfWeek + 1).ToString());)
                    if (!inDay())
                        nextStartTime = increment(nextStartTime);
                    else
                        nextStartTime = nextStartTime.AddDays(1);

            }
            return nextStartTime;
        }

        private DateTime increment(DateTime dStart)
        {
            DateTime nextStartTime;
			switch (_unitOfQt)
			{
				case UnitOfQt.Year:
					nextStartTime = dStart.AddYears(_qt);
					break;
				case UnitOfQt.Month:
					nextStartTime = dStart.AddMonths(_qt);
					break;
				case UnitOfQt.Day:
					nextStartTime = dStart.AddDays(_qt);
					break;
				default:
					nextStartTime = dStart.AddSeconds(_qt * (int)_unitOfQt);
					break;
			}
			return nextStartTime;
        }

        private bool inDay()
        {
            return ((int)_unitOfQt > 0 && _unitOfQt < UnitOfQt.Day);
        }

        public virtual void Tick()
        {
            LastStartTime = DateTime.Now;
            NextStartTime = GetNextStartTime(LastStartTime);
        }
    }
}
