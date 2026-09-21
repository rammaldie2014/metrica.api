using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Messaging.Contracts
{
    public sealed record FileLoadNotificationRequested(long FileLoadId);
}
