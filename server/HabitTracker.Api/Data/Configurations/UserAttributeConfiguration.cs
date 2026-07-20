using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class UserAttributeConfiguration
    : IEntityTypeConfiguration<UserAttribute>
{
    public void Configure(
        EntityTypeBuilder<UserAttribute> builder)
    {
        builder.HasKey(userAttribute => userAttribute.Id);

        builder.Property(userAttribute => userAttribute.Id)
            .ValueGeneratedNever();

        builder.Property(userAttribute => userAttribute.UserId)
            .IsRequired();

        builder.Property(userAttribute => userAttribute.AttributeType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(userAttribute => userAttribute.CurrentXp)
            .IsRequired();

        builder.Property(userAttribute => userAttribute.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(userAttribute => new
        {
            userAttribute.UserId,
            userAttribute.AttributeType,
        })
            .IsUnique();

        builder.HasOne(userAttribute => userAttribute.User)
            .WithMany(user => user.UserAttributes)
            .HasForeignKey(userAttribute => userAttribute.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_user_attributes_current_xp",
                """
                "current_xp" >= 0
                """);

            tableBuilder.HasCheckConstraint(
                "ck_user_attributes_attribute_type",
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