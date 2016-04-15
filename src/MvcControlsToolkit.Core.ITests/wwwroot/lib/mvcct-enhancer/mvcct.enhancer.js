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
                var asyncReady = false;
                var waitAsync = null;
                var oldReady = null;
                if (jQuery) {
                    oldReady = jQuery.fn.ready;
                    jQuery.fn.ready = function (x) {
                        enhancer["register"](null, null, x)
                    };
                }
                function init(options) {
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.processOptions) {
                            item.processOptions(options);
                        }
                        if (item.initialize && item.transform) {
                            item.transform(document.querySelector('body'));
                        }
                    }
                }
                enhancer["init"] = function (options) {
                    options = options || {};
                    if (jQuery) {
                        jQuery.fn.ready = oldReady;
                        jQuery(document).ready(function () { init(options); });
                    }
                    else init(options);
                };
                enhancer["asyncReady"] = function () {
                    if (waitAsync) enhancer["init"](waitAsync);
                    asyncReady = true;
                };
                enhancer["waitAsync"] = function (options) {
                    if (asyncReady) enhancer["init"](options);
                    waitAsync = options || {};
                };
                enhancer["register"] = function (transform, initialize, processOptions) {
                    transformations.push({
                        transform: transform,
                        initialize: initialize,
                        processOptions: processOptions
                    });
                };
                enhancer["transform"] = function (node) {
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.transform) {
                            item.transform(node);
                        }
                    }
                };
                
                //html5 enhancement
                (function (enhancer) {
                    var originalSupport = autodetect();
                    var processedSupport = {};
                    var html5Infos = {
                        Html5InputOriginalSupport: originalSupport,
                        Html5InputSupport: processedSupport
                    };
                    var html5ProcessInfos = null;//describe required processing based on user options and defaults
                    var html5ProcessInfosDefaults = {
                        cookie: "_browser_basic_capabilities",
                        forms: "_browser_basic_capabilities"
                    };
                    var handlers = null;
                    enhancer["getSupport"] = function () {
                        return html5Infos;
                    };
                    function needFallback(type) {
                        try {
                            var input = document.createElement("input");
                            input.setAttribute("type", type);
                            if (input.type === type) {
                                var illegal = '1illegal';
                                input.setAttribute('value', illegal);
                                return (input.value === illegal);
                            } else return true;
                        } catch (e) {
                            return true;;
                        }
                    }
                    function addToOptions(prefix, object, outputArray) {
                        for(var prop in object){
                            var val = object[prop];
                            if(typeof(val) === boolean) val= val ? "True":"False";
                            else val = ''+val;
                            if (!object.hasOwnProperty(prop)) continue;
                            outputArray.push({
                                Key: prefix ? prefix + "." + prop : prop,
                                Value: val
                            });
                        }
                    }
                    function copyAttrs(elm, mains) {
                        for (var iAttr = 0; iAttr < elm.attributes.length; iAttr++) {
                            var attribute = elm.attributes[iAttr].name;
                            if (attribute === "min" || attribute === "max" || attribute === "type" || attribute === "value") continue;
                            else mains[attribute] = elm.attributes[iAttr].value;
                        }
                    }
                    function autodetect() {
                        //html5 auto detection
                        return {
                            number: !needFallback('number'),
                            range: !needFallback('range'),
                            date: !needFallback('date'),
                            month: !needFallback('month'),
                            week: !needFallback('week'),
                            time: !needFallback('time'),
                            datetime: !needFallback('datetime-local'),
                            email: !needFallback('email'),
                            search: !needFallback('search'),
                            tel: !needFallback('tel'),
                            url: !needFallback('url'),
                            color: !needFallback('color')
                        }
                    }
                    function packInfosForServer() {
                        //pack html5Infos in cookie and or hidden field for server
                        var infos = [];
                        addToOptions("Html5InputSupport", html5Infos.Html5InputSupport, infos);
                        addToOptions("Html5InputOriginalSupport", Html5InputOriginalSupport.Html5InputSupport, infos);
                        var sInfos = JSON.stringify(infos);
                        if (html5ProcessInfos.forms) {
                            var flist = document.querySelectorAll('form');
                            for (var i = 0; i < flist.length; i++)
                            {
                                var input = document.createElement("input");
                                input.setAttribute("type", "hidden");
                                input.setAttribute("name", html5ProcessInfos.forms);
                                input.setAttribute("value", sInfos);
                                flist[i].appendChild(input);
                            }
                        }
                        if (html5ProcessInfos.cookie) {
                            document.cookie = html5ProcessInfos.cookie + "=" + sInfos + "; path=/";
                        }
                    }
                    function processOptions(options) {
                        options = options || {};
                        html5ProcessInfos = {
                            fallbackHtml5: options.fallbackHtml5 === undefined ? true : options.fallbackHtml5,
                            cookie: options.cookie === undefined ? html5ProcessInfosDefaults.cookie : options.cookie,
                            forms: options.forms === undefined ? html5ProcessInfosDefaults.forms : options.forms,
                            fallbacks: options.fallbacks || {},
                            handlers
                        };
                        if (!options.handlers || !options.handlers.replace)
                        {
                            html5ProcessInfos.handlers.replace = function (type, support) {
                                if (type == "number" || type == "date" || type == "datetime-local" || type == "month" || type == "time" || type == "week")
                                    return "text";
                                else if (type == "range") {
                                    if (support.range > 2 || support.number < 4) return "text";
                                    return "number";
                                }
                                else return type;
                            };
                        }
                        else html5ProcessInfos.handlers.replace = options.handlers.replace;
                        if (!options.handlers || !options.handlers.translateVal)
                            html5ProcessInfos.handlers.translateVal = function (val, type, el) { return val;}
                        //compute html5Infos and html5ProcessInfos

                        var fallbacks = html5ProcessInfos.fallbacks;
                        for (var prop in originalSupport) {
                            var support = originalSupport[prop];
                            var fallback = fallbacks[prop];
                            if (support && (!fallback || !fallback.force)) processedSupport[prop] = 4;
                            else if (!fallback) processedSupport[prop] = 1;
                            else processedSupport[prop] = fallback;
                        }
                        handlers = html5ProcessInfos.handlers;
                        handlers.fullReplace = options && options.handlers ? options.handlers.fullReplace : null;
                        handlers.enhance = options && options.handlers ? options.handlers.enhance : {};
                        
                        packInfosForServer();
                    }
                    function process(node) {
                        var type = node.getAttribute("type");
                        var stype = type == "datetime-local" ? "datetime" : type;
                        if (processedSupport[stype] > 3) return;
                        var replace = handlers.replace(type, processedSupport);
                        if (replace == type) return;
                        if (handlers.fullReplace) {
                            handlers.fullReplace(node);
                            return;
                        }
                        var input = document.createElement("input");
                        input.setAttribute("type", type);
                        input.setAttribute("value", handlers.translateVal(node.getAttribute("value"), stype, input));
                        copyAttrs(node, input);
                        node.parentNode.replaceChild(input, node);
                        if (handlers.enhance && handlers[type]) handlers.enhance[type](input);
                    }
                    if (html5ProcessInfos.fallbackHtml5) enhancer["register"](process, true, function (options) {options=options || {}; processOptions(options.browserSupport)});
                }(enhancer));

                //Finish actual code
            })
        ));
    }());
})();