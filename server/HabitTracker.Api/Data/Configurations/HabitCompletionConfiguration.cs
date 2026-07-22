using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class HabitCompletionConfiguration
    : IEntityTypeConfiguration<HabitCompletion>
{
    public void Configure(EntityTypeBuilder<HabitCompletion> builder)
    {
        builder.HasKey(completion => completion.Id);

        builder.Property(completion => completion.Id)
            .ValueGeneratedNever();

        builder.Property(completion =>
                completion.HabitConfigurationVersionId)
            .IsRequired();

        builder.Property(completion => completion.Notes)
            .HasMaxLength(500);

        builder.HasIndex(completion => new
        {
            completion.HabitId,
            completion.CompletedDate,
        })
            .IsUnique();

        builder.HasIndex(completion => new
        {
            completion.UserId,
            completion.CompletedDate,
        });

        builder.HasIndex(completion =>
            completion.HabitConfigurationVersionId);

        builder.HasOne(completion => completion.User)
            .WithMany(user => user.HabitCompletions)
            .HasForeignKey(completion => completion.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(completion => completion.Habit)
            .WithMany(habit => habit.HabitCompletions)
            .HasForeignKey(completion => completion.HabitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(completion =>
                completion.HabitConfigurationVersion)
            .WithMany(configuration =>
                configuration.HabitCompletions)
            .HasForeignKey(completion =>
                completion.HabitConfigurationVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
