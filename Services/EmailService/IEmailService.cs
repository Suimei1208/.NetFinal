using NetTechnology_Final.Models;

namespace NetTechnology_Final.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailDto request, Accounts accounts, string link);
        
    }
}
