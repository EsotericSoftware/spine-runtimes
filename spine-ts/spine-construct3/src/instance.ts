// / <reference types="editor/sdk" />

import type { AnimationState, AssetLoader, C3Matrix, C3RendererEditor, Skeleton, SpineBoundsProvider, TextureAtlas, } from "@esotericsoftware/spine-construct3-lib";
import type { SpineC3PluginType } from "./type";

const SDK = globalThis.SDK;

const PLUGIN_CLASS = SDK.Plugins.EsotericSoftware_SpineConstruct3;

let spine: typeof globalThis.spine;

type SpineBoundsProviderType = "setup" | "animation-skin" | "AABB";

class SpineC3PluginInstance extends SDK.IWorldInstanceBase {
	private layoutView?: SDK.UI.ILayoutView;
	private renderer?: SDK.Gfx.IWebGLRenderer;

	private textureAtlasSID = -1;
	private textureAtlas?: TextureAtlas;
	private textureAtlasBasePath?: string;

	skeleton?: Skeleton;
	state?: AnimationState;
	skins: string[] = [];
	animation?: string;

	private assetLoader: AssetLoader;
	private skeletonRenderer?: C3RendererEditor;
	private matrix: C3Matrix;

	// position mode
	private positioningBounds = false;
	private positionModePrevX = 0;
	private positionModePrevY = 0;
	private positionModePrevAngle = 0;

	/*
	 * C3 GameObjects have two sizes:
	 * - the original size that is determined implementing GetOriginalWidth/GetOriginalHeight
	 * - the current size that is set using SetSize or SetWidth/SetHeight
	 * The ratio between this two size determines the C3 GameObjects scale.
	 *
	 * The origin is by default in the center and set using SetOrigin;
	 * it's usually moved in the Image Editor, but that's disable with Spine C3 GameObjects.
	 *
	 * In a Spine C3 GameObject:
	 * - the original size is equivalent to spineBounds that is set selecting the BoundsProvider
	 * - changing the C3 GameObject size from the editor will scale the skeleton by using skeleton.scaleX/Y
	 *   This information is stored into (PROP_SKELETON_SCALE_X and Y) and later passed to the runtime
	 * - the origin is position at the skeleton root
	 *
	 * positioningBounds allows to offset the position and the size of the C3 GameObject
	 * with the one of the skeleton. When selected it allows to:
	 * - move the C3 GameObjects position (visually the rectangle) keeping the skeleton still.
	 *   This is obtained by adding an offset to the GameObject position.
	 *   This information is stored into (PROP_SKELETON_SCALE_X and Y) and later passed to the runtime
	 * - scale the C3 GameObjects keeping the skeleton.scaleX/Y as-is.
	 */
	private spineBounds = {
		x: 0, // determine the origin x (-x/width)
		y: 0, // determine the origin y (-y/height)
		width: 200, // determine the original width (and the origin x)
		height: 200, // determine the original height (and the origin y)
	};
	private spineBoundsInit = false;

	// errors
	private errorTextureAtlas?: string;
	private errorSkeleton?: string;
	private errorTextC3?: SDK.Gfx.IWebGLText;

	constructor (sdkType: SDK.ITypeBase, inst: SDK.IWorldInstance) {
		super(sdkType, inst);

		if (!spine) spine = globalThis.spine;
		spine.Skeleton.yDown = true;

		this.assetLoader = new spine.AssetLoader();
		this.matrix = new spine.C3Matrix();
	}

	Release () {
		this.textureAtlas?.dispose();
	}

