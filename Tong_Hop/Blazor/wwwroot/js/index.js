window.startCarousel = () => {
    var myCarousel = document.querySelector('#carouselExampleInterval');
    if (myCarousel) {
        var carousel = new bootstrap.Carousel(myCarousel, {
            interval: 2000,
            ride: 'carousel'
        });
    }
};
