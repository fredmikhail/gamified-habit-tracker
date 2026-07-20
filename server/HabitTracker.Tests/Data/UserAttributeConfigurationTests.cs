using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HabitTracker.Tests.Data;

public sealed class UserAttributeConfigurationTests
{
    [Fact]
    public void UserAttribute_WhenCreated_GeneratesIdAndStartsWithZeroXp()
    {
        var userAttribute = new UserAttribute();

        Assert.NotEqual(Guid.Empty, userAttribute.Id);
        Assert.Equal(0, userAttribute.CurrentXp);
    }

    [Fact]
    public void UserAttributeModel_ConfiguresExpectedProperties()
    {
        using var dbContext = CreateDbContext();

        var userAttributeEntity = GetUserAttributeEntityType(dbContext);

        var idProperty = GetProperty(
            userAttributeEntity,
            nameof(UserAttribute.Id));

        var userIdProperty = GetProperty(
            userAttributeEntity,
            nameof(UserAttribute.UserId));

        var attributeTypeProperty = GetProperty(
            userAttributeEntity,
            nameof(UserAttribute.AttributeType));

        var currentXpProperty = GetProperty(
            userAttributeEntity,
            nameof(UserAttribute.CurrentXp));

        var updatedAtUtcProperty = GetProperty(
            userAttributeEntity,
            nameof(UserAttribute.UpdatedAtUtc));

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);

        Assert.False(userIdProperty.IsNullable);

        Assert.False(attributeTypeProperty.IsNullable);
        Assert.Equal(30, attributeTypeProperty.GetMaxLength());

        var attributeTypeConverter =
            attributeTypeProperty.GetTypeMapping().Converter;

        Assert.NotNull(attributeTypeConverter);
        Assert.Equal(
            typeof(string),
            attributeTypeConverter.ProviderClrType);

        Assert.False(currentXpProperty.IsNullable);
        Assert.False(updatedAtUtcProperty.IsNullable);
    }

    [Fact]
    public void UserAttributeModel_ConfiguresUniqueUserAndAttributeTypeIndex()
    {
        using var dbContext = CreateDbContext();

        var userAttributeEntity = GetUserAttributeEntityType(dbContext);

        var index = Assert.Single(
            userAttributeEntity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    new[]
                    {
                        nameof(UserAttribute.UserId),
                        nameof(UserAttribute.AttributeType),
                    }));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void UserAttributeModel_ConfiguresRequiredUserRelationshipWithCascadeDelete()
    {
        using var dbContext = CreateDbContext();

        var userAttributeEntity = GetUserAttributeEntityType(dbContext);

        var foreignKey = Assert.Single(
            userAttributeEntity.GetForeignKeys());

        var foreignKeyProperty = Assert.Single(
            foreignKey.Properties);

        Assert.Equal(
            typeof(User),
            foreignKey.PrincipalEntityType.ClrType);

        Assert.Equal(
            nameof(UserAttribute.UserId),
            foreignKeyProperty.Name);

        Assert.True(foreignKey.IsRequired);

        Assert.Equal(
            DeleteBehavior.Cascade,
            foreignKey.DeleteBehavior);
    }

    [Fact]
    public void UserAttributeModel_ConfiguresExpectedCheckConstraints()
    {
        using var dbContext = CreateDbContext();

        var userAttributeEntity = GetUserAttributeEntityType(dbContext);

        Assert.NotNull(
            userAttributeEntity.FindCheckConstraint(
                "ck_user_attributes_current_xp"));

        Assert.NotNull(
            userAttributeEntity.FindCheckConstraint(
                "ck_user_attributes_attribute_type"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType GetUserAttributeEntityType(
        AppDbContext dbContext)
    {
        return Assert.IsAssignableFrom<IEntityType>(
            dbContext.Model.FindEntityType(typeof(UserAttribute)));
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return Assert.IsAssignableFrom<IProperty>(
            entityType.FindProperty(propertyName));
    }
}