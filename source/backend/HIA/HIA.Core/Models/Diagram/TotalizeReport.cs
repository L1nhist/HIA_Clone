namespace HIA.Core.Models.Diagram;

public class TotalizeItem
{
    public string MaCSKCB { get; set; } = string.Empty;

    public string Hospital { get; set; } = string.Empty;

    public int TotalFiles { get; set; }

    public DateTime FromDate { get; set; }
}

public class TotalizeReport : List<TotalizeItem>
{
    public DateTime SelectedDate { get; set; }
}
