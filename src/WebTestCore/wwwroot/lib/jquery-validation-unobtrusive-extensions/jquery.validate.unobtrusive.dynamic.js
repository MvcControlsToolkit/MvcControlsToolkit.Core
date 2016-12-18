(function ($) {
    var formsToFix = [];
    function registerFormFix(el, form) {
        var iform = form[0];
        if (iform._mvcct_form_expando_) return;
        iform._mvcct_form_expando_ = true;
        formsToFix.push(form);
        if (formsToFix.length == 1)
            setTimeout(function () {
                while (formsToFix.length) {
                    var cform = formsToFix.pop();
                    cform[0]._mvcct_form_expando_ = false;
                    fixForm(cform);
                    cform.removeData('unobtrusiveValidation');
                }
            });
    }
    function fixForm(form) {
        var unobtrusiveValidation = form.data('unobtrusiveValidation');
        if (!unobtrusiveValidation) return;
        var validator = form.validate();
        var elements = form[0].elements;
        $.each(unobtrusiveValidation.options.rules, function (elname, elrules) {
            if (validator.settings.rules[elname] == undefined) {
                var args = {};
                $.extend(args, elrules);
                args.messages = unobtrusiveValidation.options.messages[elname];
                $(elements.namedItem(elname)).rules("add", args);
            } else {
                $.each(elrules, function (rulename, data) {
                    if (validator.settings.rules[elname][rulename] == undefined) {
                        var args = {};
                        args[rulename] = data;
                        args.messages = unobtrusiveValidation.options.messages[elname][rulename];
                        $(elements.namedItem(elname)).rules("add", args);
                    }
                });
            }
        });
    }

    $.validator.unobtrusive.parseDynamic = function (selector) {
        var form = $(selector).first().closest('form');
        if (!form.length && !$(selector).find('form').length) return;
        $.validator.unobtrusive.parse(selector);
        if (form.length) 
            registerFormFix(selector, form);
    }
    $.validator.unobtrusive.parseElementDynamic = function (selector) {
        var form = $(selector).first().closest('form');
        if (form.length == 0) return;
        $.validator.unobtrusive.parseElement(selector, true);
        registerFormFix(selector, form);
    }
    var mvcct = window["mvcct"]||{};
    var enhancer = mvcct["enhancer"];
    if (enhancer) enhancer.register(function(node, init){
            if(init) {
                $.validator.unobtrusive.parse(document);
                $('form').removeData('unobtrusiveValidation');
            }
            else $.validator.unobtrusive.parseDynamic(node);
        }, 
        true, null, "unobtrusive validation");
})(jQuery);