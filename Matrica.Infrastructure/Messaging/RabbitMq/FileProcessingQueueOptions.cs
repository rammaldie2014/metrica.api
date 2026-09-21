using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public sealed class FileProcessingQueueOptions
    {
        public const string SectionName = "FileProcessingQueue";

        public string QueueName { get; init; } = string.Empty;

        public string RoutingKey { get; init; } = string.Empty;

        public int MaxProcessingAttempts { get; init; } = 3;

        public int RetryDelaySeconds { get; init; } = 5;
    }
}
