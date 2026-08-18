using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(string emailTo, string confirmationLink, string subject);
        Task<string> SendResetPasswordEmailAsync(string emailTo, string token, string controllerName, string reqUrl, string Subject);
        Task<bool> SendSimpleEmailAsync(string emailTo, string subject, string body);
    }
}