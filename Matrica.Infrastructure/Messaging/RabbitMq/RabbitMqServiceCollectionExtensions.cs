using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Metrica.Infrastructure.Messaging.RabbitMq
{
    public static class RabbitMqServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection(
                    RabbitMqOptions.SectionName))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.HostName),
                    "RabbitMq:HostName es obligatorio.")
                .Validate(
                    options => options.Port is > 0 and <= 65535,
                    "RabbitMq:Port debe estar entre 1 y 65535.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.UserName) &&
                               !string.IsNullOrWhiteSpace(options.Password),
                    "RabbitMq:UserName y Password son obligatorios.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.VirtualHost),
                    "RabbitMq:VirtualHost es obligatorio.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.ExchangeName),
                    "RabbitMq:ExchangeName es obligatorio.")
                .ValidateOnStart();

            services.AddSingleton<
                IRabbitMqConnectionProvider,
                RabbitMqConnectionProvider>();

            return services;
        }

        public static IServiceCollection AddFileProcessingQueue(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<FileProcessingQueueOptions>()
                .Bind(configuration.GetSection(
                    FileProcessingQueueOptions.SectionName))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.QueueName),
                    "FileProcessingQueue:QueueName es obligatorio.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.RoutingKey),
                    "FileProcessingQueue:RoutingKey es obligatorio.")
                .Validate(
                    options => options.MaxProcessingAttempts > 0,
                    "FileProcessingQueue:MaxProcessingAttempts debe ser mayor que cero.")
                .Validate(
                    options => options.RetryDelaySeconds > 0,
                    "FileProcessingQueue:RetryDelaySeconds debe ser mayor que cero.")
                .ValidateOnStart();

            return services;
        }

        public static IServiceCollection AddFileNotificationQueue(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<FileNotificationQueueOptions>()
                .Bind(configuration.GetSection(
                    FileNotificationQueueOptions.SectionName))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.QueueName),
                    "FileNotificationQueue:QueueName es obligatorio.")
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.RoutingKey),
                    "FileNotificationQueue:RoutingKey es obligatorio.")
                .Validate(
                    options => options.MaxNotificationAttempts > 0,
                    "FileNotificationQueue:MaxNotificationAttempts debe ser mayor que cero.")
                .Validate(
                    options => options.RetryDelaySeconds > 0,
                    "FileNotificationQueue:RetryDelaySeconds debe ser mayor que cero.")
                .ValidateOnStart();

            return services;
        }

    }
}