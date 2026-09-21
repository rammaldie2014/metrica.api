using Metrica.Application.Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileLoadNotificationPublisher
    {
        Task PublishAsync(
            FileLoadNotificationRequested message,
            CancellationToken cancellationToken = default);
    }
}
