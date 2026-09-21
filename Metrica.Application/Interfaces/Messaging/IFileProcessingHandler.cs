using Metrica.Application.Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileProcessingHandler
    {
        Task HandleAsync(
            ProcessFileRequested message,
            CancellationToken cancellationToken = default);
    }
}
