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
        public async Task SendEmailAsync(EmailSettings st, string ToEmail, string Subject, string Body, string Add = null, string CC = null, string BC = null)
        {
            try
            {
                if (string.IsNullOrEmpty(ToEmail) && !string.IsNullOrEmpty(Add))
                {
                    ToEmail = Add.Split(',')[0].Trim();
                }
                else if(!string.IsNullOrEmpty(ToEmail) && string.IsNullOrEmpty(Add))
                {

                }
                else
                {
                    return;
                }
                var fromAddress = new MailAddress(st.Email);
                var toAddress = new MailAddress(ToEmail);
                string fromPassword = st.Password;
                string subject = (Subject??"").Replace("http","").Replace("https", "").Replace(".", " , ");
                string body = (Body ?? "").Replace("http", "").Replace("https", "").Replace(".", " , ");

                var smtp = new SmtpClient
                {
                    Host = st.Host,
                    Port = st.Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,

                })
                {
                     message.CC.Add(st.Email);

                    if (!string.IsNullOrEmpty(Add))
                    {
                        message.To.Add(Add);
                    }
                    if (!string.IsNullOrEmpty(CC))
                    {
                        message.CC.Add(CC);
                    }
                    if (!string.IsNullOrEmpty(BC))
                    {
                        message.Bcc.Add(BC);
                    }
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                string a = ex.ToString();
            }
        }
        public async Task<bool> SendAsync(EmailSettings st, string ToEmail, string Subject, string Body)
        {
            try
            {
                var fromAddress = new MailAddress(st.Email);
                var toAddress = new MailAddress(ToEmail);
                string fromPassword = st.Password;

                string subject = (Subject ?? "").Replace("http", "").Replace("https", "").Replace(".", " , ");
                string body = (Body ?? "").Replace("http", "").Replace("https", "").Replace(".", " , ");

                var smtp = new SmtpClient
                {
                    Host = st.Host,
                    Port = st.Port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Subject = subject,
                    Body = body,
                })
                {
                    await smtp.SendMailAsync(message);
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}