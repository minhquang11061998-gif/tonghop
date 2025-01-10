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
    if (current.value.length >= 1) {
        // Chuy?n sang � ti?p theo n?u � hi?n t?i ?� c� k� t?
        const nextElement = document.getElementById(nextId);
        if (nextElement) {
            nextElement.focus();
        }
    } else {
        // Quay l?i � tr??c n?u ng??i d�ng x�a (n?u c� � tr??c ?�)
        const previousId = getPreviousId(current.id);
        if (previousId) {
            const previousElement = document.getElementById(previousId);
            if (previousElement) {
                previousElement.focus();
            }
        }
    }
    validateCode(); // G?i h�m validate (n?u c� logic th�m)
}

function getPreviousId(currentId) {
    const currentIndex = parseInt(currentId.replace('box', '')) - 1;
    return currentIndex >= 1 ? `box${currentIndex}` : null;
}

function validateCode() {
    const code = Array.from({ length: 6 }, (_, i) => document.getElementById(`box${i + 1}`).value).join('');
    const submitBtn = document.getElementById('submitBtn');
    submitBtn.disabled = code.length < 6;
}