using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class AttributeServiceTests
{
    [Fact]
    public async Task GetUserAttributesAsync_ReturnsAllSupportedAttributes()
    {
        await using var dbContext =
            CreateDbContext();

        var userId =
            Guid.CreateVersion7();

        dbContext.UserAttributes.AddRange(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Discipline,
                CurrentXp = 99,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 225,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service =
            new AttributeService(
                dbContext,
                new XpService());

        var attributes =
            await service.GetUserAttributesAsync(
                userId);

        Assert.Equal(
            Enum.GetValues<AttributeType>(),
            attributes
                .Select(attribute =>
                    attribute.AttributeType)
                .ToArray());

        var discipline =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Discipline);

        Assert.Equal(99, discipline.CurrentXp);
        Assert.Equal(1, discipline.Level);
        Assert.Equal(
            99,
            discipline.XpIntoCurrentLevel);
        Assert.Equal(
            100,
            discipline.XpNeededForNextLevel);

        var fitness =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Fitness);

        Assert.Equal(225, fitness.CurrentXp);
        Assert.Equal(3, fitness.Level);
        Assert.Equal(
            0,
            fitness.XpIntoCurrentLevel);
        Assert.Equal(
            150,
            fitness.XpNeededForNextLevel);

        var vitality =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Vitality);

        Assert.Equal(0, vitality.CurrentXp);
        Assert.Equal(1, vitality.Level);
        Assert.Equal(
            0,
            vitality.XpIntoCurrentLevel);
        Assert.Equal(
            100,
            vitality.XpNeededForNextLevel);
    }

    [Fact]
    public async Task GetUserAttributesAsync_DoesNotReturnAnotherUsersXp()
    {
        await using var dbContext =
            CreateDbContext();

        var requestingUserId =
            Guid.CreateVersion7();

        var otherUserId =
            Guid.CreateVersion7();

        dbContext.UserAttributes.Add(
            new UserAttribute
            {
                UserId = otherUserId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 999,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service =
            new AttributeService(
                dbContext,
                new XpService());

        var attributes =
            await service.GetUserAttributesAsync(
                requestingUserId);

        Assert.All(
            attributes,
            attribute =>
                Assert.Equal(
                    0,
                    attribute.CurrentXp));
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
}
