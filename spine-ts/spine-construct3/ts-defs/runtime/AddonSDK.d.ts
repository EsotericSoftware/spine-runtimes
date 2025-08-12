
// Additional type definitions for use with the addon SDK only.
declare namespace C3 {
    function compare(a: number, cmp: number, b: number): boolean;

    let Plugins: any;
    let Behaviors: any;

    class Event {
        constructor(name: string, isCancelable?: boolean);
    }
}

type IAnyProjectLayout = ILayout;
type IAnyProjectLayer = ILayer;

type IConstructProjectObjects = {};
type IConstructProjectGlobalVariables = {};

// For DOM side
declare class IDOMHandler {
    constructor(iRuntime: IRuntimeInterface, domComponentId: string);

    AddRuntimeMessageHandler(handler: string, callback: (data: JSONValue) => JSONValue | Promise<JSONValue> | void): void;
    AddRuntimeMessageHandlers(arr: Array<[string, (data: JSONValue) => JSONValue | Promise<JSONValue> | void]>): void;

    PostToRuntime(handler: string, data: JSONValue): void;
    PostToRuntimeAsync(handler: string, data: JSONValue): Promise<JSONValue>;
}

declare class IDOMElementHandler extends IDOMHandler {
    constructor(iRuntime: IRuntimeInterface, domComponentId: string);

    AddDOMElementMessageHandler(handler: string, func: (elem: HTMLElement, e: JSONValue) => JSONValue | Promise<JSONValue> | void): void;
    PostToRuntimeElement(handler: string, elementId: number, data?: JSONValue): void;
    CreateElement(elementId: number, e: JSONValue): void;
    DestroyElement(elem: HTMLElement): void;
    UpdateState(elem: HTMLElement, e: JSONValue): void;
}

declare class IRuntimeInterface {
    static AddDOMHandlerClass<T extends IDOMHandler>(clazz: new (iRuntime: IRuntimeInterface) => T): void;

    GetExportType(): PlatformInfoExportType;    // note re-uses type from IPlatformInfoObjectType
}

declare var RuntimeInterface: typeof IRuntimeInterface;
declare var DOMHandler: typeof IDOMHandler;
declare var DOMElementHandler: typeof IDOMElementHandler;
