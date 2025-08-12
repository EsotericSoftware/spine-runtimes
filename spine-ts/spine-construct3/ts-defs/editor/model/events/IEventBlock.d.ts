type EditorACParameterType = number | string | SDK.IObjectClass;

declare namespace SDK {
	class IEventBlock extends IEventParentRow {
		AddCondition(iObjectClass: SDK.IObjectClass, iBehaviorType: null, cndId: string, params?: EditorACParameterType[]): void;
		AddAction(iObjectClass: SDK.IObjectClass, iBehaviorType: null, actId: string, params?: EditorACParameterType[]): void;
	}
}