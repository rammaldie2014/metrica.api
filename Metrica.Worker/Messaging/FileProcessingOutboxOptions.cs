namespace Metrica.Worker.Messaging
{
    public sealed class FileProcessingOutboxOptions
    {
        public const string SectionName = "FileProcessingOutbox";

        public int BatchSize { get; init; } = 20;

        public int PollingIntervalSeconds { get; init; } = 5;
    }
}