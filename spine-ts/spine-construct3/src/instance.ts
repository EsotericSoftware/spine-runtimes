// / <reference types="editor/sdk" />

import type { AnimationState, AssetLoader, Skeleton, SkeletonRendererCore, SpineBoundsProvider, TextureAtlas } from "@esotericsoftware/spine-construct3-lib";

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

	private offsetX = 0;
	private offsetY = 0;
	private offsetWidth = 0;
	private offsetHeight = 0;
	private positioningBounds = false;
	private spineBoundsProviderType: SpineBoundsProviderType = "setup";
	private spineBoundsProvider?: SpineBoundsProvider;
	private _spineBounds?: {
		x: number;
		y: number;
		width: number;
		height: number;
	};

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
	}

	OnPlacedInLayout () {
		this.OnMakeOriginalSize();
	}

	private tempVertices = new Float32Array(4096);
	Draw (iRenderer: SDK.Gfx.IWebGLRenderer, iDrawParams: SDK.Gfx.IDrawParams) {
		console.log("DRAW");
		this.layoutView ||= iDrawParams.GetLayoutView();
		this.renderer ||= iRenderer;

		this.loadAtlas();
		this.loadSkeleton();

		if (this.skeleton) {
			this.setSkin();

			let x = this.overallOffsetX;
			let y = this.overallOffsetY;

			let width = this.offsetWidth;
			let height = this.offsetHeight;

			if (!this.positioningBounds) {
				x += this._inst.GetX();
				y += this._inst.GetY();
				width = this._inst.GetWidth();
				height = this._inst.GetHeight();
			} else {
				// x += this._inst.GetX();
				// y += this._inst.GetY();
				// width += this._inst.GetWidth();
				// height += this._inst.GetHeight();
			}

			const rx = width / this._spineBounds!.width;

			console.log(width, this._spineBounds!.width, rx);

			const ry = height / this._spineBounds!.height;
			this.skeleton.scaleX = rx;
			this.skeleton.scaleY = ry;

			this.update(0);

			let command = this.skeletonRenderer.render(this.skeleton);
			while (command) {
				const { numVertices, positions, uvs, indices, numIndices } = command;

				const vertices = this.tempVertices;
				for (let i = 0; i < numVertices; i++) {
					const srcIndex = i * 2;
					const dstIndex = i * 3;
					vertices[dstIndex] = positions[srcIndex] + x;
					vertices[dstIndex + 1] = positions[srcIndex + 1] + y;
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

			// iRenderer.SetAlphaBlend();
			// iRenderer.SetColorFillMode();
			// iRenderer.SetColorRgba(0.25, 0, 0, 0.25);
			// iRenderer.LineRect(this._inst.GetX(), this._inst.GetY(), this._inst.GetWidth(), this._inst.GetHeight());

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

	private overallOffsetX = 0;
	private overallOffsetY = 0;
	private overallScaleX = 1
	async OnPropertyChanged (id: string, value: EditorPropertyValueType) {
		console.log("Prop change - Name: " + id + " - Value: " + value);

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
				this.layoutView?.Refresh();
				break;
			case PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE:
				value = value as boolean
				if (value) {
					this.overallOffsetX += this._inst.GetX();
					this.overallOffsetY += this._inst.GetY();
					this.offsetX = this._inst.GetX();
					this.offsetY = this._inst.GetY();
					this.offsetWidth = this._inst.GetWidth();
					this.offsetHeight = this._inst.GetHeight();
				} else {
					const w = this.offsetWidth;

					this.overallOffsetX -= this._inst.GetX();
					this.overallOffsetY -= this._inst.GetY();
					this.offsetX -= this._inst.GetX();
					this.offsetY -= this._inst.GetY();
					this.offsetWidth -= this._inst.GetWidth();
					this.offsetHeight -= this._inst.GetHeight();



					console.log("OFFSETS");
					console.log(this.offsetX);
					console.log(this.offsetY);
					console.log(this.overallOffsetX);
					console.log(this.overallOffsetY);
					console.log("OFFSETS");


					this.spineBoundsProvider = new spine.AABBRectangleBoundsProvider(
						this._spineBounds!.x - this.offsetX,
						this._spineBounds!.y - this.offsetY,
						this._spineBounds!.width - this.offsetWidth,
						this._spineBounds!.height - this.offsetHeight,
					);

					this._spineBounds = this.spineBoundsProvider.calculateBounds(this);
					const { x, y, width, height } = this._spineBounds;

					console.log(this._inst.GetX(), this._inst.GetY());
					console.log(x, y, width, height, (-x) / width, (-y) / height);

					// this._inst.SetSize(width, height);
					// this._inst.SetOrigin(-x / width, -y / height);

				}
				this.positioningBounds = value;
				break;
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
			this.setC3Bounds(true);

			console.log("SKELETON LOADED");
		}
	}

	private setC3Bounds (init = false) {
		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) as SpineBoundsProviderType;

		console.log(propValue);

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



		this._spineBounds = this.spineBoundsProvider.calculateBounds(this);

		const { x, y, width, height } = this._spineBounds;

		if (width <= 0 || height <= 0 || !init) return;

		this._inst.SetSize(width, height);
		this._inst.SetOrigin(-x / width, -y / height);
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
		if (!this._spineBounds) return 100;
		return this._spineBounds.width;
	}

	GetOriginalHeight () {
		if (!this._spineBounds) return 100;
		return this._spineBounds.height;
	}

	OnMakeOriginalSize () {
		if (!this._spineBounds)
			this._inst.SetSize(100, 100);
		else
			this._inst.SetSize(this._spineBounds.width, this._spineBounds.height);
	}

	HasDoubleTapHandler () {
		return false;
	}

	OnDoubleTap () { }

	LoadC2Property (name: string, valueString: string) {
		return false;		// not handled
	}
};

PLUGIN_CLASS.Instance = MyDrawingInstance;

export type { MyDrawingInstance as SDKEditorInstanceClass };
