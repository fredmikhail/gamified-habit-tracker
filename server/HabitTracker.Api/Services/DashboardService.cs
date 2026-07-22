using HabitTracker.Api.Data;
using HabitTracker.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services;

public sealed class DashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly XpService _xpService;

    public DashboardService(
        AppDbContext dbContext,
        XpService xpService)
    {
        _dbContext = dbContext;
        _xpService = xpService;
    }

    public async Task<DashboardResponse> GetDashboardAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var totalXp =
            await _dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId)
                .SumAsync(
                    transaction => transaction.Amount,
                    cancellationToken);

        var progress =
            _xpService.CalculateOverallLevelProgress(
                totalXp);

        return new DashboardResponse
        {
            OverallProgress =
                new OverallProgressResponse
                {
                    TotalXp = totalXp,
                    Level = progress.Level,
                    XpIntoCurrentLevel =
                        progress.XpIntoCurrentLevel,
                    XpNeededForNextLevel =
                        progress.XpNeededForNextLevel
                }
        };
    }
}
