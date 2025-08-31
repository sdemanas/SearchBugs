using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchBugs.Domain.Users;

namespace SearchBugs.Persistence.Configurations;

public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                profileId => profileId.Value,
                value => new ProfileId(value))
            .ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .IsRequired();

        builder.Property(p => p.Bio)
            .HasMaxLength(2000);

        builder.Property(p => p.Location)
            .HasMaxLength(100);

        builder.Property(p => p.Website)
            .HasMaxLength(500);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Company)
            .HasMaxLength(200);

        builder.Property(p => p.JobTitle)
            .HasMaxLength(200);

        builder.Property(p => p.TwitterHandle)
            .HasMaxLength(50);

        builder.Property(p => p.LinkedInProfile)
            .HasMaxLength(500);

        builder.Property(p => p.GitHubProfile)
            .HasMaxLength(500);

        builder.Property(p => p.IsPublic)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.DateOfBirth);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(p => p.TimeZone)
            .HasMaxLength(100);

        builder.Property(p => p.PreferredLanguage)
            .HasMaxLength(10);

        builder.Property(p => p.CreatedOnUtc)
            .IsRequired();

        builder.Property(p => p.ModifiedOnUtc);

        // Relationships
        builder.HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<Profile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.HasIndex(p => p.IsPublic);
        builder.HasIndex(p => p.CreatedOnUtc);

        builder.ToTable("profiles");
    }
}
