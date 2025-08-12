

declare namespace SDK {
	class IPluginBase {

		constructor(id: string);

		_info: SDK.IPluginInfo;

		Release(): void;

		static Register(id: string, class_: new () => SDK.IPluginBase): void;
	}
}
