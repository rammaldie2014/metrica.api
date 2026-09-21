using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Interfaces.Notifications
{
    public interface IEmailSender
    {
        Task SendAsync(
            string recipient,
            string subject,
            string body,
            CancellationToken cancellationToken = default);
    }
}
