using Metrica.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;


namespace Metrica.Application.Queries.FileLoads.GetFileLoadById
{
    public sealed class GetFileLoadByIdQueryHandler
    {
        private readonly IFileLoadRepository _fileLoadRepository;

        public GetFileLoadByIdQueryHandler(
            IFileLoadRepository fileLoadRepository)
        {
            _fileLoadRepository = fileLoadRepository;
        }

        public async Task<GetFileLoadByIdResponse?> HandleAsync(
            GetFileLoadByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            var fileLoad = await _fileLoadRepository.GetByIdAsync(
                query.Id,
                cancellationToken);

            if (fileLoad is null)
            {
                return null;
            }

            return new GetFileLoadByIdResponse(
                fileLoad.Id,
                fileLoad.FileName,
                fileLoad.UserEmail,
                fileLoad.Period,
                fileLoad.Status,
                fileLoad.CreatedAt,
                fileLoad.FinishedAt);
        }
    }
}
