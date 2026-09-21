using Metrica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matrica.Infrastructure.Persistence.Configurations
{
    public class FileLoadStatusHistoryConfiguration
        : IEntityTypeConfiguration<FileLoadStatusHistory>
    {
        public void Configure(EntityTypeBuilder<FileLoadStatusHistory> builder)
        {
            builder.ToTable("FileLoadStatusHistories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.FileLoadId)
                .IsRequired();

            builder.Property(x => x.PreviousStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasOne<FileLoad>()
                .WithMany()
                .HasForeignKey(x => x.FileLoadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.FileLoadId, x.CreatedAt });
        }
    }
}
