/// <reference path="jquery.validation.d.ts" />
/// <reference path="jquery-validation-unobtrusive.d.ts" />

declare namespace MicrosoftJQueryUnobtrusiveValidation {

    interface Validator {
        parseDynamic(selector: JQuerySelector): void;
        parseElementDynamic(element: JQuerySelector): void;
    }
}