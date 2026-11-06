using System;
using System.Collections.Generic;

namespace HIA.Core.Models.Diagram
{
    public class Summaryitem
    {
        public string MaCSKCB { get; set; }

        public string Hospital { get; set; }

        public int TotalUploads { get; set; }

        public bool isUploadFiles { get; set; }

        public bool isActive { get; set; }

        public DateTime FromDate { get; set; }
    }

    public class Summary : List<Summaryitem>
    {
        public DateTime SelectedDate { get; set; }
    }
}
