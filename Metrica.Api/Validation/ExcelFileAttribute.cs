using System.ComponentModel.DataAnnotations;

namespace Metrica.Api.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class ExcelFileAttribute : ValidationAttribute
    {
        private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not IFormFile file)
            {
                return new ValidationResult(
                    "El valor recibido no es un archivo.");
            }

            if (file.Length == 0)
            {
                return new ValidationResult(
                    "El archivo está vacío.");
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                return new ValidationResult(
                    "El archivo no debe superar los 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (!string.Equals(
                extension,
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
            {
                return new ValidationResult(
                    "Solo se permiten archivos con extensión .xlsx.");
            }

            return ValidationResult.Success;
        }
    }
}
