
/** SDK base class for an object instance.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/addon-sdk-interfaces/isdkinstancebase | ISDKInstanceBase documentation } */

// HACK: TypeScript doesn't seem to have good support for mixins yet.
// As a workaround, the members of ISDKInstanceBase_ are defined twice: once deriving
// from IInstance, and again deriving from IWorldInstance.
declare class ISDKInstanceBase_ extends IInstance
{
     constructor(opts?: { domComponentId?: string, wrapperComponentId?: string });
 
     readonly objectType: ISDKObjectTypeBase_<this>;
 
      _release(): void;
      _getInitProperties(): SDKPropertyType[];
      _trigger(method: Function): void;
      _triggerAsync(method: Function): Promise<void>;
  
      _setTicking(isTicking: boolean): void;
      _setTicking2(isTicking: boolean): void;
      _isTicking(): boolean;
      _isTicking2(): boolean;
  
      _tick(): void;
      _tick2(): void;
  
      _getDebuggerProperties(): any[];
  
      _saveToJson(): JSONValue;
      _loadFromJson(o: JSONValue): void;
  
      // DOM methods
      _addDOMMessageHandler(handler: string, callback: (e: JSONValue) => void): void;
      _addDOMMessageHandlers(arr: Array<[string, (e: JSONValue) => void]>): void;
  
      _postToDOM(handler: string, data?: JSONValue): void;
      _postToDOMAsync(handler: string, data?: JSONValue): Promise<JSONValue>;
      _postToDOMMaybeSync(handler: string, data?: JSONValue): void;
  
      // Wrapper extension methods
      _isWrapperExtensionAvailable(): boolean;
      _addWrapperExtensionMessageHandler(messageId: string, callback: (e: JSONValue) => void): void;
      _addWrapperMessageHandlers(arr: Array<[string, (e: JSONValue) => void]>): void;
  
      _sendWrapperExtensionMessage(messageId: string, params?: WrapperExtensionParameterType[]): void;
      _sendWrapperExtensionMessageAsync(messageId: string, params?: WrapperExtensionParameterType[]): Promise<JSONValue>;
}

declare class IWorldInstanceSDKBase_ extends IWorldInstance
{
     readonly objectType: ISDKObjectTypeBase_<this>;
     
      _release(): void;
      _getInitProperties(): SDKPropertyType[];
      _trigger(method: Function): void;
  
      _setTicking(isTicking: boolean): void;
      _setTicking2(isTicking: boolean): void;
      _isTicking(): boolean;
      _isTicking2(): boolean;
  
      _tick(): void;
      _tick2(): void;
  
      _getDebuggerProperties(): any[];
  
      _saveToJson(): JSONValue;
      _loadFromJson(o: JSONValue): void;
  
      // DOM methods
      _addDOMMessageHandler(handler: string, callback: (e: JSONValue) => void): void;
      _addDOMMessageHandlers(arr: Array<[string, (e: JSONValue) => void]>): void;
  
      _postToDOM(handler: string, data: JSONValue): void;
      _postToDOMAsync(handler: string, data: JSONValue): Promise<JSONValue>;
      _postToDOMMaybeSync(handler: string, data: JSONValue): void;
  
      // Wrapper extension methods
      _isWrapperExtensionAvailable(): boolean;
      _addWrapperExtensionMessageHandler(messageId: string, callback: (e: JSONValue) => void): void;
      _addWrapperMessageHandlers(arr: Array<[string, (e: JSONValue) => void]>): void;
  
      _sendWrapperExtensionMessage(messageId: string, params?: WrapperExtensionParameterType[]): void;
      _sendWrapperExtensionMessageAsync(messageId: string, params?: WrapperExtensionParameterType[]): Promise<JSONValue>;
}

declare var ISDKInstanceBase: typeof ISDKInstanceBase_;
declare var IWorldInstanceSDKBase: typeof IWorldInstanceSDKBase_;
 