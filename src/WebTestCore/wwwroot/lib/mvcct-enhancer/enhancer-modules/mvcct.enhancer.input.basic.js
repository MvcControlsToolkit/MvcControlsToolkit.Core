(function () {
    var DEBUG = true;
    (function (undefined) {
        var window = this || (0, eval)('this');
        (function (factory) {
            if (typeof define === 'function' && define['amd']) {
                // [1] AMD anonymous module
                define(["../mvcct.enhancer.min"], factory);
            } else if (typeof exports === 'object' && typeof module === 'object') {
                // [2] CommonJS/Node.js
                module["exports"] = factory(require("mvcct-enhancer"));  // module.exports is for Node.js
            } else {
                // [3] No module loader (plain <script> tag) - put directly in global namespace
                var mvcct = window["mvcct"]  = window["mvcct"] || {};
                factory(mvcct['enhancer'], window["globalize"]);
            }
        }(

            (function (Enhancer) {
                
                //Start actual code
                var defaults = {
                    "dateFormat": { "date": "short" },
                    "timeFormat": { "skeleton": "Hms" },
                    "timeFormat1": { "skeleton": "Hms" },
                    "datetimeFormat": { "datetime": "short" },
                    "datetimeFormat1": { "datetime": "short" },
                    "monthFormat": { "date": "short" },
                    "weekFormat": { "date": "short" }
                };
                var neutralTimeParser, sTimeParser, gtimeParser, neutralDateTimeParser,
                    sDateTimeParser, gDatetimeParser, neutralDateParser, neutralMonthParser;
                var neutralTimeFormatter, neutralDateTimeFormatter, neutralDateFormatter, neutralMonthFormatter;
                
                var localNumberParser, localTimeParser, localDateTimeParser, localDateParser, 
                    localMonthParser, localWeekParser;     
                var numberFormatter, timeFormatter, dateTimeFormatter, dateFormatter, 
                    weekFormatter, monthFormatter;
                
                var options={};
                var initialized = false;
                function getType(node) {
                    var type = node.getAttribute("data-val-correcttype-type") || "3";
                    return parseInt(type);
                }
                function getDateOfISOWeek(cw) {
                    var parts=cw.split('-W');
                    
                    var y=parseInt(parts[0]);
                    var w = parseInt(parts[1]);
                    
                    var simple = new Date(y, 0, 1 + (w - 1) * 7);
                    var dow = simple.getDay();
                    var ISOweekStart = simple;
                    if (dow <= 4)
                        ISOweekStart.setDate(simple.getDate() - simple.getDay() + 1);
                    else
                        ISOweekStart.setDate(simple.getDate() + 8 - simple.getDay());
                    return ISOweekStart;
                }
                function getIsoWeek(d) {
                    d = new Date(+d);
                    d.setHours(0,0,0);
                    d.setMilliseconds(0);
                    d.setDate(d.getDate() + 4 - (d.getDay()||7));
                    var yearStart = new Date(d.getFullYear(),0,1);
                    var weekNo = Math.ceil(( ( (d - yearStart) / 86400000) + 1)/7);
                    weekNo=weekNo+"";
                    if (weekNo.length<2) weekNo="0"+weekNo
                    var year = d.getFullYear() +"";
                    while(year.length<4) year="0"+year;
                    return year+"-W"+weekNo;
                }
                function initialize(toptions, Globalize){
                    if(initialized) return;
                    Enhancer["Globalize"] = function(){return Globalize;};
                    var userOptions=toptions["editFormats"] || {};
                    for(var prop in defaults){
                        options[prop]=userOptions[prop] === undefined ? defaults[prop] : userOptions[prop];
                    }
                    var locale = Globalize["locale"]()["attributes"]["language"];
                    var nInfos = Globalize["cldr"]["get"]('main/' + locale)["numbers"]["symbols-numberSystem-latn"];
                        
                        neutralTimeParser = Globalize["dateParser"]({ "raw": "HH:mm:ss" });
                        sTimeParser = Globalize["dateParser"]({ "raw": "HH:mm" });
                        gtimeParser = function (x) { return neutralTimeParser(x) || sTimeParser(x); };
                        neutralDateTimeParser = Globalize["dateParser"]({ "raw": "yyyy-MM-ddTHH:mm:ss" });
                        sDateTimeParser = Globalize["dateParser"]({ "raw": "yyyy-MM-ddTHH:mm" });
                        gDatetimeParser = function (x) { return neutralDateTimeParser(x) || sDateTimeParser(x);};
                        neutralDateParser = Globalize["dateParser"]({ "raw": "yyyy-MM-dd" });
                        neutralMonthParser = Globalize["dateParser"]({ "raw": "yyyy-MM" });
                        
                        neutralTimeFormatter=Globalize["dateFormatter"]({ "raw": "HH:mm:ss" });
                        neutralDateTimeFormatter=Globalize["dateFormatter"]({ "raw": "yyyy-MM-ddTHH:mm:ss" });
                        neutralDateFormatter=Globalize["dateFormatter"]({ "raw": "yyyy-MM-dd" });
                        neutralMonthFormatter=Globalize["dateFormatter"]({ "raw": "yyyy-MM" });
                        
                        localNumberParser=Globalize["numberParser"]();
                        var localTimeParser1=  Globalize["dateParser"](options["timeFormat"]);
                        var localTimeParser2=  Globalize["dateParser"](options["timeFormat1"]);
                        localTimeParser=function(x){return localTimeParser1(x) || localTimeParser2(x)};
                        var localDateTimeParser1=Globalize["dateParser"](options["datetimeFormat"]); 
                        var localDateTimeParser2 = Globalize["dateParser"](options["datetimeFormat1"]);
                        localDateTimeParser = function(x){return  localDateTimeParser1(x) || localDateTimeParser2(x)};
                        localDateParser = Globalize["dateParser"](options["dateFormat"]);
                        localWeekParser = Globalize["dateParser"](options["weekFormat"]);
                        localMonthParser = Globalize["dateParser"](options["monthFormat"]);
                        
                        numberFormatter = Globalize["numberFormatter"]();
                        timeFormatter =  Globalize["dateFormatter"](options["timeFormat"]);
                        dateTimeFormatter = Globalize["dateFormatter"](options["datetimeFormat1"]);
                        dateFormatter = Globalize["dateFormatter"](options["dateFormat"]);
                        weekFormatter = Globalize["dateFormatter"](options["weekFormat"]);
                        monthFormatter = Globalize["dateFormatter"](options["monthFormat"]);
                        
                    function numericInputCharHandler(charCode, el, decimalSeparator, plus, minus) {
                        var type = getType(el);
                        var canDecimal = type==3, canNegative = type != 1;
                        var min = el.getAttribute("min");
                        if (!min) min = el.getAttribute("data-val-range-min");
                        if (min && parseFloat(min) >= 0) canNegative = false;
                        if (charCode == 0 || charCode == 13 || charCode == 9 || charCode == 8 || (charCode >= 48 && charCode <= 57)) return true;
                        if (canNegative && (charCode == plus.charCodeAt(0) || charCode == minus.charCodeAt(0))) {
                            var value = el.value;
                            return value.indexOf(plus) < 0 && value.indexOf(minus) < 0;
                        }
                        if (canDecimal && charCode == decimalSeparator.charCodeAt(0)) {
                            var value = el.value;
                            return value.indexOf(decimalSeparator) < 0;
                        }
                        return false;
                    }
                    // event.type must be keypress
                    function getChar(event) {
                        if (event.which == null) {
                                return event.keyCode // IE
                        } else if (event.which!=0 && event.charCode!=0) {
                                return event.which   // the rest
                        } else {
                            return null; // special key
                        }
                    }

                    function enhanceNumeric(input) {
                        if (input.getAttribute("type") != "text") return;
                        input.addEventListener('keypress', function (event) {
                            event = event || window.event;
                            if (!numericInputCharHandler(getChar(event), input, nInfos["decimal"], nInfos["plusSign"], nInfos["minusSign"]))
                                event.preventDefault();
                        });  
                    }
                    if (!toptions["browserSupport"]) toptions["browserSupport"] = {};
                    toptions["browserSupport"]["fallbackHtml5"]=true;
                    
                    var fallbacks = toptions["browserSupport"]["fallbacks"] 
                        = toptions["browserSupport"]["fallbacks"] || {};
                    
                    if (!fallbacks["number"]) fallbacks["number"]={};
                    if (!fallbacks["number"]["type"]) fallbacks["number"]["type"]=2;
                    
                    if (!fallbacks["date"]) fallbacks["date"]={};
                    if (!fallbacks["date"]["type"]) fallbacks["date"]["type"]=2;
                    
                    if (!fallbacks["datetime"]) fallbacks["datetime"]={};
                    if (!fallbacks["datetime"]["type"]) fallbacks["datetime"]["type"]=2;
                    
                    if (!fallbacks["time"]) fallbacks["time"]={};
                    if (!fallbacks["time"]["type"]) fallbacks["time"]["type"]=2;
                    
                    if (!fallbacks["week"]) fallbacks["week"]={};
                    if (!fallbacks["week"]["type"]) fallbacks["week"]["type"]=2;
                    
                    if (!fallbacks["month"]) fallbacks["month"]={};
                    if (!fallbacks["month"]["type"]) fallbacks["month"]["type"]=2;
                    
                    
                    var handlers = toptions["browserSupport"]["handlers"];
                    if (!handlers) handlers = toptions["browserSupport"]["handlers"] = {};
                    if (!handlers["enhance"]) handlers["enhance"] = {};
                    if (!handlers["enhance"]["number"]) handlers["enhance"]["number"] = enhanceNumeric;
                    if (!handlers["enhance"]["range"]) handlers["enhance"]["range"] = enhanceNumeric;
                    if (!handlers["translateVal"]) {
                    
                        var dict = {
                            "range": function (x) { return x ? numberFormatter(parseFloat(x)) : ""; },
                            "number": function (x) { return x ? numberFormatter(parseFloat(x)) : ""; },
                            "time": function (x) { return x ? timeFormatter(gtimeParser(x)) : ""; },
                            "datetime": function (x) { return x ? dateTimeFormatter(gDatetimeParser(x)) : ""; },
                            "date": function (x) { return x ? dateFormatter(neutralDateParser(x)) : ""; },
                            "month": function (x) { return x ? monthFormatter(neutralMonthParser(x)) : ""; },
                            "week": function (x) {
                                return weekFormatter(getDateOfISOWeek(x));
                            }

                        };
                        handlers["translateVal"] = function (value, type, newType) {
                            if(!value) return "";
                            var tr = dict[type];
                            if (tr && newType=="text") return tr(value, type, newType);
                            else return value;
                        };
                    }
                    
                    
                    initialized = true;
                }
                function initializeOptions(toptions){
                     support=Enhancer["getSupport"]()["Html5InputSupport"];
                     var dict = {
                            "range": support["range"]>2 ? parseFloat : localNumberParser,
                            "number":support["number"]>2 ? parseFloat : localNumberParser,
                            "time": support["time"]>2 ? gtimeParser : localTimeParser,
                            "datetime": support["datetime"]>2 ? gDatetimeParser : localDateTimeParser,
                            "date": support["date"]>2 ? neutralDateParser : localDateParser,
                            "month": support["month"]>2 ? neutralMonthParser : localMonthParser,
                            "week": support["week"]>2 ? getDateOfISOWeek : localWeekParser
                     };
                      var dictI = {
                            "range": parseFloat,
                            "number": parseFloat,
                            "time": gtimeParser,
                            "datetime": gDatetimeParser,
                            "date": neutralDateParser,
                            "month": neutralMonthParser,
                            "week": getDateOfISOWeek
                     };
                     var fdict = {
                            "range": support["range"]>2 ? function(x){return x+"";} : numberFormatter,
                            "number":support["number"]>2 ? function(x){return x+"";} : numberFormatter,
                            "time": support["time"]>2 ? neutralTimeFormatter : timeFormatter,
                            "datetime": support["datetime"]>2 ? neutralDateTimeFormatter : dateTimeFormatter,
                            "date": support["date"]>2 ? neutralDateFormatter : dateFormatter,
                            "month": support["month"]>2 ? neutralMonthFormatter : monthFormatter,
                            "week": support["week"]>2 ? getIsoWeek : weekFormatter
                     }; 
                     var fdictI = {
                            "range": function(x){return x+"";},
                            "number": function(x){return x+"";},
                            "time":  neutralTimeFormatter,
                            "datetime": neutralDateTimeFormatter,
                            "date": neutralDateFormatter,
                            "month": neutralMonthFormatter,
                            "week": getIsoWeek
                     }; 
                     Enhancer["format"]=function(type, val, invariant){
                         if(!val && val !== 0 ) return "";
                         var formatter = (invariant ? fdictI : fdict)[type];
                         if(formatter) return formatter(val);
                         else return val;
                     } 
                     Enhancer["parse"]=function(type, val, invariant){
                         if(!val ) return null;
                         var parser = (invariant ? dictI : dict)[type];
                         if(parser) return parser(val);
                         else return parser;
                     } 
                            
                }
                Enhancer["addBasicInput"]=function(Globalize){
                    Enhancer["register"](null, false, initializeOptions, "html5 globalized fallback", function(x) {initialize(x, Globalize);});
                }

                //Finish actual code
                return Enhancer;
            })
        ));
    }());
})();