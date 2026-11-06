using System;
using System.Collections.Generic;

namespace HIA.Core.Models.Diagram
{
    public class TotalizeItem
    {
        public string MaCSKCB { get; set; }

        public string Hospital { get; set; }

        public int TotalFiles { get; set; }

        public DateTime FromDate { get; set; }
    }

    public class TotalizeReport : List<TotalizeItem>
    {
        public DateTime SelectedDate { get; set; }
    }
}
