using Metrica.Application.Exceptions;
using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Excel;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Interfaces.Storage;
using Metrica.Application.Messaging.Contracts;
using Metrica.Domain.Entities;
using System.Data;

namespace Metrica.Application.Commands.FileLoads.CreateFileLoad
{
    public sealed class CreateFileLoadCommandHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorage _fileStorage;
        private readonly IFileProcessingOutbox _fileProcessingOutbox;
        private readonly IFileLoadPeriodResolver _fileLoadPeriodResolver;

        public CreateFileLoadCommandHandler(
            IFileLoadRepository fileLoadRepository,
            IUnitOfWork unitOfWork,
            IFileStorage fileStorage,
            IFileProcessingOutbox fileProcessingOutbox,
            IFileLoadPeriodResolver fileLoadPeriodResolver)
        {
            _fileLoadRepository = fileLoadRepository;
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _fileProcessingOutbox = fileProcessingOutbox;
            _fileLoadPeriodResolver = fileLoadPeriodResolver;
        }

        public async Task<long> HandleAsync(
            CreateFileLoadCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Content);

            var period = _fileLoadPeriodResolver.Resolve(
                command.Content,
                cancellationToken);

            var fileLoad = new FileLoad(
                command.FileName,
                command.UserEmail);

            fileLoad.AssignPeriod(period);

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
                        var periodReserved =
                            await _fileLoadRepository.TryReservePeriodAsync(
                                period,
                                transactionToken);

                        if (!periodReserved)
                        {
                            throw new FileLoadPeriodConflictException(period);
                        }

                        await _fileLoadRepository.AddAsync(
                            fileLoad,
                            transactionToken);

                        
                        await _unitOfWork.SaveChangesAsync(transactionToken);

                        await _fileProcessingOutbox.AddAsync(
                            new ProcessFileRequested(fileLoad.Id),
                            transactionToken);

                        await _unitOfWork.SaveChangesAsync(transactionToken);
                    },
                    isolationLevel: IsolationLevel.Serializable,
                    cancellationToken: cancellationToken);
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