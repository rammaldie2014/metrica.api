using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Interfaces.Messaging
{
    public interface IFileLoadFailureHandler
    {
        Task HandleAsync(
            long fileLoadId,
            string errorCode,
            string errorMessage,
            CancellationToken cancellationToken = default);
    }
}
