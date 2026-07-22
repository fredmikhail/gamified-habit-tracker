using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HabitTracker.Tests.Data;

public sealed class HabitConfigurationVersionConfigurationTests
{
    [Fact]
    public void HabitConfigurationVersion_WhenCreated_GeneratesId()
    {
        var configuration =
            new HabitConfigurationVersion();

        Assert.NotEqual(
            Guid.Empty,
            configuration.Id);
    }

    [Fact]
    public void HabitConfigurationVersionModel_ConfiguresExpectedProperties()
    {
        using var dbContext =
            CreateDbContext();

        var entityType =
            GetEntityType(dbContext);

        var categoryProperty =
            GetProperty(
                entityType,
                nameof(
                    HabitConfigurationVersion.Category));

        var frequencyProperty =
            GetProperty(
                entityType,
                nameof(
                    HabitConfigurationVersion.FrequencyType));

        var difficultyProperty =
            GetProperty(
                entityType,
                nameof(
                    HabitConfigurationVersion.Difficulty));

        Assert.False(categoryProperty.IsNullable);
        Assert.Equal(50, categoryProperty.GetMaxLength());
        Assert.Equal(
            typeof(string),
            categoryProperty
                .GetTypeMapping()
                .Converter!
                .ProviderClrType);

        Assert.False(frequencyProperty.IsNullable);
        Assert.Equal(20, frequencyProperty.GetMaxLength());
        Assert.Equal(
            typeof(string),
            frequencyProperty
                .GetTypeMapping()
                .Converter!
                .ProviderClrType);

        Assert.False(difficultyProperty.IsNullable);
        Assert.Equal(20, difficultyProperty.GetMaxLength());
        Assert.Equal(
            typeof(string),
            difficultyProperty
                .GetTypeMapping()
                .Converter!
                .ProviderClrType);
    }

    [Fact]
    public void HabitConfigurationVersionModel_ConfiguresUniqueHistoryIndexes()
    {
        using var dbContext =
            CreateDbContext();

        var entityType =
            GetEntityType(dbContext);

        Assert.Contains(
            entityType.GetIndexes(),
            index =>
                index.IsUnique
                && index.Properties
                    .Select(property => property.Name)
                    .SequenceEqual(
                        new[]
                        {
                            nameof(
                                HabitConfigurationVersion.HabitId),
                            nameof(
                                HabitConfigurationVersion.VersionNumber)
                        }));

        Assert.Contains(
            entityType.GetIndexes(),
            index =>
                index.IsUnique
                && index.Properties
                    .Select(property => property.Name)
                    .SequenceEqual(
                        new[]
                        {
                            nameof(
                                HabitConfigurationVersion.HabitId),
                            nameof(
                                HabitConfigurationVersion.EffectiveFromDate)
                        }));
    }

    [Fact]
    public void HabitConfigurationVersionModel_ConfiguresExpectedConstraints()
    {
        using var dbContext =
            CreateDbContext();

        var entityType =
            GetEntityType(dbContext);

        Assert.NotNull(
            entityType.FindCheckConstraint(
                "ck_habit_configuration_versions_version_number"));

        Assert.NotNull(
            entityType.FindCheckConstraint(
                "ck_habit_configuration_versions_frequency_target_count"));

        Assert.NotNull(
            entityType.FindCheckConstraint(
                "ck_habit_configuration_versions_difficulty"));

        Assert.NotNull(
            entityType.FindCheckConstraint(
                "ck_habit_configuration_versions_effective_date_range"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbContext(options);
    }

    private static IEntityType GetEntityType(
        AppDbContext dbContext)
    {
        return Assert.IsAssignableFrom<IEntityType>(
            dbContext.Model.FindEntityType(
                typeof(HabitConfigurationVersion)));
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return Assert.IsAssignableFrom<IProperty>(
            entityType.FindProperty(propertyName));
    }
}
