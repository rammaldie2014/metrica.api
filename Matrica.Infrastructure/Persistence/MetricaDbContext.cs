using Metrica.Application.Interfaces;
using Metrica.Domain.Entities;
using Metrica.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrica.Infrastructure.Persistence
{
    public class MetricaDbContext : DbContext, IUnitOfWork
    {
        public MetricaDbContext(DbContextOptions<MetricaDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileLoad> FileLoads => Set<FileLoad>();

        public DbSet<FileLoadStatusHistory> FileLoadStatusHistories =>
            Set<FileLoadStatusHistory>();

        public DbSet<ProcessedProduct> ProcessedProducts =>
            Set<ProcessedProduct>();

        public DbSet<FileLoadError> FileLoadErrors =>
            Set<FileLoadError>();

        public DbSet<FileLoadNotificationOutboxMessage> FileLoadNotificationOutboxMessages =>
            Set<FileLoadNotificationOutboxMessage>();

        public DbSet<FileProcessingOutboxMessage> FileProcessingOutboxMessages =>
            Set<FileProcessingOutboxMessage>();

        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(action);

            await using var transaction =
                await Database.BeginTransactionAsync(
                    isolationLevel,
                    cancellationToken);

            await action(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MetricaDbContext).Assembly);
        }
    }
}
