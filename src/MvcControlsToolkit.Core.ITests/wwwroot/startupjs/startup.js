/// <reference path="../lib/mvcct-enhancer/mvcct.enhancer.js" />
(function () {
    var options = {};
    options.browserSupport = {
        cookie: "_browser_basic_capabilities",
        forms: null,
        fallbacks: {
            number: {
                force: true,
                type: 2
            },
            range: {
                force: false,
                type: 2
            },
            time: {
                force: true,
                type: 1
            },
            date: {
                force: true,
                type: 1
            },
            datetime: {
                force: false,
                type: 1
            },
            month: {
                force: true,
                type: 1
            },
            week: {
                force: true,
                type: 1
            },
            color: {
                force: true
            }
        },
        handlers: {
            enhance: {
                datetime: undefined
            }
        }
    };

    mvcct.enhancer.waitAsync(options);
})();