using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Domain.Entities
{
    public class ProcessedProduct
    {
        public long Id { get; private set; }
        public long FileLoadId { get; private set; }
        public string Period { get; private set; } = string.Empty;
        public string ProductCode { get; private set; } = string.Empty;
        public string ProductName { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private ProcessedProduct()
        {
        }

        public ProcessedProduct(
            long fileLoadId,
            string period,
            string productCode,
            string productName,
            string description,
            decimal price,
            int stock)
        {
            if (fileLoadId <= 0)
                throw new ArgumentException(
                    "El identificador de la carga debe ser mayor que cero.",
                    nameof(fileLoadId));

            if (string.IsNullOrWhiteSpace(period))
                throw new ArgumentException(
                    "El periodo es obligatorio.",
                    nameof(period));

            if (string.IsNullOrWhiteSpace(productCode))
                throw new ArgumentException(
                    "El código del producto es obligatorio.",
                    nameof(productCode));

            FileLoadId = fileLoadId;
            Period = period.Trim();
            ProductCode = productCode.Trim();
            ProductName = productName?.Trim() ?? string.Empty;
            Description = description?.Trim() ?? string.Empty;
            Price = price;
            Stock = stock;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
