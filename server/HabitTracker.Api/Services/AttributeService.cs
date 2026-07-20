using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services;

public sealed class AttributeService
{
    private const string HabitCompletionReason =
        "Habit completion";

    private readonly AppDbContext _dbContext;

    public AttributeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
        var transactions =
            await _dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId
                    && transaction.HabitCompletionId
                        == completion.Id)
                .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
        {
            return;
        }

        var attributeTypes =
            transactions
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

        foreach (var transaction in transactions)
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
        }

        _dbContext.XpTransactions.RemoveRange(
            transactions);
    }
}
