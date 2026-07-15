using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitTracker.Api.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .ValueGeneratedNever();

        builder.Property(user => user.Email)
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        builder.Property(user => user.Username)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(user => user.NormalizedUsername)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(user => user.NormalizedUsername)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired();

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();
    }
}
