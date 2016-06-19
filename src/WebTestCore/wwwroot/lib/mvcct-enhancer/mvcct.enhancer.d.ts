declare namespace mvcct {
    export namespace enhancer {
        export interface InputSupportOption {
            force: boolean;
            type: number;
        }
        export interface InputSupportOptions {
            number?: InputSupportOption;
            range?: InputSupportOption;
            date?: InputSupportOption;
            month?: InputSupportOption;
            week?: InputSupportOption;
            time?: InputSupportOption;
            datetime?: InputSupportOption;
            email?: InputSupportOption;
            search?: InputSupportOption;
            tel?: InputSupportOption;
            url?: InputSupportOption;
            color?: InputSupportOption;
        }
        
        export interface EnhancementProcessors{
            number?: (node: HTMLElement, originalNode: HTMLElement) => void;
            range?: (node: HTMLElement, originalNode: HTMLElement) => void;
            date?: (node: HTMLElement, originalNode: HTMLElement) => void;
            month?: (node: HTMLElement, originalNode: HTMLElement) => void;
            week?: (node: HTMLElement, originalNode: HTMLElement) => void;
            time?: (node: HTMLElement, originalNode: HTMLElement) => void;
            datetime?: (node: HTMLElement, originalNode: HTMLElement) => void;
            email?: (node: HTMLElement, originalNode: HTMLElement) => void;
            search?: (node: HTMLElement, originalNode: HTMLElement) => void;
            tel?: (node: HTMLElement, originalNode: HTMLElement) => void;
            url?: (node: HTMLElement, originalNode: HTMLElement) => void;
            color?: (node: HTMLElement, originalNode: HTMLElement) => void;
        }
        
        export interface Html5FallbackHandler{
            replace?: (type: string, support: Html5InputSupport) => string;
            fullReplace?: (HTMLElement) => void;  
            translateVal?: (value: string, type: string, node: HTMLHtmlElement) => string;
            enhance?: EnhancementProcessors;
        }
        export interface BrowserSupportOptions {
            fallbackHtml5?: boolean;
            cookie?: string;
            forms?: string;
            fallbacks?: InputSupportOptions;
            handlers?: Html5FallbackHandler;
        }
        export interface Html5InputOriginalSupport {
            number: boolean;
            range: boolean;
            date: boolean;
            month: boolean;
            week: boolean;
            time: boolean;
            datetime: boolean;
            email: boolean;
            search: boolean;
            tel: boolean;
            url: boolean;
            color: boolean;
        }
        export interface Html5InputSupport {
            number: number;
            range: number;
            date: number;
            month: number;
            week: number;
            time: number;
            datetime: number;
            email: number;
            search: number;
            tel: number;
            url: number;
            color: number;
        }
        export interface Html5InputFallbackWidgetsOptions{
            number?: any;
            range?: any;
            date?: any;
            month?: any;
            week?: any;
            time?: any;
            datetime?: any;
            email?: any;
            search?: any;
            tel?: any;
            url?: any;
            color?: any;
        }
        export interface Formats{
            dateFormat?: string;
            timeFormat?: string;
            timeFormat1?: string;
            datetimeFormat?: string;
            datetimeFormat1?: string;
            monthFormat?: string;
            weekFormat?: string;
        }
        export interface Options {
            browserSupport?: BrowserSupportOptions;
            html5FallbackWidgets?: Html5InputFallbackWidgetsOptions;
            editFormats?: Formats;
            runReady?: boolean;
            [propName: string]: any;
        }
        export interface Html5Infos {
            Html5InputOriginalSupport: Html5InputOriginalSupport;
            Html5InputSupport: Html5InputSupport;
        }

        export function init(options: Options): void;
        export function waitAsync(options: Options): void;
        export function asyncReady(): void;
        export function register(
            transform: (node: HTMLElement, isInitialize?: boolean) => void,
            initialize: boolean,
            processOptions: (options: Options) => void,
            name: string,
            preProcessOptions: (options: Options) => void): void;
        export function transform(node: HTMLElement): void;
        export function dependency(name: string,
            sourceNode: HTMLElement,
            targetNode: HTMLElement,
            eventNames: string[],
            action: (targetNode: HTMLElement, sourceNode: HTMLElement) => void): any;
        export function removeDependency(handle: any): void;
        export function getSupport(): Html5Infos;
        export function addBasicInput(Globalize: any);
        export function format(type: string, value: any, invariant?: boolean): string;
        export function parse(type: string, value: string, invariant?: boolean): any;
        export function Globalize(): any;
    }
}

declare module "mvcct.enhancer" {
    export = mvcct.enhancer
}