namespace VicusAnalytics.Web.Models;

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
    public decimal PositionSize { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset GeneratedAt { get; set; }
    public decimal? RealizedPnl { get; set; }
}
