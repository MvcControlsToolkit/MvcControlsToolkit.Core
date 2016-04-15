/// <reference path="../../node_modules/globalize/dist/globalize.js" />
/// <reference path="../../node_modules/globalize/dist/globalize/number.js" />
/// <reference path="../../node_modules/globalize/dist/globalize/date.js" />
(function ($) {
    
    var mvcct = window["mvcct"] = window["mvcct"] || {};
    var support = null;
    var enhancer = mvcct.enhancer;
    if (enhancer) {
       
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
        support = mvcct.enhancer.getSupport();
    }

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

    var parsers = [null, null, null, null, null, null, null, null, null];
    var neutralParsers = [null, null, null, null, null, null, null, null, null];
    function getType(jElement) {
        return parseInt(jElement.attr("data-val-correcttype-type"));
    }
    function initialize(toptions) {
        if (initialized) return;
        
        $.extend(options, defaults);
        $.extend(options, $.validator.attributeRules);
        
        dateParser = support.date > 1 ? Globalize.dateParser({ raw: "yyyy-MM-dd" }) : Globalize.dateParser(options.dateFormat);

        neutralTimeParser = Globalize.dateParser({ raw: "HH:mm:ss" });
        timeParser = support.time> 1 ?  neutralTimeParser : Globalize.dateParser(options.timeFormat);
        timeParser1 = support.time>1 ?  Globalize.dateParser({ raw: "HH:mm" }) : Globalize.dateParser(options.timeFormat1);

        neutralDateTimeParser = Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm:ss" })
        dateTimeParser = support.datetime > 1 ? neutralDateTimeParser : Globalize.dateParser(options.datetimeFormat);
        dateTimeParser1 = support.datetime > 1 ? Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm" }) : Globalize.dateParser(options.datetimeFormat1);

        neutralMonthParser = Globalize.dateParser({ raw: "yyyy-MM" });
        neutralWeekParser = support.week > 1 ? 
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

        monthParser = support.month > 1 ? neutralMonthParser : Globalize.dateParser(options.monthFormat);
        weekParser = support.week > 1 ? function (x) { return x } : dateParser;

        numberParser = support.number ? parseFloat : Globalize.numberParser();

        parsers[0] = function (x) { return x };
        neutralParsers[0] = parsers[0];

        parsers[1] = parsers[2] = parsers[3] = numberParser;
        neutralParsers[1] = neutralParsers[2] = neutralParsers[3] = neutralNumberParser;

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

        parsers[7] = weekParser;
        neutralParsers[7] = neutralWeekParser;

        parsers[8] = monthParser;
        neutralParsers[8] = neutralMonthParser;

        if (enhancer && toptions) {
            var nInfos = Globalize.cldr.attributes.numbers["symbols-numberSystem-latn"];
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
                this.bind('keypress', function (event) {
                    return numericInputCharHandler(event.which, event.target, nInfos.decimal, nInfos.plusSign, nInfos.minusSign);
                });
            }
            if (!toptions.browserSupport) toptions.browserSupport = {};
            var handlers = toptions.browserSupport.handlers;
            if (!handlers) handlers = toptions.browserSupport.handlers = {};
            if (!handlers.enhance) handlers.enhance = {};
            if (!handlers.enhance.number) handlers.number = enhanceNumeric;
            if (!handlers.enhance.number) handlers.range = enhanceNumeric;
            if (!handlers.translateVal) {
                var numberFormatter = Globalize.numberFormatter();
                var timeFormatter = Globalize.dateFormatter(options.timeFormat)();
                var sTimeParser = Globalize.dateParser({ raw: "HH:mm" })();
                var gtimeParser = function (x) { return neutralTimeParser(x) || sTimeParser(x);};
                var dateTimeFormatter = Globalize.dateFormatter(options.datetimeFormat1)();
                var sDateTimeParser = Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm" })();
                var gDatetimeParser = function (x) { return neutralDateTimeParser(x) || sDateTimeParser(x);};
                var dateFormatter = Globalize.dateFormatter(options.dateFormat)();
                var neutralDateParser=Globalize.dateParser({ raw: "yyyy-MM-dd" })();
                var dict = {
                    range: function (x) { return x ? numberFormatter(Globalize.foparseFloat(x)) : ""; },
                    number: function (x) { return x ? numberFormatter(Globalize.foparseFloat(x)) : ""; },
                    time: function (x) { return x ? timeFormatter(gtimeParser(x)) : ""; },
                    datetime: function (x) { return x ? dateTimeFormatter(gDatetimeParser(x)) : ""; },
                    date: function (x) { return x ? dateFormatter(neutralDateParser(x)) : ""; },
                    month: function (x) { return x ? dateFormatter(neutralMonthParser(x)) : ""; },
                    week: function (x, t, input) { return input.getAttribute("data-date-value");}

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
        parser = parsers[type];
        neutralParser = neutralParsers[type];
        return parser(value) >= neutralParser(param);
    }
    $.validator.methods.maxE = function (value, element, param) {
        if(!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        parser = parsers[type];
        neutralParser = neutralParsers[type];
        return parser(value) <= neutralParser(param);
    }
    $.validator.methods.rangeE = function (value, element, param) {
        if(!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        parser = parsers[type];
        neutralParser = neutralParsers[type];
        return parser(value) <= neutralParser(param[1]) && parser(value) >= neutralParser(param[0]);
    }
    $.validator.methods.correcttype = function (value, element, param) {
        if(!enhancer) initialize();
        if (!value) return true;
        var type = parseInt(param);
        parser = parsers[type];
        var val = parser(value);
        return (val || val === 0)
            && ((type != 1 && type != 2) || (val % 1) === 0 )
            && (type != 1 || val >= 0);
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
    adapters.addMinMax("range", "minE", "maxE", "rangeE")
    if (enhancer) {
        enhancer["register"](null, false, initialize);
    }
})(jQuery);