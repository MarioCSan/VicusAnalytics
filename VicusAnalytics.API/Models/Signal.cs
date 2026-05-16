namespace VicusAnalytics.API.Models;

public class Signal
{
    public Guid SignalId { get; set; }
    public string MarketId { get; set; } = "";
    public string? Question { get; set; }
    public string? Slug { get; set; }
    public string Direction { get; set; } = "";
    public decimal MarketPrice { get; set; }
    public decimal EstimatedProb { get; set; }
    public decimal Edge { get; set; }
    public decimal Confidence { get; set; }
    public decimal KellyFraction { get; set; }
    public decimal PositionSize { get; set; }
    public decimal Spread { get; set; }
    public decimal Liquidity { get; set; }
    public string? VolatilityRegime { get; set; }
    public string? ModelName { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset GeneratedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public decimal? RealizedPnl { get; set; }
    public string? OrderId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? IngestedAt { get; set; }
}
