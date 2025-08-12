declare namespace SDK {
	class IBehaviorTypeBase {
		constructor(sdkBehavior: SDK.IBehaviorBase, iBehaviorType: SDK.IBehaviorType);

		_sdkBehavior: SDK.IBehaviorBase;
		_behaviorType: SDK.IBehaviorType;
	}
}