using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HabitTracker.Tests.Data;

public sealed class XpTransactionConfigurationTests
{
    [Fact]
    public void XpTransaction_WhenCreated_GeneratesId()
    {
        var transaction = new XpTransaction();

        Assert.NotEqual(Guid.Empty, transaction.Id);
    }

    [Fact]
    public void XpTransactionModel_ConfiguresExpectedProperties()
    {
        using var dbContext = CreateDbContext();

        var transactionEntity = GetTransactionEntityType(dbContext);

        var idProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.Id));

        var userIdProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.UserId));

        var habitCompletionIdProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.HabitCompletionId));

        var attributeTypeProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.AttributeType));

        var amountProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.Amount));

        var reasonProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.Reason));

        var createdAtUtcProperty = GetProperty(
            transactionEntity,
            nameof(XpTransaction.CreatedAtUtc));

        Assert.Equal(
            ValueGenerated.Never,
            idProperty.ValueGenerated);

        Assert.False(userIdProperty.IsNullable);
        Assert.False(habitCompletionIdProperty.IsNullable);

        Assert.False(attributeTypeProperty.IsNullable);
        Assert.Equal(30, attributeTypeProperty.GetMaxLength());

        var attributeTypeConverter =
            attributeTypeProperty.GetTypeMapping().Converter;

        Assert.NotNull(attributeTypeConverter);

        Assert.Equal(
            typeof(string),
            attributeTypeConverter.ProviderClrType);

        Assert.False(amountProperty.IsNullable);

        Assert.False(reasonProperty.IsNullable);
        Assert.Equal(100, reasonProperty.GetMaxLength());

        Assert.False(createdAtUtcProperty.IsNullable);
    }

    [Fact]
    public void XpTransactionModel_ConfiguresUniqueCompletionAttributeAndReasonIndex()
    {
        using var dbContext = CreateDbContext();

        var transactionEntity = GetTransactionEntityType(dbContext);

        var index = Assert.Single(
            transactionEntity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    new[]
                    {
                        nameof(XpTransaction.HabitCompletionId),
                        nameof(XpTransaction.AttributeType),
                        nameof(XpTransaction.Reason)
                    }));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void XpTransactionModel_ConfiguresUserHistoryIndex()
    {
        using var dbContext = CreateDbContext();

        var transactionEntity = GetTransactionEntityType(dbContext);

        Assert.Contains(
            transactionEntity.GetIndexes(),
            index => index.Properties
                .Select(property => property.Name)
                .SequenceEqual(
                    new[]
                    {
                        nameof(XpTransaction.UserId),
                        nameof(XpTransaction.CreatedAtUtc),
                    }));
    }

    [Fact]
    public void XpTransactionModel_ConfiguresExpectedRelationships()
    {
        using var dbContext = CreateDbContext();

        var transactionEntity = GetTransactionEntityType(dbContext);
        var foreignKeys = transactionEntity.GetForeignKeys().ToList();

        Assert.Equal(2, foreignKeys.Count);

        var userForeignKey = Assert.Single(
            foreignKeys,
            foreignKey =>
                foreignKey.PrincipalEntityType.ClrType
                    == typeof(User));

        Assert.Equal(
            nameof(XpTransaction.UserId),
            Assert.Single(userForeignKey.Properties).Name);

        Assert.True(userForeignKey.IsRequired);

        Assert.Equal(
            DeleteBehavior.Cascade,
            userForeignKey.DeleteBehavior);

        var completionForeignKey = Assert.Single(
            foreignKeys,
            foreignKey =>
                foreignKey.PrincipalEntityType.ClrType
                    == typeof(HabitCompletion));

        Assert.Equal(
            nameof(XpTransaction.HabitCompletionId),
            Assert.Single(completionForeignKey.Properties).Name);

        Assert.True(completionForeignKey.IsRequired);

        Assert.Equal(
            DeleteBehavior.Cascade,
            completionForeignKey.DeleteBehavior);
    }

    [Fact]
    public void XpTransactionModel_ConfiguresExpectedCheckConstraints()
    {
        using var dbContext = CreateDbContext();

        var transactionEntity = GetTransactionEntityType(dbContext);

        Assert.NotNull(
            transactionEntity.FindCheckConstraint(
                "ck_xp_transactions_amount"));

        Assert.NotNull(
            transactionEntity.FindCheckConstraint(
                "ck_xp_transactions_attribute_type"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType GetTransactionEntityType(
        AppDbContext dbContext)
    {
        return Assert.IsAssignableFrom<IEntityType>(
            dbContext.Model.FindEntityType(
                typeof(XpTransaction)));
    }

    private static IProperty GetProperty(
        IEntityType entityType,
        string propertyName)
    {
        return Assert.IsAssignableFrom<IProperty>(
            entityType.FindProperty(propertyName));
    }
}
