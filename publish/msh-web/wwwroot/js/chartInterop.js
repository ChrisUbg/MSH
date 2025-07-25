window.chartInterop = {
    createChart: function (canvasId, type, data, options) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return null;

        const ctx = canvas.getContext('2d');
        return new Chart(ctx, {
            type: type,
            data: data,
            options: options
        });
    },

    updateChart: function (chartId, data) {
        const chart = Chart.getChart(chartId);
        if (chart) {
            chart.data = data;
            chart.update();
        }
    },

    destroyChart: function (chartId) {
        const chart = Chart.getChart(chartId);
        if (chart) {
            chart.destroy();
        }
    }
}; 