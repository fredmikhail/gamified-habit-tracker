using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class UserSettingsConfiguration
    : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(userSettings => userSettings.Id);

        builder.Property(userSettings => userSettings.Id)
            .ValueGeneratedNever();

        builder.Property(userSettings => userSettings.DisplayName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(userSettings => userSettings.TimeZone)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(userSettings => userSettings.CreatedAtUtc)
            .IsRequired();

        builder.Property(userSettings => userSettings.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(userSettings => userSettings.UserId)
            .IsUnique();

        builder.HasOne(userSettings => userSettings.User)
            .WithOne(user => user.UserSettings)
            .HasForeignKey<UserSettings>(
                userSettings => userSettings.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
