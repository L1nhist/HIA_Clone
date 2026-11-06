using System.Globalization;

namespace HIA.Core.Base;

public class TimePeriod
{
    public class TimeSlot
    {
        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public bool IsValid => Start != null && End != null;

        public TimeSlot(string? slot)
        {
            if (slot == null || slot.Trim() == "" || !slot.Contains('-')) return;

            string[] times = slot.Split('-');
            if (times.Length != 2) return;

            Start = Parse(times[0]);
            End = Parse(times[1]);

            if (Start != null && End == null)
            {
                End = Start.Value.Date.AddDays(1).AddMilliseconds(-1);
            }
            else if (Start == null && End != null)
            {
                Start = End.Value.Date;
            }
            else
            {
                End = new DateTime(Start!.Value.Year, Start.Value.Month, Start.Value.Day, End!.Value.Hour, End.Value.Minute, End.Value.Second);
            }
        }

        private static DateTime? Parse(string time)
        {
            try
            {
                DateTime d = DateTime.ParseExact(time, new string[] { "HH:mm", "HH:mm:ss", "H:m", "H:m:s" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                return d;
            }
            catch { }

            return null;
        }

        public bool InRange(DateTime? d)
        {
            if (d == null || !IsValid) return false;

            //Linhist changed 05/07/24
            var d1 = new DateTime(Start!.Value.Year, Start.Value.Month, Start.Value.Day, d.Value.Hour, d.Value.Minute, d.Value.Second, d.Value.Millisecond);
            return (Start.Value <= d1 && d1 <= End!.Value);
        }
    }

    public class DateSlot
    {
        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public bool IsValid => Start != null && End != null;

        public DateSlot(string? slot)
        {
            if (slot == null || slot.Trim() == "" || !slot.Contains('-')) return;

            string[] times = slot.Split('-');
            if (times.Length != 2) return;

            Start = Parse(times[0]);
            End = Parse(times[1]);

            if (Start != null && End == null)
            {
                End = DateTime.Now.Date.AddDays(1).AddMilliseconds(-1);
            }
            else if (Start == null && End != null)
            {
                Start = DateTime.Now.Date;
            }
        }

        private static DateTime? Parse(string time)
        {
            try
            {
                if (time.Contains('/'))
                {
                    DateTime d = DateTime.ParseExact(time, new string[] { "d/M", "dd/MM", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                    return (d.Year == DateTime.Now.Year ? d : new DateTime(DateTime.Now.Year, d.Month, d.Day));
                }
            }
            catch { }

            return null;
        }

        public bool InRange(DateTime? d)
        {
            if (d == null || !IsValid) return false;

            DateTime d1 = new DateTime(DateTime.Now.Year, d.Value.Month, d.Value.Day);
            return (Start <= d1 && d1 <= End);
        }
    }

    private readonly string _origin = "";

    public static TimePeriod OfficialTime => new TimePeriod("[7:00-12:00;13:30-17:00]");

    private readonly List<DateSlot> _SummerDates = new List<DateSlot>();

    private readonly List<TimeSlot> _SummerTimes = new List<TimeSlot>();

    private readonly List<TimeSlot> _WinterTimes = new List<TimeSlot>();

    public bool IsValid => _WinterTimes.Any(s => s.IsValid) && (!_SummerDates.Any() || _SummerDates.Any(s => s.IsValid));

    public TimePeriod(string? period)
    {
        _origin = period ?? "";
        if (period == null || period.Trim() == "" || !period.Contains('[') || !period.Contains(']')) return;

        string[] pers = period.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        if (pers.Length == 1 || pers.Length >= 3)
        {
            ParseTimes(pers[0], _WinterTimes);
        }
        if (pers.Length >= 3)
        {
            ParseTimes(pers[1], _SummerTimes);
            ParseDates(pers[2], _SummerDates);
        }
    }

    public void ParseTimes(string period, List<TimeSlot> lst)
    {
        if (!period.Contains('-')) return;

        string[] slots = period.Split(';');
        foreach (string slot in slots)
        {
            lst.Add(new TimeSlot(slot.Trim()));
        }
    }

    public void ParseDates(string period, List<DateSlot> lst)
    {
        if (!period.Contains('-')) return;

        string[] slots = period.Split(';');
        foreach (string slot in slots)
        {
            lst.Add(new DateSlot(slot.Trim()));
        }
    }

    public bool IsSummer(DateTime? d)
    {
        if (d == null || _SummerDates.Count == 0) return false;

        foreach (DateSlot slot in _SummerDates)
        {
            if (slot.IsValid && slot.InRange(d)) return true;
        }

        return false;
    }

    public bool InRange(DateTime? d)
    {
        if (d == null) return false;
        if (IsSummer(d) && _SummerTimes.Count > 0)
        {
            foreach (TimeSlot slot in _SummerTimes)
            {
                if (slot.IsValid && slot.InRange(d)) return true;
            }
        }
        else if (_WinterTimes.Count > 0)
        {
            foreach (TimeSlot slot in _WinterTimes)
            {
                if (slot.IsValid && slot.InRange(d)) return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return _origin;
    }
}
