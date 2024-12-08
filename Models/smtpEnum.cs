namespace HCMSIU_SSPS.Models
{
    public class smtpEnum
    {
        public enum SmtpSettings
        {
            SmtpSettings,
            Host,
            Port,
            Username,
            Password
        }
        public enum NotifyTempData
        {
            NotifySuccess,
            NotifyFailure
        }
    }
}
