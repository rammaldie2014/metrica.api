using Metrica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Metrica.Infrastructure.Persistence.Configurations
{
    public class FileLoadErrorConfiguration
            : IEntityTypeConfiguration<FileLoadError>
    {
        public void Configure(EntityTypeBuilder<FileLoadError> builder)
        {
            builder.ToTable("FileLoadErrors");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseIdentityColumn();

            builder.Property(x => x.FileLoadId)
                .IsRequired();

            builder.Property(x => x.RowNumber)
                .IsRequired(false);

            builder.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Message)
                .HasMaxLength(2000)
                .IsRequired();

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
