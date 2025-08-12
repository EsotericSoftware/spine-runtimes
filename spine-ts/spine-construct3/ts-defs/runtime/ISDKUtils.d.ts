
/** Utility class for scripting APIs intended for the Addon SDK. */
declare class ISDKUtils
{
	addLoadPromise(promise: Promise<void>): void;

	sendWrapperExtensionMessage(wrapperComponentId: string, messageId: string, params?: WrapperExtensionParameterType[]): void;
    sendWrapperExtensionMessageAsync(wrapperComponentId: string, messageId: string, params?: WrapperExtensionParameterType[]): Promise<JSONValue>;

	createLoopingConditionContext(loopName?: string): ILoopingConditionContext;

	isAutoSuspendEnabled: boolean;
	setSuspended(isSuspended: boolean): void;

	getObjectClassBySid(sid: number): IObjectClass<IInstance>;
}
