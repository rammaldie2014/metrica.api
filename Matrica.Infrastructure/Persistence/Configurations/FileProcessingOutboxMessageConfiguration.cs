using Metrica.Domain.Entities;
using Metrica.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Metrica.Infrastructure.Persistence.Configurations
{
    public sealed class FileProcessingOutboxMessageConfiguration
        : IEntityTypeConfiguration<FileProcessingOutboxMessage>
    {
        public void Configure(
            EntityTypeBuilder<FileProcessingOutboxMessage> builder)
        {
            builder.ToTable("FileProcessingOutboxMessages");

            builder.HasKey(message => message.Id);

            builder.Property(message => message.Id)
                .ValueGeneratedOnAdd();

            builder.Property(message => message.FileLoadId)
                .IsRequired();

            builder.Property(message => message.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(message => message.PublishedAt)
                .HasColumnType("datetime2");

            builder.HasIndex(message => message.FileLoadId)
                .IsUnique();

            builder.HasIndex(message => new
            {
                message.CreatedAt,
                message.Id
            })
                .HasFilter("[PublishedAt] IS NULL");

            builder.HasOne<FileLoad>()
                .WithMany()
                .HasForeignKey(message => message.FileLoadId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}