/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import type { SpineBoundsProviderType } from "@esotericsoftware/spine-construct3-lib";

import type { SDKEditorInstanceClass } from "./instance";

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

const PLUGIN_CLASS = class SpineC3Plugin extends SDK.IPluginBase {
	static PROP_ATLAS = "spine-atlas-file";
	static PROP_SKELETON = "spine-skeleton-file";
	static PROP_LOADER_SCALE = "spine-loader-scale";
	static PROP_SKIN = "spine-skin";
	static PROP_ANIMATION = "spine-animation";
	static PROP_RATIO_WIDTH = "spine-restore-ratio-width";
	static PROP_RATIO_HEIGHT = "spine-restore-ratio-height";
	static PROP_BOUNDS_PROVIDER_GROUP = "spine-bounds-provider-group";
	static PROP_BOUNDS_PROVIDER = "spine-bounds-provider";
	static PROP_BOUNDS_PROVIDER_MOVE = "spine-bounds-provider-move";
	static PROP_BOUNDS_OFFSET_X = "spine-bounds-offset-x";
	static PROP_BOUNDS_OFFSET_Y = "spine-bounds-offset-y";
	static PROP_BOUNDS_OFFSET_ANGLE = "spine-bounds-offset-angle";
	static PROP_SKELETON_OFFSET_SCALE_X = "spine-offset-scale-x";
	static PROP_SKELETON_OFFSET_SCALE_Y = "spine-offset-scale-y";
	static PROP_DEBUG_SKELETON = "spine-debug-skeleton";
	static PROP_ENABLE_COLLISION = "spine-enable-collision";

	static TYPE_BOUNDS_SETUP: SpineBoundsProviderType = "setup";
	static TYPE_BOUNDS_ANIMATION_SKIN: SpineBoundsProviderType = "animation-skin";

	constructor () {
		super(PLUGIN_ID);

		SDK.Lang.PushContext(`plugins.${PLUGIN_ID.toLowerCase()}`);

		this._info.SetName(globalThis.lang(".name"));
		this._info.SetDescription(globalThis.lang(".description"));
		this._info.SetCategory(PLUGIN_CATEGORY);
		this._info.SetAuthor("Esoteric Software");
		this._info.SetHelpUrl(globalThis.lang(".help-url"));
		this._info.SetPluginType("world"); // mark as world plugin, which can draw

		this._info.SetIsResizable(true); // allow to be resized
		this._info.SetIsRotatable(true); // allow to be rotated
		this._info.SetHasImage(false);
		this._info.SetSupportsEffects(true); // allow effects
		this._info.SetMustPreDraw(true);
		this._info.SetSupportsColor(true); // enable system colour/transparency
		this._info.SetSupportsZElevation(true); // enable z elevation

		this._info.AddCommonPositionACEs(); // Position: Set X/Y, Set position, etc.
		this._info.AddCommonSizeACEs(); // Size: Set size, width, height
		this._info.AddCommonAngleACEs(); // Angle: Set angle, rotate
		this._info.AddCommonAppearanceACEs(); // Appearance: Set opacity, visible, color
		this._info.AddCommonZOrderACEs(); // Z order: bring to front/back, move up/down
		this._info.AddCommonSceneGraphACEs(); // Enables hierarchies: parent/children relations

		this._info.SetRuntimeModuleMainScript("c3runtime/main.js");
		this._info.AddC3RuntimeScript("c3runtime/spine-construct3-lib.js");
		this._info.AddFileDependency({
			filename: "c3runtime/spine-construct3-lib.js",
			type: "external-runtime-script"
		});

		SDK.Lang.PushContext(".properties");

		this._info.SetProperties([
			new SDK.PluginProperty("projectfile", SpineC3Plugin.PROP_ATLAS, { initialValue: "", filter: ".atlas" }),
			new SDK.PluginProperty("projectfile", SpineC3Plugin.PROP_SKELETON, { initialValue: "", filter: ".json,.skel" }),
			new SDK.PluginProperty("float", SpineC3Plugin.PROP_LOADER_SCALE, 1),
			new SDK.PluginProperty("link", "select-skin", {
				linkCallback: async (instance) => {
					const sdkInst = instance as SDKEditorInstanceClass;
					await sdkInst.selectSkin();
				},
				callbackType: "for-each-instance"
			}),
			new SDK.PluginProperty("text", SpineC3Plugin.PROP_SKIN, ""),
			new SDK.PluginProperty("link", "select-animation", {
				linkCallback: async (instance) => {
					const sdkInst = instance as SDKEditorInstanceClass;
					await sdkInst.selectAnimation();
				},
				callbackType: "for-each-instance"
			}),
			new SDK.PluginProperty("text", SpineC3Plugin.PROP_ANIMATION, ""),
			new SDK.PluginProperty("check", SpineC3Plugin.PROP_DEBUG_SKELETON, false),
			new SDK.PluginProperty("check", SpineC3Plugin.PROP_ENABLE_COLLISION, false),

			new SDK.PluginProperty("group", SpineC3Plugin.PROP_BOUNDS_PROVIDER_GROUP),
			new SDK.PluginProperty("combo", SpineC3Plugin.PROP_BOUNDS_PROVIDER, {
				initialValue: "setup",
				items: [
					SpineC3Plugin.TYPE_BOUNDS_SETUP,
					SpineC3Plugin.TYPE_BOUNDS_ANIMATION_SKIN,
				],
			}),
			new SDK.PluginProperty("check", SpineC3Plugin.PROP_BOUNDS_PROVIDER_MOVE, false),
			new SDK.PluginProperty("float", SpineC3Plugin.PROP_BOUNDS_OFFSET_X, 0),
			new SDK.PluginProperty("float", SpineC3Plugin.PROP_BOUNDS_OFFSET_Y, 0),
			new SDK.PluginProperty("float", SpineC3Plugin.PROP_BOUNDS_OFFSET_ANGLE, 0),
			new SDK.PluginProperty("float", SpineC3Plugin.PROP_SKELETON_OFFSET_SCALE_X, 1),
			new SDK.PluginProperty("float", SpineC3Plugin.PROP_SKELETON_OFFSET_SCALE_Y, 1),
			new SDK.PluginProperty("link", "set-bounds", {
				linkCallback: (instance) => {
					const sdkInst = instance as SDKEditorInstanceClass;
					sdkInst._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE, false);
					sdkInst.resetBounds(true);
				},
				callbackType: "for-each-instance"
			}),
		]);

		SDK.Lang.PopContext();		// .properties

		SDK.Lang.PopContext();
	}
};

SDK.Plugins.EsotericSoftware_SpineConstruct3 = PLUGIN_CLASS;

PLUGIN_CLASS.Register(PLUGIN_ID, PLUGIN_CLASS);
