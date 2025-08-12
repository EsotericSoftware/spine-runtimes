
type PluginPropertyInitialValueType = number | Vec3Arr | string | boolean;

type PluginPropertyType = "integer" | "float" | "percent" | "text" | "longtext" | "check" | "font" | "combo" | "color" | "object" | "group" | "link" | "info" | "projectfile";

type PluginPropertyCallbackType = "for-each-instance" | "once-for-type";

interface PluginPropertyOptions {
	initialValue?: PluginPropertyInitialValueType,
	minValue?: number,
	maxValue?: number,
	items?: string[],
	dragSpeedMultiplier?: number,
	allowedPluginIds?: string[],
	linkCallback?: (p: SDK.IWorldInstanceBase | SDK.ITypeBase) => void,
	infoCallback?: (inst: SDK.IInstanceBase) => string,
	callbackType?: PluginPropertyCallbackType,
	interpolatable?: boolean,
	filter?: string
}

declare namespace SDK {
	class PluginProperty {
		constructor(type: PluginPropertyType, id: string, initialValue_or_options?: PluginPropertyOptions | PluginPropertyInitialValueType);
	}
}