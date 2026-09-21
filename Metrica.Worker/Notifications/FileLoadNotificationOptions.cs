using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Worker.Notifications
{
    public sealed class FileLoadNotificationOptions
    {
        public const string SectionName = "FileLoadNotifications";

        public int BatchSize { get; init; } = 20;

        public int PollingIntervalSeconds { get; init; } = 30;
    }
}
