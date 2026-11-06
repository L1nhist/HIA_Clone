namespace HIA.Core.Base;

public static class RateLimit
{
    protected class RateLimitItem
    {
        public int Concurent { get; set; }
        public DateTime Since { get; set; }
        public DateTime Then { get; set; }

        public RateLimitItem(DateTime since, DateTime then)
        {
            Concurent = 1;
            Since = since;
            Then = then;
        }
    }

    protected class RateLimitCondition
    {
        //URL cua API
        public string ReqPath { get; set; }

        //Khoang thoi gian request den tinh theo giay (s)
        public int Period { get; set; }

        //So luong request trong khoang thoi gian
        public int Times { get; set; }

        public Dictionary<string, RateLimitItem> Items { get; set; }

        public RateLimitCondition(string request = "", int period = 1, int times = 5)
        {
            ReqPath = (request ?? "").Trim().ToLower();
            Period = period;
            Times = times;
            Items = new Dictionary<string, RateLimitItem>();
        }

        public bool ReachLimited(string token)
        {
            RateLimitItem? item = Items.TryGetValue(token, out var found) ? found : null;
            if (item == null)
            {
                DateTime newSince = DateTime.Now;
                DateTime newThen = newSince.AddSeconds(Period);
                Items.Add(token, new RateLimitItem(newSince, newThen));
                return false;
            }

            if (item.Concurent + 1 > Times)
            {
                return true;
            }

            if (item.Then < DateTime.Now)
            {
                item.Since = DateTime.Now;
                item.Then = item.Since.AddSeconds(Period);
                item.Concurent = 1;
            }
            else
            {
                item.Concurent++;
            }

            return false;
        }
    }

    private static readonly List<RateLimitCondition> _Conditions;

    static RateLimit()
    {
        _Conditions =
        [
            new RateLimitCondition("/api/getfilesbydate", 1, 5)
        ];
    }

    public static bool IsLimited(string? request, string? token)
    {
        request = (request ?? "").Trim().ToLower();
        token = (token ?? "").Trim().ToLower();
        if (request == "" && token == "")
        {
            return true;
        }

        foreach (RateLimitCondition cond in _Conditions)
        {
            if (cond.ReqPath == "*" || cond.ReqPath == request)
            {
                if (cond.ReachLimited(token)) return true;
            }
        }

        return false;
    }
}
