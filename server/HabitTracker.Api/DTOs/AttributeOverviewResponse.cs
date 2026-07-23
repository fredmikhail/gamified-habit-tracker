namespace HabitTracker.Api.DTOs;

public sealed class AttributeOverviewResponse
{
    public IReadOnlyList<UserAttributeResponse> Attributes { get; set; }
        = [];

    public int TotalAttributeXp { get; set; }

    public int BalanceScore { get; set; }

    public UserAttributeResponse? StrongestAttribute { get; set; }

    public UserAttributeResponse? NeedsFocusAttribute { get; set; }

    public IReadOnlyList<AttributeLevelUpResponse> ClosestToLevelUp
    { get; set; }
        = [];

    public IReadOnlyList<XpTransactionResponse> RecentXpTransactions
    { get; set; }
        = [];
}
