namespace SystemZglaszaniaUsterek.Models.Options
{
    public class SmtpOptions
    {
        public const string SectionName = "SmtpSettings";

        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? FromAddress { get; set; }

        public string FromDisplayName { get; set; } = "System Zgłaszania Usterek";
    }
}
