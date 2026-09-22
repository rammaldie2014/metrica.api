using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Notifications;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Notifications.Handlers;
using Metrica.Infrastructure.Messaging.RabbitMq;
using Metrica.Infrastructure.Notifications;
using Metrica.Infrastructure.Persistence;
using Metrica.Infrastructure.Persistence.Repositories;
using Metrica.Notifications.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services);
});

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<MetricaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnitOfWork>(provider =>
    provider.GetRequiredService<MetricaDbContext>());

builder.Services.AddScoped<
    IFileLoadRepository,
    FileLoadRepository>();

builder.Services.AddScoped<
    IFileLoadStatusHistoryRepository,
    FileLoadStatusHistoryRepository>();

builder.Services.AddScoped<
    IFileLoadNotificationHandler,
    FileLoadNotificationHandler>();

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection(SmtpOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Host),
        "Smtp:Host es obligatorio.")
    .Validate(
        options => options.Port is > 0 and <= 65535,
        "Smtp:Port debe estar entre 1 y 65535.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.SenderEmail),
        "Smtp:SenderEmail es obligatorio.")
    .Validate(
        options =>
            string.IsNullOrWhiteSpace(options.UserName) ==
            string.IsNullOrWhiteSpace(options.Password),
        "Smtp:UserName y Password deben configurarse juntos o dejarse ambos vacíos.")
    .ValidateOnStart();

builder.Services.AddRabbitMq(builder.Configuration);

builder.Services.AddFileNotificationQueue(builder.Configuration);

builder.Services.AddSingleton<IRabbitMqMessageConsumer, RabbitMqMessageConsumer>();
builder.Services.AddSingleton<IFileLoadNotificationConsumer, RabbitMqFileLoadNotificationConsumer>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
