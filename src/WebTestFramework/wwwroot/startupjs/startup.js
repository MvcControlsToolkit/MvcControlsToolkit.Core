/// <reference path="../lib/mvcct-enhancer/mvcct.enhancer.js" />
(function () {
    var options = {};
    options.browserSupport = {
        cookie: "_browser_basic_capabilities",
        forms: null,
        fallbacks: {
            number: {
                force: true
            },
            range: {
                force: false
            },
            time: {
                force: true
            },
            date: {
                force: true
            },
            datetime: {
                force: false
            },
            month: {
                force: true
            },
            week: {
                force: true
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