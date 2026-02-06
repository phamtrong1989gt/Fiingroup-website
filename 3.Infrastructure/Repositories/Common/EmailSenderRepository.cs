using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using PT.Domain.Model;
using PT.Infrastructure.Interfaces;

namespace PT.Infrastructure.Repositories
{
    public class EmailSenderRepository : IEmailSenderRepository
    {
        private readonly ILogger<EmailSenderRepository> _logger;

        public EmailSenderRepository(ILogger<EmailSenderRepository> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Helper method to merge và deduplicate email addresses
        /// </summary>
        private string MergeEmailAddresses(params string[] emailLists)
        {
            var allEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var emailList in emailLists)
            {
                if (string.IsNullOrWhiteSpace(emailList))
                    continue;

                // Split by comma, semicolon, or space
                var emails = emailList.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var email in emails)
                {
                    var trimmedEmail = email.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedEmail) && IsValidEmail(trimmedEmail))
                    {
                        allEmails.Add(trimmedEmail);
                    }
                }
            }

            return allEmails.Any() ? string.Join(",", allEmails) : null;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendEmailAsync(EmailSetting st, string ToEmail, string Subject, string Body, string Add = null, string CC = null, string supportEmail = null)
        {
            SmtpClient smtp = null;
            try
            {
                // Validate input
                if (st == null)
                {
                    _logger?.LogWarning("EmailSetting is null - skipping email send");
                    return;
                }

                if (string.IsNullOrEmpty(ToEmail) && !string.IsNullOrEmpty(Add))
                {
                    ToEmail = Add.Split(',')[0].Trim();
                }
                
                if (string.IsNullOrEmpty(ToEmail))
                {
                    _logger?.LogWarning("No recipient email provided - skipping email send");
                    return;
                }

                // Validate email settings
                if (string.IsNullOrEmpty(st.EmailServer) || string.IsNullOrEmpty(st.Password) || 
                    string.IsNullOrEmpty(st.Host) || st.Port <= 0)
                {
                    _logger?.LogWarning("Invalid email settings: EmailServer={EmailServer}, Host={Host}, Port={Port} - skipping email send", 
                        st.EmailServer, st.Host, st.Port);
                    return;
                }

                // Use "FiinGroup" as display name instead of email address
                var fromAddress = new MailAddress(st.EmailServer, st.From);
                var toAddress = new MailAddress(ToEmail);
                
                string subject = Subject ?? "";
                string body = Body ?? "";

                // Merge CC emails: support email (if provided) + sender email + custom CC
                var mergedCC = MergeEmailAddresses(supportEmail, st.EmailServer, CC);

                // Configure SMTP with proper settings for port 587 (TLS/STARTTLS)
                smtp = new SmtpClient
                {
                    Host = st.Host,
                    Port = st.Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(st.EmailServer, st.Password),
                    Timeout = 30000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,
                })
                {
                    // Add TO addresses
                    if (!string.IsNullOrEmpty(Add))
                    {
                        var additionalEmails = Add.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var email in additionalEmails)
                        {
                            var trimmedEmail = email.Trim();
                            if (IsValidEmail(trimmedEmail))
                            {
                                message.To.Add(trimmedEmail);
                            }
                        }
                    }

                    // Add CC addresses (merged)
                    if (!string.IsNullOrEmpty(mergedCC))
                    {
                        var ccEmails = mergedCC.Split(',');
                        foreach (var email in ccEmails)
                        {
                            var trimmedEmail = email.Trim();
                            if (IsValidEmail(trimmedEmail) && 
                                !string.Equals(trimmedEmail, ToEmail, StringComparison.OrdinalIgnoreCase))
                            {
                                message.CC.Add(trimmedEmail);
                            }
                        }
                    }

                    _logger?.LogInformation("Sending email to {ToEmail} via {Host}:{Port} (TLS/STARTTLS)", ToEmail, st.Host, st.Port);
                    await smtp.SendMailAsync(message);
                    _logger?.LogInformation("✅ Email sent successfully to {ToEmail}", ToEmail);
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger?.LogError(smtpEx, 
                    "❌ SMTP error sending email to {ToEmail}. StatusCode: {StatusCode}. " +
                    "This error was caught and will not affect the main flow.", 
                    ToEmail, smtpEx.StatusCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, 
                    "❌ Error sending email to {ToEmail}: {Message}. " +
                    "This error was caught and will not affect the main flow.", 
                    ToEmail, ex.Message);
            }
            finally
            {
                smtp?.Dispose();
            }
        }

        public async Task SendEmailAsync(EmailSettings st, string ToEmail, string Subject, string Body, string Add = null, string CC = null, string supportEmail = null)
        {
            SmtpClient smtp = null;
            try
            {
                // Validate input
                if (st == null)
                {
                    _logger?.LogWarning("EmailSettings is null - skipping email send");
                    return;
                }

                if (string.IsNullOrEmpty(ToEmail) && !string.IsNullOrEmpty(Add))
                {
                    ToEmail = Add.Split(',')[0].Trim();
                }
                
                if (string.IsNullOrEmpty(ToEmail))
                {
                    _logger?.LogWarning("No recipient email provided - skipping email send");
                    return;
                }

                // Validate email settings
                if (string.IsNullOrEmpty(st.Email) || string.IsNullOrEmpty(st.Password) || 
                    string.IsNullOrEmpty(st.Host) || st.Port <= 0)
                {
                    _logger?.LogWarning("Invalid email settings: Email={Email}, Host={Host}, Port={Port} - skipping email send", 
                        st.Email, st.Host, st.Port);
                    return;
                }

                // Use "FiinGroup" as display name
                var fromAddress = new MailAddress(st.Email, "FiinGroup");
                var toAddress = new MailAddress(ToEmail);
                
                string subject = Subject ?? "";
                string body = Body ?? "";

                // Merge CC emails: support email (if provided) + sender email + custom CC
                var mergedCC = MergeEmailAddresses(supportEmail, st.Email, CC);

                // Configure SMTP with proper settings for port 587 (TLS/STARTTLS)
                smtp = new SmtpClient
                {
                    Host = st.Host,
                    Port = st.Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(st.Email, st.Password),
                    Timeout = 30000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,
                })
                {
                    // Add TO addresses
                    if (!string.IsNullOrEmpty(Add))
                    {
                        var additionalEmails = Add.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var email in additionalEmails)
                        {
                            var trimmedEmail = email.Trim();
                            if (IsValidEmail(trimmedEmail))
                            {
                                message.To.Add(trimmedEmail);
                            }
                        }
                    }

                    // Add CC addresses (merged)
                    if (!string.IsNullOrEmpty(mergedCC))
                    {
                        var ccEmails = mergedCC.Split(',');
                        foreach (var email in ccEmails)
                        {
                            var trimmedEmail = email.Trim();
                            if (IsValidEmail(trimmedEmail) && 
                                !string.Equals(trimmedEmail, ToEmail, StringComparison.OrdinalIgnoreCase))
                            {
                                message.CC.Add(trimmedEmail);
                            }
                        }
                    }

                    _logger?.LogInformation("Sending email to {ToEmail} via {Host}:{Port} (TLS/STARTTLS)", ToEmail, st.Host, st.Port);
                    await smtp.SendMailAsync(message);
                    _logger?.LogInformation("✅ Email sent successfully to {ToEmail}", ToEmail);
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger?.LogError(smtpEx, 
                    "❌ SMTP error sending email to {ToEmail}. StatusCode: {StatusCode}. " +
                    "This error was caught and will not affect the main flow.", 
                    ToEmail, smtpEx.StatusCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, 
                    "❌ Error sending email to {ToEmail}: {Message}. " +
                    "This error was caught and will not affect the main flow.", 
                    ToEmail, ex.Message);
            }
            finally
            {
                smtp?.Dispose();
            }
        }

        public async Task<bool> SendAsync(EmailSettings st, string ToEmail, string Subject, string Body)
        {
            SmtpClient smtp = null;
            try
            {
                if (st == null || string.IsNullOrEmpty(ToEmail))
                {
                    _logger?.LogWarning("Invalid parameters for SendAsync");
                    return false;
                }

                // Use "FiinGroup" as display name
                var fromAddress = new MailAddress(st.Email, "FiinGroup");
                var toAddress = new MailAddress(ToEmail);

                string subject = Subject ?? "";
                string body = Body ?? "";

                smtp = new SmtpClient
                {
                    Host = st.Host,
                    Port = st.Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(st.Email, st.Password),
                    Timeout = 30000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,
                })
                {
                    await smtp.SendMailAsync(message);
                    _logger?.LogInformation("✅ Email sent successfully to {ToEmail}", ToEmail);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Failed to send email to {ToEmail}", ToEmail);
                return false;
            }
            finally
            {
                smtp?.Dispose();
            }
        }
    }
}