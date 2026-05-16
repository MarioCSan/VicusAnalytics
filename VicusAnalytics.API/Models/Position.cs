namespace VicusAnalytics.API.Models;

public class Position
{
    public Guid PositionId { get; set; }
    public Guid? SignalId { get; set; }
    public string MarketId { get; set; } = "";
    public string? Question { get; set; }
    public string? CityCode { get; set; }
    public string? Direction { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? EntryTempEstimateC { get; set; }
    public decimal SizeUsd { get; set; }
    public string? Slug { get; set; }
    public string? OrderId { get; set; }
    public string? TokenId { get; set; }
    public decimal? TempShiftThresholdC { get; set; }
    public decimal? StopLossPct { get; set; }
    public decimal? TakeProfitPct { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal? ExitPrice { get; set; }
    public string? ExitReason { get; set; }
    public decimal? RealizedPnl { get; set; }
    public DateTimeOffset? IngestedAt { get; set; }
}
