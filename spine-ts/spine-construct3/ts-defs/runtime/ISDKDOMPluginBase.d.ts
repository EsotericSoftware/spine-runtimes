
/** SDK base class for a plugin that creates a HTML element.
 * @see {@link hhttps://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/addon-sdk-interfaces/isdkdompluginbase | ISDKDOMPluginBase documentation } */

declare class ISDKDOMPluginBase_
{
    constructor(opts: { domComponentId: string });

    _addElementMessageHandler<InstType>(handler: string, func: (inst: InstType, e: JSONValue) => void): void;
    _addElementMessageHandlers<InstType>(arr: Array<[string, (inst: InstType, e: JSONValue) => void]>): void;
}

// TypeScript magic to make classes available on globalThis
declare var ISDKDOMPluginBase: typeof ISDKDOMPluginBase_;
