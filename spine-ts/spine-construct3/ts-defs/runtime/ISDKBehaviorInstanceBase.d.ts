
/** SDK base class for a behavior instance.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/addon-sdk-interfaces/isdkbehaviorinstancebase | ISDKBehaviorInstanceBase documentation } */
declare class ISDKBehaviorInstanceBase_<InstType> extends IBehaviorInstance<InstType>
{
    _postCreate(): void;
    _release(): void;
    _getInitProperties(): SDKPropertyType[];
    _trigger(method: Function): void;
    _triggerAsync(method: Function): Promise<void>;

    _setTicking(isTicking: boolean): void;
    _setTicking2(isTicking: boolean): void;
    _setPostTicking(isTicking: boolean): void;
    _isTicking(): boolean;
    _isTicking2(): boolean;
    _isPostTicking(): boolean;

    _tick(): void;
    _tick2(): void;
    _postTick(): void;

    _getDebuggerProperties(): any[];

    _saveToJson(): JSONValue;
    _loadFromJson(o: JSONValue): void;
}

declare var ISDKBehaviorInstanceBase: typeof ISDKBehaviorInstanceBase_;
