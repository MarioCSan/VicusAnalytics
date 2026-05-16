namespace VicusAnalytics.Web.Models;

public record StatsModel(
    decimal TotalPnl,
    decimal AvgPnl,
    decimal WinRate,
    int Wins,
    int Resolved,
    int TotalSignals,
    decimal? Sharpe,
    decimal MaxDrawdown,
    decimal BestTrade,
    decimal WorstTrade
);
