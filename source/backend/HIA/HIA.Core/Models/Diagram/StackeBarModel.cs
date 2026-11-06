using System.Text.Json.Serialization;

namespace HIA.Core.Models.Diagram;

public class StackeBarModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("showInLegend")]
    public bool ShowInLegend { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("axisYType")]
    public string AxisYType { get; set; } = string.Empty;
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
    [JsonPropertyName("dataPoints")]
    public List<DataPoints> DataPoints { get; set; } = new();
}

public class DataPoints
{
    [JsonPropertyName("y")]
    public double Y { get; set; }
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}

public class DataPointsPie
{
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("name")]
    public string name { get; set; } = string.Empty;
    [JsonPropertyName("exploded")]
    public bool exploded { get; set; }
}

public class Report
{
    public List<StackeBarModel> report1 { get; set; } = new();
    public List<DataPointsPie> report2 { get; set; } = new();
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

    public Dictionary<string, IEnumerable<DataPoints>> ChartPoints { get; set; } = new();

    public List<DataPointsPie> PieCharts { get; set; } = new();
}
