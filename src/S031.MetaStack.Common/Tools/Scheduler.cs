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
        static readonly string dayFormat = "yyyy-MM-dd";
        static readonly string timeFormat = "HH:mm:ss";

        DateTime _createTime = vbo.Date();
        DateTime _lastStartTime = DateTime.MinValue;
        DateTime _nextStartTime = DateTime.MinValue;

        string _dayStartTime = "00:00:00";
        string _dayEndTime = "23:59:00";
		readonly int _qt;
		readonly UnitOfQt _unitOfQt;
        string _weekDayMask = "1,2,3,4,5,6,7";

        public string DayStartTime
        {
            get
            {
                return _dayStartTime;
            }

            set
            {
				if (DateTime.TryParse(value, out DateTime d))
					_dayStartTime = d.ToString(timeFormat);
			}
        }

        public string DayEndTime
        {
            get
            {
                return _dayEndTime;
            }

            set
            {
				if (DateTime.TryParse(value, out DateTime d))
					_dayEndTime = d.ToString(timeFormat);
			}
        }

        public string WeekDayMask
        {
            get
            {
                return _weekDayMask;
            }

            set
            {
                if (!value.IsEmpty())
                    _weekDayMask = value;
            }
        }

        public DateTime CreateTime
        {
            get
            {
                return _createTime;
            }

            set
            {
                _createTime = value;
            }
        }

        public DateTime LastStartTime
        {
            get
            {
                return _lastStartTime;
            }
        }

        public DateTime NextStartTime
        {
            get
            {
                return _nextStartTime;
            }

        }

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
            DateTime nextStartTime = dayStartTime > _createTime ? dayStartTime : _createTime;

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
            if (_unitOfQt == UnitOfQt.Year)
                nextStartTime = dStart.AddYears(_qt);
            else if (_unitOfQt == UnitOfQt.Month)
                nextStartTime = dStart.AddMonths(_qt);
            else if (_unitOfQt == UnitOfQt.Day)
                nextStartTime = dStart.AddDays(_qt);
            else
                nextStartTime = dStart.AddSeconds(_qt * (int)_unitOfQt);
            return nextStartTime;
        }

        private bool inDay()
        {
            return ((int)_unitOfQt > 0 && _unitOfQt < UnitOfQt.Day);
        }

        public virtual void Tick()
        {
            _lastStartTime = DateTime.Now;
            _nextStartTime = GetNextStartTime(LastStartTime);
        }
    }
}
