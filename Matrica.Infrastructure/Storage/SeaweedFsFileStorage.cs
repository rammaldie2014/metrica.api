using Metrica.Application.Interfaces.Storage;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Metrica.Infrastructure.Storage
{
    public sealed class SeaweedFsFileStorage : IFileStorage
    {
        private readonly HttpClient _httpClient;
        private readonly string _directory;

        public SeaweedFsFileStorage(
            HttpClient httpClient,
            IOptions<SeaweedFsOptions> options)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(options);

            var configuration = options.Value;

            if (string.IsNullOrWhiteSpace(configuration.BaseUrl))
            {
                throw new InvalidOperationException(
                    "La URL de SeaweedFS no está configurada.");
            }

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(
                configuration.BaseUrl.TrimEnd('/') + "/");

            _directory = NormalizeDirectory(configuration.Directory);
        }

        public async Task<string> SaveAsync(
            Stream content,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (!content.CanRead)
            {
                throw new ArgumentException(
                    "El contenido del archivo no se puede leer.",
                    nameof(content));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "El nombre del archivo es obligatorio.",
                    nameof(fileName));
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = BuildFilePath(storedFileName);

            using var multipartContent = new MultipartFormDataContent();

            using var streamContent = new StreamContent(content);

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            multipartContent.Add(
                streamContent,
                "file",
                storedFileName);

            using var response = await _httpClient.PostAsync(
                filePath,
                multipartContent,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return filePath;
        }

        public async Task<Stream> OpenReadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            ValidateFilePath(filePath);

            using var response = await _httpClient.GetAsync(
                filePath,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStreamAsync(
                cancellationToken);

            var memoryStream = new MemoryStream();

            await content.CopyToAsync(
                memoryStream,
                cancellationToken);

            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            ValidateFilePath(filePath);

            using var response = await _httpClient.DeleteAsync(
                filePath,
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }

        private string BuildFilePath(string storedFileName)
        {
            return string.IsNullOrWhiteSpace(_directory)
                ? storedFileName
                : $"{_directory}/{storedFileName}";
        }

        private static string NormalizeDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return string.Empty;
            }

            return directory
                .Trim()
                .Trim('/');
        }

        private void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(
                    "La ruta del archivo es obligatoria.",
                    nameof(filePath));
            }

            var normalizedPath = filePath.TrimStart('/');

            if (!string.IsNullOrWhiteSpace(_directory) &&
                !normalizedPath.StartsWith(
                    $"{_directory}/",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "La ruta del archivo no pertenece al directorio configurado.",
                    nameof(filePath));
            }
        }
    }
}