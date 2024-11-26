
function openLearningSummaryModal() {
    // Mã để mở modal tổng kết
    $('#yourModalId').modal('show'); // Sử dụng jQuery hoặc bất kỳ mã nào để hiển thị modal
}

// Thêm hàm để mở modal
window.openLearningSummaryModal = () => {
    $('#learningSummaryModal').modal('show');
};

// Thêm hàm để đóng modal (nếu bạn cần gọi từ C#)
window.closeLearningSummaryModal = () => {
    $('#learningSummaryModal').modal('hide');
};

function closeLearningSummaryModal() {
    const modal = document.querySelector('.modal-overlay');
    if (modal) {
        modal.style.display = 'none';
    }
}

window.bootstrapModal = {
    hide: function (selector) {
        const modal = document.querySelector(selector);
        if (modal) {
            // Lấy hoặc tạo instance mới
            let bsModal = bootstrap.Modal.getInstance(modal);
            if (!bsModal) {
                bsModal = new bootstrap.Modal(modal);
            }
            bsModal.hide();
        }
    },
    show: function (selector) {
        const modal = document.querySelector(selector);
        if (modal) {
            let bsModal = bootstrap.Modal.getInstance(modal);
            if (!bsModal) {
                bsModal = new bootstrap.Modal(modal);
            }
            bsModal.show();
        }
    }
};


function CalculateFinalScores() {
    // Hiển thị spinner
    showSpinner(true);

    // Thực hiện tính toán nặng
    setTimeout(() => {
        // Giả lập công việc tính toán
        console.log("Đang tính toán...");
        setTimeout(() => {
            // Giả lập kết quả tính toán
            console.log("Tính toán xong!");
            showSpinner(false); // Ẩn spinner
        }, 1000);
    }, 3000); // Giả lập công việc tính toán tốn thời gian
}

