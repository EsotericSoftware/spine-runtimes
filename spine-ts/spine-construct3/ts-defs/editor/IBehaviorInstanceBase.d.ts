declare namespace SDK {
	class IBehaviorInstanceBase {

		constructor(sdkBehaviorType: SDK.IBehaviorTypeBase, iBehaviorInstance: SDK.IBehaviorInstance);

		_sdkBehaviorType: SDK.IBehaviorTypeBase;
		_behaviorInstance: SDK.IBehaviorInstance;

		GetBehaviorInstance(): SDK.IBehaviorInstance;
		GetSdkBehaviorType(): SDK.IBehaviorTypeBase;

		Release(): void;
		OnCreate(): void;

		OnPropertyChanged(id: string, value: EditorPropertyValueType): void;
	}
}