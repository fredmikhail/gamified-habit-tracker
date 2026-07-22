using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class XpTransactionConfiguration
    : IEntityTypeConfiguration<XpTransaction>
{
    public void Configure(
        EntityTypeBuilder<XpTransaction> builder)
    {
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedNever();

        builder.Property(transaction => transaction.UserId)
            .IsRequired();

        builder.Property(transaction => transaction.HabitCompletionId)
            .IsRequired();

        builder.Property(transaction => transaction.AttributeType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(transaction => transaction.Amount)
            .IsRequired();

        builder.Property(transaction => transaction.Reason)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(transaction => transaction.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(transaction => new
        {
            transaction.HabitCompletionId,
            transaction.AttributeType,
            transaction.Reason,
        })
            .IsUnique();

        builder.HasIndex(transaction => new
        {
            transaction.UserId,
            transaction.CreatedAtUtc,
        });

        builder.HasOne(transaction => transaction.User)
            .WithMany(user => user.XpTransactions)
            .HasForeignKey(transaction => transaction.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(transaction => transaction.HabitCompletion)
            .WithMany(completion => completion.XpTransactions)
            .HasForeignKey(transaction => transaction.HabitCompletionId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.ToTable(tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "ck_xp_transactions_amount",
                """
                "amount" <> 0
                """);

            tableBuilder.HasCheckConstraint(
                "ck_xp_transactions_attribute_type",
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
