namespace VicusAnalytics.Web.Models;

public record EdgeDistribution(
    List<string> Labels,
    List<int> Values
);
