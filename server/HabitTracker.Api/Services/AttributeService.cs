using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services;

public sealed class AttributeService
{
    private const string HabitCompletionReason =
        "Habit completion";

    private const string HabitCompletionUndoReason =
        "Habit completion undo";

    private const int RecentXpTransactionLimit = 6;
    private const int LevelUpQueueLimit = 3;

    private readonly AppDbContext _dbContext;
    private readonly XpService _xpService;

    public AttributeService(
        AppDbContext dbContext,
        XpService xpService)
    {
        _dbContext = dbContext;
        _xpService = xpService;
    }

    public async Task<IReadOnlyList<UserAttributeResponse>>
        GetUserAttributesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var storedAttributes =
            await _dbContext.UserAttributes
                .AsNoTracking()
                .Where(attribute =>
                    attribute.UserId == userId)
                .ToDictionaryAsync(
                    attribute =>
                        attribute.AttributeType,
                    cancellationToken);

        var responses =
            new List<UserAttributeResponse>();

        foreach (var attributeType
            in Enum.GetValues<AttributeType>())
        {
            var currentXp =
                storedAttributes.TryGetValue(
                    attributeType,
                    out var storedAttribute)
                    ? storedAttribute.CurrentXp
                    : 0;

            var progress =
                _xpService
                    .CalculateAttributeLevelProgress(
                        currentXp);

            responses.Add(
                new UserAttributeResponse
                {
                    AttributeType = attributeType,
                    CurrentXp = currentXp,
                    Level = progress.Level,
                    XpIntoCurrentLevel =
                        progress.XpIntoCurrentLevel,
                    XpNeededForNextLevel =
                        progress.XpNeededForNextLevel
                });
        }

