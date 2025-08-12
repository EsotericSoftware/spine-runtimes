
declare namespace SDK {
	class ITypeBase {

		constructor(sdkPlugin: SDK.IPluginBase, iObjectType: SDK.IObjectType);

		_sdkPlugin: SDK.IPluginBase;
		_objectType: SDK.IObjectType;

		GetObjectType(): SDK.IObjectType;
	}
}