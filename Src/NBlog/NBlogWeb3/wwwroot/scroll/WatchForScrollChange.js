window.scrollHandler = {
    initialize: function (dotnetHelper) {
        window.onscroll = () => {
            let showButton = document.documentElement.scrollTop > 20 || document.body.scrollTop > 20;
            dotnetHelper.invokeMethodAsync('OnScroll', showButton);
        };
    },

    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};
