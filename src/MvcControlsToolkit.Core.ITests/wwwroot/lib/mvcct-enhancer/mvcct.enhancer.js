(function () {
    var DEBUG = true;
    (function (undefined) {
        var window = this || (0, eval)('this');
        (function (factory) {
            if (typeof define === 'function' && define['amd']) {
                // [1] AMD anonymous module
                define(['exports', 'require'], factory);
            } else if (typeof exports === 'object' && typeof module === 'object') {
                // [2] CommonJS/Node.js
                factory(module['exports'] || exports);  // module.exports is for Node.js
            } else {
                // [3] No module loader (plain <script> tag) - put directly in global namespace
                window["mvcct"] = mvcct = window["mvcct"] || {};
                factory(mvcct['enhancer'] = {});
            }
        }(

            (function (enhancerExports, amdRequire) {
                var enhancer = typeof enhancerExports !== 'undefined' ? enhancerExports : {};
                //Start actual code
                var transformations = [];
                enhancer["init"] = function (options) {
                    for (var i = 0; i < transformations.length; i++){
                        var item = transformations[i];
                        if (item.processOptions) {
                            item.processOptions(options);
                        }
                        if (item.initialize && item.transform) {
                            item.transform(document.querySelector('body'));
                        }
                    }
                };
                enhancer["register"]=function(transform, initialize, processOptions){
                    transformations.push({
                        transform: transform,
                        initialize: initialize,
                        processOptions: processOptions
                    });
                }
                enhancer["transform"] = function (node) {
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.transform) {
                            item.transform(node);
                        }
                    }
                }

                //html5 enhancement
                (function (enhancer) {
                    var support = autodetect();
                    var html5Infos = null;//describe situation after transformation
                    var html5ProcessInfos = null;//describe required processing based on user options and defaults
                    function autodetect() {
                        //html5 auto detection
                    }
                    function packInfosForServer() {
                        //pack html5Infos in cookie and or hidden field for server
                    }
                    function processOptions(options) {
                        //compute html5Infos and html5ProcessInfos

                        packInfosForServer();
                    }
                    function process(node) {
                        //do html5 enhancement
                    }
                    enhancer["register"](process, true, processOptions);
                }(enhancer));

                //Finish actual code
            })
        ));
    }());
})();