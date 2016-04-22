/// <reference path="../lib/mvcct-enhancer/mvcct.enhancer.js" />
(function () {
    var options = {};
    options.browserSupport = {
        cookie: "_browser_basic_capabilities",
        forms: null,
        fallbacks: {
            number: {
                force: false,
                type: 2
            },
            range: {
                force: false,
                type: 2
            },
            time: {
                force: false,
                type: 1
            },
            date: {
                force: false,
                type: 1
            },
            datetime: {
                force: false,
                type: 1
            },
            month: {
                force: false,
                type: 1
            },
            week: {
                force: false,
                type: 1
            }
        }
    };

    mvcct.enhancer.waitAsync(options);
})();