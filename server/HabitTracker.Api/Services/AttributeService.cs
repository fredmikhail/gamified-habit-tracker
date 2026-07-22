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
}
