using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Renova.Domain.Entities;

namespace Renova.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> entity)
    {
        entity.ToTable("Documents");

        entity.HasKey(document => document.Id);

        entity.Property(document => document.Title)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(document => document.FileName)
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(document => document.ContentType)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(document => document.UploadedBy)
            .HasMaxLength(200);

        entity.Property(document => document.UploadDate)
            .IsRequired();

        entity.Property(document => document.CreatedAt)
            .IsRequired();

        entity.Property(document => document.IsDeleted)
            .IsRequired();

        entity.HasOne(document => document.Person)
            .WithMany(person => person.Documents)
            .HasForeignKey(document => document.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(document => document.Tenant)
            .WithMany()
            .HasForeignKey(document => document.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(document => new { document.PersonId, document.UploadDate });

        entity.HasQueryFilter(document => !document.IsDeleted);
    }
}
