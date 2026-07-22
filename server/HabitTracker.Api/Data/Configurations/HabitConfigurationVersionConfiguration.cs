using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class HabitConfigurationVersionConfiguration
    : IEntityTypeConfiguration<HabitConfigurationVersion>
{
    public void Configure(
        EntityTypeBuilder<HabitConfigurationVersion> builder)
    {
        builder.HasKey(configuration => configuration.Id);

        builder.Property(configuration => configuration.Id)
            .ValueGeneratedNever();

        builder.Property(configuration => configuration.HabitId)
            .IsRequired();

        builder.Property(configuration => configuration.VersionNumber)
            .IsRequired();

        builder.Property(configuration => configuration.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(configuration => configuration.FrequencyType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(configuration => configuration.TargetCount)
            .IsRequired();

        builder.Property(configuration => configuration.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(configuration => configuration.EffectiveFromDate)
            .IsRequired();

        builder.Property(configuration =>
                configuration.EffectiveToDateExclusive);

        builder.Property(configuration => configuration.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(configuration => new
        {
            configuration.HabitId,
            configuration.VersionNumber
        })
            .IsUnique();

        builder.HasIndex(configuration => new
        {
            configuration.HabitId,
            configuration.EffectiveFromDate
        })
            .IsUnique();

        builder.HasIndex(configuration => configuration.HabitId)
            .HasFilter(
                "\"effective_to_date_exclusive\" IS NULL")
            .IsUnique();

        builder.HasOne(configuration => configuration.Habit)
            .WithMany(habit => habit.HabitConfigurationVersions)
            .HasForeignKey(configuration => configuration.HabitId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_habit_configuration_versions_version_number",
                """
                "version_number" >= 1
                """);

            tableBuilder.HasCheckConstraint(
                "ck_habit_configuration_versions_frequency_target_count",
                """
                ("frequency_type" = 'Daily' AND "target_count" = 1)
                OR
                ("frequency_type" = 'Weekly' AND "target_count" BETWEEN 1 AND 7)
                """);

            tableBuilder.HasCheckConstraint(
                "ck_habit_configuration_versions_difficulty",
                """
                "difficulty" IN ('Easy', 'Medium', 'Hard', 'Elite')
                """);

            tableBuilder.HasCheckConstraint(
                "ck_habit_configuration_versions_effective_date_range",
                """
                "effective_to_date_exclusive" IS NULL
                OR
                "effective_to_date_exclusive" > "effective_from_date"
                """);
        });
    }
}
