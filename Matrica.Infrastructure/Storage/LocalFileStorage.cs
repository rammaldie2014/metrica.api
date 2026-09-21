using Metrica.Application.Interfaces.Storage;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Storage
{
    public sealed class LocalFileStorage : IFileStorage
    {
        private readonly string _rootPath;

        public LocalFileStorage(IOptions<FileStorageOptions> options)
        {
            var rootPath = options.Value.RootPath;

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                throw new InvalidOperationException(
                    "La carpeta de almacenamiento no está configurada.");
            }

            if (!Path.IsPathFullyQualified(rootPath))
            {
                throw new InvalidOperationException(
                    "La carpeta de almacenamiento debe ser una ruta absoluta.");
            }

            _rootPath = Path.GetFullPath(rootPath);
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

            cancellationToken.ThrowIfCancellationRequested();

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = GetFullPath(storedFileName);

            Directory.CreateDirectory(_rootPath);

            var destination = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            try
            {
                await using (destination)
                {
                    await content.CopyToAsync(
                        destination,
                        cancellationToken);
                }
            }
            catch
            {
                // El stream de destino ya está cerrado.
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                    // Conservamos la excepción original de la escritura.
                }

                throw;
            }

            return storedFileName;
        }

        public Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = GetFullPath(filePath);

            File.Delete(fullPath);

            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = GetFullPath(filePath);

            Stream stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult(stream);
        }

        private string GetFullPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) ||
                Path.IsPathRooted(filePath) ||
                filePath.Contains('/') ||
                filePath.Contains('\\') ||
                filePath.Contains(':') ||
                filePath is "." or ".." ||
                filePath.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException(
                    "El identificador del archivo no es válido.",
                    nameof(filePath));
            }

            return Path.Combine(_rootPath, filePath);
        }
    }
}
