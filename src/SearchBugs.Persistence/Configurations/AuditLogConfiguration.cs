using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchBugs.Domain.AuditLogs;
using SearchBugs.Domain.Users;

namespace SearchBugs.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Id)
            .HasConversion(
                auditLogId => auditLogId.Value,
                value => new AuditLogId(value))
            .ValueGeneratedNever();

        builder.Property(al => al.RequestName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(al => al.RequestData)
            .IsRequired();

        builder.Property(al => al.ResponseData);

        builder.Property(al => al.IsSuccess)
            .IsRequired();

        builder.Property(al => al.ErrorMessage);

        builder.Property(al => al.Duration)
            .IsRequired();

        builder.Property(al => al.UserId)
            .HasConversion(
                userId => userId != null ? userId.Value : (Guid?)null,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.Property(al => al.UserName)
            .HasMaxLength(100);

        builder.Property(al => al.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(al => al.UserAgent)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(al => al.CreatedOnUtc)
            .IsRequired();

        builder.Property(al => al.ModifiedOnUtc);

        builder.HasIndex(al => al.RequestName);
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => al.CreatedOnUtc);
        builder.HasIndex(al => al.IsSuccess);

        builder.ToTable("audit_logs");
    }
}
