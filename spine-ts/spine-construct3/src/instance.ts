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

import type { AnimationState, AssetLoader, C3Matrix, C3RendererEditor, Skeleton, SpineBoundsProvider, SpineBoundsProviderType, TextureAtlas, } from "@esotericsoftware/spine-construct3-lib";
import type { SpineC3PluginType } from "./type";

const SDK = globalThis.SDK;

const PLUGIN_CLASS = SDK.Plugins.EsotericSoftware_SpineConstruct3;

let spine: typeof globalThis.spine;

class SpineC3PluginInstance extends SDK.IWorldInstanceBase {
	private layoutView?: SDK.UI.ILayoutView;
	private renderer?: SDK.Gfx.IWebGLRenderer;

	private textureAtlas?: TextureAtlas;
	private textureAtlasBasePath?: string;
	private atlasLoad?: ReturnType<AssetLoader["loadAtlasEditor"]>;
	private skeletonLoad?: ReturnType<AssetLoader["loadSkeletonEditor"]>;

	skeleton?: Skeleton;
	state?: AnimationState;
	skins: string[] = [];
	animation?: string;
	private appliedSkin?: string;

	private assetLoader: AssetLoader;
	private skeletonRenderer?: C3RendererEditor;
	private matrix: C3Matrix;

	// position mode
	private positioningBounds = false;
	private positionModePrevX = 0;
	private positionModePrevY = 0;
	private positionModePrevAngle = 0;
	private positionModePrevWidth = 0;
	private positionModePrevHeight = 0;
	private positionModePrevScaleX = 1;
	private positionModePrevScaleY = 1;
	private positionModePrevOffsetX = 0;
	private positionModePrevOffsetY = 0;
	private positionModePrevOffsetAngle = 0;
	private suppressPositionModePropertyChange = false;

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
	 * - changing the C3 GameObject size from the editor will scale the skeleton by using a C3Matrix,
	 *   not the skeleton.scaleX/Y.
	 * - the origin is position at the skeleton root
	 *
	 * positioningBounds allows to offset the position and the size of the C3 GameObject
	 * with the one of the skeleton. When selected it allows to:
	 * - move the C3 GameObjects position (visually the rectangle) keeping the skeleton still.
	 *   This is obtained by adding an offset to the GameObject position.
	 *   This information is stored into (PROP_BOUNDS_OFFSET_X and Y) and later passed to the runtime
	 * - scale the C3 GameObjects keeping the skeleton.scaleX/Y as-is, and storing the scale offset in (PROP_SKELETON_OFFSET_SCALE_X and Y).
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
		this.atlasLoad = undefined;
		this.skeletonLoad = undefined;
		this.textureAtlas?.dispose();
		this.textureAtlas = undefined;
		this.errorTextC3?.Release();
		this.errorTextC3 = undefined;
	}

	OnCreate () {
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE, false);
	}

	OnPlacedInLayout () {
		this.OnMakeOriginalSize();
	}

	async OnPropertyChanged (id: string, value: EditorPropertyValueType) {
		if (id === PLUGIN_CLASS.PROP_ATLAS) {
			this.atlasLoad = undefined;
			this.skeletonLoad = undefined;
			this.errorTextureAtlas = undefined;
			this.errorSkeleton = undefined;
			this.textureAtlas?.dispose();
			this.textureAtlas = undefined;
			this.textureAtlasBasePath = undefined;
			this.skins = [];
			this.skeleton = undefined;
			this.spineBoundsInit = false;
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKELETON) {
			this.skeletonLoad = undefined;
			this.errorSkeleton = undefined;
			this.skeleton = undefined;
			this.skins = [];
			this.spineBoundsInit = false;
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_LOADER_SCALE) {
			this.skeletonLoad = undefined;
			this.errorSkeleton = undefined;
			this.skeleton = undefined;
			this.skins = [];
			this.spineBoundsInit = false;
			this.resetBounds();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKIN) {
			const skinString = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKIN) as string;
			const validatedString = await this.validateSkinString(skinString);
			if (validatedString !== undefined && validatedString !== skinString) {
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKIN, validatedString);
				return;
			}

			this.setSkin();
			this.update(0);
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_ANIMATION) {

			const animationString = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ANIMATION) as string;
			const validatedString = await this.validateAnimationString(animationString);
			if (validatedString !== undefined && validatedString !== this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ANIMATION)) {
				this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_ANIMATION, validatedString);
				return;
			}

			this.setAnimation();
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_BOUNDS_PROVIDER) {
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE, false);
			this.positioningBounds = false;
			this.resetBounds(true);
			this.layoutView?.Refresh();
			return;
		}

		if (id === PLUGIN_CLASS.PROP_BOUNDS_PROVIDER_MOVE) {
			value = value as boolean;
			if (value) this.capturePositionModeState();
			this.positioningBounds = value;
			return;
		}

		if (id === PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_X || id === PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_Y) {
			if (!this.suppressPositionModePropertyChange && this.positioningBounds) {
				this.applyPositionModeScalePropertyChange(id);
				this.layoutView?.Refresh();
			}
			return;
		}

		if (id === PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X || id === PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y || id === PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) {
			if (!this.suppressPositionModePropertyChange && this.positioningBounds) {
				this.applyPositionModeOffsetPropertyChange(id);
				this.layoutView?.Refresh();
			}
			return;
		}

		if (id === PLUGIN_CLASS.PROP_ENABLE_COLLISION) {
			this.managePropCollision(value as boolean);
			return;
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

			if (this.positioningBounds) {
				const rectAngle = _inst.GetAngle();

				this.propOffsetX += this.positionModePrevX - rectX;
				this.propOffsetY += this.positionModePrevY - rectY;
				this.propOffsetAngle = this.propOffsetAngle + this.positionModePrevAngle - rectAngle;

				this.positionModePrevX = rectX;
				this.positionModePrevY = rectY;
				this.positionModePrevAngle = rectAngle;

				const currentWidth = _inst.GetWidth();
				const currentHeight = _inst.GetHeight();
				if (currentWidth !== this.positionModePrevWidth || currentHeight !== this.positionModePrevHeight) {
					this.propScaleX = this.propScaleX * this.positionModePrevWidth / currentWidth;
					this.propScaleY = this.propScaleY * this.positionModePrevHeight / currentHeight;
					this.positionModePrevWidth = currentWidth;
					this.positionModePrevHeight = currentHeight;
					this.positionModePrevScaleX = this.propScaleX;
					this.positionModePrevScaleY = this.propScaleY;
				}
			}

			this.update(0);
			this.skeletonRenderer ||= new spine.C3RendererEditor(iRenderer, this.matrix);
			const color = _inst.GetColor();
			const slotZOffset = iDrawParams.GetLayoutView().GetViewMode() === "3d"
				? _inst.GetPropertyValue(PLUGIN_CLASS.PROP_SLOT_Z_OFFSET) as number
				: 0;
			this.skeletonRenderer.draw(skeleton, [color.getR(), color.getG(), color.getB()], color.getA() * _inst.GetOpacity(), true, slotZOffset);
			const quad = _inst.GetQuad();
			if (_inst.GetPropertyValue(PLUGIN_CLASS.PROP_DEBUG_SKELETON) as boolean)
				this.skeletonRenderer.drawDebug(skeleton, rectX, rectY, quad);
			this.skeletonRenderer.renderGameObjectBounds(rectX, rectY, quad, _inst.GetPropertyValue(PLUGIN_CLASS.PROP_ENABLE_COLLISION) as boolean);

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
				webglText.SetSize(_inst.GetWidth(), _inst.GetHeight(), iDrawParams.GetLayoutView().GetZoomFactor());
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

	private async validateSkinString (skins: string) {
		if (skins === "" || !this.skeleton) return;

		const split = skins.split(",").filter(s => s !== "");
		if (split.length === 0) return;

		const availableSkins = this.skeleton.data.skins.map(skin => skin.name).filter(s => s !== "default");

		const invalidSkins = split.filter(skinName => !availableSkins.includes(skinName));

		if (invalidSkins.length > 0) {
			await spine.showAlertModal({
				darkMode: false,
				title: this.lang('invalid-skins.title'),
				message: this.lang('invalid-skins.message', [invalidSkins.join(", ")])
			});

			return split.filter(skinName => availableSkins.includes(skinName)).join(",");
		}

		return split.join(",");
	}

	private async validateAnimationString (animation: string) {
		if (animation === "" || !this.skeleton) return;

		const availableAnimations = this.skeleton.data.animations.map(anim => anim.name);

		if (!availableAnimations.includes(animation)) {
			await spine.showAlertModal({
				darkMode: false,
				title: this.lang('invalid-animation.title'),
				message: this.lang('invalid-animation.message', [animation])
			});

			return "";
		}

		return animation;
	}

	private setSkin () {
		const { skeleton } = this;
		if (!skeleton) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKIN) as string;
		if (propValue === this.appliedSkin) return;

		const skins = propValue === "" ? [] : propValue.split(",");

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
				const skin = skeleton.data.findSkin(s);
				if (!skin) return;
				customSkin.addSkin(skin);
			}
			skeleton.setSkin(customSkin);
		}

		this.skins = skins;
		skeleton.setupPose();
		this.appliedSkin = propValue;
	}

	private async loadAtlas () {
		if (!this.renderer) return;
		this.checkAtlasTexturesValidity();
		if (this.textureAtlas || this.atlasLoad) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_ATLAS) as number;
		if (propValue === -1) return;

		const load = this.assetLoader.loadAtlasEditor(propValue, this._inst, this.renderer);
		this.atlasLoad = load;
		const result = await load.catch((error: Error) => {
			if (this.atlasLoad === load) {
				this.errorTextureAtlas = error.message;
				this.layoutView?.Refresh();
			}
		});
		if (!result) return;
		if (this.atlasLoad !== load) {
			result.textureAtlas.dispose();
			return;
		}

		this.errorTextureAtlas = undefined;
		this.textureAtlas = result.textureAtlas;
		this.textureAtlasBasePath = result.basePath;
		this.layoutView?.Refresh();
	}

	private async loadSkeleton () {
		const textureAtlas = this.textureAtlas;
		if (!this.renderer || !textureAtlas || this.skeleton || this.skeletonLoad) return;

		const propValue = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON) as number;
		if (propValue === -1) return;

		const loaderScale = this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_LOADER_SCALE) as number;
		const load = this.assetLoader.loadSkeletonEditor(propValue, textureAtlas, loaderScale, this._inst);
		this.skeletonLoad = load;
		const skeletonData = await load.catch((error: Error) => {
			if (this.skeletonLoad === load) {
				this.errorSkeleton = `${error.message}\n. Likely Atlas and Skeleton are not corresponding.`;
				this.layoutView?.Refresh();
			}
		});
		if (!skeletonData || this.skeletonLoad !== load || this.textureAtlas !== textureAtlas) return;

		this.errorSkeleton = undefined;
		this.skeleton = new spine.Skeleton(skeletonData);
		const animationStateData = new spine.AnimationStateData(skeletonData);
		this.state = new spine.AnimationState(animationStateData);
		this.appliedSkin = undefined;

		this.setSkin();
		this.setAnimation();
		this.update(0);

		this.layoutView?.Refresh();
	}

	private capturePositionModeState () {
		this.positionModePrevX = this._inst.GetX();
		this.positionModePrevY = this._inst.GetY();
		this.positionModePrevAngle = this._inst.GetAngle();
		this.positionModePrevWidth = this._inst.GetWidth();
		this.positionModePrevHeight = this._inst.GetHeight();
		this.positionModePrevScaleX = this.propScaleX;
		this.positionModePrevScaleY = this.propScaleY;
		this.positionModePrevOffsetX = this.propOffsetX;
		this.positionModePrevOffsetY = this.propOffsetY;
		this.positionModePrevOffsetAngle = this.propOffsetAngle;
	}

	private applyPositionModeOffsetPropertyChange (id: string) {
		if (id === PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) {
			this._inst.SetX(this._inst.GetX() + this.positionModePrevOffsetX - this.propOffsetX);
		} else if (id === PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) {
			this._inst.SetY(this._inst.GetY() + this.positionModePrevOffsetY - this.propOffsetY);
		} else {
			this._inst.SetAngle(this._inst.GetAngle() + this.positionModePrevOffsetAngle - this.propOffsetAngle);
		}

		this.capturePositionModeState();
	}

	private applyPositionModeScalePropertyChange (id: string) {
		const scaleXChanged = id === PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_X;
		const oldScale = scaleXChanged ? this.positionModePrevScaleX : this.positionModePrevScaleY;
		const newScale = scaleXChanged ? this.propScaleX : this.propScaleY;
		if (oldScale === newScale || newScale === 0) return;

		const width = this._inst.GetWidth();
		const height = this._inst.GetHeight();
		if (scaleXChanged) {
			this._inst.SetSize(width * oldScale / newScale, height);
		} else {
			this._inst.SetSize(width, height * oldScale / newScale);
		}

		this.capturePositionModeState();
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
		const { _inst } = this;

		if (!this.skeleton || !this.textureAtlas) {
			_inst.SetSize(200, 200);
			this.spineBounds.width = 200;
			this.spineBounds.height = 200;
			this.propOffsetX = 0;
			this.propOffsetY = 0;
			this.propOffsetAngle = 0;
			this.propScaleX = 1;
			this.propScaleY = 1;
			return;
		}

		const { width: oldBoundsWidth, height: oldBoundsHeight } = this.spineBounds;
		this.setBoundsFromBoundsProvider();
		if (this.getErrorsString()) {
			this.spineBoundsInit = false;
			return;
		}

		this.spineBoundsInit = true;

		let { x, y, width, height } = this.spineBounds;
		_inst.SetOrigin(-x / width, -y / height);
		if (keepScale && oldBoundsWidth !== Infinity && oldBoundsHeight !== Infinity && oldBoundsWidth !== 0 && oldBoundsHeight !== 0) {
			width *= (_inst.GetWidth() / oldBoundsWidth) * this.propScaleX;
			height *= (_inst.GetHeight() / oldBoundsHeight) * this.propScaleY;
		}

		_inst.SetSize(width, height);
		_inst.SetXY(_inst.GetX() + this.propOffsetX, _inst.GetY() + this.propOffsetY);
		_inst.SetAngle(_inst.GetAngle() + this.propOffsetAngle);

		this.propOffsetX = 0;
		this.propOffsetY = 0;
		this.propOffsetAngle = 0;
		this.propScaleX = 1;
		this.propScaleY = 1;
	}

	private initBounds () {
		if (this.spineBoundsInit || !this.skeleton) return;

		const matchesOldBounds = this._inst.GetWidth() === this.spineBounds.width && this._inst.GetHeight() === this.spineBounds.height;

		this.setBoundsFromBoundsProvider();

		const { x, y, width, height } = this.spineBounds;
		if (width === 0 || height === 0) {
			this.spineBoundsInit = true;
			return;
		}
		this._inst.SetOrigin(-x / width, -y / height);

		if (matchesOldBounds) this._inst.SetSize(width, height);

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

		if (animation && !skeleton?.data.findAnimation(animation))
			errors.push("Not existing animation");

		if (skins.length > 0) {
			const missingSkins = skins.filter(skin => !skeleton?.data.findSkin(skin)).join(", ");
			if (missingSkins)
				errors.push(`Not existing skin(s): ${missingSkins}`);
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
		errorTextC3.SetFontSize(12);
		errorTextC3.SetColorRgb(1, 0, 0);
		errorTextC3.SetTextureUpdateCallback(() => iLayoutView.Refresh());
		return errorTextC3;
	}

	private update (delta: number) {
		const { state, skeleton } = this;

		if (!skeleton || !state) return;

		state.update(delta);
		skeleton.update(delta);
		state.apply(skeleton);

		const actualScaleX = (this._inst.GetWidth() / this.spineBounds.width) * this.propScaleX;
		const actualScaleY = (this._inst.GetHeight() / this.spineBounds.height) * this.propScaleY;

		this.matrix.update(
			this._inst.GetX() + this.propOffsetX,
			this._inst.GetY() + this.propOffsetY,
			this._inst.GetTotalZ(),
			this._inst.GetAngle(),
			actualScaleX,
			actualScaleY,
			this._inst.GetQuaternion(),
			this.propOffsetAngle);
		skeleton.updateWorldTransform(spine.Physics.update);
	}

	private get propScaleX () {
		return this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_X) as number;
	}

	private set propScaleX (value: number) {
		this.suppressPositionModePropertyChange = true;
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_X, value);
		this.suppressPositionModePropertyChange = false;
	}

	private get propScaleY () {
		return this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_Y) as number;
	}

	private set propScaleY (value: number) {
		this.suppressPositionModePropertyChange = true;
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKELETON_OFFSET_SCALE_Y, value);
		this.suppressPositionModePropertyChange = false;
	}

	private get propOffsetX () {
		return this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X) as number;
	}

	private set propOffsetX (value: number) {
		this.suppressPositionModePropertyChange = true;
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_X, value);
		this.suppressPositionModePropertyChange = false;
	}

	private get propOffsetY () {
		return this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y) as number;
	}

	private set propOffsetY (value: number) {
		this.suppressPositionModePropertyChange = true;
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_Y, value);
		this.suppressPositionModePropertyChange = false;
	}

	private get propOffsetAngle () {
		return this._inst.GetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE) as number;
	}

	private set propOffsetAngle (value: number) {
		this.suppressPositionModePropertyChange = true;
		this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_BOUNDS_OFFSET_ANGLE, value);
		this.suppressPositionModePropertyChange = false;
	}

	private async managePropCollision (value: boolean) {
		const project = this._inst.GetProject();
		const objectType = this._inst.GetObjectType();
		const objectTypeName = objectType.GetName();
		const collisionSpriteName = `${objectTypeName}_CollisionBody`;
		const spriteType = project.GetObjectTypeByName(collisionSpriteName);

		const type = PLUGIN_CLASS.PROP_ENABLE_COLLISION;
		if (!value) {
			for (const inst of objectType.GetAllInstances()) {
				if (inst !== this._inst && inst.GetPropertyValue(PLUGIN_CLASS.PROP_ENABLE_COLLISION) as boolean) return;
			}
			if (spriteType) {
				const action = await spine.showModal({
					darkMode: false,
					title: this.lang(`${type}.title`),
					text: this.lang(`${type}.message`, [collisionSpriteName]),
					buttons: [
						{
							text: this.lang(`${type}.buttons.${0}`),
							value: 'disable'
						},
						{
							text: this.lang(`${type}.buttons.${1}`, [collisionSpriteName]),
							value: 'delete'
						}
					]
				});

				switch (action) {
					case 'disable': break;
					case 'delete': spriteType.Delete(); break;
					default: this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_ENABLE_COLLISION, true); break;
				}
			}
			return;
		}

		if (!spriteType) await project.CreateObjectType("Sprite", collisionSpriteName);
	}

	public async selectAnimation () {
		if (!this.skeleton) {
			await spine.showAlertModal({
				darkMode: false,
				title: this.lang('skeleton-not-loaded.title'),
				message: this.lang('skeleton-not-loaded.message'),
			});
			return;
		}

		const animations = this.skeleton.data.animations.map(anim => anim.name);
		if (animations.length === 0) {
			await spine.showAlertModal({
				darkMode: false,
				title: this.lang('no-animations.title'),
				message: this.lang('no-animations.message'),
			});
			return;
		}

		const selectedAnimation = await spine.showListSelectionModal({
			darkMode: false,
			title: this.lang('select-animation.title'),
			items: animations,
		});

		if (selectedAnimation) {
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_ANIMATION, selectedAnimation);
		}
	}

	public async selectSkin () {
		if (!this.skeleton) {
			await spine.showAlertModal({
				darkMode: false,
				title: this.lang('skeleton-not-loaded.title'),
				message: this.lang('skeleton-not-loaded.message'),
			});
			return;
		}

		const skins = this.skeleton.data.skins.map(skin => skin.name).filter(s => s !== "default");
		if (skins.length === 0) {
			await spine.showAlertModal({
				darkMode: false,
				title: this.lang('no-skins.title'),
				message: this.lang('no-skins.message'),
			});
			return;
		}

		const selectedSkins = await spine.showMultiListSelectionModal({
			darkMode: false,
			title: this.lang('select-skins.title'),
			items: skins,
			selectedItems: this.skins,
		});

		if (selectedSkins !== undefined) {
			this._inst.SetPropertyValue(PLUGIN_CLASS.PROP_SKIN, selectedSkins.join(","));
		}
	}

	private lang (stringKey: string, interpolate: (string | number)[] = []): string {
		const pluginContext = "plugins.esotericsoftware_spineconstruct3.custom_ui.";
		let intlString = globalThis.lang(`${pluginContext}${stringKey}`);
		interpolate.forEach((toInterpolate, index) => {
			intlString = intlString.replace(`{${index}}`, `${toInterpolate}`);
		});
		return intlString;
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
