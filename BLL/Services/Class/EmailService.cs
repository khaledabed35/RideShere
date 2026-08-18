using BLL.Helper;
using BLL.Services.Interface;
using DAL.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace BLL.Services.Class
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSetting;
        private readonly UserManager<App_User> _userManager;

        public EmailService(
           IOptions<MailSettings> mailSetting, UserManager<App_User> userManager)
        {
            _mailSetting = mailSetting.Value;
            _userManager = userManager;
        }
     

        public async Task<string> SendEmailAsync(string emailTo, string confirmationLink, string subject)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(emailTo);
                if (user is null)
                    return "Email is incorrect.";

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_mailSetting.displayname, _mailSetting.Email));
                email.To.Add(MailboxAddress.Parse(emailTo));
                email.Subject = subject;
                email.Sender = MailboxAddress.Parse(_mailSetting.Email);

                var builder = new BodyBuilder
                {
                    HtmlBody = $@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        </head>
        <body style=""margin:0;padding:0;background-color:#f4f7f6;font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
            <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""table-layout:fixed;background-color:#f4f7f6;padding:40px 0;"">
                <tr>
                    <td align=""center"">
                        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 15px rgba(0,0,0,0.08);"">
                            <!-- Header -->
                            <tr>
                                <td style=""background:linear-gradient(135deg, #0d6efd 0%, #0043a8 100%);padding:30px;text-align:center;"">
                                    <h1 style=""color:#ffffff;margin:0;font-size:24px;font-weight:700;letter-spacing:0.5px;"">RideShere</h1>
                                </td>
                            </tr>
                            <!-- Body Content -->
                            <tr>
                                <td style=""padding:40px 30px;"">
                                    <h2 style=""color:#333333;margin-top:0;font-size:20px;font-weight:600;"">Confirm Your Email</h2>
                                    <p style=""font-size:15px;color:#555555;line-height:1.6;margin-bottom:20px;"">
                                        Hello <strong>{user.UserName}</strong>,
                                    </p>
                                    <p style=""font-size:15px;color:#555555;line-height:1.6;margin-bottom:30px;"">
                                        Thank you for registering with <strong>RideShere</strong>. Please confirm your email address by clicking the secure button below.
                                    </p>
                                    <!-- Button -->
                                    <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                        <tr>
                                            <td align=""center"" style=""padding:10px 0 30px 0;"">
                                                <a href=""{confirmationLink}"" target=""_blank"" style=""background-color:#0d6efd;color:#ffffff;padding:14px 32px;text-decoration:none;border-radius:6px;font-weight:600;font-size:16px;display:inline-block;box-shadow:0 4px 10px rgba(13,110,253,0.3);"">Confirm Email</a>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style=""font-size:13px;color:#888888;line-height:1.5;margin-bottom:0;"">
                                        If you didn't create this account, you can safely ignore this email.
                                    </p>
                                </td>
                            </tr>
                            <!-- Footer -->
                            <tr>
                                <td style=""background-color:#fafbfc;padding:20px;text-align:center;border-top:1px solid #eeeeee;"">
                                    <p style=""font-size:12px;color:#999999;margin:0;"">
                                        &copy; 2026 RideShere. All Rights Reserved.
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>"
                };


                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_mailSetting.host, int.Parse(_mailSetting.port), SecureSocketOptions.SslOnConnect);
                await smtp.AuthenticateAsync(_mailSetting.Email, _mailSetting.password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return string.Empty;
            }
    catch(Exception ex)
        {
                return ex.Message;
            }
        }


        public async Task<string> SendResetPasswordEmailAsync(string emailTo, string token, string controllerName, string reqUrl, string subject)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(emailTo);

                if (user is null)
                    return "Email is incorrect.";

                var resetLink = $"{reqUrl}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_mailSetting.displayname, _mailSetting.Email));
                email.To.Add(MailboxAddress.Parse(emailTo));
                email.Subject = subject;
                email.Sender = MailboxAddress.Parse(_mailSetting.Email);

                var builder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    </head>
                    <body style=""margin:0;padding:0;background-color:#f4f7f6;font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
                        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""table-layout:fixed;background-color:#f4f7f6;padding:40px 0;"">
                            <tr>
                                <td align=""center"">
                                    <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 15px rgba(0,0,0,0.08);"">
                                        <!-- Header -->
                                        <tr>
                                            <td style=""background:linear-gradient(135deg, #dc3545 0%, #b02a37 100%);padding:30px;text-align:center;"">
                                                <h1 style=""color:#ffffff;margin:0;font-size:24px;font-weight:700;letter-spacing:0.5px;"">RideShere</h1>
                                            </td>
                                        </tr>
                                        <!-- Body Content -->
                                        <tr>
                                            <td style=""padding:40px 30px;"">
                                                <h2 style=""color:#333333;margin-top:0;font-size:20px;font-weight:600;"">Reset Your Password</h2>
                                                <p style=""font-size:15px;color:#555555;line-height:1.6;margin-bottom:20px;"">
                                                    Hello <strong>{user.UserName}</strong>,
                                                </p>
                                                <p style=""font-size:15px;color:#555555;line-height:1.6;margin-bottom:20px;"">
                                                    We received a request to reset the password for your <strong>RideShere</strong> account.
                                                </p>
                                                <!-- Button -->
                                                <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                                    <tr>
                                                        <td align=""center"" style=""padding:10px 0 30px 0;"">
                                                            <a href=""{resetLink}"" target=""_blank"" style=""background-color:#dc3545;color:#ffffff;padding:14px 32px;text-decoration:none;border-radius:6px;font-weight:600;font-size:16px;display:inline-block;box-shadow:0 4px 10px rgba(220,53,69,0.3);"">Reset Password</a>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <p style=""font-size:13px;color:#888888;line-height:1.5;margin-bottom:0;"">
                                                    If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
                                                </p>
                                            </td>
                                        </tr>
                                        <!-- Footer -->
                                        <tr>
                                            <td style=""background-color:#fafbfc;padding:20px;text-align:center;border-top:1px solid #eeeeee;"">
                                                <p style=""font-size:12px;color:#999999;margin:0;"">
                                                    &copy; 2026 RideShere. All Rights Reserved.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </body>
                    </html>"
                };

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                Console.WriteLine($"Email: {_mailSetting.Email}");
                Console.WriteLine($"Password Length: {_mailSetting.password?.Length}");
                Console.WriteLine($"Host: {_mailSetting.host}");
                Console.WriteLine($"Port: {_mailSetting.port}");

                await smtp.ConnectAsync(_mailSetting.host, int.Parse(_mailSetting.port), SecureSocketOptions.SslOnConnect);
                await smtp.AuthenticateAsync(_mailSetting.Email, _mailSetting.password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }



        public async Task<bool> SendSimpleEmailAsync(string emailTo, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_mailSetting.displayname, _mailSetting.Email));
                email.To.Add(MailboxAddress.Parse(emailTo));
                email.Subject = subject;
                email.Sender = MailboxAddress.Parse(_mailSetting.Email);

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                Console.WriteLine($"Email: {_mailSetting.Email}");
                Console.WriteLine($"Password Length: {_mailSetting.password?.Length}");
                Console.WriteLine($"Host: {_mailSetting.host}");
                Console.WriteLine($"Port: {_mailSetting.port}");
                await smtp.ConnectAsync(_mailSetting.host, int.Parse(_mailSetting.port), SecureSocketOptions.SslOnConnect);
                await smtp.AuthenticateAsync(_mailSetting.Email, _mailSetting.password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        }
}