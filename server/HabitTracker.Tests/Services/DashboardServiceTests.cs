using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_WhenUserHasNoXp_ReturnsLevelOneWithZeroProgress()
    {
        await using var dbContext =
            CreateDbContext();

        var dashboardService =
            new DashboardService(
                dbContext,
                new XpService());

        var response =
            await dashboardService.GetDashboardAsync(
                Guid.CreateVersion7());

        Assert.Equal(
            0,
            response.OverallProgress.TotalXp);

        Assert.Equal(
            1,
            response.OverallProgress.Level);

        Assert.Equal(
            0,
            response.OverallProgress
                .XpIntoCurrentLevel);

        Assert.Equal(
            200,
            response.OverallProgress
                .XpNeededForNextLevel);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasXp_SumsOnlyOwnedTransactions()
    {
        await using var dbContext =
            CreateDbContext();

        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var createdAtUtc = DateTime.UtcNow;

        dbContext.XpTransactions.AddRange(
            CreateTransaction(
                userId,
                AttributeType.Fitness,
                140,
                createdAtUtc),
            CreateTransaction(
                userId,
                AttributeType.Discipline,
                100,
                createdAtUtc.AddMinutes(1)),
            CreateTransaction(
                userId,
                AttributeType.Focus,
                60,
                createdAtUtc.AddMinutes(2)),
            CreateTransaction(
                otherUserId,
                AttributeType.Mind,
                999,
                createdAtUtc.AddMinutes(3)));

        await dbContext.SaveChangesAsync();

        var dashboardService =
            new DashboardService(
                dbContext,
                new XpService());

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        Assert.Equal(
            300,
            response.OverallProgress.TotalXp);

        Assert.Equal(
            2,
            response.OverallProgress.Level);

        Assert.Equal(
            100,
            response.OverallProgress
                .XpIntoCurrentLevel);

        Assert.Equal(
            250,
            response.OverallProgress
                .XpNeededForNextLevel);
    }

    private static XpTransaction CreateTransaction(
        Guid userId,
        AttributeType attributeType,
        int amount,
        DateTime createdAtUtc)
    {
        return new XpTransaction
        {
            UserId = userId,
            HabitCompletionId =
                Guid.CreateVersion7(),
            AttributeType = attributeType,
            Amount = amount,
            Reason = "Dashboard test",
            CreatedAtUtc = createdAtUtc
        };
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
