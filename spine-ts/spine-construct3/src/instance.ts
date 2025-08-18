// / <reference types="editor/sdk" />

import { AnimationState, AssetLoader, Skeleton, SkeletonRendererCore, SpineBoundsProvider, TextureAtlas } from "@esotericsoftware/spine-construct3-lib";

const SDK = globalThis.SDK;

const PLUGIN_CLASS = SDK.Plugins.EsotericSoftware_SpineConstruct3;

let spine: typeof globalThis.spine;

type SpineBoundsProviderType = "setup" | "animation-skin" | "AABB";

class MyDrawingInstance extends SDK.IWorldInstanceBase {
	private layoutView?: SDK.UI.ILayoutView;
	private renderer?: SDK.Gfx.IWebGLRenderer;

	private currentAtlasFileSID = -1;
	private textureAtlas?: TextureAtlas;

	skeleton?: Skeleton;
	state?: AnimationState;
	skins: string[] = [];
	animation?: string;

	private assetLoader: AssetLoader;
	private skeletonRenderer: SkeletonRendererCore;

	// position mode
	private positioningBounds = false;
	private positionModePrevX = 0;
	private positionModePrevY = 0;
	private positionModePrevAngle = 0;
	private spineBounds = {
		x: 0,
		y: 0,
		width: 100,
		height: 100,
	};

	// utils for drawing
	private tempVertices = new Float32Array(4096);

	// errors
	private errors: Record<string, string> = {};

	constructor (sdkType: SDK.ITypeBase, inst: SDK.IWorldInstance) {
		super(sdkType, inst);

		if (!spine) spine = globalThis.spine;
		spine.Skeleton.yDown = true;

		this.assetLoader = new spine.AssetLoader();
		this.skeletonRenderer = new spine.SkeletonRendererCore();

		(this._inst as any).errors = this.errors;
	}

	Release () {
	}

