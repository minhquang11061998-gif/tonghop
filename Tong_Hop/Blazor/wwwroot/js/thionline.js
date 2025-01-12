async function enterExamMode() {

    await document.documentElement.requestFullscreen();
    if (navigator.keyboard) {
        await navigator.keyboard.lock();
    }
    document.addEventListener("contextmenu", (event) => {
        event.preventDefault();
    });
}

function reloadCameraFeed() {
    const cameraImage = document.querySelector("#cameraImage");
    if (cameraImage) {
        cameraImage.src = "https://localhost:7046/api/FaceId/camera-stream?" + new Date().getTime();
    }
}
function getCode() {
    let code = '';
    for (let i = 1; i <= 6; i++) {
        code += document.getElementById(`box${i}`).value;
    }
    return code;
}

function moveToNext(current, nextId) {
    const currentId = current.id;
    const previousId = getPreviousId(currentId);

    // Kiểm tra nếu ô cuối cùng bị xóa
    if (currentId === "box6" && current.value.length === 0) {
        clearAllInputs(); // Xóa tất cả dữ liệu
        const firstElement = document.getElementById("box1");
        if (firstElement) {
            firstElement.focus(); // Đưa focus về ô đầu tiên
        }
        return;
    }

    // Chuyển sang ô tiếp theo nếu ô hiện tại có ký tự
    if (current.value.length === 1 && nextId) {
        const nextElement = document.getElementById(nextId);
        if (nextElement) {
            nextElement.focus();
        }
    }
    // Quay lại ô trước nếu ô hiện tại bị xóa
    else if (current.value.length === 0 && previousId) {
        const previousElement = document.getElementById(previousId);
        if (previousElement) {
            previousElement.focus();
            previousElement.value = ""; // Xóa dữ liệu ô trước
        }
    }

    validateCode(); // Kiểm tra trạng thái mã
}

// Hàm xóa tất cả dữ liệu trong các ô nhập
function clearAllInputs() {
    const boxes = document.querySelectorAll('input[id^="box"]');
    boxes.forEach((box) => {
        box.value = ""; // Xóa giá trị từng ô
    });
}

function getPreviousId(currentId) {
    const currentNumber = parseInt(currentId.replace('box', ''), 10); // Lấy số từ ID
    if (!isNaN(currentNumber) && currentNumber > 1) {
        return 'box' + (currentNumber - 1); // Tạo ID cho ô trước đó
    }
    return null;
}

function validateCode() {
    const code = Array.from({ length: 6 }, (_, i) => document.getElementById(`box${i + 1}`).value).join('');
    const submitBtn = document.getElementById('submitBtn');
    submitBtn.disabled = code.length < 6;
}