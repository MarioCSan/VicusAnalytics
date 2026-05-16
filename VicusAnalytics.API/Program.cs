using Dapper;
using Npgsql;
using VicusAnalytics.API.Hubs;
using VicusAnalytics.API.Models;

// Enable snake_case → PascalCase column mapping
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// ── PostgreSQL ────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration["POSTGRES_DSN"]
    ?? Environment.GetEnvironmentVariable("POSTGRES_DSN")
    ?? throw new InvalidOperationException("POSTGRES_DSN is required");

builder.Services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]
    ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
    ?? "http://localhost:5200";

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins.Split(','))
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
app.UseCors();

// ── Helpers ───────────────────────────────────────────────────────────────────
static DateTime PeriodCutoff(string? period) => period switch
{
    "1d"  => DateTime.UtcNow.AddDays(-1),
    "7d"  => DateTime.UtcNow.AddDays(-7),
    "30d" => DateTime.UtcNow.AddDays(-30),
    "90d" => DateTime.UtcNow.AddDays(-90),
    _     => DateTime.MinValue,
};

// ── Health ────────────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new { status = "ok", ts = DateTimeOffset.UtcNow }));

// ── Stats ─────────────────────────────────────────────────────────────────────
app.MapGet("/api/stats", async (NpgsqlDataSource ds, string? period) =>
{
    var cutoff = PeriodCutoff(period);
    await using var conn = await ds.OpenConnectionAsync();

    var pos = await conn.QuerySingleAsync<dynamic>(@"
        SELECT
            COUNT(*)::int                                                AS resolved,
            COALESCE(SUM(realized_pnl), 0)                             AS total_pnl,
            COALESCE(AVG(realized_pnl), 0)                             AS avg_pnl,
            COALESCE(STDDEV_SAMP(realized_pnl), 0)                     AS std_pnl,
            COUNT(*) FILTER (WHERE realized_pnl > 0)::int              AS wins,
            COALESCE(MAX(realized_pnl), 0)                             AS best_trade,
            COALESCE(MIN(realized_pnl), 0)                             AS worst_trade
        FROM positions
        WHERE status = 'CLOSED' AND realized_pnl IS NOT NULL
          AND (@cutoff = TIMESTAMPTZ '0001-01-01' OR opened_at >= @cutoff)",
        new { cutoff });

    var totalSignals = await conn.ExecuteScalarAsync<int>(
        @"SELECT COUNT(*)::int FROM signals
          WHERE @cutoff = TIMESTAMPTZ '0001-01-01' OR generated_at >= @cutoff",
        new { cutoff });

    double? sharpe = null;
    if ((double)pos.std_pnl > 0)
        sharpe = Math.Round((double)pos.avg_pnl / (double)pos.std_pnl, 2);

    int resolved = (int)pos.resolved;
    int wins     = (int)pos.wins;
    double winRate = resolved > 0 ? (double)wins / resolved : 0.0;

    // Max drawdown: peak-to-trough from cumulative PnL series
    var dailyRows = await conn.QueryAsync<decimal>(
        @"SELECT COALESCE(SUM(realized_pnl), 0)
          FROM positions
          WHERE status = 'CLOSED' AND realized_pnl IS NOT NULL
            AND (@cutoff = TIMESTAMPTZ '0001-01-01' OR opened_at >= @cutoff)
          GROUP BY DATE(closed_at)
          ORDER BY DATE(closed_at)",
        new { cutoff });

    double maxDd = 0, peak = 0, cum = 0;
    foreach (var d in dailyRows)
    {
        cum  += (double)d;
        if (cum > peak) peak = cum;
        var dd = peak - cum;
        if (dd > maxDd) maxDd = dd;
    }

    return Results.Ok(new
    {
        totalPnl    = Math.Round((double)pos.total_pnl, 2),
        avgPnl      = Math.Round((double)pos.avg_pnl, 2),
        winRate,
        wins,
        resolved,
        totalSignals,
        sharpe,
        maxDrawdown = Math.Round(maxDd, 2),
        bestTrade   = Math.Round((double)pos.best_trade, 2),
        worstTrade  = Math.Round((double)pos.worst_trade, 2),
    });
});

// ── PnL Chart ─────────────────────────────────────────────────────────────────
app.MapGet("/api/pnl-chart", async (NpgsqlDataSource ds, string? period) =>
{
    var cutoff = PeriodCutoff(period);
    await using var conn = await ds.OpenConnectionAsync();

    var rows = await conn.QueryAsync<(DateTime date, double pnl)>(@"
        SELECT DATE(closed_at) AS date, SUM(realized_pnl)::float AS pnl
        FROM positions
        WHERE status = 'CLOSED' AND realized_pnl IS NOT NULL
          AND (@cutoff = TIMESTAMPTZ '0001-01-01' OR closed_at >= @cutoff)
        GROUP BY DATE(closed_at)
        ORDER BY DATE(closed_at)",
        new { cutoff });

    var list      = rows.ToList();
    var labels    = list.Select(r => r.date.ToString("MM/dd")).ToList();
    var perTrade  = list.Select(r => Math.Round(r.pnl, 2)).ToList();
    var cumulative = new List<double>();
    double running = 0;
    foreach (var v in perTrade) { running += v; cumulative.Add(Math.Round(running, 2)); }

    return Results.Ok(new { labels, cumulative, perTrade });
});