        return responses;
    }

    public async Task<AttributeOverviewResponse>
        GetAttributeOverviewAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var attributes =
            await GetUserAttributesAsync(
                userId,
                cancellationToken);

        var totalAttributeXp =
            attributes.Sum(attribute =>
                attribute.CurrentXp);

        var progressedAttributes =
            attributes
                .Where(attribute =>
                    attribute.CurrentXp > 0)
                .ToList();

        var strongestAttribute =
            progressedAttributes
                .OrderByDescending(attribute =>
                    attribute.CurrentXp)
                .ThenBy(attribute =>
                    attribute.AttributeType)
                .FirstOrDefault();

        var needsFocusAttribute =
            progressedAttributes.Count == 0
                ? null
                : attributes
                    .OrderBy(attribute =>
                        attribute.CurrentXp)
                    .ThenBy(attribute =>
                        attribute.AttributeType)
                    .First();

        var closestToLevelUp =
            attributes
                .Select(attribute =>
                    new AttributeLevelUpResponse
                    {
                        AttributeType =
                            attribute.AttributeType,
                        CurrentLevel =
                            attribute.Level,
                        XpRemaining =
                            attribute.XpNeededForNextLevel
                            - attribute.XpIntoCurrentLevel
                    })
                .OrderBy(attribute =>
                    attribute.XpRemaining)
                .ThenBy(attribute =>
                    attribute.AttributeType)
                .Take(LevelUpQueueLimit)
                .ToList();

        var recentXpTransactions =
            await _dbContext.XpTransactions
                .AsNoTracking()
                .Where(transaction =>
                    transaction.UserId == userId)
                .OrderByDescending(transaction =>
                    transaction.CreatedAtUtc)
                .ThenByDescending(transaction =>
                    transaction.Id)
                .Select(transaction =>
                    new XpTransactionResponse
                    {
                        Id = transaction.Id,
                        HabitName =
                            transaction
                                .HabitCompletion
                                .Habit
                                .Name,
                        AttributeType =
                            transaction.AttributeType,
                        Amount = transaction.Amount,
                        Reason = transaction.Reason,
                        CreatedAtUtc =
                            transaction.CreatedAtUtc
                    })
                .Take(RecentXpTransactionLimit)
                .ToListAsync(cancellationToken);

        return new AttributeOverviewResponse
        {
            Attributes = attributes,
            TotalAttributeXp = totalAttributeXp,
            BalanceScore =
                CalculateBalanceScore(
                    attributes),
            StrongestAttribute =
                strongestAttribute,
            NeedsFocusAttribute =
                needsFocusAttribute,
            ClosestToLevelUp =
                closestToLevelUp,
            RecentXpTransactions =
                recentXpTransactions
        };
    }

    public async Task ApplyCompletionRewardsAsync(
        Guid userId,
        HabitCompletion completion,
        ICollection<HabitAttributeReward> rewards,
        DateTime awardedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var attributeTypes =
            rewards
                .Select(reward => reward.AttributeType)
                .Distinct()
                .ToList();

        var userAttributes =
            await _dbContext.UserAttributes
                .Where(attribute =>
                    attribute.UserId == userId
                    && attributeTypes.Contains(
                        attribute.AttributeType))
                .ToDictionaryAsync(
                    attribute => attribute.AttributeType,
                    cancellationToken);

        foreach (var reward in rewards)
        {
            if (!userAttributes.TryGetValue(
                    reward.AttributeType,
                    out var userAttribute))
            {
                userAttribute = new UserAttribute
                {
                    UserId = userId,
                    AttributeType = reward.AttributeType,
                    CurrentXp = 0,
                    UpdatedAtUtc = awardedAtUtc
                };

                _dbContext.UserAttributes.Add(
                    userAttribute);

                userAttributes.Add(
                    reward.AttributeType,
                    userAttribute);
            }

            userAttribute.CurrentXp += reward.XpAmount;
            userAttribute.UpdatedAtUtc = awardedAtUtc;

            completion.XpTransactions.Add(
                new XpTransaction
                {
                    UserId = userId,
                    HabitCompletionId = completion.Id,
                    AttributeType = reward.AttributeType,
                    Amount = reward.XpAmount,
                    Reason = HabitCompletionReason,
                    CreatedAtUtc = awardedAtUtc
                });
        }
    }

    public async Task ReverseCompletionRewardsAsync(
        Guid userId,
        HabitCompletion completion,
        DateTime reversedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var awardedTransactions =
            await _dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId
                    && transaction.HabitCompletionId
                        == completion.Id
                    && transaction.Amount > 0
                    && transaction.Reason
                        == HabitCompletionReason)
                .ToListAsync(cancellationToken);

        if (awardedTransactions.Count == 0)
        {
            return;
        }

        var attributeTypes =
            awardedTransactions
                .Select(transaction =>
                    transaction.AttributeType)
                .Distinct()
                .ToList();

        var userAttributes =
            await _dbContext.UserAttributes
                .Where(attribute =>
                    attribute.UserId == userId
                    && attributeTypes.Contains(
                        attribute.AttributeType))
                .ToDictionaryAsync(
                    attribute => attribute.AttributeType,
                    cancellationToken);

        foreach (var transaction in awardedTransactions)
        {
            if (!userAttributes.TryGetValue(
                    transaction.AttributeType,
                    out var userAttribute))
            {
                throw new InvalidOperationException(
                    "The XP transaction cannot be reversed because its user attribute is missing.");
            }

            if (userAttribute.CurrentXp
                < transaction.Amount)
            {
                throw new InvalidOperationException(
                    "The XP transaction cannot be reversed because the user attribute contains insufficient XP.");
            }

            userAttribute.CurrentXp -= transaction.Amount;
            userAttribute.UpdatedAtUtc = reversedAtUtc;

            completion.XpTransactions.Add(
                new XpTransaction
                {
                    UserId = userId,
                    HabitCompletionId = completion.Id,
                    AttributeType =
                        transaction.AttributeType,
                    Amount = -transaction.Amount,
                    Reason =
                        HabitCompletionUndoReason,
                    CreatedAtUtc = reversedAtUtc
                });
        }
    }

    private static int CalculateBalanceScore(
        IReadOnlyList<UserAttributeResponse> attributes)
    {
        if (attributes.Count == 0)
        {
            return 0;
        }

        var strongestXp =
            attributes.Max(attribute =>
                attribute.CurrentXp);

        if (strongestXp == 0)
        {
            return 0;
        }

        var averageXp =
            attributes.Average(attribute =>
                (decimal)attribute.CurrentXp);

        return (int)Math.Round(
            averageXp / strongestXp * 100m,
            MidpointRounding.AwayFromZero);
    }
}
