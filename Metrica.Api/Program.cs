using Metrica.Api.ExceptionHandling;
using Metrica.Application.Commands.FileLoads.CreateFileLoad;
using Metrica.Application.Excel;
using Metrica.Application.Interfaces;
using Metrica.Application.Interfaces.Excel;
using Metrica.Application.Interfaces.Messaging;
using Metrica.Application.Interfaces.Repositories;
using Metrica.Application.Queries.FileLoads.GetFileLoadById;
using Metrica.Application.Queries.FileLoads.GetFileLoadContent;
using Metrica.Application.Queries.FileLoads.GetFileLoadErrors;
using Metrica.Application.Queries.FileLoads.GetFileLoadProducts;
using Metrica.Application.Queries.FileLoads.GetFileLoads;
using Metrica.Application.Queries.FileLoads.GetFileLoadStatusHistory;
using Metrica.Infrastructure.Excel;
using Metrica.Infrastructure.Persistence;
using Metrica.Infrastructure.Persistence.Outbox;
using Metrica.Infrastructure.Persistence.Repositories;
using Metrica.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Issuer.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:Audience.");

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException(
        "No se encontró Jwt:SigningKey.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSigningKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FileLoads.Upload", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("permission", "fileloads:upload");
    });
});



// Add services to the container.

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<MetricaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFileLoadRepository, FileLoadRepository>();
builder.Services.AddScoped<IFileLoadStatusHistoryRepository, FileLoadStatusHistoryRepository>();
builder.Services.AddScoped<IProcessedProductRepository, ProcessedProductRepository>();
builder.Services.AddScoped<IFileLoadErrorRepository, FileLoadErrorRepository>();
builder.Services.AddScoped<IProductExcelReader, ProductExcelReader>();
builder.Services.AddScoped<IFileLoadPeriodResolver, FileLoadPeriodResolver>();

builder.Services.AddSeaweedFsStorage(
    builder.Configuration);

builder.Services.AddScoped<IUnitOfWork>(provider =>
    provider.GetRequiredService<MetricaDbContext>());

builder.Services.AddScoped<CreateFileLoadCommandHandler>();
builder.Services.AddScoped<GetFileLoadByIdQueryHandler>();
builder.Services.AddScoped<GetFileLoadsQueryHandler>();
builder.Services.AddScoped<GetFileLoadStatusHistoryQueryHandler>();
builder.Services.AddScoped<GetFileLoadProductsQueryHandler>();
builder.Services.AddScoped<GetFileLoadErrorsQueryHandler>();
builder.Services.AddScoped<GetFileLoadContentQueryHandler>();

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    var defaultResponseFactory = options.InvalidModelStateResponseFactory;

    options.InvalidModelStateResponseFactory = context =>
    {
        var exceedsSizeLimit = context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Any(error =>
                error.ErrorMessage.Contains(
                    "Request body too large",
                    StringComparison.OrdinalIgnoreCase) ||
                error.ErrorMessage.Contains(
                    "Multipart body length limit",
                    StringComparison.OrdinalIgnoreCase));

        if (exceedsSizeLimit)
        {
            return new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status413PayloadTooLarge,
                Title = "The file is too large.",
                Detail = "Please select an Excel file no larger than 10 MB."
            })
            {
                StatusCode = StatusCodes.Status413PayloadTooLarge
            };
        }

        return defaultResponseFactory(context);
    };
});

builder.Services.AddScoped<IFileProcessingOutbox, FileProcessingOutbox>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa únicamente el JWT obtenido desde /auth/login."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
