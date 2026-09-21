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
    public class FileLoadConfiguration : IEntityTypeConfiguration<FileLoad>
    {
        public void Configure(EntityTypeBuilder<FileLoad> builder)
        {
            builder.ToTable("FileLoads");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.UserEmail)
                .HasMaxLength(254)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(2048)
                .IsRequired(false);

            builder.Property(x => x.Period)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(x => x.FinishedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            builder.HasIndex(x => new { x.Period, x.Status });
        }
    }
}
