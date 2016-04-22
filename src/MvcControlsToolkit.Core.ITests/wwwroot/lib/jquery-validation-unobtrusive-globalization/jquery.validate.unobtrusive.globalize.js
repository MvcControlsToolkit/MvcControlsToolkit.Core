/// <reference path="../../node_modules/globalize/dist/globalize.js" />
/// <reference path="../../node_modules/globalize/dist/globalize/number.js" />
/// <reference path="../../node_modules/globalize/dist/globalize/date.js" />
(function ($) {
    
    var mvcct = window["mvcct"] = window["mvcct"] || {};
    var support = null;
    var enhancer = mvcct.enhancer;
    if (!enhancer) {
       
        support=Html5InputSupport= {
            number: 4,
            range: 4,
            date: 4,
            month: 4,
            week: 4,
            time: 4,
            datetime: 4,
            email: 4,
            search: 4,
            tel: 4,
            url: 4,
            color: 4
        };

    }
    else {
        support = null;
    }
    var numberRegEx = null;
   
    $.validator.attributeRules = function () { };
    $.validator.globalization = {};
    var initialized = false;
    var defaults = {
        dateFormat: { date: "short" },
        timeFormat: { skeleton: "Hms" },
        timeFormat1: { skeleton: "Hm" },
        datetimeFormat: { skeleton: "yMdHms" },
        datetimeFormat1: { skeleton: "yMdHm" },
        monthFormat: { date: "short" },
        weekFormat: { date: "short" }
    };
    

    var options={};
    var dateParser;

    var timeParser;
    var timeParser1;
    var neutralTimeParser;
    var dateTimeParser;
    var dateTimeParser1;
    var neutralDateTimeParser;
    
    var neutralMonthParser;
    var neutralWeekParser;
    
    var monthParser;
    var weekParser;

    var neutralNumberParser = parseFloat;
    var numberParser;

    var dateTimeAdder, weekAdder, numberAdder;

    var parsers = [null, null, null, null, null, null, null, null, null];
    var neutralParsers = [null, null, null, null, null, null, null, null, null];
    var adders = [null, null, null, null, null, null, null, null, null];
    function getType(jElement) {
        return parseInt(jElement.attr("data-val-correcttype-type"));
    }
    function otherElement(el, ref) {
        var name = el.name;
        var otherName=null;
        var index = name.lastIndexOf(".");
        if (index < 0) otherName = ref;
        else otherElement = name.substring(0, index + 1) + ref;
        if(!otherElement) return null;
        var res = $("[name='" + otherName + "']");
        return res.length > 0 ? res[0] : null;
    };
    function otherElements(el, names) {
        if (!names) return [];
        names = names.split(" ");
        var res = [];
        for (var i = 0; i < names.length; i++) res.push(otherElement(el, names[i]));
        return res;
    }
    function initialize(toptions) {
        if (initialized) return;
        if (!support) support = mvcct.enhancer.getSupport().Html5InputSupport;
        var locale = Globalize.locale().attributes.language;
        var nInfos = Globalize.cldr.get('main/' + locale).numbers["symbols-numberSystem-latn"];

        if (support.number > 2) {
            numberRegEx = new RegExp("^[\+\-\.0-9]*$");
        }
        else {
            numberRegEx = new RegExp("^[\\" + nInfos.plusSign + "\\" + nInfos.minusSign + "\\" + nInfos.decimal + "0-9]*$");
        };
        $.extend(options, defaults);
        $.extend(options, toptions.editFormats || {});
        


        dateParser = support.date > 2 ? Globalize.dateParser({ raw: "yyyy-MM-dd" }) : Globalize.dateParser(options.dateFormat);

        neutralTimeParser = Globalize.dateParser({ raw: "HH:mm:ss" });
        timeParser = support.time> 2 ?  neutralTimeParser : Globalize.dateParser(options.timeFormat);
        timeParser1 = support.time>2 ?  Globalize.dateParser({ raw: "HH:mm" }) : Globalize.dateParser(options.timeFormat1);

        neutralDateTimeParser = Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm:ss" })
        dateTimeParser = support.datetime > 2 ? neutralDateTimeParser : Globalize.dateParser(options.datetimeFormat);
        dateTimeParser1 = support.datetime > 2 ? Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm" }) : Globalize.dateParser(options.datetimeFormat1);

        neutralMonthParser = Globalize.dateParser({ raw: "yyyy-MM" });
        neutralWeekParser = support.week > 2 ? 
            function (s) {
                if (!s) return null;
                parts=s.split("|");
                return parts[0] ;
            }
            :
            function(s){
                if (!s) return null;
                parts = s.split("|");
                var subparts = parts[0].split("-W");
                return new Date(subparts[0], parts[1], parts[2]);
            };

        monthParser = support.month > 2 ? neutralMonthParser : Globalize.dateParser(options.monthFormat);
        weekParser = support.week > 2 ? function (x) { return x } : dateParser;

        numberParser = support.number > 2 ? parseFloat : Globalize.numberParser();

        numberAdder = function (base, value) {
            if (!base ) return null;
            if (!value) return base;
            return base + parseFloat(value);
        }

        dateTimeAdder = function (base, value) {
            if (!base) return null;
            if (!value) return base;
            return new Date(base.getTime() + parseInt(value));
        }
        var weekHelp = document.createElement("INPUT");
        weekHelp.setAttribute("type", "week");
        weekAdder = support.week > 2 ?
            function (base, value) {
                if (!base) return null;
                if (!value) return base;
                var toAdd = parseInt(value) / (1000 * 3600 * 24 * 7);
                if (toAdd == 0) return base;
                weekHelp.value = base;
                if (toAdd > 0) weekHelp.stepUp(toAdd);
                else weekHelp.stepDown(-toAdd);
                return weekHelp.value;
            } :
            dateTimeAdder;

        parsers[0] = function (x) { return x };
        neutralParsers[0] = parsers[0];
        adders[0] = function (x) { return x };

        parsers[1] = parsers[2] = parsers[3] = numberParser;
        neutralParsers[1] = neutralParsers[2] = neutralParsers[3] = neutralNumberParser;
        adders[1] = adders[2] = adders[3] = numberAdder;

        parsers[4] = function (x) {
            return timeParser(x) || timeParser1(x);
        }
        neutralParsers[4] = neutralTimeParser;
        

        parsers[5] = dateParser;
        neutralParsers[5] = neutralDateTimeParser;

        parsers[6] = function (x) {
            return dateTimeParser(x) || dateTimeParser1(x);
        }
        neutralParsers[6] = neutralDateTimeParser;
        adders[4] = adders[5] = adders[6] = dateTimeAdder;

        parsers[7] = weekParser;
        neutralParsers[7] = neutralWeekParser;
        adders[7] = weekAdder;

        parsers[8] = monthParser;
        neutralParsers[8] = neutralMonthParser;
        adders[8] = dateTimeAdder;

        if (enhancer && toptions) {
            
            function numericInputCharHandler(charCode, el, decimalSeparator, plus, minus) {
                var jEl = $(el);
                var type = getType(jEl);
                var canDecimal = type==3, canNegative = type != 1;
                var min = jEl.attr("min");
                if (min === undefined) min = jEl.attr("data-val-range-min");
                if (min !== undefined && parseFloat(min) >= 0) canNegative = false;
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
            function enhanceNumeric(input) {
                if (input.getAttribute("type") != "text") return;
                $(input).bind('keypress', function (event) {
                    return numericInputCharHandler(event.which, event.target, nInfos.decimal, nInfos.plusSign, nInfos.minusSign);
                });
            }
            if (!toptions.browserSupport) toptions.browserSupport = {};
            var handlers = toptions.browserSupport.handlers;
            if (!handlers) handlers = toptions.browserSupport.handlers = {};
            if (!handlers.enhance) handlers.enhance = {};
            if (!handlers.enhance.number) handlers.enhance.number = enhanceNumeric;
            if (!handlers.enhance.range) handlers.enhance.range = enhanceNumeric;
            if (!handlers.translateVal) {
                var numberFormatter = Globalize.numberFormatter();
                var timeFormatter = Globalize.dateFormatter(options.timeFormat);
                var sTimeParser = Globalize.dateParser({ raw: "HH:mm" });
                var gtimeParser = function (x) { return neutralTimeParser(x) || sTimeParser(x);};
                var dateTimeFormatter = Globalize.dateFormatter(options.datetimeFormat1);
                var sDateTimeParser = Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm" });
                var gDatetimeParser = function (x) { return neutralDateTimeParser(x) || sDateTimeParser(x);};
                var dateFormatter = Globalize.dateFormatter(options.dateFormat);
                var neutralDateParser=Globalize.dateParser({ raw: "yyyy-MM-dd" });
                var dict = {
                    range: function (x) { return x ? numberFormatter(parseFloat(x)) : ""; },
                    number: function (x) { return x ? numberFormatter(parseFloat(x)) : ""; },
                    time: function (x) { return x ? timeFormatter(gtimeParser(x)) : ""; },
                    datetime: function (x) { return x ? dateTimeFormatter(gDatetimeParser(x)) : ""; },
                    date: function (x) { return x ? dateFormatter(neutralDateParser(x)) : ""; },
                    month: function (x) { return x ? dateFormatter(neutralMonthParser(x)) : ""; },
                    week: function (x, t, input) {
                        var res = input.getAttribute("data-date-value");
                        if (!res) return res;
                        return dateFormatter(neutralDateParser(res));
                    }

                };
                handlers.translateVal = function (value, type, input) {
                    var tr = dict[type];
                    if (tr) return tr(value, type, input);
                    else return value;
                };
            }
        }

        
        initialized = true;
    }
    var $jQval = $.validator;
    $.validator.methods.minE = function (value, element, param) {
        if (!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        var parser = parsers[type];
        var neutralParser = neutralParsers[type];
        return parser(value) >= neutralParser(param);
    }
    $.validator.methods.maxE = function (value, element, param) {
        if(!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        var parser = parsers[type];
        var neutralParser = neutralParsers[type];
        return parser(value) <= neutralParser(param);
    }
    $.validator.methods.rangeE = function (value, element, param) {
        if(!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        var parser = parsers[type];
        var neutralParser = neutralParsers[type];
        return parser(value) <= neutralParser(param[1]) && parser(value) >= neutralParser(param[0]);
    }
    $.validator.methods.correcttype = function (value, element, param) {
        if(!enhancer) initialize();
        if (!value) return true;
        var type = parseInt(param);
        var parser = parsers[type];
        var val = parser(value);
        
        return (val || val === 0)
            &&(!(typeof val === "number") || numberRegEx.test(value))
            && ((type != 1 && type != 2) || (val % 1) === 0 )
            && (type != 1 || val >= 0);
    }
    $.validator.methods.drange = function (value, element, param) {
        if (!enhancer) initialize();
        if (!value) return true;
        var type = getType($(element));
        var mins = param[0];
        var minDelays = param[1];
        var maxs = param[2];
        var maxDelays = param[3];
        var parser = parsers[type];
        var adder = adders[type];
        var tvalue = parser(value);
        for (var i = 0; i < mins.length; i++) {
            var otherVal = mins[i].value;
            if (!otherVal) continue;
            otherVal = parser(otherVal);
            if (!otherVal) continue;
            otherVal=adder(otherVal, minDelays[i]);
            if (tvalue < otherVal) return false;
        }
        for (var i = 0; i < maxs.length; i++) {
            var otherVal = maxs[i].value;
            if (!otherVal) continue;
            otherVal = parser(otherVal);
            if (!otherVal) continue;
            otherVal=adder(otherVal, maxDelays[i]);
            if (tvalue > otherVal) return false;
        }
        return true;
    }
    var adapters = $jQval.unobtrusive.adapters;
    var index = 0;
    for (index = 0; index < adapters.length; index++) if (adapters[index].name == "range") break;
    delete adapters[index];
    function setValidationValues(options, ruleName, value) {
        options.rules[ruleName] = value;
        if (options.message) {
            options.messages[ruleName] = options.message;
        }
    }
    adapters.addMinMax = function (adapterName, minRuleName, maxRuleName, minMaxRuleName, minAttribute, maxAttribute) {
        return this.add(adapterName, [minAttribute || "min", maxAttribute || "max"], function (options) {
            var min = options.params.min,
                max = options.params.max;
            if ((min || min === 0) && (max || max === 0)) {
                setValidationValues(options, minMaxRuleName, [min, max]);
            }
            else if (min || min === 0) {
                setValidationValues(options, minRuleName, min);
            }
            else if (max || max === 0) {
                setValidationValues(options, maxRuleName, max);
            }
        });
    };

    adapters.addCorrecttype = function (adapterName, ruleName, paramName) {
        return this.add(adapterName, [paramName || "type"], function (options) {
            var type = options.params.type;
            setValidationValues(options, ruleName, type);
        });
    };
    
    adapters.addCorrecttype("correcttype", "correcttype");
    adapters.addMinMax("range", "minE", "maxE", "rangeE");
    
    if (enhancer) {
        function addHandler(el, limits) {
            if (!limits) return;
            for (var i = 0; i < limits.length; i++) {
                enhancer.dependency("drange", limits[i], el, ["blur", "keyup"], function (t, s) {
                    $(t).closest('form').validate().element(t);
                });
            }
        }
        adapters.addDRange = function (adapterName, ruleName, minsName, minDelaysName, maxsName, maxDelaysName) {
            return this.add(adapterName, [minsName, minDelaysName, maxsName, maxDelaysName], function (options) {
                var el = options.element;
                var mins = otherElements(el, options.params[minsName]);
                var maxs = otherElements(el, options.params[maxsName]); 
                addHandler(el, mins);
                addHandler(el, maxs);
                var mdelays = options.params[minDelaysName].split(" ");
                var maxdelays = options.params[maxDelaysName].split(" ");
                setValidationValues(options, ruleName, [mins, mdelays, maxs, maxdelays]);
            });
        };
        adapters.addDRange("drange", "drange", "dmins", "dminds", "dmaxs", "dmaxds");
        enhancer["register"](null, false, initialize, "html5 globalized fallback");
    }
})(jQuery);