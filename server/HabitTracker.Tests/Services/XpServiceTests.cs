using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Services;

namespace HabitTracker.Tests.Services;

public sealed class XpServiceTests
{
    private readonly XpService _xpService = new();

    [Theory]
    [InlineData(HabitDifficulty.Easy, 10)]
    [InlineData(HabitDifficulty.Medium, 20)]
    [InlineData(HabitDifficulty.Hard, 30)]
    [InlineData(HabitDifficulty.Elite, 50)]
    public void CalculateRewards_UsesDifficultyTotal(
        HabitDifficulty difficulty,
        int expectedTotal)
    {
        var rewards = _xpService.CalculateRewards(
            HabitCategory.FitnessAndMovement,
            difficulty);

        Assert.Equal(expectedTotal, rewards.Values.Sum());
    }

    [Theory]
    [InlineData(
        HabitCategory.FitnessAndMovement,
        AttributeType.Fitness,
        AttributeType.Discipline)]
    [InlineData(
        HabitCategory.HealthAndRecovery,
        AttributeType.Vitality,
        AttributeType.Discipline)]
    [InlineData(
        HabitCategory.LearningAndSkills,
        AttributeType.Mind,
        AttributeType.Focus)]
    [InlineData(
        HabitCategory.WorkAndCareer,
        AttributeType.Focus,
        AttributeType.Discipline)]
    [InlineData(
        HabitCategory.DailyLifeAndOrganization,
        AttributeType.Discipline,
        AttributeType.Focus)]
    [InlineData(
        HabitCategory.MoneyAndFinance,
        AttributeType.Mind,
        AttributeType.Discipline)]
    [InlineData(
        HabitCategory.RelationshipsAndCommunity,
        AttributeType.Social,
        AttributeType.Resilience)]
    [InlineData(
        HabitCategory.EmotionalWellBeing,
        AttributeType.Resilience,
        AttributeType.Vitality)]
    [InlineData(
        HabitCategory.SpiritualityAndPurpose,
        AttributeType.Purpose,
        AttributeType.Resilience)]
    [InlineData(
        HabitCategory.CreativityAndRecreation,
        AttributeType.Mind,
        AttributeType.Vitality)]
    [InlineData(
        HabitCategory.SelfControlAndBoundaries,
        AttributeType.Discipline,
        AttributeType.Resilience)]
    [InlineData(
        HabitCategory.GeneralGrowth,
        AttributeType.Discipline,
        AttributeType.Mind)]
    public void CalculateRewards_UsesCategoryAttributes(
        HabitCategory category,
        AttributeType primaryAttribute,
        AttributeType secondaryAttribute)
    {
        var rewards = _xpService.CalculateRewards(
            category,
            HabitDifficulty.Medium);

        Assert.Equal(2, rewards.Count);
        Assert.Equal(14, rewards[primaryAttribute]);
        Assert.Equal(6, rewards[secondaryAttribute]);
    }

    [Fact]
    public void CalculateRewards_InvalidDifficulty_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _xpService.CalculateRewards(
                HabitCategory.FitnessAndMovement,
                (HabitDifficulty)999));
    }

    [Fact]
    public void CalculateRewards_InvalidCategory_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _xpService.CalculateRewards(
                (HabitCategory)999,
                HabitDifficulty.Medium));
    }
}
