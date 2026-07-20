using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HabitTracker.Tests.Data;

public sealed class HabitAttributeRewardConfigurationTests
{
    [Fact]
    public void HabitAttributeReward_WhenCreated_GeneratesId()
    {
        var reward = new HabitAttributeReward();

        Assert.NotEqual(Guid.Empty, reward.Id);
    }

    [Fact]
    public void HabitAttributeRewardModel_ConfiguresExpectedProperties()
    {
        using var dbContext = CreateDbContext();

        var rewardEntity = GetRewardEntityType(dbContext);

        var idProperty = GetProperty(
            rewardEntity,
            nameof(HabitAttributeReward.Id));

        var habitIdProperty = GetProperty(
            rewardEntity,
            nameof(HabitAttributeReward.HabitId));

        var attributeTypeProperty = GetProperty(
            rewardEntity,
            nameof(HabitAttributeReward.AttributeType));

        var xpAmountProperty = GetProperty(
            rewardEntity,
            nameof(HabitAttributeReward.XpAmount));

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);

        Assert.False(habitIdProperty.IsNullable);

        Assert.False(attributeTypeProperty.IsNullable);
        Assert.Equal(30, attributeTypeProperty.GetMaxLength());

        var attributeTypeConverter =
            attributeTypeProperty.GetTypeMapping().Converter;

        Assert.NotNull(attributeTypeConverter);

        Assert.Equal(
            typeof(string),
            attributeTypeConverter.ProviderClrType);

        Assert.False(xpAmountProperty.IsNullable);
    }

    [Fact]
    public void HabitAttributeRewardModel_ConfiguresUniqueHabitAndAttributeIndex()
    {
        using var dbContext = CreateDbContext();

        var rewardEntity = GetRewardEntityType(dbContext);

        var index = Assert.Single(
            rewardEntity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    new[]
                    {
                        nameof(HabitAttributeReward.HabitId),
                        nameof(HabitAttributeReward.AttributeType),
                    }));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void HabitAttributeRewardModel_ConfiguresRequiredHabitRelationshipWithCascadeDelete()
    {
        using var dbContext = CreateDbContext();

        var rewardEntity = GetRewardEntityType(dbContext);

        var foreignKey = Assert.Single(
            rewardEntity.GetForeignKeys());

        var foreignKeyProperty = Assert.Single(
            foreignKey.Properties);

        Assert.Equal(
            typeof(Habit),
            foreignKey.PrincipalEntityType.ClrType);

        Assert.Equal(
            nameof(HabitAttributeReward.HabitId),
            foreignKeyProperty.Name);

        Assert.True(foreignKey.IsRequired);

        Assert.Equal(
            DeleteBehavior.Cascade,
            foreignKey.DeleteBehavior);
    }

    [Fact]
    public void HabitAttributeRewardModel_ConfiguresExpectedCheckConstraints()
    {
        using var dbContext = CreateDbContext();

        var rewardEntity = GetRewardEntityType(dbContext);

        Assert.NotNull(
            rewardEntity.FindCheckConstraint(
                "ck_habit_attribute_rewards_xp_amount"));

        Assert.NotNull(
            rewardEntity.FindCheckConstraint(
                "ck_habit_attribute_rewards_attribute_type"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType GetRewardEntityType(
        AppDbContext dbContext)
    {
        return Assert.IsAssignableFrom<IEntityType>(
            dbContext.Model.FindEntityType(
                typeof(HabitAttributeReward)));
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return Assert.IsAssignableFrom<IProperty>(
            entityType.FindProperty(propertyName));
    }
}