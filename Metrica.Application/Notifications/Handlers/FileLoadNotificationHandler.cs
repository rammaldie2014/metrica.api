using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Notifications;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Domain.Entities;
using Metrica.Domain.Enums;

namespace Metrica.Application.Notifications.Handlers
{
    public sealed class FileLoadNotificationHandler
            : IFileLoadNotificationHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IFileLoadStatusHistoryRepository _statusHistoryRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public FileLoadNotificationHandler(
            IFileLoadRepository fileLoadRepository,
            IFileLoadStatusHistoryRepository statusHistoryRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _fileLoadRepository = fileLoadRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(
            long fileLoadId,
            CancellationToken cancellationToken = default)
        {
            if (fileLoadId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fileLoadId),
                    "El identificador de la carga debe ser mayor que cero.");
            }

            var fileLoad = await _fileLoadRepository.GetByIdAsync(
                fileLoadId,
                cancellationToken);

            if (fileLoad is null)
            {
                throw new InvalidOperationException(
                    $"No existe la carga con ID {fileLoadId}.");
            }

            if (fileLoad.Status == FileLoadStatus.Notified)
            {
                return;
            }

            if (fileLoad.Status != FileLoadStatus.Finished)
            {
                throw new InvalidOperationException(
                    $"La carga {fileLoadId} no puede notificarse " +
                    $"en estado {fileLoad.Status}.");
            }

            var subject = $"Metrica: carga {fileLoad.Id} finalizada";

            var body = $"""
                El procesamiento de tu archivo finalizó correctamente.

                Identificador de carga: {fileLoad.Id}
                Archivo: {fileLoad.FileName}
                Periodo: {fileLoad.Period}

                Los productos fueron registrados correctamente.
                """;

            await _emailSender.SendAsync(
                fileLoad.UserEmail,
                subject,
                body,
                cancellationToken);

            var previousStatus = fileLoad.Status;

            fileLoad.MarkAsNotified();

            var history = new FileLoadStatusHistory(
                fileLoadId: fileLoad.Id,
                previousStatus: previousStatus,
                newStatus: fileLoad.Status,
                description: "El servidor SMTP aceptó el correo de notificación.");

            await _statusHistoryRepository.AddAsync(
                history,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
