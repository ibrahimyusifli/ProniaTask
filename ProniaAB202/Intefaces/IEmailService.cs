namespace ProniaAB202.Intefaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailTo, string subject, string body, bool isHtml = false);
    }
}
