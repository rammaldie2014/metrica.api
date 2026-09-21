using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Interfaces.Notifications
{
    public interface IFileLoadNotificationHandler
    {
        Task HandleAsync(
            long fileLoadId,
            CancellationToken cancellationToken = default);
    }
}
