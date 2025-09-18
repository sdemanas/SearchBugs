using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchBugs.Domain.Roles;
using SearchBugs.Persistence.Constants;
using Shared.Extensions;

namespace SearchBugs.Persistence.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder) =>
        builder
            .Tap(ConfigureDataStructure)
            .Tap(ConfigureRelationships);

    private static void ConfigureDataStructure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(TableNames.Roles);

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id).ValueGeneratedNever();

        builder.Property(role => role.Name).HasMaxLength(100);

        builder.HasData(Role.GetValues());
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Role> builder)
    {
        builder.HasMany(role => role.Permissions)
            .WithMany()
            .UsingEntity<RolePermission>(
                l => l.HasOne<Permission>().WithMany().HasForeignKey(rp => rp.PermissionId),
                r => r.HasOne<Role>().WithMany().HasForeignKey(rp => rp.RoleId),
                j => j.ToTable(TableNames.RolePermissions)
            );
    }
}
