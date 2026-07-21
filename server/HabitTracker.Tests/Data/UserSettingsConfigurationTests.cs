using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HabitTracker.Tests.Data;

public sealed class UserSettingsConfigurationTests
{
    [Fact]
    public void UserSettings_WhenCreated_DefaultsWeekStartToMonday()
    {
        var userSettings = new UserSettings();

        Assert.Equal(
            WeekStartDay.Monday,
            userSettings.WeekStartsOn);
    }

    [Fact]
    public void UserSettingsModel_ConfiguresWeekStartAsRequiredString()
    {
        using var dbContext = CreateDbContext();

        var entityType = GetUserSettingsEntityType(dbContext);

        var property = Assert.IsAssignableFrom<IProperty>(
            entityType.FindProperty(
                nameof(UserSettings.WeekStartsOn)));

        Assert.False(property.IsNullable);
        Assert.Equal(20, property.GetMaxLength());

        var converter = property.GetTypeMapping().Converter;

        Assert.NotNull(converter);
        Assert.Equal(
            typeof(string),
            converter.ProviderClrType);

        Assert.Equal(
            WeekStartDay.Monday,
            property.GetDefaultValue());
    }

    [Fact]
    public void UserSettingsModel_ConfiguresWeekStartCheckConstraint()
    {
        using var dbContext = CreateDbContext();

        var entityType = GetUserSettingsEntityType(dbContext);

        Assert.NotNull(
            entityType.FindCheckConstraint(
                "ck_user_settings_week_starts_on"));
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

    private static IEntityType GetUserSettingsEntityType(
        AppDbContext dbContext)
    {
        return Assert.IsAssignableFrom<IEntityType>(
            dbContext.Model.FindEntityType(
                typeof(UserSettings)));
    }
}
