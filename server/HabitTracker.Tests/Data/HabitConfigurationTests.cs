using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HabitTracker.Tests.Data;

public sealed class HabitConfigurationTests
{
    [Fact]
    public void Habit_WhenCreated_GeneratesIdAndStartsActive()
    {
        var habit = new Habit();

        Assert.NotEqual(Guid.Empty, habit.Id);
        Assert.True(habit.IsActive);
    }

    [Fact]
    public void HabitModel_ConfiguresExpectedProperties()
    {
        using var dbContext = CreateDbContext();

        var habitEntity = GetHabitEntityType(dbContext);

        var idProperty = GetProperty(habitEntity, nameof(Habit.Id));
        var nameProperty = GetProperty(habitEntity, nameof(Habit.Name));
        var descriptionProperty =
            GetProperty(habitEntity, nameof(Habit.Description));
        var categoryProperty =
            GetProperty(habitEntity, nameof(Habit.Category));
        var frequencyProperty =
            GetProperty(habitEntity, nameof(Habit.FrequencyType));
        var difficultyProperty =
            GetProperty(habitEntity, nameof(Habit.Difficulty));

        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);

        Assert.False(nameProperty.IsNullable);
        Assert.Equal(100, nameProperty.GetMaxLength());

        Assert.True(descriptionProperty.IsNullable);
        Assert.Equal(500, descriptionProperty.GetMaxLength());

        Assert.True(categoryProperty.IsNullable);
        Assert.Equal(50, categoryProperty.GetMaxLength());

        Assert.False(frequencyProperty.IsNullable);
        Assert.Equal(20, frequencyProperty.GetMaxLength());

        var frequencyConverter =
            frequencyProperty.GetTypeMapping().Converter;

        Assert.NotNull(frequencyConverter);
        Assert.Equal(
            typeof(string),
            frequencyConverter.ProviderClrType);

        Assert.False(difficultyProperty.IsNullable);
        Assert.Equal(20, difficultyProperty.GetMaxLength());

        var difficultyConverter =
            difficultyProperty.GetTypeMapping().Converter;

        Assert.NotNull(difficultyConverter);
        Assert.Equal(
            typeof(string),
            difficultyConverter.ProviderClrType);
    }

    [Fact]
    public void HabitModel_ConfiguresRequiredUserRelationshipWithCascadeDelete()
    {
        using var dbContext = CreateDbContext();

        var habitEntity = GetHabitEntityType(dbContext);
        var foreignKey = Assert.Single(habitEntity.GetForeignKeys());
        var foreignKeyProperty = Assert.Single(foreignKey.Properties);

        Assert.Equal(typeof(User), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(Habit.UserId), foreignKeyProperty.Name);
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void HabitModel_ConfiguresUserAndActiveStatusIndex()
    {
        using var dbContext = CreateDbContext();

        var habitEntity = GetHabitEntityType(dbContext);

        Assert.Contains(
            habitEntity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    new[]
                    {
                        nameof(Habit.UserId),
                        nameof(Habit.IsActive)
                    }));
    }

    [Fact]
    public void HabitModel_ConfiguresExpectedCheckConstraints()
    {
        using var dbContext = CreateDbContext();

        var habitEntity = GetHabitEntityType(dbContext);

        Assert.NotNull(
            habitEntity.FindCheckConstraint(
                "ck_habits_frequency_target_count"));

        Assert.NotNull(
            habitEntity.FindCheckConstraint(
                "ck_habits_difficulty"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType GetHabitEntityType(
        AppDbContext dbContext)
    {
        return Assert.IsAssignableFrom<IEntityType>(
            dbContext.Model.FindEntityType(typeof(Habit)));
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return Assert.IsAssignableFrom<IProperty>(
            entityType.FindProperty(propertyName));
    }
}