// ── Edge Distribution ─────────────────────────────────────────────────────────
app.MapGet("/api/edge-distribution", async (NpgsqlDataSource ds, string? period) =>
{
    var cutoff = PeriodCutoff(period);
    await using var conn = await ds.OpenConnectionAsync();

    var rows = await conn.QueryAsync<(string range, int count)>(@"
        SELECT
            CASE
                WHEN edge < 0.05 THEN '0-5%'
                WHEN edge < 0.10 THEN '5-10%'
                WHEN edge < 0.15 THEN '10-15%'
                WHEN edge < 0.20 THEN '15-20%'
                ELSE '20%+'
            END AS range,
            COUNT(*)::int AS count
        FROM signals
        WHERE edge IS NOT NULL
          AND (@cutoff = TIMESTAMPTZ '0001-01-01' OR generated_at >= @cutoff)
        GROUP BY 1
        ORDER BY MIN(edge)",
        new { cutoff });

    var list   = rows.ToList();
    var labels = list.Select(r => r.range).ToList();
    var values = list.Select(r => r.count).ToList();
    return Results.Ok(new { labels, values });
});

// ── Signals ───────────────────────────────────────────────────────────────────
app.MapGet("/api/signals", async (NpgsqlDataSource ds, string? period, string? status, int limit = 200) =>
{
    var cutoff = PeriodCutoff(period);
    await using var conn = await ds.OpenConnectionAsync();

    var sql = @"SELECT * FROM signals
                WHERE (@cutoff = TIMESTAMPTZ '0001-01-01' OR generated_at >= @cutoff)
                  AND (@status IS NULL OR status = @status)
                ORDER BY generated_at DESC
                LIMIT @limit";

    var rows = await conn.QueryAsync<Signal>(sql, new { cutoff, status, limit });
    return Results.Ok(rows);
});

app.MapGet("/api/signals/{id:guid}", async (NpgsqlDataSource ds, Guid id) =>
{
    await using var conn = await ds.OpenConnectionAsync();
    var row = await conn.QuerySingleOrDefaultAsync<Signal>(
        "SELECT * FROM signals WHERE signal_id = @id", new { id });
    return row is null ? Results.NotFound() : Results.Ok(row);
});

// ── Positions ─────────────────────────────────────────────────────────────────
app.MapGet("/api/positions", async (NpgsqlDataSource ds, string? status, int limit = 100) =>
{
    await using var conn = await ds.OpenConnectionAsync();
    var sql = status is null
        ? "SELECT * FROM positions ORDER BY opened_at DESC LIMIT @limit"
        : "SELECT * FROM positions WHERE status = @status ORDER BY opened_at DESC LIMIT @limit";
    var rows = await conn.QueryAsync<Position>(sql, new { status, limit });
    return Results.Ok(rows);
});

// ── Performance (legacy) ──────────────────────────────────────────────────────
app.MapGet("/api/performance", async (NpgsqlDataSource ds) =>
{
    await using var conn = await ds.OpenConnectionAsync();
    var s = await conn.QuerySingleAsync<dynamic>(@"
        SELECT COUNT(*)::int AS total_signals,
               COUNT(*) FILTER (WHERE status='PENDING')::int  AS pending_signals,
               COUNT(*) FILTER (WHERE status='EXECUTED')::int AS executed_signals,
               COUNT(*) FILTER (WHERE status='RESOLVED')::int AS resolved_signals,
               COALESCE(AVG(edge),0) AS avg_edge,
               MAX(generated_at) AS last_signal_at
        FROM signals");
    var p = await conn.QuerySingleAsync<dynamic>(@"
        SELECT COUNT(*) FILTER (WHERE status='OPEN')::int    AS open_positions,
               COUNT(*) FILTER (WHERE status='CLOSED')::int AS closed_positions,
               COALESCE(SUM(realized_pnl) FILTER (WHERE status='CLOSED'),0) AS total_pnl,
               COALESCE(AVG(CASE WHEN realized_pnl>0 THEN 1.0 ELSE 0.0 END)
                        FILTER (WHERE status='CLOSED'),0) AS win_rate
        FROM positions");
    return Results.Ok(new PerformanceSummary(
        (int)s.total_signals, (int)s.pending_signals, (int)s.executed_signals, (int)s.resolved_signals,
        (int)p.open_positions, (int)p.closed_positions,
        (decimal)p.total_pnl, (decimal)p.win_rate, (decimal)s.avg_edge, s.last_signal_at));
});

// ── SignalR ───────────────────────────────────────────────────────────────────
app.MapHub<AnalyticsHub>("/hubs/analytics");

app.Run();
