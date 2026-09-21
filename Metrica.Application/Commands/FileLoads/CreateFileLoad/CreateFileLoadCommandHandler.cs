using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Interfaces.Storage;
using Metrica.Application.Messaging.Contracts;
using Metrica.Domain.Entities;

namespace Metrica.Application.Commands.FileLoads.CreateFileLoad
{
    public sealed class CreateFileLoadCommandHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorage _fileStorage;
        private readonly IFileProcessingOutbox _fileProcessingOutbox;

        public CreateFileLoadCommandHandler(
            IFileLoadRepository fileLoadRepository,
            IUnitOfWork unitOfWork,
            IFileStorage fileStorage,
            IFileProcessingOutbox fileProcessingOutbox)
        {
            _fileLoadRepository = fileLoadRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _fileProcessingOutbox = fileProcessingOutbox;
        }

        public async Task<long> HandleAsync(
            CreateFileLoadCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Content);

            var fileLoad = new FileLoad(
                command.FileName,
                command.UserEmail);

            var filePath = await _fileStorage.SaveAsync(
                command.Content,
                command.FileName,
                cancellationToken);

            try
            {
                fileLoad.SetFilePath(filePath);

                await _unitOfWork.ExecuteInTransactionAsync(
                    async transactionToken =>
                    {
                        await _fileLoadRepository.AddAsync(
                            fileLoad,
                            transactionToken);

                        // Obtiene el ID generado sin confirmar la transacción.
                        await _unitOfWork.SaveChangesAsync(transactionToken);

                        await _fileProcessingOutbox.AddAsync(
                            new ProcessFileRequested(fileLoad.Id),
                            transactionToken);

                        await _unitOfWork.SaveChangesAsync(transactionToken);
                    },
                    cancellationToken);
            }
            catch (Exception registrationException)
            {
                try
                {
                    await _fileStorage.DeleteAsync(
                        filePath,
                        CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    throw new AggregateException(
                        "Falló el registro de la carga y la eliminación del archivo.",
                        registrationException,
                        cleanupException);
                }

                throw;
            }

            return fileLoad.Id;
        }
    }
}