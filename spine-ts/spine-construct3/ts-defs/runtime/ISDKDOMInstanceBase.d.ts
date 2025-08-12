
/** SDK base class for an instance that creates a HTML element.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/addon-sdk-interfaces/isdkdominstancebase | ISDKDOMInstanceBase documentation } */
declare class ISDKDOMInstanceBase_ extends ISDKWorldInstanceBase
{
    constructor(opts: { domComponentId: string });
    
    _postToDOMElement(handler: string, data: JSONValue): void;
    _postToDOMElementAsync(handler: string, data: JSONValue): Promise<JSONValue>;
    _postToDOMElementMaybeSync(handler: string, data: JSONValue): void;

    _createElement(data?: JSONValue): void;
    _getElementState(): JSONValue;
    _updateElementState(): void;

    focusElement(): void;
    blurElement(): void;
    isElementFocused(): boolean;

    setElementVisible(isVisible: boolean): void;

    setElementCSSStyle(prop: string, val: string): void;

    setElementAttribute(attribName: string, value: string): void;
    removeElementAttribute(attribName: string): void;

    _getElementInDOMMode(): HTMLElement;
}

declare var ISDKDOMInstanceBase: typeof ISDKDOMInstanceBase_;
