using Metrica.Api.Dtos.FileLoads;
using Metrica.Application.Commands.FileLoads.CreateFileLoad;
using Metrica.Application.Queries.FileLoads.GetFileLoadById;
using Metrica.Application.Queries.FileLoads.GetFileLoadContent;
using Metrica.Application.Queries.FileLoads.GetFileLoadErrors;
using Metrica.Application.Queries.FileLoads.GetFileLoadProducts;
using Metrica.Application.Queries.FileLoads.GetFileLoads;
using Metrica.Application.Queries.FileLoads.GetFileLoadStatusHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Metrica.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/file-loads")]
    public sealed class FileLoadsController : ControllerBase
    {
        private readonly CreateFileLoadCommandHandler _createFileLoadHandler;
        private readonly GetFileLoadByIdQueryHandler _getFileLoadByIdHandler;
        private readonly GetFileLoadsQueryHandler _getFileLoadsHandler;
        private readonly GetFileLoadStatusHistoryQueryHandler _getFileLoadStatusHistoryHandler;
        private readonly GetFileLoadProductsQueryHandler _getFileLoadProductsHandler;
        private readonly GetFileLoadErrorsQueryHandler _getFileLoadErrorsHandler;
        private readonly GetFileLoadContentQueryHandler _getFileLoadContentHandler;

        public FileLoadsController(
            CreateFileLoadCommandHandler createFileLoadHandler,
            GetFileLoadByIdQueryHandler getFileLoadByIdHandler,
            GetFileLoadsQueryHandler getFileLoadsHandler,
            GetFileLoadStatusHistoryQueryHandler getFileLoadStatusHistoryHandler,
            GetFileLoadProductsQueryHandler getFileLoadProductsHandler,
            GetFileLoadErrorsQueryHandler getFileLoadErrorsHandler,
            GetFileLoadContentQueryHandler getFileLoadContentHandler)
        {
            _createFileLoadHandler = createFileLoadHandler;
            _getFileLoadByIdHandler = getFileLoadByIdHandler;
            _getFileLoadsHandler = getFileLoadsHandler;
            _getFileLoadStatusHistoryHandler = getFileLoadStatusHistoryHandler;
            _getFileLoadProductsHandler = getFileLoadProductsHandler;
            _getFileLoadErrorsHandler = getFileLoadErrorsHandler;
            _getFileLoadContentHandler = getFileLoadContentHandler;
        }

        [HttpPost]
        [Authorize(Policy = "FileLoads.Upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            [FromForm] CreateFileLoadRequest request,
            CancellationToken cancellationToken)
        {
           
            var fileName = Path.GetFileName(
                request.File.FileName.Replace('\\', '/'));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("El nombre del archivo no es válido.");
            }

            await using var content = request.File.OpenReadStream();

            var userEmail = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Unauthorized();
            }

            var command = new CreateFileLoadCommand(
                fileName,
                userEmail,
                content);

            var fileLoadId = await _createFileLoadHandler.HandleAsync(
                command,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = fileLoadId },
                new { Id = fileLoadId });
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<GetFileLoadByIdResponse>> GetById(
            long id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la carga debe ser mayor que cero.");
            }

            var query = new GetFileLoadByIdQuery(id);

            var response = await _getFileLoadByIdHandler.HandleAsync(
                query,
                cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<GetFileLoadsResponse>> GetAll(
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageNumber <= 0)
            {
                return BadRequest(
                    "El número de página debe ser mayor que cero.");
            }

            if (pageSize is < 1 or > 100)
            {
                return BadRequest(
                    "El tamaño de página debe estar entre 1 y 100.");
            }

            var query = new GetFileLoadsQuery(pageNumber, pageSize);

            var response = await _getFileLoadsHandler.HandleAsync(
                query,
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("{id:long}/status-history")]
        public async Task<ActionResult<IReadOnlyList<GetFileLoadStatusHistoryResponse>>> GetStatusHistory(
            long id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la carga debe ser mayor que cero.");
            }

            var query = new GetFileLoadStatusHistoryQuery(id);

            var response = await _getFileLoadStatusHistoryHandler.HandleAsync(
                query,
                cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpGet("{id:long}/products")]
        public async Task<ActionResult<GetFileLoadProductsResponse>> GetProducts(
            long id,
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la carga debe ser mayor que cero.");
            }

            if (pageNumber <= 0)
            {
                return BadRequest(
                    "El número de página debe ser mayor que cero.");
            }

            if (pageSize is < 1 or > 100)
            {
                return BadRequest(
                    "El tamaño de página debe estar entre 1 y 100.");
            }

            var query = new GetFileLoadProductsQuery(
                id,
                pageNumber,
                pageSize);

            var response = await _getFileLoadProductsHandler.HandleAsync(
                query,
                cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }


        [HttpGet("{id:long}/errors")]
        public async Task<ActionResult<GetFileLoadErrorsResponse>> GetErrors(
            long id,
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la carga debe ser mayor que cero.");
            }

            if (pageNumber <= 0)
            {
                return BadRequest(
                    "El número de página debe ser mayor que cero.");
            }

            if (pageSize is < 1 or > 100)
            {
                return BadRequest(
                    "El tamaño de página debe estar entre 1 y 100.");
            }

            var query = new GetFileLoadErrorsQuery(
                id,
                pageNumber,
                pageSize);

            var response = await _getFileLoadErrorsHandler.HandleAsync(
                query,
                cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [HttpGet("{id:long}/content")]
        public async Task<ActionResult<GetFileLoadContentResponse>> GetContent(
            long id,
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            if (id <= 0)
            {
                return BadRequest(
                    "El identificador de la carga debe ser mayor que cero.");
            }

            if (pageNumber <= 0)
            {
                return BadRequest(
                    "El número de página debe ser mayor que cero.");
            }

            if (pageSize is < 1 or > 100)
            {
                return BadRequest(
                    "El tamaño de página debe estar entre 1 y 100.");
            }

            var query = new GetFileLoadContentQuery(
                id,
                pageNumber,
                pageSize);

            var response = await _getFileLoadContentHandler.HandleAsync(
                query,
                cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }



    }
}
