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
    public class ProcessedProductConfiguration
        : IEntityTypeConfiguration<ProcessedProduct>
    {
        public void Configure(EntityTypeBuilder<ProcessedProduct> builder)
        {
            builder.ToTable("ProcessedProducts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.FileLoadId)
                .IsRequired();

            builder.Property(x => x.Period)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.ProductCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Stock)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasOne<FileLoad>()
                .WithMany()
                .HasForeignKey(x => x.FileLoadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ProductCode)
                .IsUnique();

            builder.HasIndex(x => x.FileLoadId);
        }
    }
}
