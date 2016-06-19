(function($){
    function getPattern(options) {
	var dateSkeleton, result, skeleton, timeSkeleton, type, Globalize, infos, enhancer, locale;
    enhancer = mvcct.enhancer;
    Globalize = enhancer.Globalize();
    locale=Globalize.locale().attributes.language;
    infos = Globalize.cldr.get("main/"+locale);
    
    function toString( variable ) {
	    return typeof variable === "string" ? variable : ( typeof variable === "number" ? "" +
		    variable : JSON.stringify( variable ) );
    };
    function formatMessage( message, data ) {
	    message = message.replace( /{[0-9a-zA-Z-_. ]+}/g, function( name ) {
		    name = name.replace( /^{([^}]*)}$/, "$1" );
		    return toString( data[ name ] );
	    });

	    return message;
    };
	function combineDateTime( type, datePattern, timePattern ) {
		return formatMessage(
			infos.dates.calendars.gregorian.dateTimeFormats[type],
			[ timePattern, datePattern ]
		);
	}

	switch ( true ) {
		case "skeleton" in options:
			skeleton = options.skeleton;
			result =infos.dates.calendars.gregorian.dateTimeFormats.availableFormats[skeleton];
			if ( !result ) {
				timeSkeleton = skeleton.split( /[^hHKkmsSAzZOvVXx]/ ).slice( -1 )[ 0 ];
				dateSkeleton = skeleton.split( /[^GyYuUrQqMLlwWdDFgEec]/ )[ 0 ];
				if ( /(MMMM|LLLL).*[Ec]/.test( dateSkeleton ) ) {
					type = "full";
				} else if ( /MMMM/g.test( dateSkeleton ) ) {
					type = "long";
				} else if ( /MMM/g.test( dateSkeleton ) || /LLL/g.test( dateSkeleton ) ) {
					type = "medium";
				} else {
					type = "short";
				}
				result = combineDateTime( type,
					infos.dates.calendars.gregorian.dateTimeFormats.availableFormats[dateSkeleton],
                    infos.dates.calendars.gregorian.dateTimeFormats.availableFormats[timeSkeleton]
				);
			}
			break;

		case "date" in options:
		case "time" in options:
            result = infos.dates.calendars.gregorian["date" in options ? "dateFormats" : "timeFormats"][options.date || options.time];
			break;

		case "datetime" in options:
			result = combineDateTime( options.datetime,
                infos.dates.calendars.gregorian.dateFormats[options.datetime],
                infos.dates.calendars.gregorian.timeFormats[options.datetime]
			);
			break;
		case "raw" in options:
			result = options.raw;
			break;

		default:
			result = null;
	}
	return result;
  }
  var dayDict={
                  "sun": 0,
                  "mon": 1,
                  "tue": 2,
                  "wed": 3,
                  "thu": 4,
                  "fri": 5,
                  "sat": 6
                }
   var defaults = {
                    "dateFormat": { "date": "short" },
                    "timeFormat": { "skeleton": "Hms" },
                    "timeFormat1": { "skeleton": "Hms" },
                    "datetimeFormat": { "datetime": "short" },
                    "datetimeFormat1": { "datetime": "short" },
                    "monthFormat": { "date": "short" },
                    "weekFormat": { "date": "short" }
                };;
  function localizeWidgets(culture){
    var enhancer = mvcct.enhancer;
    var Globalize = enhancer.Globalize();
    culture= culture || Globalize.locale().attributes.language;
    var country = culture.length > 2 ? culture.substring(3) : culture.toUpperCase();
    var infos = Globalize.cldr.get("main/"+culture);
    var supp = Globalize.cldr.get("supplemental");
    var months=infos.dates.calendars.gregorian.months.format;
    var days = infos.dates.calendars.gregorian.days.format;
    var dayPeriods = infos.dates.calendars.gregorian.dayPeriods.format.abbreviated;
    var mformatter = Globalize(culture).messageFormatter;
    var res= {
        days: [days.wide.sun, days.wide.mon, days.wide.tue, days.wide.wed, days.wide.thu, days.wide.fri, days.wide.sat, days.wide.sun],
        daysShort: [days.abbreviated.sun, days.abbreviated.mon, days.abbreviated.tue, days.abbreviated.wed, days.abbreviated.thu, days.abbreviated.fri, days.abbreviated.sat, days.abbreviated.sun],
        daysMin: [days.narrow.sun, days.narrow.mon, days.narrow.tue, days.narrow.wed, days.narrow.thu, days.narrow.fri, days.narrow.sat, days.narrow.sun],
        months: [months.wide["1"], months.wide["2"], months.wide["3"], months.wide["4"], months.wide["5"], months.wide["6"], months.wide["7"], months.wide["8"], months.wide["9"], months.wide["10"], months.wide["11"], months.wide["12"]],
        monthsShort: [months.abbreviated["1"], months.abbreviated["2"], months.abbreviated["3"], months.abbreviated["4"], months.abbreviated["5"], months.abbreviated["6"], months.abbreviated["7"], months.abbreviated["8"], months.abbreviated["9"], months.abbreviated["10"],               months.abbreviated["11"], months.abbreviated["12"]],
        meridiem: [dayPeriods.am, dayPeriods.pm],//gregorian calendar
        weekStart: dayDict[supp.weekData.firstDay[country]], //supp weekData.json
        today: infos.dates.fields.day["relative-type-0"], //main dateFields.json
        rtl: infos.layout.orientation.characterOrder == "right-to-left", //main layout.json
        clear: mformatter ? mformatter("generic/clear") : undefined
    };
    $.fn.datetimepicker.dates = $.fn.datetimepicker.dates || {};
    $.fn.datetimepicker.dates[culture]=res;
  }
  function getOptions(options, name){
      var userOptions=options.html5FallbackWidgets ||{};
      userOptions=userOptions[name] ||{};
      var res= {};
      for(var prop in defaults){
          res[prop]=userOptions[prop];;
      }
      return res;
  } 
  function adjustOptions(pattern){
      var prev, next, nextNext, res="";
      for(x=0; x<pattern.length; x++){
          c=pattern.charAt(x);
          next = x+1<pattern.length ? pattern.charAt(x+1) : null; 
          nextNext = x+2<pattern.length ? pattern.charAt(x+2) : null;
          nextNextNext = x+3<pattern.length ? pattern.charAt(x+3) : null;
          if (c == "m") res = res + "i";
          else if (c == "H") res = res + "h";
          else if (c == "h") res = res + "H";
          else if (c == "y") {
              if (c != prev && c != next) res = res + "yy";
              else if (c != prev && c == next && c == nextNext && c != nextNextNext) res = res + "yy";
              else res = res + c;
          }
          else if (c == "M") {
              if (c != prev && c != nextNext) {
                  res = res + "m";
                  if (c == next) {
                      res = res + "m";
                      x++;
                  }
              }
              else if (c == prev && c == next) res = res + c;
          }
          else if (c == "a") res = res + "P";
          else res = res + c;
          prev=c;
      }
      return res;
  }
  function preProcessOptions(options){
      var enhancer = mvcct.enhancer;
      var Globalize = enhancer.Globalize();
      var culture= Globalize.locale().attributes.language;
      //reading edit formats
      var editFormats = options.editFormats || {};
      for(var prop in defaults){
          editFormats[prop]=editFormats[prop] === undefined ? defaults[prop] : editFormats[prop];
          editFormats[prop]=adjustOptions(getPattern(editFormats[prop]));
      }
      
      //setting provided fallback
      var browserSupport = options.browserSupport = options.browserSupport || {};
      browserSupport.fallbackHtml5=true;
      var fallback = browserSupport.fallbacks = browserSupport.fallbacks || {};
      fallback.number=fallback.number|| {};
      fallback.number.type=2;
      fallback.range=fallback.range|| {};
      fallback.range.type=3;
      fallback.color=fallback.color|| {};
      fallback.color.type=3;
      fallback.date=fallback.date|| {};
      fallback.date.type=2;
      fallback.time=fallback.time|| {};
      fallback.time.type=2;
      fallback.datetime=fallback.datetime|| {};
      fallback.datetime.type=2;
      fallback.week=fallback.week|| {};
      fallback.week.type=2;
      fallback.month=fallback.month|| {};
      fallback.month.type=2;
      
      //localize date/time widgets
      localizeWidgets();
      
      //setting enhancer function
      var handlers=browserSupport.handlers=browserSupport.handlers ||{};
      var enhance=handlers.enhance=handlers.enhance || {};
      if (enhance.date !== null)
      enhance.date = function (fNode, oNode) {
          var o = getOptions(options, "date");
          o.language = culture;
          o.startView = 2;
          if (typeof o.autoclose == "undefined") o.autoclose = true;
		  o.minView= 2;
          o.format=editFormats.dateFormat;
		  o.forceParse= 0;
          o.startDate= enhancer.parse("date", oNode.getAttribute("min"), true);
          o.endDate= enhancer.parse("date", oNode.getAttribute("max"), true);
          $(fNode).datetimepicker(o).on("changeDate", function(){
              var ev = document.createEvent("Event");
              ev.initEvent('blur', false, true);
              fNode.dispatchEvent(ev);
              $(fNode).blur();
          })
          .on("show", function(){
              $(fNode).datetimepicker("update", enhancer.parse("date", fNode.value));
          });
      };
      if (enhance.week !== null)
      enhance.week = function(fNode, oNode){
          var o = getOptions(options, "week");
          o.language = culture;
          o.startView = 2;
          if (typeof o.autoclose == "undefined") o.autoclose = true;
		  o.minView= 2;
          o.format=editFormats.weekFormat;
		  o.forceParse= 0;
          o.startDate= enhancer.parse("week", oNode.getAttribute("min"), true);
          o.endDate= enhancer.parse("week", oNode.getAttribute("max"), true);
          $(fNode).datetimepicker(o).on("changeDate", function(){
              var ev = document.createEvent("Event");
              ev.initEvent('blur', false, true);
              fNode.dispatchEvent(ev);
              $(fNode).blur();
          })
          .on("show", function(){
              $(fNode).datetimepicker("update", enhancer.parse("week", fNode.value));
          });
      };
      if (enhance.month !== null)
      enhance.month = function(fNode, oNode){
          var o = getOptions(options, "month");
          o.language = culture;
          o.startView = 3;
          if (typeof o.autoclose == "undefined") o.autoclose = true;
		  o.minView= 3;
          o.format=editFormats.monthFormat;
		  o.forceParse= 0;
          o.startDate= enhancer.parse(oNode.getAttribute("min"), true);
          o.endDate= enhancer.parse("month", oNode.getAttribute("max"), true);
          $(fNode).datetimepicker(o).on("changeDate", function(){
              var ev = document.createEvent("Event");
              ev.initEvent('blur', false, true);
              fNode.dispatchEvent(ev);
              $(fNode).blur();
          })
          .on("show", function(){
              $(fNode).datetimepicker("update", enhancer.parse("month", fNode.value));
          });
      };
      if (enhance.time !== null)
      enhance.time = function(fNode, oNode){
          var o = getOptions(options, "time");
          o.language = culture;
          o.startView = o.maxView = 1;
          if (typeof o.autoclose == "undefined") o.autoclose = true;
		  o.minView= 0;
          o.format=editFormats.timeFormat;
		  o.forceParse= 0;
          o.startDate= enhancer.parse("time", oNode.getAttribute("min"), true);
          o.endDate= enhancer.parse("time", oNode.getAttribute("max"), true);
          $(fNode).datetimepicker(o).on("changeDate", function(){
              var ev = document.createEvent("Event");
              ev.initEvent('blur', false, true);
              fNode.dispatchEvent(ev);
              $(fNode).blur();
          })
          .on("show", function(){
              $(fNode).datetimepicker("update", enhancer.parse("time", fNode.value));
          });
      };
      if (enhance.datetime !== null)
      enhance.datetime = function(fNode, oNode){
          var o = getOptions(options, "datetime");
          o.language = culture;
          o.startView = 2;
          if (typeof o.autoclose == "undefined") o.autoclose = true;
          o.format=editFormats.datetimeFormat;
		  o.forceParse= 0;
          o.startDate= enhancer.parse("datetime", oNode.getAttribute("min"), true);
          o.endDate= enhancer.parse("datetime", oNode.getAttribute("max"), true);
          $(fNode).datetimepicker(o).on("changeDate", function(){
              var ev = document.createEvent("Event");
              ev.initEvent('blur', false, true);
              fNode.dispatchEvent(ev);
              $(fNode).blur();
          })
          .on("show", function(){
              $(fNode).datetimepicker("update", enhancer.parse("datetime", fNode.value));
          });
      };
      var slider = $.fn.bootstrapSlider ? 'bootstrapSlider' : 'slider';
      var humanFNFormatter = mvcct.enhancer.Globalize().numberFormatter();
      if (enhance.range !== null)
      enhance.range = function (fNode, oNode) {
          var o = getOptions(options, "range");
          o.min= enhancer.parse("range", oNode.getAttribute("min"), true);
          o.max= enhancer.parse("range", oNode.getAttribute("max"), true);
          o.step = enhancer.parse("range", oNode.getAttribute("step"), true);
          if(!o.formatter){
            o.formatter= function(value) {
                return humanFNFormatter(value);
            }
          }
          o.value = enhancer.parse("range", fNode.value);
          $(fNode)[slider](o).on("slideStop", function () {
              var ev = document.createEvent("Event");
              ev.initEvent('_enhancer.dependency.main', false, true);
              fNode.dispatchEvent(ev);
              $(fNode).blur();
          });
          fNode.addEventListener("_enhancer.dependency.main", function () {
              setTimeout(function () {
                  $(fNode)[slider]("setValue", enhancer.parse("range", fNode.value))
              }, 0);
          });
      };
      if (enhance.color !== null)
      enhance.color = function(fNode, oNode){
          var o = getOptions(options, "range");
          if(typeof o.format == "undefined") o.format = "hex";
          var newContent;
          if(o.makeComponent){
              var  $fNode = $(fNode);
              newContent=$('<div class="input-group colorpicker-component">'+
                                '<span class="input-group-addon"><i></i></span>' +
                                '</div>').insertAfter($fNode);
              $fNode.remove();
              newContent.prepend($fNode);
          }
          else newContent=$(fNode);
          newContent.colorpicker(o).on("showPicker", function(){
              newContent.colorpicker('setValue', fNode.value)
          });
      };
  }
  var oldInput = mvcct.enhancer["addBasicInput"];
  mvcct.enhancer["addBasicInput"] = function (Globalize) {
    oldInput(Globalize);
    mvcct.enhancer["register"](null, false, null, "html5 bootstrap/widgets fallback", preProcessOptions);
  };
  
})(jQuery);
