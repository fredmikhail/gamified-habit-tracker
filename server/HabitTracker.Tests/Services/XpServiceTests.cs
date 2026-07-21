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

    [Theory]
    [InlineData(0, 1, 0, 100)]
    [InlineData(99, 1, 99, 100)]
    [InlineData(100, 2, 0, 125)]
    [InlineData(224, 2, 124, 125)]
    [InlineData(225, 3, 0, 150)]
    [InlineData(550, 5, 0, 200)]
    [InlineData(700, 5, 150, 200)]
    public void CalculateAttributeLevelProgress_ReturnsExpectedProgress(
    int currentXp,
    int expectedLevel,
    int expectedXpIntoCurrentLevel,
    int expectedXpNeededForNextLevel)
    {
        var progress =
            _xpService.CalculateAttributeLevelProgress(
                currentXp);

        Assert.Equal(
            expectedLevel,
            progress.Level);

        Assert.Equal(
            expectedXpIntoCurrentLevel,
            progress.XpIntoCurrentLevel);

        Assert.Equal(
            expectedXpNeededForNextLevel,
            progress.XpNeededForNextLevel);
    }

    [Theory]
    [InlineData(0, 1, 0, 200)]
    [InlineData(199, 1, 199, 200)]
    [InlineData(200, 2, 0, 250)]
    [InlineData(449, 2, 249, 250)]
    [InlineData(450, 3, 0, 300)]
    [InlineData(1000, 4, 250, 350)]
    [InlineData(1100, 5, 0, 400)]
    public void CalculateOverallLevelProgress_ReturnsExpectedProgress(
        int totalXp,
        int expectedLevel,
        int expectedXpIntoCurrentLevel,
        int expectedXpNeededForNextLevel)
    {
        var progress =
            _xpService.CalculateOverallLevelProgress(
                totalXp);

        Assert.Equal(
            expectedLevel,
            progress.Level);

        Assert.Equal(
            expectedXpIntoCurrentLevel,
            progress.XpIntoCurrentLevel);

        Assert.Equal(
            expectedXpNeededForNextLevel,
            progress.XpNeededForNextLevel);
    }

    [Fact]
    public void ProgressionCurves_FocusedXpProducesIntuitiveSpecialization()
    {
        var overallProgress =
            _xpService.CalculateOverallLevelProgress(
                1000);

        var primaryAttributeProgress =
            _xpService.CalculateAttributeLevelProgress(
                700);

        var secondaryAttributeProgress =
            _xpService.CalculateAttributeLevelProgress(
                300);

        Assert.Equal(4, overallProgress.Level);
        Assert.Equal(5, primaryAttributeProgress.Level);
        Assert.Equal(3, secondaryAttributeProgress.Level);
    }

    [Fact]
    public void CalculateAttributeLevelProgress_NegativeXp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _xpService.CalculateAttributeLevelProgress(
                -1));
    }

    [Fact]
    public void CalculateOverallLevelProgress_NegativeXp_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _xpService.CalculateOverallLevelProgress(
                -1));
    }
}
