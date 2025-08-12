
const SDK = globalThis.SDK;

const PLUGIN_CLASS = SDK.Plugins.EsotericSoftware_SpineConstruct3;

PLUGIN_CLASS.Type = class MyDrawingPluginType extends SDK.ITypeBase {
	constructor (sdkPlugin: SDK.IPluginBase, iObjectType: SDK.IObjectType) {
		super(sdkPlugin, iObjectType);
	}

};

export { }