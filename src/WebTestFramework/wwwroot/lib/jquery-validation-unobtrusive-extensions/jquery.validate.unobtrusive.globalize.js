
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
    var numberRegEx = null, rangeRegEx=null;
    
    $.validator.attributeRules = function () { };
    $.validator.globalization = {};
    var initialized = false;
    var neutralTimeParser, neutralDateTimeParser, neutralMonthParser, neutralWeekParser, weekParser;
    var neutralNumberParser = parseFloat;
    

    var dateTimeAdder, weekAdder, numberAdder;

    var parsers = [null, null, null, null, null, null, null, null, null, null, null, null, null, null];
    var neutralParsers = [null, null, null, null, null, null, null, null, null, null, null, null, null, null];
    var adders = [null, null, null, null, null, null, null, null, null, null, null, null, null, null];
    var formatters = [null, null, null, null, null, null, null, null, null, null, null, null, null, null];
    function getType(jElement) {
        return parseInt(jElement.attr("data-val-correcttype-type"))+(jElement.attr("data-is-range") 
            || jElement.attr("type") == "range" ? 10 : 0);
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
    function initialize(toptions) {
        if (initialized) return;
        var Globalize = enhancer.Globalize();
        if (!support) support = mvcct.enhancer.getSupport().Html5InputSupport;
        var locale = Globalize.locale().attributes.language;
        var nInfos = Globalize.cldr.get('main/' + locale).numbers["symbols-numberSystem-latn"];

        if (support.number > 2) {
            numberRegEx = new RegExp("^[\+\-\.0-9]*$");
        }
        else {
            numberRegEx = new RegExp("^[\\" + nInfos.plusSign + "\\" + nInfos.minusSign + "\\" + nInfos.decimal + "0-9]*$");
        };
        if (support.range > 2) {
            rangeRegEx = new RegExp("^[\+\-\.0-9]*$");
        }
        else {
            rangeRegEx = new RegExp("^[\\" + nInfos.plusSign + "\\" + nInfos.minusSign + "\\" + nInfos.decimal + "0-9]*$");
        };
        neutralTimeParser = Globalize.dateParser({ raw: "HH:mm:ss" });
        neutralDateTimeParser = Globalize.dateParser({ raw: "yyyy-MM-ddTHH:mm:ss" })

        neutralMonthParser = Globalize.dateParser({ raw: "yyyy-MM" });
        neutralWeekParser = support.week > 2 ? 
            function (s) {
                return s;
            }
            :
            getDateOfISOWeek;

        weekParser = support.week > 2 ? function (x) { return x } : function(x){return enhancer.parse("week", x)};
        weekFormatter = support.week > 2 ? function (x) { return x } : function(x){return enhancer.format("week", x)};

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
        formatters[0] = parsers[0];

        parsers[1] = parsers[2] = parsers[3] = function(x){return enhancer.parse("number", x);};
        neutralParsers[1] = neutralParsers[2] = neutralParsers[3] = neutralNumberParser;
        adders[1] = adders[2] = adders[3] = numberAdder;
        formatters[1] = formatters[2] = formatters[3] = function(x){return enhancer.format("number", x);};

        parsers[11] = parsers[12] = parsers[13] = function(x){return enhancer.parse("range", x);};
        neutralParsers[11] = neutralParsers[12] = neutralParsers[13] = neutralNumberParser;
        adders[11] = adders[12] = adders[13] = numberAdder;
        formatters[11] = formatters[12] = formatters[13] = function(x){return enhancer.format("range", x);};

        parsers[4] = function(x){return enhancer.parse("time", x);};
        neutralParsers[4] = neutralTimeParser;
        formatters[4] = function(x){return enhancer.format("time", x);};

        parsers[5] = function(x){return enhancer.parse("date", x);};
        neutralParsers[5] = neutralDateTimeParser;
        formatters[5] = function(x){return enhancer.format("date", x);};

        parsers[6] = function(x){return enhancer.parse("datetime", x);};
        neutralParsers[6] = neutralDateTimeParser;
        adders[4] = adders[5] = adders[6] = dateTimeAdder;
        formatters[6] = function(x){return enhancer.format("datetime", x);};
        

        parsers[7] = weekParser;
        neutralParsers[7] = neutralWeekParser;
        adders[7] = weekAdder;
        formatters[7] = weekFormatter;

        parsers[8] = function(x){return enhancer.parse("month", x);};
        neutralParsers[8] = neutralMonthParser;
        adders[8] = dateTimeAdder;
        formatters[8] = function(x){return enhancer.format("month", x);};

        
        initialized = true;
    }
    var $jQval = $.validator;
    $.validator.methods.minE = function (value, element, param) {
        if (!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        var parser = parsers[type];
        var go = param[1];
        if (go && $(element).is(":focus")) return true;
        var neutralParser = neutralParsers[type];
        if (go) {
            var val = parser(value);
            var otherVal = neutralParser(param[0]);
            if (val < otherVal) $(element).val(formatters[type](otherVal));
            return true;
        }
        else return parser(value) >= neutralParser(param[0]);
    }
    $.validator.methods.maxE = function (value, element, param) {
        if(!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        var parser = parsers[type];
        var go = param[1];
        if (go && $(element).is(":focus")) return true;
        var neutralParser = neutralParsers[type];
        if (go) {
            var val = parser(value);
            var otherVal = neutralParser(param[0]);
            if (val > otherVal) $(element).val(formatters[type](otherVal));
            return true;
        }
        else return parser(value) <= neutralParser(param[0]);
    }
    $.validator.methods.rangeE = function (value, element, param) {
        if(!enhancer) initialize();
        var type = getType($(element));
        if (!value) return true;
        var parser = parsers[type];
        var go = param[2];
        if (go && $(element).is(":focus")) return true;
        var neutralParser = neutralParsers[type];
        if (go) {
            var val = parser(value);
            var otherVal = neutralParser(param[0]);
            if (val < otherVal) $(element).val(formatters[type](otherVal));
            else {
                otherVal = neutralParser(param[1]);
                if (val > otherVal) $(element).val(formatters[type](otherVal));
            }
            return true;
        }
        else return parser(value) <= neutralParser(param[1]) && parser(value) >= neutralParser(param[0]);
    }
    $.validator.methods.correcttype = function (value, element, param) {
        if(!enhancer) initialize();
        if (!value) return true;
        var type = getType($(element));
        var parser = parsers[type];
        var val = parser(value);
        
        return (val || val === 0)
            && (!(typeof val === "number") || (type < 10 ? numberRegEx.test(value) : rangeRegEx.test(value)))
            && ((type != 1 && type != 2 && type != 11 && type != 12) || (val % 1) === 0 )
            && ((type != 1 && type != 11)  || val >= 0);
    }
    $.validator.methods.drange = function (value, element, param) {
        if (!enhancer) initialize();
        if (!value) return true;
        var type = getType($(element));
        var mins = param[0];
        var minDelays = param[1];
        var maxs = param[2];
        var maxDelays = param[3];
        var go = param[4];
        if (go && $(element).is(":focus")) return true;
        var parser = parsers[type];
        var adder = adders[type];
        var tvalue = parser(value);
        var min, max, violated, vMAx, vMin;
        for (var i = 0; i < mins.length; i++) {
            var otherVal = mins[i].value;
            if (!otherVal) continue;
            otherVal = parser(otherVal);
            if (!otherVal) continue;
            otherVal = adder(otherVal, minDelays[i]);
            if (go && !(otherVal <= min)) min = otherVal;
            if (tvalue < otherVal) {
                if (go) {
                    violated = true;
                    vMin = true;
                }
                else return false;
            }
            
        }
        for (var i = 0; i < maxs.length; i++) {
            var otherVal = maxs[i].value;
            if (!otherVal) continue;
            otherVal = parser(otherVal);
            if (!otherVal) continue;
            otherVal=adder(otherVal, maxDelays[i]);
            if (go && !(otherVal >= max)) max = otherVal;
            if (tvalue > otherVal) {
                if (go) {
                    violated = true;
                    vMAx = true;
                }
                else return false;
            }
        }
        if (go && violated) {
            if (max < min ) return false;
            if (vMAx) {
                $(element).val(formatters[type](max))
            }
            else if (vMin) $(element).val(formatters[type](min))
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
        return this.add(adapterName, [minAttribute || "min", maxAttribute || "max", "go"], function (options) {
            var min = options.params.min,
                max = options.params.max,
                go = options.params.go == "true";
            if ((min || min === 0) && (max || max === 0)) {
                setValidationValues(options, minMaxRuleName, [min, max, go]);
            }
            else if (min || min === 0) {
                setValidationValues(options, minRuleName, [min, go]);
            }
            else if (max || max === 0) {
                setValidationValues(options, maxRuleName, [max, go]);
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
        function addHandler(el, limits, preventKeyUp) {
            if (!limits) return;
            for (var i = 0; i < limits.length; i++) {
                enhancer.dependency("main", limits[i], el, preventKeyUp? ["blur"] : ["blur", "keyup"], function (t, s) {
                    $(t).closest('form').validate().element(t);
                });
            }
        }
        adapters.addDRange = function (adapterName, ruleName, minsName, minDelaysName, maxsName, maxDelaysName) {
            return this.add(adapterName, [minsName, minDelaysName, maxsName, maxDelaysName, "go"], function (options) {
                var el = options.element;
                var mins = otherElements(el, options.params[minsName]);
                var maxs = otherElements(el, options.params[maxsName]); 
                addHandler(el, mins, options.params.go);
                addHandler(el, maxs, options.params.go);
                var mdelays = options.params[minDelaysName].split(" ");
                var maxdelays = options.params[maxDelaysName].split(" ");
                var go = options.params.go == "true";
                setValidationValues(options, ruleName, [mins, mdelays, maxs, maxdelays, go]);
            });
        };
        adapters.addDRange("drange", "drange", "dmins", "dminds", "dmaxs", "dmaxds");
        enhancer["register"](null, false, initialize, "html5 globalized fallback");
    }
})(jQuery);