//window.startCamera = async function () {
//    const video = document.getElementById('video');
//    const canvas = document.getElementById('canvas');
//    const context = canvas.getContext('2d');
//    let streaming = false;

//    try {
//        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
//        video.srcObject = stream;
//        video.onloadedmetadata = () =>
//        {
//            video.play();
//            trackFaces();
//        };
//    } catch (err) {
//        console.error("Lỗi khi truy cập camera: ", err);
//    }
//    const trackFaces=  function() {
//        if (typeof cv === 'undefined') {
//            console.error('OpenCV.js chưa sẵn sàng');
//            return;
//        }

//        let src = new cv.Mat(video.height, video.width, cv.CV_8UC4);
//        let dst = new cv.Mat(video.height, video.width, cv.CV_8UC4);
//        let gray = new cv.Mat();
//        let cap = new cv.VideoCapture(video);
//        let faces = new cv.RectVector();
//        let classifier = new cv.CascadeClassifier();
//        classifier.load('wwwroot/haarcascade_frontalface_default.xml');

//        const FPS = 30;

//        const processVideo =  ()=> {
//            if (!streaming) {
//                src.delete();
//                dst.delete();
//                gray.delete();
//                faces.delete();
//                classifier.delete();
//                return;
//            }

//            let begin = Date.now();

//            // Xử lý frame video
//            cap.read(src);
//            src.copyTo(dst);
//            cv.cvtColor(dst, gray, cv.COLOR_RGBA2GRAY, 0);
//            classifier.detectMultiScale(gray, faces, 1.1, 3, 0);

//            // Vẽ hình chữ nhật quanh khuôn mặt
//            for (let i = 0; i < faces.size(); ++i) {
//                let face = faces.get(i);
//                let point1 = new cv.Point(face.x, face.y);
//                let point2 = new cv.Point(face.x + face.width, face.y + face.height);
//                cv.rectangle(dst, point1, point2, [255, 0, 0, 255]);
//            }

//            cv.imshow('canvas', dst);

//            let delay = 1000 / FPS - (Date.now() - begin);
//            setTimeout(processVideo, delay);
//        }

//        // Bắt đầu xử lý video
//        streaming = true;
//        processVideo();
//    }
//};

//// Hàm để chụp hình ảnh từ video và chuyển thành base64
//window.captureImage = function () {
//    const video = document.getElementById('video');
//    const canvas = document.getElementById('canvas');
//    const context = canvas.getContext('2d');
//    canvas.width = video.videoWidth;
//    canvas.height = video.videoHeight;
//    context.drawImage(video, 0, 0, canvas.width, canvas.height);

//    const base64Image = canvas.toDataURL('image/png');
//    return base64Image.split(',')[1]; // Trả về chuỗi base64 mà không có tiền tố "data:image/png;base64,"
//};
function Utils(errorOutputId) { 
    let self = this;
    this.errorOutput = document.getElementById(errorOutputId);

    const OPENCV_URL = 'opencv.js';
    this.loadOpenCv = function (onloadCallback) {
        let script = document.createElement('script');
        script.setAttribute('async', '');
        script.setAttribute('type', 'text/javascript');
        script.addEventListener('load', () => {
            if (cv.getBuildInformation) {
                console.log(cv.getBuildInformation());
                onloadCallback();
            }
            else {
                // WASM
                cv['onRuntimeInitialized'] = () => {
                    console.log(cv.getBuildInformation());
                    onloadCallback();
                }
            }
        });
        script.addEventListener('error', () => {
            self.printError('Failed to load ' + OPENCV_URL);
        });
        script.src = OPENCV_URL;
        let node = document.getElementsByTagName('script')[0];
        node.parentNode.insertBefore(script, node);
    };

    this.createFileFromUrl = function (path, url, callback) {
        let request = new XMLHttpRequest();
        request.open('GET', url, true);
        request.responseType = 'arraybuffer';
        request.onload = function (ev) {
            if (request.readyState === 4) {
                if (request.status === 200) {
                    let data = new Uint8Array(request.response);
                    cv.FS_createDataFile('/', path, data, true, false, false);
                    callback();
                } else {
                    self.printError('Failed to load ' + url + ' status: ' + request.status);
                }
            }
        };
        request.send();
    };

    this.loadImageToCanvas = function (url, cavansId) {
        let canvas = document.getElementById(cavansId);
        let ctx = canvas.getContext('2d');
        let img = new Image();
        img.crossOrigin = 'anonymous';
        img.onload = function () {
            canvas.width = img.width;
            canvas.height = img.height;
            ctx.drawImage(img, 0, 0, img.width, img.height);
        };
        img.src = url;
    };

    this.executeCode = function (textAreaId) {
        try {
            this.clearError();
            let code = document.getElementById(textAreaId).value;
            eval(code);
        } catch (err) {
            this.printError(err);
        }
    };

    this.clearError = function () {
        this.errorOutput.innerHTML = '';
    };

    this.printError = function (err) {
        if (typeof err === 'undefined') {
            err = '';
        } else if (typeof err === 'number') {
            if (!isNaN(err)) {
                if (typeof cv !== 'undefined') {
                    err = 'Exception: ' + cv.exceptionFromPtr(err).msg;
                }
            }
        } else if (typeof err === 'string') {
            let ptr = Number(err.split(' ')[0]);
            if (!isNaN(ptr)) {
                if (typeof cv !== 'undefined') {
                    err = 'Exception: ' + cv.exceptionFromPtr(ptr).msg;
                }
            }
        } else if (err instanceof Error) {
            err = err.stack.replace(/\n/g, '<br>');
        }
        this.errorOutput.innerHTML = err;
    };

    this.loadCode = function (scriptId, textAreaId) {
        let scriptNode = document.getElementById(scriptId);
        let textArea = document.getElementById(textAreaId);
        if (scriptNode.type !== 'text/code-snippet') {
            throw Error('Unknown code snippet type');
        }
        textArea.value = scriptNode.text.replace(/^\n/, '');
    };

    this.addFileInputHandler = function (fileInputId, canvasId) {
        let inputElement = document.getElementById(fileInputId);
        inputElement.addEventListener('change', (e) => {
            let files = e.target.files;
            if (files.length > 0) {
                let imgUrl = URL.createObjectURL(files[0]);
                self.loadImageToCanvas(imgUrl, canvasId);
            }
        }, false);
    };

    function onVideoCanPlay() {
        if (self.onCameraStartedCallback) {
            self.onCameraStartedCallback(self.stream, self.video);
        }
    };

    this.startCamera = function (resolution, callback, videoId) {
        const constraints = {
            'qvga': { width: { exact: 320 }, height: { exact: 240 } },
            'vga': { width: { exact: 640 }, height: { exact: 480 } }
        };
        let video = document.getElementById(videoId);
        if (!video) {
            video = document.createElement('video');
        }

        let videoConstraint = constraints[resolution];
        if (!videoConstraint) {
            videoConstraint = true;
        }

        navigator.mediaDevices.getUserMedia({ video: videoConstraint, audio: false })
            .then(function (stream) {
                video.srcObject = stream;
                video.play();
                self.video = video;
                self.stream = stream;
                self.onCameraStartedCallback = callback;
                video.addEventListener('canplay', onVideoCanPlay, false);
            })
            .catch(function (err) {
                self.printError('Camera Error: ' + err.name + ' ' + err.message);
            });
    };
};
