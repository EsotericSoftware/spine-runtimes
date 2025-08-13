// / <reference types="editor/sdk" />

import { AnimationState, AssetLoader, Skeleton, SkeletonRendererCore, SpineBoundsProvider, TextureAtlas } from "@esotericsoftware/spine-construct3-lib";

const SDK = globalThis.SDK;

const PLUGIN_CLASS = SDK.Plugins.EsotericSoftware_SpineConstruct3;

let spine: typeof globalThis.spine;

type SpineBoundsProviderType = "setup" | "animation-skin" | "AABB";

class MyDrawingInstance extends SDK.IWorldInstanceBase {
	private layoutView?: SDK.UI.ILayoutView;
	private renderer?: SDK.Gfx.IWebGLRenderer;

	private currentSkelName = "";
	private currentAtlasName = "";
	private textureAtlas?: TextureAtlas;

	skeleton?: Skeleton;
	state?: AnimationState;
	skins: string[] = [];
	currentSkinString?: string;
	animation?: string;

	private assetLoader: AssetLoader;
	private skeletonRenderer: SkeletonRendererCore;

	private positioningBounds = false;
	private spineBoundsProviderType: SpineBoundsProviderType = "setup";
	private spineBoundsProvider?: SpineBoundsProvider;
	private initialBounds = {
		x: 0,
		y: 0,
		width: 0,
		height: 0,
	};

	// position mode
	private positionModePrevX = 0;
	private positionModePrevY = 0;
	private positionModePrevAngle = 0;
	private tempVertices = new Float32Array(4096);

	constructor (sdkType: SDK.ITypeBase, inst: SDK.IWorldInstance) {
		super(sdkType, inst);

		if (!spine) spine = globalThis.spine;
		spine.Skeleton.yDown = true;

		this.assetLoader = new spine.AssetLoader("editor");
		this.skeletonRenderer = new spine.SkeletonRendererCore();

	}

	Release () {
	}

