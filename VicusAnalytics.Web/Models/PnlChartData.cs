namespace VicusAnalytics.Web.Models;

public record PnlChartData(
    List<string> Labels,
    List<decimal> Cumulative,
    List<decimal> PerTrade
);
