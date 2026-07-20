using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class HabitAttributeRewardConfiguration
    : IEntityTypeConfiguration<HabitAttributeReward>
{
    public void Configure(
        EntityTypeBuilder<HabitAttributeReward> builder)
    {
        builder.HasKey(reward => reward.Id);

        builder.Property(reward => reward.Id)
            .ValueGeneratedNever();

        builder.Property(reward => reward.HabitId)
            .IsRequired();

        builder.Property(reward => reward.AttributeType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(reward => reward.XpAmount)
            .IsRequired();

        builder.HasIndex(reward => new
        {
            reward.HabitId,
            reward.AttributeType,
        })
            .IsUnique();

        builder.HasOne(reward => reward.Habit)
            .WithMany(habit => habit.HabitAttributeRewards)
            .HasForeignKey(reward => reward.HabitId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_habit_attribute_rewards_xp_amount",
                """
                "xp_amount" > 0
                """);

            tableBuilder.HasCheckConstraint(
                "ck_habit_attribute_rewards_attribute_type",
                """
                "attribute_type" IN (
                    'Discipline',
                    'Fitness',
                    'Vitality',
                    'Focus',
                    'Mind',
                    'Resilience',
                    'Social',
                    'Purpose'
                )
                """);
        });
    }
}