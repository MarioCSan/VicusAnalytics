using System.Net.Http.Json;
using VicusAnalytics.Web.Models;

namespace VicusAnalytics.Web.Services;

public class ApiService(HttpClient http)
{
    public Task<StatsModel?> GetStatsAsync(string period = "all") =>
        http.GetFromJsonAsync<StatsModel>($"api/stats?period={period}");

    public Task<PnlChartData?> GetPnlChartAsync(string period = "all") =>
        http.GetFromJsonAsync<PnlChartData>($"api/pnl-chart?period={period}");

    public Task<EdgeDistribution?> GetEdgeDistributionAsync(string period = "all") =>
        http.GetFromJsonAsync<EdgeDistribution>($"api/edge-distribution?period={period}");

    public Task<List<Signal>?> GetSignalsAsync(string period = "all", string? status = null, int limit = 200) =>
        http.GetFromJsonAsync<List<Signal>>(
            $"api/signals?period={period}&limit={limit}{(status is null or "ALL" ? "" : $"&status={status}")}");

    public Task<List<Position>?> GetPositionsAsync(string? status = null, int limit = 100) =>
        http.GetFromJsonAsync<List<Position>>(
            $"api/positions?limit={limit}{(status is null ? "" : $"&status={status}")}");

    public Task<PerformanceSummary?> GetPerformanceAsync() =>
        http.GetFromJsonAsync<PerformanceSummary>("api/performance");
}
