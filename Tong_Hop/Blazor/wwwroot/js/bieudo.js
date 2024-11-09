window.charts = {}; // Đối tượng lưu trữ các biểu đồ theo id

window.drawPieChart = (canvasId, labels, data, title) => {
    var ctx = document.getElementById(canvasId).getContext('2d');

    // Kiểm tra và hủy biểu đồ cũ nếu nó đã tồn tại
    if (window.charts[canvasId]) {
        window.charts[canvasId].destroy();
    }

    // Tạo biểu đồ mới và lưu nó vào đối tượng `charts`
    window.charts[canvasId] = new Chart(ctx, {
        type: 'pie',
        data: {
            datasets: [{
                data: data,
                backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0'],
                hoverBackgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0']
            }],
            labels: labels,
        },
        options: {
            responsive: true,
            title: {
                display: true,
                text: title
            }
        }
    });
};
