window._vc = {};

function _destroy(id) {
    if (window._vc[id]) { window._vc[id].destroy(); delete window._vc[id]; }
}

const _tip = {
    backgroundColor: '#21262d',
    borderColor: '#30363d',
    borderWidth: 1,
    titleColor: '#8b949e',
    bodyColor: '#e6edf3',
    padding: 10,
};
const _xScale = (extra) => ({
    grid: { color: 'rgba(48,54,61,0.4)', drawTicks: false },
    ticks: { color: '#8b949e', font: { size: 11 } },
    border: { display: false },
    ...extra
});
const _yScale = (fmt, extra) => ({
    grid: { color: 'rgba(48,54,61,0.6)', drawTicks: false },
    ticks: { color: '#8b949e', font: { size: 11 }, callback: fmt },
    border: { display: false },
    ...extra
});

window.createPnlCharts = function (labels, cumulative, perTrade) {
    _destroy('pnl-cum'); _destroy('pnl-per');

    const last = cumulative[cumulative.length - 1] ?? 0;
    const lineColor = last >= 0 ? '#3fb950' : '#f85149';

    const c1 = document.getElementById('pnl-cum');
    if (c1) {
        window._vc['pnl-cum'] = new Chart(c1, {
            type: 'line',
            data: {
                labels,
                datasets: [{
                    data: cumulative,
                    borderColor: lineColor,
                    backgroundColor: (ctx) => {
                        const g = ctx.chart.ctx.createLinearGradient(0, 0, 0, 220);
                        g.addColorStop(0, lineColor + '55');
                        g.addColorStop(1, lineColor + '00');
                        return g;
                    },
                    fill: true, tension: 0.3,
                    pointRadius: 2, pointHoverRadius: 4, borderWidth: 2,
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { ...(_tip), callbacks: { label: c => '$' + c.parsed.y.toFixed(2) } }
                },
                scales: {
                    x: _xScale(),
                    y: _yScale(v => '$' + v),
                }
            }
        });
    }

    const c2 = document.getElementById('pnl-per');
    if (c2) {
        window._vc['pnl-per'] = new Chart(c2, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    data: perTrade,
                    backgroundColor: perTrade.map(v => v >= 0 ? '#3fb95066' : '#f8514966'),
                    borderColor:      perTrade.map(v => v >= 0 ? '#3fb950'   : '#f85149'),
                    borderWidth: 1, borderRadius: 3,
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { ...(_tip), callbacks: { label: c => (c.parsed.y >= 0 ? '+' : '') + '$' + c.parsed.y.toFixed(2) } }
                },
                scales: { x: _xScale(), y: _yScale(v => '$' + v) }
            }
        });
    }
};

window.createEdgeChart = function (labels, values) {
    _destroy('edge-chart');
    const c = document.getElementById('edge-chart');
    if (!c) return;
    window._vc['edge-chart'] = new Chart(c, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: '#bc8cff44',
                borderColor: '#bc8cff',
                borderWidth: 1, borderRadius: 3,
            }]
        },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: { ...(_tip), callbacks: { label: c => 'Signals: ' + c.parsed.y } }
            },
            scales: { x: _xScale(), y: _yScale(v => v) }
        }
    });
};
