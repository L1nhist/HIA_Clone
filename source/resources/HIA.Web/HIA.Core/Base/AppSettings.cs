namespace HIA.Core.Base
{
    public class AppSettings
    {
        public EmailConfig EmailConfig { get; set; }
        public BackgroundServices BackgroundServices { get; set; }
    }
    #region EmailConfig
    public class EmailConfig
    {
        public string Sender { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }
        public NewUser NewUser { get; set; }
        public ResetPass ResetPass { get; set; }
    }
    public class NewUser
    {
        public string Subject { get; set; }
    }
    public class ResetPass
    {
        public string Subject { get; set; }
    }
    #endregion

    #region BackgroundServices
    public class BackgroundServices
    {
        public VerifyXML VerifyXML { get; set; }
    }
    public class VerifyXML
    {
        public int MinuteVerifyTime { get; set; }
        public int PercentItemInTime { get; set; }
    }
    #endregion
}
