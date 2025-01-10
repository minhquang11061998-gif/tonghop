function loadLottieFromJson(elementId, jsonUrl) {
    var animationContainer = document.getElementById(elementId);
    var animation = lottie.loadAnimation({
        container: animationContainer,
        renderer: 'svg',  // Sử dụng renderer SVG
        loop: true,       // Lặp vô hạn
        autoplay: true,   // Tự động phát
        path: jsonUrl     // Đường dẫn tới tệp JSON
    });
}