	OnCreate () {
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE, false);
		this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON) as number;
	}

	OnPlacedInLayout () {
		this.OnMakeOriginalSize();
	}

	Draw (iRenderer: SDK.Gfx.IWebGLRenderer, iDrawParams: SDK.Gfx.IDrawParams) {
		this.layoutView ||= iDrawParams.GetLayoutView();
		this.renderer ||= iRenderer;

		this.loadAtlas();
		this.loadSkeleton();

		const hasErrors = this.hasErrors();
		if (this.skeleton && !hasErrors) {
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

				const baseScaleX = this._inst.GetWidth() / this.spineBounds.width;
				const baseScaleY = this._inst.GetHeight() / this.spineBounds.height;
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

				iRenderer.ResetColor();
				iRenderer.SetAlphaBlend();
				iRenderer.SetTextureFillMode();
				iRenderer.SetTexture(command.texture.texture);

				iRenderer.DrawMesh(
					vertices.subarray(0, numVertices * 3),
					uvs.subarray(0, numVertices * 2),
					// workaround for this bug: https://github.com/Scirra/Construct-bugs/issues/8746
					this.padUint16ArrayForWebGPU(indices.subarray(0, numIndices)),
				);


				command = command.next;
			}

			iRenderer.SetAlphaBlend();
			iRenderer.SetColorFillMode();
			iRenderer.SetColorRgba(0.25, 0, 0, 0.25);
			iRenderer.LineQuad(this._inst.GetQuad());
			iRenderer.Line(rectX, rectY, offsetX, offsetY);

			// if (this.hasErrors()) {
			// 	iRenderer.SetColorFillMode();
			// 	iRenderer.SetColorRgba(1, 0, 0, .5);
			// 	iRenderer.Quad(this._inst.GetQuad());
			// }
		} else {

			const sdkType = (this._sdkType as any);


			const logo = sdkType.getSpineLogo(iRenderer);
			if (logo) {
				iRenderer.ResetColor();
				iRenderer.SetAlphaBlend();
				iRenderer.SetTexture(logo);
				if (hasErrors) {
					iRenderer.SetColorRgba(1, 0, 0, 1);
				}
				iRenderer.Quad(this._inst.GetQuad());
			} else {
				iRenderer.SetAlphaBlend();
				iRenderer.SetColorFillMode();

				if (this.HadTextureError())
					iRenderer.SetColorRgba(0.25, 0, 0, 0.25);
				else
					iRenderer.SetColorRgba(0, 0, 0.1, 0.1);

				iRenderer.Quad(this._inst.GetQuad());
			}


		}
	}

	padUint16ArrayForWebGPU (originalArray: Uint16Array) {
		const currentLength = originalArray.length;

		const alignedLength = Math.ceil(currentLength / 6) * 6;

		if (alignedLength === currentLength) {
			return originalArray;
		}

		const paddedArray = new Uint16Array(alignedLength);
		paddedArray.set(originalArray);

		return paddedArray;
	}

	async OnPropertyChanged (id: string, value: EditorPropertyValueType) {
		console.log(`Prop change - Name: ${id} - Value: ${value}`);

		if (id === PLUGIN_CLASS.PROP_ATLAS) {
			this.textureAtlas?.dispose();
			this.textureAtlas = undefined;
			this.skins = [];
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKELETON) {
			this.skeleton = undefined;
			this.skins = [];
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_LOADER_SCALE) {
			this.skeleton = undefined;
			this.skins = [];
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKIN) {
			this.skins = [];
			this.setSkin();
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_ANIMATION) {
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) {
			this.resetBounds();
			this.layoutView?.Refresh();
			return
		}

		if (id === PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE) {
			value = value as boolean
			if (value) {
				this.positionModePrevX = this._inst.GetX();
				this.positionModePrevY = this._inst.GetY();
				this.positionModePrevAngle = this._inst.GetAngle();
			} else {
				const scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
				const scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
				this.spineBounds.width = this._inst.GetWidth() / scaleX;
				this.spineBounds.height = this._inst.GetHeight() / scaleY;
			}
			this.positioningBounds = value;
			return
		}

		console.log("Prop change end");
	}

	private setSkin () {
		const { skeleton } = this;
		if (!skeleton) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKIN) as string;

		const skins = propValue === "" ? [] : propValue.split(",");
		this.skins = skins;

		if (skins.length === 0) {
			skeleton.setSkin(null);
		} else if (skins.length === 1) {
			const skinName = skins[0];
			const skin = skeleton.data.findSkin(skinName);
			if (!skin) {
				// TODO: signal error
				return;
			}
			skeleton.setSkin(skins[0]);
		} else {
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
	}

	private async loadSkeleton () {
		if (!this.renderer || !this.textureAtlas) return;
		if (this.skeleton) return;

		console.log("Loading skeleton");

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON) as number;
		const loaderScale = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_LOADER_SCALE) as number;
		const skeletonData = await this.assetLoader.loadSkeletonEditor(propValue, this.textureAtlas, loaderScale, this._inst)
			.catch((error) => {
				console.log("ATLAS AND SKELETON NOT CORRESPONDING", error);
			});
		if (!skeletonData) return;

		this.skeleton = new spine.Skeleton(skeletonData);
		const animationStateData = new spine.AnimationStateData(skeletonData);
		this.state = new spine.AnimationState(animationStateData);

		this.setSkin();
		this.update(0);
		this.setBoundsFromBoundsProvider();
		this.initBounds();

		this.layoutView?.Refresh();
		console.log("SKELETON LOADED");
	}

	private async loadAtlas () {
		if (!this.renderer) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ATLAS) as number;

		if (this.currentAtlasFileSID === propValue) return;
		this.currentAtlasFileSID = propValue;

		console.log("Loading atlas");

		const textureAtlas = await this.assetLoader.loadAtlasEditor(propValue, this._inst, this.renderer);
		if (!textureAtlas) return;

		this.textureAtlas = textureAtlas;
		this.layoutView?.Refresh();
	}

	private setBoundsFromBoundsProvider () {
		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) as SpineBoundsProviderType;

		let spineBoundsProvider: SpineBoundsProvider;
		if (propValue === "animation-skin") {
			const { skins, animation } = this;
			if ((skins && skins.length > 0) || animation) {
				spineBoundsProvider = new spine.SkinsAndAnimationBoundsProvider(animation, skins);
			} else {
				return false;
			}
		} else if (propValue === "setup") {
			spineBoundsProvider = new spine.SetupPoseBoundsProvider();
		} else {
			spineBoundsProvider = new spine.AABBRectangleBoundsProvider(0, 0, 100, 100);
		}

		this.spineBounds = spineBoundsProvider.calculateBounds(this);

		return true;
	}

	private resetBounds () {
		this.setBoundsFromBoundsProvider();

		if (this.hasErrors()) return;
		const { x, y, width, height } = this.spineBounds;

		this._inst.SetOrigin(-x / width, -y / height);
		this._inst.SetSize(width, height);

		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, 0);
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, 0);
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, 0);
		return;
	}

	private initBounds () {
		const offsetX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number;
		const offsetY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number;
		const offsetAngle = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number;
		const shiftedBounds = offsetX !== 0 || offsetY !== 0 || offsetAngle !== 0;

		const scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
		const scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
		const scaledBounds = scaleX !== 1 || scaleY !== 1;

		if (shiftedBounds || scaledBounds) {
			this.spineBounds.width = this._inst.GetWidth() / scaleX;
			this.spineBounds.height = this._inst.GetHeight() / scaleY;
			return;
		}

		this.resetBounds();
	}

	private update (delta: number) {
		const { state, skeleton } = this;

		if (!skeleton || !state) return;

		state.update(delta);
		skeleton.update(delta);
		state.apply(skeleton);
		skeleton.updateWorldTransform(spine.Physics.update);
	}

	private setError (key: string, condition: boolean, message: string) {
		if (condition) {
			this.errors[key] = message;
			return;
		}
		delete this.errors[key];
	}
	private hasErrors () {
		const { errors, skins, animation, spineBounds } = this;

		const boundsType = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) as SpineBoundsProviderType;
		this.setError(
			"boundsAnimationSkinType",
			boundsType === "animation-skin" && ((!skins || skins.length === 0) && !animation),
			"Animation/Skin bounds provider requires one between skin and animation to be set."
		);

		const { width, height } = spineBounds;
		this.setError(
			"boundsNoDimension",
			width <= 0 || height <= 0,
			"A bounds cannot have negative dimensions. This might happen when the setup pose is empty. Try to set a skin and the Animation/Skin bounds provider."
		);

		return Object.keys(errors).length > 0;
	}

	GetTexture () {
		const image = this.GetObjectType().GetImage();
		return super.GetTexture(image);
	}

	IsOriginalSizeKnown () {
		return true;
	}

	GetOriginalWidth () {
		return this.spineBounds.width;
	}

	GetOriginalHeight () {
		return this.spineBounds.height;
	}

	OnMakeOriginalSize () {
		this._inst.SetSize(this.spineBounds.width, this.spineBounds.height);
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
