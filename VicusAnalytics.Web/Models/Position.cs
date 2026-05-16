namespace VicusAnalytics.Web.Models;

public class Position
{
    public Guid PositionId { get; set; }
    public Guid? SignalId { get; set; }
    public string MarketId { get; set; } = "";
    public string? Question { get; set; }
    public string? Direction { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal SizeUsd { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal? ExitPrice { get; set; }
    public decimal? RealizedPnl { get; set; }
    public string? ExitReason { get; set; }
}
