using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Application.Commands.FileLoads.CreateFileLoad
{
    public sealed record CreateFileLoadCommand(
        string FileName,
        string UserEmail,
        Stream Content);
}
