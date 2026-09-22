namespace Metrica.Application.Exceptions
{
    public sealed class FileLoadPeriodConflictException : Exception
    {
        public string Period { get; }

        public FileLoadPeriodConflictException(string period)
            : base(CreateMessage(period))
        {
            Period = period.Trim();
        }

        private static string CreateMessage(string period)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(period);

            return $"El periodo '{period.Trim()}' ya tiene una carga " +
                   "activa o finalizada.";
        }
    }
}