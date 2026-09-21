using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Notifications
{
    public sealed class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; init; } = string.Empty;

        public int Port { get; init; } = 587;

        public string UserName { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;

        public string SenderEmail { get; init; } = string.Empty;

        public string SenderName { get; init; } = "Metrica";

        public bool UseStartTls { get; init; } = true;
    }
}
