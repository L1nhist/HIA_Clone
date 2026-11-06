namespace HIA.Core.Base;

public class AppSettings
{
    public EmailConfig EmailConfig { get; set; } = new();
    public BackgroundServices BackgroundServices { get; set; } = new();
}

#region EmailConfig
public class EmailConfig
{
    public string Sender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public int Port { get; set; }
    public NewUser NewUser { get; set; } = new();
    public ResetPass ResetPass { get; set; } = new();
}

public class NewUser
{
    public string Subject { get; set; } = string.Empty;
}

public class ResetPass
{
    public string Subject { get; set; } = string.Empty;
}
#endregion

#region BackgroundServices
public class BackgroundServices
{
    public VerifyXML VerifyXML { get; set; } = new();
}

public class VerifyXML
{
    public int MinuteVerifyTime { get; set; }
    public int PercentItemInTime { get; set; }
}
#endregion
