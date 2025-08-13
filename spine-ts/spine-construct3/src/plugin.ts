import type { SDKEditorInstanceClass } from "./instance.ts";

const SDK = globalThis.SDK;

////////////////////////////////////////////
// The plugin ID is how Construct identifies different kinds of plugins.
// *** NEVER CHANGE THE PLUGIN ID! ***
// If you change the plugin ID after releasing the plugin, Construct will think it is an entirely different
// plugin and assume it is incompatible with the old one, and YOU WILL BREAK ALL EXISTING PROJECTS USING THE PLUGIN.
// Only the plugin name is displayed in the editor, so to rename your plugin change the name but NOT the ID.
// If you want to completely replace a plugin, make it deprecated (it will be hidden but old projects keep working),
// and create an entirely new plugin with a different plugin ID.
const PLUGIN_ID = "EsotericSoftware_SpineConstruct3";
////////////////////////////////////////////

const PLUGIN_CATEGORY = "general";

let app = null;

const PLUGIN_CLASS = class MyDrawingPlugin extends SDK.IPluginBase {
	static PROP_ATLAS = "spine-atlas-file";
	static PROP_SKELETON = "spine-skeleton-file";
	static PROP_SKIN = "spine-skin";
	static PROP_ANIMATION = "spine-animation";
	static PROP_ERRORS = "spine-errors";
	static PROP_RATIO_WIDTH = "spine-restore-ratio-width";
	static PROP_RATIO_HEIGHT = "spine-restore-ratio-height";
	static PROP_SKELETON_BLOB = "spine-skeleton-file-blob";
	static PROP_BOUNDS_PROVIDER_GROUP = "spine-bounds-provider-group";
	static PROP_BOUNDS_PROVIDER = "spine-bounds-provider";
	static PROP_BOUNDS_PROVIDER_MOVE = "spine-bounds-provider-move";
	static PROP_BOUNDS_OFFSET_X = "spine-bounds-offset-x";
	static PROP_BOUNDS_OFFSET_Y = "spine-bounds-offset-y";
	static PROP_BOUNDS_OFFSET_ANGLE = "spine-bounds-offset-angle";
	static PROP_SKELETON_SCALE_X = "spine-scale-x";
	static PROP_SKELETON_SCALE_Y = "spine-scale-y";

	static TYPE_BOUNDS_SETUP = "setup";
	static TYPE_BOUNDS_ANIMATION_SKIN = "animation-skin";
	static TYPE_BOUNDS_AABB = "AABB";

	constructor () {
		super(PLUGIN_ID);


		SDK.Lang.PushContext("plugins." + PLUGIN_ID.toLowerCase());

		// @ts-ignore
		this._info.SetName(globalThis.lang(".name"));
		// @ts-ignore
		this._info.SetDescription(globalThis.lang(".description"));
		this._info.SetCategory(PLUGIN_CATEGORY);
		this._info.SetAuthor("Esoteric Software");
		// @ts-ignore
		this._info.SetHelpUrl(globalThis.lang(".help-url"));
		this._info.SetPluginType("world");			// mark as world plugin, which can draw
		this._info.SetIsResizable(true);			// allow to be resized
		this._info.SetIsRotatable(true);			// allow to be rotated
		this._info.SetHasImage(false);
		this._info.SetSupportsEffects(true);		// allow effects
		this._info.SetMustPreDraw(true);
		this._info.SetRuntimeModuleMainScript("c3runtime/main.js");
		this._info.AddC3RuntimeScript("c3runtime/spine-construct3-lib.js");
		this._info.AddFileDependency({
			filename: "c3runtime/spine-construct3-lib.js",
			type: "external-runtime-script"
		})

		SDK.Lang.PushContext(".properties");

		this._info.SetProperties([
			new SDK.PluginProperty("text", MyDrawingPlugin.PROP_ATLAS, ""),
			new SDK.PluginProperty("text", MyDrawingPlugin.PROP_SKELETON, ""),
			new SDK.PluginProperty("text", MyDrawingPlugin.PROP_SKIN, ""),
			new SDK.PluginProperty("text", MyDrawingPlugin.PROP_ANIMATION, ""),
			new SDK.PluginProperty("projectfile", MyDrawingPlugin.PROP_SKELETON_BLOB, ""),
			new SDK.PluginProperty("info", MyDrawingPlugin.PROP_ERRORS, {
				infoCallback (inst) {
					const atlas = inst.GetInstance().GetPropertyValue(MyDrawingPlugin.PROP_ATLAS);
					const skeleton = inst.GetInstance().GetPropertyValue(MyDrawingPlugin.PROP_SKELETON);

					let error = "";
					if (atlas && skeleton) {
						error = "You can't set both .skel and .json skeleton file.";
					}

					if (!atlas && !skeleton) {
						error = "Missing skeleton file.";
					}

					return error;
				},
			}),


			new SDK.PluginProperty("group", MyDrawingPlugin.PROP_BOUNDS_PROVIDER_GROUP),
			new SDK.PluginProperty("combo", MyDrawingPlugin.PROP_BOUNDS_PROVIDER, {
				initialValue: "setup",
				items: [
					MyDrawingPlugin.TYPE_BOUNDS_SETUP,
					MyDrawingPlugin.TYPE_BOUNDS_ANIMATION_SKIN,
					MyDrawingPlugin.TYPE_BOUNDS_AABB
				],
			}),
			new SDK.PluginProperty("check", MyDrawingPlugin.PROP_BOUNDS_PROVIDER_MOVE, false),
			new SDK.PluginProperty("float", MyDrawingPlugin.PROP_BOUNDS_OFFSET_X, 0),
			new SDK.PluginProperty("float", MyDrawingPlugin.PROP_BOUNDS_OFFSET_Y, 0),
			new SDK.PluginProperty("float", MyDrawingPlugin.PROP_BOUNDS_OFFSET_ANGLE, 0),
			new SDK.PluginProperty("float", MyDrawingPlugin.PROP_SKELETON_SCALE_X, 1),
			new SDK.PluginProperty("float", MyDrawingPlugin.PROP_SKELETON_SCALE_Y, 1),


		]);

		SDK.Lang.PopContext();		// .properties

		SDK.Lang.PopContext();
	}
};

SDK.Plugins.EsotericSoftware_SpineConstruct3 = PLUGIN_CLASS;

PLUGIN_CLASS.Register(PLUGIN_ID, PLUGIN_CLASS);
