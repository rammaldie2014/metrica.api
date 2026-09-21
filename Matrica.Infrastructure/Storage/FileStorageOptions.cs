using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Storage
{
    public sealed class FileStorageOptions
    {
        public const string SectionName = "FileStorage";

        public string RootPath { get; init; } = string.Empty;
    }
}
