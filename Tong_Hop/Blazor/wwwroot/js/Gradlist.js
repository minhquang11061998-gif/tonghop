    $(document).on('contextmenu', '.target', function (e) {
        e.preventDefault();
        $('.context-menu').hide();
        $(this).siblings('.context-menu').css({
            display: 'block',
            left: e.pageX,
            top: e.pageY
        });
    });
    $(document).on('click', function () {
        $('.context-menu').hide();
    });
    function downloadFile(fileName, base64String) {
        const link = document.createElement('a');
        link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64String;
        link.download = fileName;
        link.click();
    };

