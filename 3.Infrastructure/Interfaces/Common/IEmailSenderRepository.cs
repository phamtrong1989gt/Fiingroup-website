using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PT.Domain.Model;

namespace PT.Infrastructure.Interfaces
{
    public interface IEmailSenderRepository
    {
        Task SendEmailAsync(EmailSettings st, string ToEmail, string Subject, string Body, string Add = null, string CC = null, string BC = null);
        Task<bool> SendAsync(EmailSettings st, string ToEmail, string Subject, string Body);
    }
}