	OnCreate () {
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE, false);
	}

	OnPlacedInLayout () {
		this.OnMakeOriginalSize();
	}

	async OnPropertyChanged (id: string, value: EditorPropertyValueType) {
		if (id === PLUGIN_CLASS.PROP_ATLAS) {
			this.textureAtlasSID = -1;
			this.textureAtlas?.dispose();
			this.textureAtlas = undefined;
			this.skins = [];
			this.skeleton = undefined;
			this.spineBoundsInit = false;
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKELETON) {
			this.errorSkeleton = undefined;
			this.skeleton = undefined;
			this.skins = [];
			this.spineBoundsInit = false;
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_LOADER_SCALE) {
			this.skeleton = undefined;
			this.skins = [];
			this.spineBoundsInit = false;
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKIN) {
			this.skins = [];
			this.setSkin();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_ANIMATION) {
			this.setAnimation();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) {
			this.resetBounds(true);
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
	}

	Draw (iRenderer: SDK.Gfx.IWebGLRenderer, iDrawParams: SDK.Gfx.IDrawParams) {
		this.layoutView ||= iDrawParams.GetLayoutView();
		this.renderer ||= iRenderer;

		this.loadAtlas();
		this.loadSkeleton();
		this.initBounds();

		const { _inst, skeleton } = this;
		const errorsString = this.getErrorsString();
		if (skeleton && this.textureAtlas && !errorsString) {
			this.setAnimation();
			this.setSkin();

			const rectX = _inst.GetX();
			const rectY = _inst.GetY();
			const rectAngle = _inst.GetAngle();
			let offsetX = _inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number;
			let offsetY = _inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number;
			let offsetAngle = _inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number;

			if (!this.positioningBounds) {
				offsetX += rectX;
				offsetY += rectY;
				offsetAngle += rectAngle;

				const baseScaleX = _inst.GetWidth() / this.spineBounds.width;
				const baseScaleY = _inst.GetHeight() / this.spineBounds.height;
				skeleton.scaleX = baseScaleX;
				skeleton.scaleY = baseScaleY;
				_inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X, baseScaleX);
				_inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y, baseScaleY);
			} else {
				offsetX += this.positionModePrevX;
				offsetY += this.positionModePrevY;
				offsetAngle += this.positionModePrevAngle;
				_inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, offsetX - rectX);
				_inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, offsetY - rectY);
				_inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, offsetAngle - rectAngle);
				this.positionModePrevX = rectX;
				this.positionModePrevY = rectY;
				this.positionModePrevAngle = rectAngle;

				skeleton.scaleX = _inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
				skeleton.scaleY = _inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
			}

			this.update(0);
			this.skeletonRenderer ||= new spine.C3RendererEditor(iRenderer, this.matrix);
			const color = _inst.GetColor();
			this.skeletonRenderer.draw(skeleton, [color.getR(), color.getG(), color.getB()], color.getA() * _inst.GetOpacity());
			const quad = _inst.GetQuad();
			if (_inst.GetPropertyValue(PLUGIN_CLASS.PROP_DEBUG_SKELETON) as boolean)
				this.skeletonRenderer.drawDebug(skeleton, rectX, rectY, quad);
			this.skeletonRenderer.renderGameObjectBounds(rectX, rectY, quad);

		} else {
			iRenderer.SetAlphaBlend();

			const logo = (this._sdkType as SpineC3PluginType).getSpineLogo(iRenderer, this.layoutView);
			if (logo) {
				iRenderer.SetColorRgba(1, 1, 1, errorsString ? 0.25 : 1);
				iRenderer.SetTexture(logo);
			} else {
				iRenderer.SetColorFillMode();
				iRenderer.SetColorRgba(0.25, 0, 0, 0.25);
			}
			const quad = _inst.GetQuad();
			iRenderer.Quad(quad);

			if (errorsString) {
				const webglText = this.getErrorTextC3(iRenderer, this.layoutView);
				webglText.SetSize(_inst.GetWidth(), _inst.GetHeight(), this.layoutView.GetZoomFactor());
				webglText.SetText(errorsString);

				const texture = webglText.GetTexture();
				if (!texture) return;

				iRenderer.SetColorRgba(1, 1, 1, 1);
				iRenderer.SetTexture(texture);
				iRenderer.Quad3(quad, webglText.GetTexRect());
			}

		}
	}

	private setAnimation () {
		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ANIMATION) as string;
		this.animation = propValue === "" ? undefined : propValue;
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
			if (!skin) return;
			skeleton.setSkin(skins[0]);
		} else {
			const customSkin = new spine.Skin(propValue);
			for (const s of skins) {
				const skin = skeleton.data.findSkin(s)
				if (!skin) return;
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}

		skeleton.setupPose();
		this.update(0);
	}

	private async loadAtlas () {
		if (!this.renderer) return;
		this.checkAtlasTexturesValidity();

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ATLAS) as number;
		if (this.textureAtlasSID === propValue && !this.errorTextureAtlas) return;
		this.textureAtlasSID = propValue;

		const result = await this.assetLoader.loadAtlasEditor(propValue, this._inst, this.renderer)
			.catch((error: Error) => {
				this.errorTextureAtlas = error.message;
				this.layoutView?.Refresh();
			});
		if (!result) return;

		this.errorTextureAtlas = undefined;
		this.textureAtlas = result.textureAtlas;
		this.textureAtlasBasePath = result.basePath;
		this.layoutView?.Refresh();
	}

	private async loadSkeleton () {
		if (!this.renderer || !this.textureAtlas) return;
		if (this.skeleton) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON) as number;
		const loaderScale = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_LOADER_SCALE) as number;
		const skeletonData = await this.assetLoader.loadSkeletonEditor(propValue, this.textureAtlas, loaderScale, this._inst)
			.catch((error) => {
				if (!this.errorSkeleton) this.layoutView?.Refresh();
				this.errorSkeleton = `${error.message}\n. Likely Atlas and Skeleton are not corresponding.`;
			});
		if (!skeletonData) return;

		this.errorSkeleton = undefined;
		this.skeleton = new spine.Skeleton(skeletonData);
		const animationStateData = new spine.AnimationStateData(skeletonData);
		this.state = new spine.AnimationState(animationStateData);

		this.setSkin();
		this.setAnimation();
		this.update(0);

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

	public resetBounds (keepScale = false) {
		if (!this.skeleton || !this.textureAtlas) {
			this._inst.SetSize(200, 200);
			this.spineBounds.width = 200;
			this.spineBounds.height = 200;
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, 0);
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, 0);
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, 0);
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X, 1);
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y, 1);
			return;
		}

		this.setBoundsFromBoundsProvider();
		if (this.getErrorsString()) {
			this.spineBoundsInit = false;
			return;
		};

		this.spineBoundsInit = true;

		const { x, y, width, height } = this.spineBounds;
		this._inst.SetOrigin(-x / width, -y / height);

		let scaleX = 1;
		let scaleY = 1;
		if (keepScale) {
			scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
			scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
		}
		this._inst.SetSize(width * scaleX, height * scaleY);

		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, 0);
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, 0);
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, 0);
		return;
	}

	private initBounds () {
		if (this.spineBoundsInit) return;

		const offsetX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number;
		const offsetY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number;
		const offsetAngle = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number;
		const shiftedBounds = offsetX !== 0 || offsetY !== 0 || offsetAngle !== 0;

		const scaleX = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_X) as number;
		const scaleY = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_SCALE_Y) as number;
		const scaledBounds = scaleX !== 1 || scaleY !== 1;

		if (!shiftedBounds && !scaledBounds) {
			this.resetBounds();
			return;
		}

		this.setBoundsFromBoundsProvider();
		this.spineBounds.width = this._inst.GetWidth() / scaleX;
		this.spineBounds.height = this._inst.GetHeight() / scaleY;

		this.spineBoundsInit = true;
	}

	private checkAtlasTexturesValidity () {
		const atlasSid = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ATLAS) as number;
		if (atlasSid === -1) return;

		const { textureAtlas, textureAtlasBasePath } = this;
		if (!textureAtlas || textureAtlasBasePath === undefined) return;

		for (const page of textureAtlas.pages) {
			if (!this._inst.GetProject().GetProjectFileByExportPath(textureAtlasBasePath + page.name)) {
				this.skeleton = undefined;
				this.OnPropertyChanged(PLUGIN_CLASS.PROP_ATLAS, atlasSid);
				return;
			}
		}
	}

	private getErrorsString () {
		const { skins, animation, spineBounds, skeleton } = this;
		const errors = [];

		const boundsType = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) as SpineBoundsProviderType;
		if (boundsType === "animation-skin" && (skins.length === 0 && !animation))
			errors.push("Animation/Skin bounds provider requires one between skin and animation to be set.");

		if (Boolean(!animation || skeleton?.data.findAnimation(animation)) === false)
			errors.push("Not existing animation");

		if (skins.length > 0) {
			const missingSkins = skins.filter(skin => !skeleton?.data.findSkin(skin)).join(", ");
			if (missingSkins)
				errors.push("Not existing skin(s): ", missingSkins);
		}

		const { width, height } = spineBounds;
		if (width <= 0 || height <= 0)
			errors.push("A bounds cannot have negative dimensions. This might happen when the setup pose is empty. Try to set a skin and the Animation/Skin bounds provider.");

		if (this.errorTextureAtlas) errors.push(this.errorTextureAtlas);
		if (this.errorSkeleton) errors.push(this.errorSkeleton);

		if (errors.length === 0) return "";

		return errors.join("\n");
	}
	private getErrorTextC3 (iRenderer: SDK.Gfx.IWebGLRenderer, iLayoutView: SDK.UI.ILayoutView) {
		if (this.errorTextC3) return this.errorTextC3;

		const errorTextC3 = iRenderer.CreateRendererText()
		this.errorTextC3 = errorTextC3;
		this.errorTextC3.SetFontSize(12);
		this.errorTextC3.SetColorRgb(1, 0, 0);
		this.errorTextC3.SetTextureUpdateCallback(() => iLayoutView.Refresh());
		return this.errorTextC3;
	}

	private update (delta: number) {
		const { state, skeleton } = this;

		if (!skeleton || !state) return;

		state.update(delta);
		skeleton.update(delta);
		state.apply(skeleton);
		this.matrix.update(
			this._inst.GetX() + (this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number),
			this._inst.GetY() + (this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number),
			this._inst.GetAngle() + (this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number));
		skeleton.updateWorldTransform(spine.Physics.update);
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

PLUGIN_CLASS.Instance = SpineC3PluginInstance;

export type { SpineC3PluginInstance as SDKEditorInstanceClass };
