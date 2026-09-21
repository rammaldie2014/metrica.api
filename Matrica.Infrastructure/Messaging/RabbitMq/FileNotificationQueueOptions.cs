using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class FileNotificationQueueOptions
    {
        public const string SectionName = "FileNotificationQueue";

        public string QueueName { get; init; } = string.Empty;

        public string RoutingKey { get; init; } = string.Empty;

        public int MaxNotificationAttempts { get; init; } = 3;

        public int RetryDelaySeconds { get; init; } = 5;
    }
}
