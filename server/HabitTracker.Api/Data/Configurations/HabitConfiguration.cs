using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(habit => habit.Id);

        builder.Property(habit => habit.Id)
            .ValueGeneratedNever();

        builder.Property(habit => habit.UserId)
            .IsRequired();

        builder.Property(habit => habit.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(habit => habit.Description)
            .HasMaxLength(500);

        builder.Property(habit => habit.Category)
            .HasMaxLength(50);

        builder.Property(habit => habit.FrequencyType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(habit => habit.TargetCount)
            .IsRequired();

        builder.Property(habit => habit.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(habit => habit.IsActive)
            .IsRequired();

        builder.Property(habit => habit.CreatedAtUtc)
            .IsRequired();

        builder.Property(habit => habit.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(habit => new
        {
            habit.UserId,
            habit.IsActive
        });

        builder.HasOne(habit => habit.User)
            .WithMany(user => user.Habits)
            .HasForeignKey(habit => habit.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_habits_frequency_target_count",
                """
                ("frequency_type" = 'Daily' AND "target_count" = 1)
                OR
                ("frequency_type" = 'Weekly' AND "target_count" BETWEEN 1 AND 7)
                """);

            tableBuilder.HasCheckConstraint(
                "ck_habits_difficulty",
                """
                "difficulty" IN ('Easy', 'Medium', 'Hard', 'Elite')
                """);
        });


    }
}