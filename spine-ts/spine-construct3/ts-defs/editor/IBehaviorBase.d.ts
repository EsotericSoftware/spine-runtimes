declare namespace SDK {
	class IBehaviorBase {

		constructor(id: string);
		
		_info: SDK.IBehaviorInfo;

		Release(): void;

		static Register(id: string, class_: new () => SDK.IBehaviorBase): void;
	}
}