using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Excel;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Interfaces.Storage;
using Metrica.Application.Messaging.Handlers;
using Metrica.Infrastructure.Excel;
using Metrica.Infrastructure.Messaging.RabbitMq;
using Metrica.Infrastructure.Persistence;
using Metrica.Infrastructure.Persistence.Outbox;
using Metrica.Infrastructure.Persistence.Repositories;
using Metrica.Infrastructure.Storage;
using Metrica.Worker;
using Metrica.Worker.Messaging;
using Metrica.Worker.Notifications;
using Microsoft.EntityFrameworkCore;


var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<MetricaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFileLoadRepository, FileLoadRepository>();
builder.Services.AddScoped<IFileLoadStatusHistoryRepository, FileLoadStatusHistoryRepository>();

builder.Services.AddScoped<IUnitOfWork>(provider =>
    provider.GetRequiredService<MetricaDbContext>());


builder.Services.AddFileProcessingQueue(builder.Configuration);

builder.Services.AddFileNotificationQueue(builder.Configuration);

builder.Services.AddSingleton<IRabbitMqMessageConsumer, RabbitMqMessageConsumer>();
builder.Services.AddSingleton<RabbitMqMessagePublisher>();

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddSingleton<IFileProcessingConsumer, RabbitMqFileProcessingConsumer>();
builder.Services.AddSingleton<IFileLoadNotificationPublisher, RabbitMqFileLoadNotificationPublisher>();

builder.Services.AddSingleton<IFileProcessingPublisher, RabbitMqFileProcessingPublisher>();

builder.Services.AddScoped<IFileProcessingOutboxDispatcher, FileProcessingOutboxDispatcher>();

builder.Services
    .AddOptions<FileStorageOptions>()
    .Bind(builder.Configuration.GetSection(FileStorageOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.RootPath) &&
                   Path.IsPathFullyQualified(options.RootPath),
        "FileStorage:RootPath debe contener una ruta absoluta válida.")
    .ValidateOnStart();

builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IProductExcelReader, ProductExcelReader>();
builder.Services.AddScoped<IFileLoadErrorRepository, FileLoadErrorRepository>();
builder.Services.AddScoped<IProcessedProductRepository, ProcessedProductRepository>();
builder.Services.AddScoped<IFileProcessingHandler, FileProcessingHandler>();
builder.Services.AddScoped<IFileLoadFailureHandler, FileLoadFailureHandler>();
builder.Services.AddScoped<IFileLoadNotificationOutbox, FileLoadNotificationOutbox>();

builder.Services.AddScoped<IFileLoadNotificationOutboxDispatcher, FileLoadNotificationOutboxDispatcher>();

builder.Services
    .AddOptions<FileLoadNotificationOptions>()
    .Bind(builder.Configuration.GetSection(
        FileLoadNotificationOptions.SectionName))
    .Validate(
        options => options.BatchSize > 0,
        "FileLoadNotifications:BatchSize debe ser mayor que cero.")
    .Validate(
        options => options.PollingIntervalSeconds > 0,
        "FileLoadNotifications:PollingIntervalSeconds debe ser mayor que cero.")
    .ValidateOnStart();

builder.Services
    .AddOptions<FileProcessingOutboxOptions>()
    .Bind(builder.Configuration.GetSection(
        FileProcessingOutboxOptions.SectionName))
    .Validate(
        options => options.BatchSize > 0,
        "FileProcessingOutbox:BatchSize debe ser mayor que cero.")
    .Validate(
        options => options.PollingIntervalSeconds > 0,
        "FileProcessingOutbox:PollingIntervalSeconds debe ser mayor que cero.")
    .ValidateOnStart();

builder.Services.AddHostedService<FileProcessingOutboxWorker>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<FileLoadNotificationOutboxWorker>();

var host = builder.Build();
host.Run();