	OnCreate () {
		console.log("OnCreate");
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE, false);
	}

	OnPlacedInLayout () {
		this.OnMakeOriginalSize();
	}

	Draw (iRenderer: SDK.Gfx.IWebGLRenderer, iDrawParams: SDK.Gfx.IDrawParams) {
		this.layoutView ||= iDrawParams.GetLayoutView();
		this.renderer ||= iRenderer;


		this.loadAtlas();
		this.loadSkeleton();

		if (this.skeleton) {
			this.setSkin();

			const rectX = this._inst.GetX();
			const rectY = this._inst.GetY();
			const rectAngle = this._inst.GetAngle();
			let offsetX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number;
			let offsetY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number;
			let offsetAngle = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number;

			if (!this.positioningBounds) {
				offsetX += rectX;
				offsetY += rectY;
				offsetAngle += rectAngle;

				const baseScaleX = this._inst.GetWidth() / this.initialBounds.width;
				const baseScaleY = this._inst.GetHeight() / this.initialBounds.height;
				this.skeleton.scaleX = baseScaleX;
				this.skeleton.scaleY = baseScaleY;
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X, baseScaleX);
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y, baseScaleY);
			} else {
				offsetX += this.positionModePrevX;
				offsetY += this.positionModePrevY;
				offsetAngle += this.positionModePrevAngle;
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, offsetX - rectX);
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, offsetY - rectY);
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, offsetAngle - rectAngle);
				this.positionModePrevX = rectX;
				this.positionModePrevY = rectY;
				this.positionModePrevAngle = rectAngle;

				this.skeleton.scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
				this.skeleton.scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
			}

			const cos = Math.cos(offsetAngle);
			const sin = Math.sin(offsetAngle);

			this.update(0);
			let command = this.skeletonRenderer.render(this.skeleton);
			while (command) {
				const { numVertices, positions, uvs, indices, numIndices } = command;

				const vertices = this.tempVertices;
				for (let i = 0; i < numVertices; i++) {
					const srcIndex = i * 2;
					const dstIndex = i * 3;
					const x = positions[srcIndex];
					const y = positions[srcIndex + 1];
					vertices[dstIndex] = x * cos - y * sin + offsetX;
					vertices[dstIndex + 1] = x * sin + y * cos + offsetY;
					vertices[dstIndex + 2] = 0;
				}

				iRenderer.SetAlphaBlend();
				iRenderer.SetTexture(command.texture.texture);
				iRenderer.DrawMesh(
					vertices.subarray(0, numVertices * 3),
					uvs.subarray(0, numVertices * 2),
					indices.subarray(0, numIndices),
				);

				command = command.next;
			}

			iRenderer.SetAlphaBlend();
			iRenderer.SetColorFillMode();
			iRenderer.SetColorRgba(0.25, 0, 0, 0.25);
			iRenderer.LineQuad(this._inst.GetQuad());
			iRenderer.Line(rectX, rectY, offsetX, offsetY);
			console.log(offsetX, offsetY);
		} else {
			// render placeholder
			iRenderer.SetAlphaBlend();
			iRenderer.SetColorFillMode();

			if (this.HadTextureError())
				iRenderer.SetColorRgba(0.25, 0, 0, 0.25);
			else
				iRenderer.SetColorRgba(0, 0, 0.1, 0.1);

			iRenderer.Quad(this._inst.GetQuad());
		}
	}

	async OnPropertyChanged (id: string, value: EditorPropertyValueType) {
		console.log(`Prop change - Name: ${id} - Value: ${value}`);

		switch (id) {
			case PLUGIN_CLASS.PROP_ATLAS:
				this.layoutView?.Refresh();
				break;
			case PLUGIN_CLASS.PROP_SKELETON:
				this.layoutView?.Refresh();
				break;
			case PLUGIN_CLASS.PROP_SKIN:
				this.layoutView?.Refresh();
				break;
			case PLUGIN_CLASS.PROP_ANIMATION:
				this.layoutView?.Refresh();
				break;
			case PLUGIN_CLASS.PROP_BOUNDS_PROVIDER:
				this.setC3Bounds(true);
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, 0);
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, 0);
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, 0);
				this.layoutView?.Refresh();
				break;
			case PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE: {
				value = value as boolean
				if (value) {
					this.positionModePrevX = this._inst.GetX();
					this.positionModePrevY = this._inst.GetY();
					this.positionModePrevAngle = this._inst.GetAngle();
				} else {
					const scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
					const scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
					this.initialBounds.width = this._inst.GetWidth() / scaleX;
					this.initialBounds.height = this._inst.GetHeight() / scaleY;
				}
				this.positioningBounds = value;
				break;
			}
		}

		console.log("Prop change end");
	}

	private setSkin () {
		const { skeleton } = this;
		if (!skeleton) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKIN) as string;

		if (this.currentSkinString === propValue) return;
		this.currentSkinString = propValue;

		const skins = propValue === "" ? [] : propValue.split(",");
		this.skins = skins;

		if (skins.length === 1) {
			const skinName = skins[0];
			const skin = skeleton.data.findSkin(skinName);
			if (!skin) {
				// TODO: signal error
				return;
			}
			skeleton.setSkin(skins[0]);
		} else if (skins.length > 1) {
			const customSkin = new spine.Skin(propValue);
			for (const s of skins) {
				const skin = skeleton.data.findSkin(s)
				if (!skin) {
					// TODO: signal error
					return;
				}
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}

		skeleton.setupPose();
		this.update(0);

		this.setC3Bounds();
	}

	private async loadSkeleton () {
		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON) as string;
		const projectFile = this._inst.GetProject().GetProjectFileByName(propValue);

		if (projectFile && this.textureAtlas) {
			if (this.currentSkelName === propValue) return;
			this.currentSkelName = propValue;

			const skeletonData = await this.assetLoader.loadSkeletonEditor(propValue, this.textureAtlas, this._inst);
			if (!skeletonData) return;

			this.skeleton = new spine.Skeleton(skeletonData);
			const animationStateData = new spine.AnimationStateData(skeletonData);
			this.state = new spine.AnimationState(animationStateData);

			this.update(0);

			this.setSkin();

			const offsetX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number;
			const offsetY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number;
			const offsetAngle = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number;
			const scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
			const scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
			const init = offsetX !== 0 && offsetY !== 0 && offsetAngle !== 0 && scaleX !== 1 && scaleY !== 1;
			this.setC3Bounds(init);

			this.layoutView?.Refresh();
			console.log("SKELETON LOADED");
		}
	}

	private setC3Bounds (init = false) {
		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) as SpineBoundsProviderType;

		if (propValue === "animation-skin") {
			const { skins, animation } = this;
			if ((skins && skins.length > 0) || animation) {
				this.spineBoundsProvider = new spine.SkinsAndAnimationBoundsProvider(animation, skins);
			} else {
				throw new Error("One among skin and animation needs to have a value to set this bounds provider.");
			}
		} else if (propValue === "setup") {
			this.spineBoundsProvider = new spine.SetupPoseBoundsProvider();
		} else {
			this.spineBoundsProvider = new spine.AABBRectangleBoundsProvider(0, 0, 100, 100);
		}

		this.initialBounds = this.spineBoundsProvider.calculateBounds(this);

		const { x, y, width, height } = this.initialBounds;

		if (width <= 0 || height <= 0 || !init) return;

		this._inst.SetSize(width, height);

		if (propValue === "AABB") {
			this._inst.SetOrigin(.5, .5);
		} else {
			this._inst.SetOrigin(-x / width, -y / height);
		}
	}

	private update (delta: number) {
		const { state, skeleton } = this;

		if (!skeleton || !state) return;

		state.update(delta);
		skeleton.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform(spine.Physics.update);
	}

	private async loadAtlas () {
		if (!this.renderer) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ATLAS) as string;

		if (this.currentAtlasName === propValue) return;
		this.currentAtlasName = propValue;

		const textureAtlas = await this.assetLoader.loadAtlasEditor(propValue, this._inst, this.renderer);
		if (!textureAtlas) return;

		this.textureAtlas = textureAtlas;
		this.layoutView?.Refresh();
	}

	GetTexture () {
		const image = this.GetObjectType().GetImage();
		return super.GetTexture(image);
	}

	IsOriginalSizeKnown () {
		return true;
	}

	GetOriginalWidth () {
		if (!this.initialBounds) return 100;
		return this.initialBounds.width;
	}

	GetOriginalHeight () {
		if (!this.initialBounds) return 100;
		return this.initialBounds.height;
	}

	OnMakeOriginalSize () {
		if (!this.initialBounds)
			this._inst.SetSize(100, 100);
		else
			this._inst.SetSize(this.initialBounds.width, this.initialBounds.height);
	}

	HasDoubleTapHandler () {
		return false;
	}

	OnDoubleTap () { }

	LoadC2Property (_name: string, _valueString: string) {
		return false;
	}
};

PLUGIN_CLASS.Instance = MyDrawingInstance;

export type { MyDrawingInstance as SDKEditorInstanceClass };
