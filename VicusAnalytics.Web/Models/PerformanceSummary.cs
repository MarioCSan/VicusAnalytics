namespace VicusAnalytics.Web.Models;

public record PerformanceSummary(
    int TotalSignals,
    int PendingSignals,
    int ExecutedSignals,
    int ResolvedSignals,
    int OpenPositions,
    int ClosedPositions,
    decimal TotalRealizedPnl,
    decimal WinRate,
    decimal AvgEdge,
    DateTimeOffset? LastSignalAt
);
