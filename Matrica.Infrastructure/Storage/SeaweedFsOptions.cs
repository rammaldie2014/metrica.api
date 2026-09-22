namespace Metrica.Infrastructure.Storage
{
    public sealed class SeaweedFsOptions
    {
        public const string SectionName = "SeaweedFs";

        public string BaseUrl { get; init; } = string.Empty;

        public string Directory { get; init; } = "metrica/uploads";
    }
}