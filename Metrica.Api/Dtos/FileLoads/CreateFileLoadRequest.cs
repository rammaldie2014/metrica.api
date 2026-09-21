using Metrica.Api.Validation;
using System.ComponentModel.DataAnnotations;

namespace Metrica.Api.Dtos.FileLoads
{
    public sealed class CreateFileLoadRequest
    {
        [Required(ErrorMessage = "El archivo es obligatorio.")]
        [ExcelFile]
        public IFormFile File { get; init; } = null!;

    }
}
