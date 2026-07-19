using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;

namespace HabitTracker.Tests.DTOs;

public sealed class HabitRequestValidationTests
{
    [Fact]
    public void CreateHabitRequest_WhenValid_HasNoValidationErrors()
    {
        var request = CreateValidCreateRequest();

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void UpdateHabitRequest_WhenValid_HasNoValidationErrors()
    {
        var request = CreateValidUpdateRequest();

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void CreateHabitRequest_WhenRequiredValuesAreMissing_HasValidationErrors()
    {
        var request = new CreateHabitRequest();

        var validationResults = Validate(request);

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.Name));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.FrequencyType));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.TargetCount));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.Difficulty));
    }

    [Fact]
    public void CreateHabitRequest_WhenTextExceedsMaximumLengths_HasValidationErrors()
    {
        var request = CreateValidCreateRequest();

        request.Name = new string('n', 101);
        request.Description = new string('d', 501);
        request.Category = new string('c', 51);

        var validationResults = Validate(request);

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.Name));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.Description));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.Category));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void CreateHabitRequest_WhenTargetCountIsOutsideRange_HasValidationError(
        int targetCount)
    {
        var request = CreateValidCreateRequest();

        request.TargetCount = targetCount;

        var validationResults = Validate(request);

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.TargetCount));
    }

    [Fact]
    public void CreateHabitRequest_WhenEnumsAreUndefined_HasValidationErrors()
    {
        var request = CreateValidCreateRequest();

        request.FrequencyType =
            (HabitFrequencyType)999;

        request.Difficulty =
            (HabitDifficulty)999;

        var validationResults = Validate(request);

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.FrequencyType));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(CreateHabitRequest.Difficulty));
    }

    [Fact]
    public void UpdateHabitRequest_WhenValuesAreInvalid_HasExpectedValidationErrors()
    {
        var request = new UpdateHabitRequest
        {
            Name = string.Empty,
            Description = new string('d', 501),
            Category = new string('c', 51),
            FrequencyType = (HabitFrequencyType)999,
            TargetCount = 8,
            Difficulty = (HabitDifficulty)999
        };

        var validationResults = Validate(request);

        AssertHasValidationErrorFor(
            validationResults,
            nameof(UpdateHabitRequest.Name));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(UpdateHabitRequest.Description));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(UpdateHabitRequest.Category));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(UpdateHabitRequest.FrequencyType));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(UpdateHabitRequest.TargetCount));

        AssertHasValidationErrorFor(
            validationResults,
            nameof(UpdateHabitRequest.Difficulty));
    }

    private static CreateHabitRequest CreateValidCreateRequest()
    {
        return new CreateHabitRequest
        {
            Name = "Read C# textbook",
            Description = "Read one chapter.",
            Category = "Learning",
            FrequencyType = HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty = HabitDifficulty.Medium
        };
    }

    private static UpdateHabitRequest CreateValidUpdateRequest()
    {
        return new UpdateHabitRequest
        {
            Name = "Read C# textbook",
            Description = "Read two sections.",
            Category = "Learning",
            FrequencyType = HabitFrequencyType.Weekly,
            TargetCount = 4,
            Difficulty = HabitDifficulty.Hard
        };
    }

    private static List<ValidationResult> Validate(
        object request)
    {
        var validationResults =
            new List<ValidationResult>();

        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        return validationResults;
    }

    private static void AssertHasValidationErrorFor(
        IEnumerable<ValidationResult> validationResults,
        string propertyName)
    {
        Assert.Contains(
            validationResults,
            validationResult =>
                validationResult.MemberNames.Contains(
                    propertyName));
    }
}