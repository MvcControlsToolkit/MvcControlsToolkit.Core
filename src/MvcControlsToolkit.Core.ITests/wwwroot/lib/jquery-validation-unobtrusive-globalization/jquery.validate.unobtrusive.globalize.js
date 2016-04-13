/// <reference path="../../node_modules/globalize/dist/globalize.js" />
/// <reference path="../../node_modules/globalize/dist/globalize/number.js" />
/// <reference path="../../node_modules/globalize/dist/globalize/date.js" />
(function ($) {
    var mvcct = window["mvcct"] = window["mvcct"] || {};
    if (!mvcct.enhancer) {
        mvcct.enhancer = {
            Browser: {
                Html5InputSupport: {
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
                }
            }
        }
    }
    $.validator.attributeRules = function () { };
    $.validator.globalization = {};
    var initialized = false;
    var defaults = {
        dateFormat: { date: "short" },
        timeFormat: { skeleton: "Hms" },
        timeFormat1: { skeleton: "Hm" },
        datetimeFormat: { skeleton: "yMdHms" },
        datetimeFormat: { skeleton: "yMdHm" },
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
    function initialize() {
        if (initialized) return;
        var support = enhancer.Browser.Html5InputSupport;
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

        
        initialized = true;
    }
    var $jQval = $.validator;
    $.validator.methods.minE = function (value, element, param) {
        initialize();
        var type = getType($(element));
        if (!value) return true;
        parser = parsers[type];
        neutralParser = neutralParsers[type];
        return parser(value) >= neutralParser(param);
    }
    $.validator.methods.maxE = function (value, element, param) {
        initialize();
        var type = getType($(element));
        if (!value) return true;
        parser = parsers[type];
        neutralParser = neutralParsers[type];
        return parser(value) <= neutralParser(param);
    }
    $.validator.methods.rangeE = function (value, element, param) {
        initialize();
        var type = getType($(element));
        if (!value) return true;
        parser = parsers[type];
        neutralParser = neutralParsers[type];
        return parser(value) <= neutralParser(param[1]) && parser(value) >= neutralParser(param[0]);
    }
    $.validator.methods.correcttype = function (value, element, param) {
        initialize();
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

})(jQuery);