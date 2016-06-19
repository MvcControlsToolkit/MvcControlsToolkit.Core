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
                var mvcct = window["mvcct"]  = window["mvcct"] || {};
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
                var baseEvent = "_enhancer.dependency.";
                var dependencyOnPathAttribute = "data-enhancer-dependency";
                var jQuery = window["jQuery"];
                if (jQuery) {
                    oldReady = jQuery["fn"]["ready"];
                    jQuery["fn"]["ready"] = function (x) {
                        transformations.push({
                            transform: null,
                            initialize: x,
                            processOptions: null,
                            name: "document.ready: "+x.constructor.name,
                            preProcessOptions: null,
                            isReady: true
                        });
                    };
                }
                function init(options) {
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.preProcessOptions) {
                            try{
                                item.preProcessOptions(options, item);
                            }
                            catch(ex){
                                if (DEBUG) {
                                    alert(ex + ". " + item.name);
                                }
                            }
                        }
                    }
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.processOptions) {
                            try{
                                item.processOptions(options, item);
                            }
                            catch(ex){
                                if (DEBUG) {
                                    alert(ex + ". " + item.name);
                                }
                            }
                        }
                    }
                    var newTransformations=[];
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.initialize ) {
                            if(item.isReady && !options.runReady) continue;
                            try{
                                if(item.transform){
                                    item.transform(document.querySelector('body'), true);
                                    newTransformations.push(item);
                                }
                                else
                                    item.initialize(document.querySelector('body'));
                            }
                            catch (ex) {
                                if (DEBUG) {
                                    alert(ex + ". " + item.name);
                                }
                            }
                        }
                    }
                    transformations=newTransformations;
                }
                enhancer["init"] = function (options) {
                    options = options || {};
                    if (jQuery) {
                        jQuery["fn"]["ready"] = oldReady;
                        jQuery(document)["ready"](function () { init(options); });
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
                enhancer["register"] = function (transform, initialize, processOptions, name, preProcessOptions) {
                    transformations.push({
                        transform: transform,
                        initialize: initialize,
                        processOptions: processOptions,
                        name: name,
                        preProcessOptions: preProcessOptions
                    });
                };
                enhancer["transform"] = function (node) {
                    for (var i = 0; i < transformations.length; i++) {
                        var item = transformations[i];
                        if (item.transform) {
                            try {
                                item.transform(node, false);
                            }
                            catch (ex) {
                                if (DEBUG) {
                                    alert(ex + ". " + item.name);
                                }
                            }
                            
                        }
                    }
                };
                enhancer["dependency"] = function (name, sourceNode, targetNode, eventNames, action) {
                    var mainF = function () {
                        if (!sourceNode.getAttribute(dependencyOnPathAttribute)) {
                            try {
                                sourceNode.setAttribute(dependencyOnPathAttribute, "true");
                                action(targetNode, sourceNode);
                                var ev = document.createEvent("Event");
                                ev.initEvent(baseEvent + name, false, true);
                                targetNode.dispatchEvent(ev);

                            }
                            finally {
                                sourceNode.setAttribute(dependencyOnPathAttribute, "");
                            }
                        }
                    };
                    var triggerF=function () {
                        var ev = document.createEvent("Event");
                        ev.initEvent(baseEvent + name, false, true);
                        sourceNode.dispatchEvent(ev);
                    };
                    sourceNode.addEventListener(baseEvent + name, mainF);
                    for (var i = 0; i < eventNames.length; i++) 
                        sourceNode.addEventListener(eventNames[i], triggerF);
                    return  {
                        mainF: mainF,
                        triggerF: triggerF,
                        name: baseEvent + name,
                        events: eventNames,
                        node: sourceNode
                    };
                }
                enhancer["removeDependency"] = function (handle) {
                    var events = handle.events;
                    for (var i = 0; i < events.length; i++)
                        handle.sourceNode.removeEventListener(events[i], handle.triggerF);
                    handle.sourceNode.removeEventListener(handle.name, handle.mainF);
                };
                //html5 enhancement
                (function (enhancer) {
                    var originalSupport = autodetect();
                    var processedSupport = {};
                    var html5Infos = {
                        "Html5InputOriginalSupport": originalSupport,
                        "Html5InputSupport": processedSupport
                    };
                    var html5ProcessInfos = null;//describe required processing based on user options and defaults
                    var html5ProcessInfosDefaults = {
                        "cookie": "_browser_basic_capabilities",
                        "forms": null
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
                            if(typeof(val) === 'boolean') val= val ? "True":"False";
                            else val = ''+val;
                            if (!object.hasOwnProperty(prop)) continue;
                            outputArray.push({
                                "Key": prefix ? prefix + "." + prop : prop,
                                "Value": val
                            });
                        }
                    }
                    function copyAttrs(elm, mains) {
                        for (var iAttr = 0; iAttr < elm.attributes.length; iAttr++) {
                            var attribute = elm.attributes[iAttr].name;
                            if (attribute === "min" || attribute === "max" || attribute === "type" || attribute === "value") continue;
                            else mains.setAttribute(attribute, elm.attributes[iAttr].value);
                        }
                    }
                    function autodetect() {
                        //html5 auto detection
                        return {
                            "number": !needFallback('number'),
                            "range": !needFallback('range'),
                            "date": !needFallback('date'),
                            "month": !needFallback('month'),
                            "week": !needFallback('week'),
                            "time": !needFallback('time'),
                            "datetime": !needFallback('datetime-local'),
                            "email": !needFallback('email'),
                            "search": !needFallback('search'),
                            "tel": !needFallback('tel'),
                            "url": !needFallback('url'),
                            "color": !needFallback('color')
                        }
                    }
                    function packInfosForServer() {
                        //pack html5Infos in cookie and or hidden field for server
                        var infos = [];
                        addToOptions("Html5InputSupport", html5Infos["Html5InputSupport"], infos);
                        addToOptions("Html5InputOriginalSupport", html5Infos["Html5InputOriginalSupport"], infos);
                        var sInfos = JSON.stringify(infos);
                        if (html5ProcessInfos["forms"]) {
                            var flist = document.querySelectorAll('form');
                            for (var i = 0; i < flist.length; i++)
                            {
                                var input = document.createElement("input");
                                input.setAttribute("type", "hidden");
                                input.setAttribute("name", html5ProcessInfos["forms"]);
                                input.setAttribute("value", sInfos);
                                flist[i].appendChild(input);
                            }
                        }
                        if (html5ProcessInfos["cookie"]) {
                            document.cookie = html5ProcessInfos["cookie"] + "=" + encodeURIComponent(sInfos) + "; path=/";
                        }
                    }
                    function preProcessOptions(options, entry) {
                        var fallbacks = options["fallbacks"];
                        for (var prop in originalSupport) {
                            var support = originalSupport[prop];
                            var fallback = fallbacks[prop];
                            if (support && (!fallback || !fallback["force"])) processedSupport[prop] = 4;
                            else if (!fallback) processedSupport[prop] = 1;
                            else processedSupport[prop] = fallback["type"];
                        }
                    }
                    function processOptions(options, entry) {
                        options = options || {};
                        
                        html5ProcessInfos = {
                            "fallbackHtml5": options["fallbackHtml5"] === undefined ? false : options["fallbackHtml5"],
                            "cookie": options["cookie"] === undefined ? html5ProcessInfosDefaults["cookie"] : options["cookie"],
                            "forms": options["forms"] === undefined ? html5ProcessInfosDefaults["forms"] : options["forms"],
                            "fallbacks": options["fallbacks"] || {},
                            "handlers": {}
                        };
                        
                        if (!options["handlers"] || !options["handlers"]["replace"])
                        {
                            html5ProcessInfos["handlers"]["replace"] = function (type, support) {
                                if (type == "number" || type == "date" || type == "datetime-local" || type == "month" || type == "time" || type == "week" || type == "color")
                                    return "text";
                                else if (type == "range") {
                                    if (support.range > 2 || support.number < 4) return "text";
                                    return "number";
                                }
                                else return type;
                            };
                        }
                        else html5ProcessInfos["handlers"]["replace"] = options["handlers"]["replace"];
                        if (!options["handlers"] || !options["handlers"]["translateVal"])
                            html5ProcessInfos["handlers"]["translateVal"] = function (val, type, el) { return val; }
                        else html5ProcessInfos["handlers"]["translateVal"] = options["handlers"]["translateVal"];
                        

                        
                        handlers = html5ProcessInfos["handlers"];
                        handlers["fullReplace"] = options && options["handlers"] ? options["handlers"]["fullReplace"] : null;
                        handlers["enhance"] = options && options["handlers"] ? options["handlers"]["enhance"] : {};
                        
                        packInfosForServer();
                    }
                    function processAllNodes(ancestor) {
                        if(!html5ProcessInfos["fallbackHtml5"]) return;
                        if (ancestor.tagName == "INPUT") process(ancestor);
                        else {
                            var allInputs = ancestor.querySelectorAll("input");
                            for (var i = 0; i < allInputs.length; i++) process(allInputs[i]);
                        }
                    }
                    function process(node) {
                        var type = node.getAttribute("type");
                        var stype = type == "datetime-local" ? "datetime" : type;
                        if (processedSupport[stype] > 3) return;
                        var replace = handlers["replace"](type, processedSupport);
                        if (replace == type) return;
                        if (handlers["fullReplace"]) {
                            handlers["fullReplace"](node);
                            return;
                        }
                        var input = document.createElement("input");
                        input.setAttribute("type", replace);
                        input.setAttribute("value", handlers["translateVal"](node.getAttribute("value"), stype, replace));
                        copyAttrs(node, input);
                        if(type == "range") input.setAttribute("data-is-range", "true");
                        node.parentNode.replaceChild(input, node);
                        if (handlers["enhance"] && handlers["enhance"][stype]) handlers["enhance"][stype](input, node);
                    }
                    enhancer["register"](null, false, function (options) { options = options || {}; preProcessOptions(options["browserSupport"]) }, "html5 support");
                    enhancer["register"](processAllNodes, true, function (options) { options = options || {}; processOptions(options["browserSupport"]) }, "html5 enhance");
                }(enhancer));

                //Finish actual code
            })
        ));
    }());
})();