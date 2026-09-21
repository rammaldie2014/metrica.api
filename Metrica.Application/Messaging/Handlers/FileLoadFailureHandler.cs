using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Domain.Entities;
using Metrica.Domain.Enums;

namespace Metrica.Application.Messaging.Handlers
{
    public sealed class FileLoadFailureHandler : IFileLoadFailureHandler
    {
        private const int MaxErrorCodeLength = 100;
        private const int MaxErrorMessageLength = 2000;

        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IFileLoadErrorRepository _errorRepository;
        private readonly IFileLoadStatusHistoryRepository _historyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FileLoadFailureHandler(
            IFileLoadRepository fileLoadRepository,
            IFileLoadErrorRepository errorRepository,
            IFileLoadStatusHistoryRepository historyRepository,
            IUnitOfWork unitOfWork)
        {
            _fileLoadRepository = fileLoadRepository;
            _errorRepository = errorRepository;
            _historyRepository = historyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(
            long fileLoadId,
            string errorCode,
            string errorMessage,
            CancellationToken cancellationToken = default)
        {
            if (fileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

            var code = errorCode.Trim();

            if (code.Length > MaxErrorCodeLength)
            {
                throw new ArgumentException(
                    "El código del error no puede superar los 100 caracteres.",
                    nameof(errorCode));
            }

            var message = errorMessage.Trim();

            if (message.Length > MaxErrorMessageLength)
            {
                message = message[..MaxErrorMessageLength];
            }

            var fileLoad = await _fileLoadRepository.GetByIdAsync(
                fileLoadId,
                cancellationToken);

            if (fileLoad is null)
            {
                throw new InvalidOperationException(
                    $"No existe la carga con ID {fileLoadId}.");
            }

            if (fileLoad.Status is FileLoadStatus.Finished
                or FileLoadStatus.Notified
                or FileLoadStatus.Rejected
                or FileLoadStatus.Failed)
            {
                return;
            }

            var previousStatus = fileLoad.Status;

            fileLoad.Fail();

            var error = new FileLoadError(
                fileLoadId: fileLoad.Id,
                code: code,
                message: message,
                rowNumber: null);

            await _errorRepository.AddAsync(
                error,
                cancellationToken);

            var history = new FileLoadStatusHistory(
                fileLoadId: fileLoad.Id,
                previousStatus: previousStatus,
                newStatus: fileLoad.Status,
                description: message);

            await _historyRepository.AddAsync(
                history,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
