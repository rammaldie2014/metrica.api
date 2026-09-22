using Metrica.Application.Interfaces.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Metrica.Infrastructure.Storage
{
    public static class SeaweedFsServiceCollectionExtensions
    {
        public static IServiceCollection AddSeaweedFsStorage(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services
                .AddOptions<SeaweedFsOptions>()
                .Bind(configuration.GetSection(
                    SeaweedFsOptions.SectionName))
                .Validate(
                    options =>
                        Uri.TryCreate(
                            options.BaseUrl,
                            UriKind.Absolute,
                            out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp ||
                         uri.Scheme == Uri.UriSchemeHttps),
                    "SeaweedFs:BaseUrl debe contener una URL HTTP o HTTPS válida.")
                .Validate(
                    options => IsValidDirectory(options.Directory),
                    "SeaweedFs:Directory no contiene una ruta válida.")
                .ValidateOnStart();

            services.AddHttpClient<
                IFileStorage,
                SeaweedFsFileStorage>(client =>
                {
                    client.Timeout = TimeSpan.FromMinutes(2);
                });

            return services;
        }

        private static bool IsValidDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            var segments = directory
                .Trim()
                .Trim('/')
                .Split(
                    '/',
                    StringSplitOptions.RemoveEmptyEntries);

            return segments.Length > 0 &&
                   segments.All(segment =>
                       segment is not "." and not ".." &&
                       !segment.Contains('\\'));
        }
    }
}