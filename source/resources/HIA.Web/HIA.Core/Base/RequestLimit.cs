using System;
using System.Collections.Generic;
using System.Linq;

namespace HIA.Core.Base
{
    public static class RequestLimit
    {
        protected class RequestLimitCondition
        {
            //URL cua API
            public string ReqPath { get; set; }

            //Dia chi IP cua Khach
            public string[] UserIP { get; set; }

            //Khoang thoi gian request den
            public int Period { get; set; }

            //So luong request trong khoang thoi gian
            public int Times { get; set; }

            public DateTime? Since { get; set; }
            public DateTime? Then { get; set; }
            public int Concurent { get; set; }

            public RequestLimitCondition() { }

            public RequestLimitCondition(string request, string userAgent, int period, int times)
            {
                ReqPath = (request ?? "").Trim().ToLower();
                UserIP = (userAgent ?? "").Trim().ToLower().Split(";", StringSplitOptions.RemoveEmptyEntries);
                Period = period;
                Times = times;
            }

            public bool ReachLimited()
            {
                if (Then == null || !Then.HasValue || Then.Value < DateTime.Now)
                {
                    Since = DateTime.Now;
                    Then = Since.Value.AddSeconds(Period);
                    Concurent = 1;
                }
                else if (Concurent + 1 > Times)
                {
                    return true;
                }
                else
                {
                    Concurent++;
                }

                return false;
            }
        }

        private static readonly List<RequestLimitCondition> _Conditions = null;

        static RequestLimit()
        {
            _Conditions = new List<RequestLimitCondition>
            {
                new RequestLimitCondition("/api/getfilesbydate", "0.0.0.1;113.190.252.24;20.212.22.28;125.235.61.17;103.124.60.121;172.16.101.188", 1, 5)
            };
        }

        public static bool IsLimited(string request, string userAgent)
        {
            request = (request ?? "").Trim().ToLower();
            userAgent = (userAgent ?? "").Trim().ToLower();
            if (request == "" && userAgent == "")
            {
                return true;
            }

            foreach (RequestLimitCondition cond in _Conditions)
            {
                if (cond.ReqPath == "*" || cond.ReqPath == request)
                {
                    if (!cond.UserIP.Any(u => u == "*" || u == userAgent)) return true;
                    if (cond.ReachLimited()) return true;
                }
            }

            return false;
        }
    }
}