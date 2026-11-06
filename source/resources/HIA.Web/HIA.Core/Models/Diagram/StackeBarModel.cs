using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIA.Core.Models.Diagram
{
    public class StackeBarModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("showInLegend")]
        public bool ShowInLegend { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("axisYType")]
        public string AxisYType { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("dataPoints")]
        public List<DataPoints> DataPoints { get; set; }
    }
    public class DataPoints
    {
        [JsonProperty("y")]
        public double Y { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
    }
    public class DataPointsPie
    {
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("exploded")]
        public bool exploded { get; set; }
        
    }

    public class Report 
    { 
        public List<StackeBarModel> report1 { get; set; }
        public List<DataPointsPie> report2 { get; set; }
    }

    public class TotalReports
    {
        public int TotalFilesDate { get; set; }
        public int TotalFilesMonth { get; set; }
        public int TotalNoiTruDate { get; set; }
        public int TotalNoiTruMonth { get; set; }
        public int TotalRaVienDate { get; set; }
        public int TotalRaVienMonth { get; set; }
        public double? TongTienDate { get; set; }
        public double? TongTienMonth { get; set; }

        public Dictionary<string, IEnumerable<DataPoints>> ChartPoints { get; set; }

        public List<DataPointsPie> PieCharts { get; set; }
    }
}
