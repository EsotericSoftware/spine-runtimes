/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated July 28, 2023. Replaces all prior versions.
 *
 * Copyright (c) 2013-2023, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software or
 * otherwise create derivative works of the Spine Runtimes (collectively,
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
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE
 * SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

import { AssetCache, AssetManager, type Bone, Color, type Disposable, Input, LoadingScreen, ManagedWebGLRenderingContext, Physics, SceneRenderer, type Skeleton, TimeKeeper, Vector2, Vector3 } from "@esotericsoftware/spine-webgl"
import type { SpineWebComponentSkeleton } from "./SpineWebComponentSkeleton.js"
import { type AttributeTypes, castValue, type Point, type Rectangle } from "./wcUtils.js"

interface OverlayAttributes {
	overlayId?: string
	noAutoParentTransform: boolean
	overflowTop: number
	overflowBottom: number
	overflowLeft: number
	overflowRight: number
}

export class SpineWebComponentOverlay extends HTMLElement implements OverlayAttributes, Disposable {
	declare parentElement: HTMLElement;

	private static OVERLAY_ID = "spine-overlay-default-identifier";
	private static OVERLAY_LIST = new Map<string, SpineWebComponentOverlay>();

	/**
	 * @internal
	 */
	static getOrCreateOverlay (overlayId: string | null): SpineWebComponentOverlay {
		const id = overlayId || SpineWebComponentOverlay.OVERLAY_ID;
		let overlay = SpineWebComponentOverlay.OVERLAY_LIST.get(id);
		if (!overlay) {
			overlay = document.createElement('spine-overlay') as SpineWebComponentOverlay;
			overlay.setAttribute('overlay-id', id);
			document.body.appendChild(overlay);
		}
		return overlay;
	}

	/**
	 * If true, enables a top-left span showing FPS (it has black text)
	 */
	public static SHOW_FPS = false;

	/**
	 * A list holding the widgets added to this overlay.
	 */
	public widgets = [] as SpineWebComponentSkeleton[];

	/**
	 * The {@link SceneRenderer} used by this overlay.
	 */
	public renderer: SceneRenderer;

	/**
	 * The {@link AssetManager} used by this overlay.
	 */
	public assetManager: AssetManager;

	/**
	 * The identifier of this overlay. This is necessary when multiply overlay are created.
	   * Connected to `overlay-id` attribute.
	 */
	public overlayId?: string;

	/**
	 * If `false` (default value), the overlay container style will be affected adding `transform: translateZ(0);` to it.
	 * The `transform` is not affected if it already exists on the container.
	 * This is necessary to make the scrolling works with containers that scroll in a different way with respect to the page, as explained in {@link appendedToBody}.
	 * Connected to `no-auto-parent-transform` attribute.
	 */
	public noAutoParentTransform = false;

	/**
	 * The canvas is continuously translated so that it covers the viewport. This translation might be slightly slower during fast scrolling.
	 * If the canvas has the same size as the viewport, while scrolling it might be slighlty misaligned with the viewport.
	 * This parameter defines, as percentage of the viewport height, the pixels to add to the top of the canvas to prevent this effect.
	 * Making the canvas too big might reduce performance.
	 * Default value: 0.2.
	 * Connected to `overflow-top` attribute.
	 */
	public overflowTop = .2;

	/**
	 * The canvas is continuously translated so that it covers the viewport. This translation might be slightly slower during fast scrolling.
	 * If the canvas has the same size as the viewport, while scrolling it might be slighlty misaligned with the viewport.
	 * This parameter defines, as percentage of the viewport height, the pixels to add to the bottom of the canvas to prevent this effect.
	 * Making the canvas too big might reduce performance.
	 * Default value: 0.
	 * Connected to `overflow-bottom` attribute.
	 */
	public overflowBottom = .0;

	/**
	 * The canvas is continuously translated so that it covers the viewport. This translation might be slightly slower during fast scrolling.
	 * If the canvas has the same size as the viewport, while scrolling it might be slighlty misaligned with the viewport.
	 * This parameter defines, as percentage of the viewport width, the pixels to add to the left of the canvas to prevent this effect.
	 * Making the canvas too big might reduce performance.
	 * Default value: 0.
	 * Connected to `overflow-left` attribute.
	 */
	public overflowLeft = .0;

	/**
	 * The canvas is continuously translated so that it covers the viewport. This translation might be slightly slower during fast scrolling.
	 * If the canvas has the same size as the viewport, while scrolling it might be slighlty misaligned with the viewport.
	 * This parameter defines, as percentage of the viewport width, the pixels to add to the right of the canvas to prevent this effect.
	 * Making the canvas too big might reduce performance.
	 * Default value: 0.
	 * Connected to `overflow-right` attribute.
	 */
	public overflowRight = .0;

	private root: ShadowRoot;

	private div: HTMLDivElement;
	private boneFollowersParent: HTMLDivElement;
	private canvas: HTMLCanvasElement;
	private fps: HTMLSpanElement;
	private fpsAppended = false;

	private intersectionObserver?: IntersectionObserver;
	private resizeObserver?: ResizeObserver;
	private input?: Input;

	private overflowLeftSize = 0;
	private overflowTopSize = 0;

	private lastCanvasBaseWidth = 0;
	private lastCanvasBaseHeight = 0;

	private zIndex?: number;

	private disposed = false;
	private loaded = false;
	private running = false;
	private visible = true;

	/**
	 * appendedToBody is assegned in the connectedCallback.
	 * When false, the overlay will have the size of the element container in contrast to the default behaviour where the
	 * overlay has always the size of the viewport.
	 * This is necessary when the overlay is inserted into a container that scroll in a different way with respect to the page.
	 * Otherwise the following problems might occur:
	 * 1) For containers appendedToBody, the widget will be slightly slower to scroll than the html behind. The effect is more evident for lower refresh rate display.
	 * 2) For containers appendedToBody, the widget will overflow the container bounds until the widget html element container is visible
	 * 3) For fixed containers, the widget will scroll in a jerky way
	 *
	 * In order to fix this behaviour, it is necessary to insert a dedicated `spine-overlay` webcomponent as a direct child of the container.
	 * Moreover, it is necessary to perform the following actions:
	 * 1) The appendedToBody container must have a `transform` css attribute. If it hasn't this attribute the `spine-overlay` will add it for you.
	 * If your appendedToBody container has already this css attribute, or if you prefer to add it by yourself (example: `transform: translateZ(0);`), set the `no-auto-parent-transform` to the `spine-overlay`.
	 * 2) The `spine-overlay` must have an `overlay-id` attribute. Choose the value you prefer.
	 * 3) Each `spine-skeleton` must have an `overlay-id` attribute. The same as the hosting `spine-overlay`.
	   * Connected to `appendedToBody` attribute.
	 */
	private appendedToBody = true;
	private hasParentTransform = true;

	readonly time = new TimeKeeper();

	constructor () {
		super();
		this.root = this.attachShadow({ mode: "open" });

		this.div = document.createElement("div");
		this.div.style.position = "absolute";
		this.div.style.top = "0";
		this.div.style.left = "0";
		this.div.style.setProperty("pointer-events", "none");
		this.div.style.overflow = "hidden"
		// this.div.style.backgroundColor = "rgba(0, 255, 0, 0.1)";

		this.root.appendChild(this.div);

		this.canvas = document.createElement("canvas");
		this.boneFollowersParent = document.createElement("div");

		this.div.appendChild(this.canvas);
		this.canvas.style.position = "absolute";
		this.canvas.style.top = "0";
		this.canvas.style.left = "0";

		this.div.appendChild(this.boneFollowersParent);
		this.boneFollowersParent.style.position = "absolute";
		this.boneFollowersParent.style.top = "0";
		this.boneFollowersParent.style.left = "0";
		this.boneFollowersParent.style.whiteSpace = "nowrap";
		this.boneFollowersParent.style.setProperty("pointer-events", "none");
		this.boneFollowersParent.style.transform = `translate(0px,0px)`;

		this.canvas.style.setProperty("pointer-events", "none");
		this.canvas.style.transform = `translate(0px,0px)`;
		// this.canvas.style.setProperty("will-change", "transform"); // performance seems to be even worse with this uncommented

		this.fps = document.createElement("span");
		this.fps.style.position = "fixed";
		this.fps.style.top = "0";
		this.fps.style.left = "0";

		const context = new ManagedWebGLRenderingContext(this.canvas, { alpha: true });
		this.renderer = new SceneRenderer(this.canvas, context);

		this.assetManager = new AssetManager(context);
	}

	connectedCallback (): void {
		this.appendedToBody = this.parentElement === document.body;

		let overlayId = this.getAttribute('overlay-id');
		if (!overlayId) {
			overlayId = SpineWebComponentOverlay.OVERLAY_ID;
			this.setAttribute('overlay-id', overlayId);
		}

		this.assetManager.setCache(AssetCache.getCache(overlayId));

		const existingOverlay = SpineWebComponentOverlay.OVERLAY_LIST.get(overlayId);
		if (existingOverlay && existingOverlay !== this) {
			throw new Error(`"SpineWebComponentOverlay - You cannot have two spine-overlay with the same overlay-id: ${overlayId}"`);
		}
		SpineWebComponentOverlay.OVERLAY_LIST.set(overlayId, this);
		// window.addEventListener("scroll", this.scrolledCallback);

		if (document.readyState !== "complete") {
			window.addEventListener("load", this.loadedCallback);
		} else {
			this.loadedCallback();
		}

		window.screen.orientation.addEventListener('change', this.orientationChangedCallback);

		this.intersectionObserver = new IntersectionObserver((widgets) => {
			for (const elem of widgets) {
				const { target, intersectionRatio } = elem;
				let { isIntersecting } = elem;
				for (const widget of this.widgets) {
					if (widget.getHostElement() !== target) continue;

					// old browsers do not have isIntersecting
					if (isIntersecting === undefined) {
						isIntersecting = intersectionRatio > 0;
					}

					widget.onScreen = isIntersecting;
					if (isIntersecting) {
						widget.onScreenFunction(widget);
						widget.onScreenAtLeastOnce = true;
					}
				}
			}
		}, { rootMargin: "30px 20px 30px 20px" });

		// if the element is not appendedToBody, the user does not disable translate tweak, and the parent did not have already a transform, add the tweak
		if (!this.appendedToBody) {
			if (this.hasCssTweakOff()) {
				this.hasParentTransform = false;
			} else {
				this.parentElement.style.transform = `translateZ(0)`;
			}
		} else {
			window.addEventListener("resize", this.windowResizeCallback);
		}
		this.resizeObserver = new ResizeObserver(() => this.resizedCallback());
		this.resizeObserver.observe(this.parentElement);

		for (const widget of this.widgets) {
			this.intersectionObserver?.observe(widget.getHostElement());
		}
		this.input = this.setupDragUtility();

		document.addEventListener('visibilitychange', this.visibilityChangeCallback);

		this.startRenderingLoop();
	}

	disconnectedCallback (): void {
		const id = this.getAttribute('overlay-id');
		if (id) SpineWebComponentOverlay.OVERLAY_LIST.delete(id);
		// window.removeEventListener("scroll", this.scrolledCallback);
		window.removeEventListener("load", this.loadedCallback);
		window.removeEventListener("resize", this.windowResizeCallback);
		document.removeEventListener('visibilitychange', this.visibilityChangeCallback);
		window.screen.orientation.removeEventListener('change', this.orientationChangedCallback);
		this.intersectionObserver?.disconnect();
		this.resizeObserver?.disconnect();
		this.input?.dispose();
	}

	static attributesDescription: Record<string, { propertyName: keyof OverlayAttributes, type: AttributeTypes, defaultValue?: OverlayAttributes[keyof OverlayAttributes] }> = {
		"overlay-id": { propertyName: "overlayId", type: "string" },
		"no-auto-parent-transform": { propertyName: "noAutoParentTransform", type: "boolean" },
		"overflow-top": { propertyName: "overflowTop", type: "number" },
		"overflow-bottom": { propertyName: "overflowBottom", type: "number" },
		"overflow-left": { propertyName: "overflowLeft", type: "number" },
		"overflow-right": { propertyName: "overflowRight", type: "number" },
	}

	static get observedAttributes (): string[] {
		return Object.keys(SpineWebComponentOverlay.attributesDescription);
	}

	attributeChangedCallback (name: string, oldValue: string | null, newValue: string | null): void {
		const { type, propertyName, defaultValue } = SpineWebComponentOverlay.attributesDescription[name];
		const val = castValue(type, newValue, defaultValue);
		(this[propertyName] as OverlayAttributes[typeof propertyName]) = val as OverlayAttributes[typeof propertyName];
		return;
	}

	private visibilityChangeCallback = () => {
		if (document.hidden) {
			this.visible = false;
		} else {
			this.visible = true;
			this.startRenderingLoop();
		}
	}

	private windowResizeCallback = () => this.resizedCallback(true);

	private resizedCallback = (onlyDiv = false) => {
		this.updateCanvasSize(onlyDiv);
	}

	private orientationChangedCallback = () => {
		this.updateCanvasSize();
		// after an orientation change the scrolling changes, but the scroll event does not fire
		this.scrolledCallback();
	}

	// right now, we scroll the canvas each frame before rendering loop, that makes scrolling on mobile waaay more smoother
	// this is way scroll handler do nothing
	private scrolledCallback = () => {
		// this.translateCanvas();
	}

	private loadedCallback = () => {
		this.updateCanvasSize();
		this.scrolledCallback();
		if (!this.loaded) {
			this.loaded = true;
			this.parentElement.appendChild(this);
		}
	}

	private hasCssTweakOff () {
		return this.noAutoParentTransform && getComputedStyle(this.parentElement).transform === "none";
	}

	/**
	 * Remove the overlay from the DOM, dispose all the contained widgets, and dispose the renderer.
	 */
	dispose (): void {
		for (const widget of [...this.widgets]) widget.dispose();

		this.remove();
		this.widgets.length = 0;
		this.renderer.dispose();
		this.disposed = true;
		this.assetManager.dispose();
	}

	/**
	 * Add the widget to the overlay.
	 * If the widget is after the overlay in the DOM, the overlay is appended after the widget.
	 * @param widget The widget to add to the overlay
	 */
	addWidget (widget: SpineWebComponentSkeleton) {
		this.widgets.push(widget);
		this.intersectionObserver?.observe(widget.getHostElement());
		if (this.loaded) {
			const comparison = this.compareDocumentPosition(widget);
			// DOCUMENT_POSITION_DISCONNECTED is needed when a widget is inside the overlay (due to followBone)
			if ((comparison & Node.DOCUMENT_POSITION_FOLLOWING) && !(comparison & Node.DOCUMENT_POSITION_DISCONNECTED)) {
				this.parentElement.appendChild(this);
			}
		}

		this.updateZIndexIfNecessary(widget);
	}

	/**
	 * Remove the widget from the overlay.
	 * @param widget The widget to remove from the overlay
	 */
	removeWidget (widget: SpineWebComponentSkeleton) {
		const index = this.widgets.findIndex(w => w === widget);
		if (index === -1) return false;

		this.widgets.splice(index, 1);
		this.intersectionObserver?.unobserve(widget.getHostElement());
		return true;
	}

	addSlotFollowerElement (element: HTMLElement) {
		this.boneFollowersParent.appendChild(element);
		this.resizedCallback();
	}

	private tempFollowBoneVector = new Vector3();
	private startRenderingLoop () {
		if (this.running) return;

		const updateWidgets = () => {
			const delta = this.time.delta;
			for (const { skeleton, state, update, onScreen, offScreenUpdateBehaviour, beforeUpdateWorldTransforms, afterUpdateWorldTransforms } of this.widgets) {
				if (!skeleton || !state) continue;
				if (!onScreen && offScreenUpdateBehaviour === "pause") continue;
				if (update) update(delta, skeleton, state)
				else {
					// delta = 0
					state.update(delta);
					skeleton.update(delta);

					if (onScreen || (!onScreen && offScreenUpdateBehaviour === "pose")) {
						state.apply(skeleton);
						beforeUpdateWorldTransforms(delta, skeleton, state);
						skeleton.updateWorldTransform(Physics.update);
						afterUpdateWorldTransforms(delta, skeleton, state);
					}
				}
			}

			// fps top-left span
			if (SpineWebComponentOverlay.SHOW_FPS) {
				if (!this.fpsAppended) {
					this.div.appendChild(this.fps);
					this.fpsAppended = true;
				}
				this.fps.innerText = `${this.time.framesPerSecond.toFixed(2)} fps`;
			} else {
				if (this.fpsAppended) {
					this.div.removeChild(this.fps);
					this.fpsAppended = false;
				}
			}
		};

		const clear = (r: number, g: number, b: number, a: number) => {
			this.renderer.context.gl.clearColor(r, g, b, a);
			this.renderer.context.gl.clear(this.renderer.context.gl.COLOR_BUFFER_BIT);
		}

		const startScissor = (divBounds: Rectangle) => {
			this.renderer.end();
			this.renderer.begin();
			this.renderer.context.gl.enable(this.renderer.context.gl.SCISSOR_TEST);
			this.renderer.context.gl.scissor(
				this.screenToWorldLength(divBounds.x),
				this.canvas.height - this.screenToWorldLength(divBounds.y + divBounds.height),
				this.screenToWorldLength(divBounds.width),
				this.screenToWorldLength(divBounds.height)
			);
		}

		const endScissor = () => {
			this.renderer.end();
			this.renderer.context.gl.disable(this.renderer.context.gl.SCISSOR_TEST);
			this.renderer.begin();
		}

		const renderWidgets = () => {
			clear(0, 0, 0, 0);
			const renderer = this.renderer;
			renderer.begin();

			let ref: DOMRect;
			let offsetLeftForOevrlay = 0;
			let offsetTopForOverlay = 0;
			if (!this.appendedToBody) {
				ref = this.parentElement.getBoundingClientRect();
				const computedStyle = getComputedStyle(this.parentElement);
				offsetLeftForOevrlay = ref.left + parseFloat(computedStyle.borderLeftWidth);
				offsetTopForOverlay = ref.top + parseFloat(computedStyle.borderTopWidth);
			}

			const tempVector = new Vector3();
			for (const widget of this.widgets) {
				const { skeleton, pma, bounds, debug, offsetX, offsetY, dragX, dragY, fit, spinner, loading, clip, drag } = widget;

				if (widget.isOffScreenAndWasMoved()) continue;
				const elementRef = widget.getHostElement();
				const divBounds = elementRef.getBoundingClientRect();
				// need to use left and top, because x and y are not available on older browser
				divBounds.x = divBounds.left + this.overflowLeftSize;
				divBounds.y = divBounds.top + this.overflowTopSize;

				if (!this.appendedToBody) {
					divBounds.x -= offsetLeftForOevrlay;
					divBounds.y -= offsetTopForOverlay;
				}

				const { padLeft, padRight, padTop, padBottom, xAxis, yAxis } = widget
				const paddingShiftHorizontal = (padLeft - padRight) / 2;
				const paddingShiftVertical = (padTop - padBottom) / 2;

				// get the desired point into the the div (center by default) in world coordinate
				const divX = divBounds.x + divBounds.width * ((xAxis + .5) + paddingShiftHorizontal);
				const divY = divBounds.y + divBounds.height * ((-yAxis + .5) + paddingShiftVertical) - 1;
				this.screenToWorld(tempVector, divX, divY);
				let divOriginX = tempVector.x;
				let divOriginY = tempVector.y;

				const paddingShrinkWidth = 1 - (padLeft + padRight);
				const paddingShrinkHeight = 1 - (padTop + padBottom);
				const divWidthWorld = this.screenToWorldLength(divBounds.width * paddingShrinkWidth);
				const divHeightWorld = this.screenToWorldLength(divBounds.height * paddingShrinkHeight);

				if (clip) startScissor(divBounds);

				if (loading) {
					if (spinner) {
						if (!widget.loadingScreen) widget.loadingScreen = new LoadingScreen(renderer);
						widget.loadingScreen?.drawInCoordinates(divOriginX, divOriginY);
					}
					if (clip) endScissor();
					continue;
				}

				if (skeleton) {
					if (fit !== "origin") {
						const { x: ax, y: ay, width: aw, height: ah } = bounds;
						if (aw <= 0 || ah <= 0) continue;

						// scale ratio
						const scaleWidth = divWidthWorld / aw;
						const scaleHeight = divHeightWorld / ah;

						// default value is used for fit = none
						let ratioW = skeleton.scaleX;
						let ratioH = skeleton.scaleY;

						if (fit === "fill") { // Fill the target box by distorting the source's aspect ratio.
							ratioW = scaleWidth;
							ratioH = scaleHeight;
						} else if (fit === "width") {
							ratioW = scaleWidth;
							ratioH = scaleWidth;
						} else if (fit === "height") {
							ratioW = scaleHeight;
							ratioH = scaleHeight;
						} else if (fit === "contain") {
							// if scaled height is bigger than div height, use height ratio instead
							if (ah * scaleWidth > divHeightWorld) {
								ratioW = scaleHeight;
								ratioH = scaleHeight;
							} else {
								ratioW = scaleWidth;
								ratioH = scaleWidth;
							}
						} else if (fit === "cover") {
							if (ah * scaleWidth < divHeightWorld) {
								ratioW = scaleHeight;
								ratioH = scaleHeight;
							} else {
								ratioW = scaleWidth;
								ratioH = scaleWidth;
							}
						} else if (fit === "scaleDown") {
							if (aw > divWidthWorld || ah > divHeightWorld) {
								if (ah * scaleWidth > divHeightWorld) {
									ratioW = scaleHeight;
									ratioH = scaleHeight;
								} else {
									ratioW = scaleWidth;
									ratioH = scaleWidth;
								}
							}
						}

						// get the center of the bounds
						const boundsX = (ax + aw / 2) * ratioW;
						const boundsY = (ay + ah / 2) * ratioH;

						// get vertices offset: calculate the distance between div center and bounds center
						divOriginX = divOriginX - boundsX;
						divOriginY = divOriginY - boundsY;

						// scale the skeleton
						if (fit !== "none" && (skeleton.scaleX !== ratioW || skeleton.scaleY !== ratioH)) {
							skeleton.scaleX = ratioW;
							skeleton.scaleY = ratioH;
							skeleton.updateWorldTransform(Physics.update);
						}
					}

					// const worldOffsetX = divOriginX + offsetX + dragX;
					const worldOffsetX = divOriginX + offsetX * window.devicePixelRatio + dragX;
					const worldOffsetY = divOriginY + offsetY * window.devicePixelRatio + dragY;

					widget.worldX = worldOffsetX;
					widget.worldY = worldOffsetY;

					renderer.drawSkeleton(skeleton, pma, -1, -1, (vertices, size, vertexSize) => {
						for (let i = 0; i < size; i += vertexSize) {
							vertices[i] = vertices[i] + worldOffsetX;
							vertices[i + 1] = vertices[i + 1] + worldOffsetY;
						}
					});

					// drawing debug stuff
					if (debug) {
						// if (true) {
						const { x: ax, y: ay, width: aw, height: ah } = bounds;

						// show bounds and its center
						if (drag) {
							renderer.rect(true,
								ax * skeleton.scaleX + worldOffsetX,
								ay * skeleton.scaleY + worldOffsetY,
								aw * skeleton.scaleX,
								ah * skeleton.scaleY,
								transparentRed);
						}

						renderer.rect(false,
							ax * skeleton.scaleX + worldOffsetX,
							ay * skeleton.scaleY + worldOffsetY,
							aw * skeleton.scaleX,
							ah * skeleton.scaleY,
							blue);
						const bbCenterX = (ax + aw / 2) * skeleton.scaleX + worldOffsetX;
						const bbCenterY = (ay + ah / 2) * skeleton.scaleY + worldOffsetY;
						renderer.circle(true, bbCenterX, bbCenterY, 10, blue);

						// show skeleton root
						const root = skeleton.getRootBone() as Bone;
						renderer.circle(true, root.applied.x + worldOffsetX, root.applied.y + worldOffsetY, 10, red);

						// show shifted origin
						renderer.circle(true, divOriginX, divOriginY, 10, green);

						// show line from origin to bounds center
						renderer.line(divOriginX, divOriginY, bbCenterX, bbCenterY, green);
					}

					if (clip) endScissor();
				}
			}

			renderer.end();
		}

		const updateBoneFollowers = () => {
			for (const widget of this.widgets) {
				if (widget.isOffScreenAndWasMoved() || !widget.skeleton) continue;

				for (const boneFollower of widget.boneFollowerList) {
					const { slot, bone, element, followVisibility, followRotation, followOpacity, followScale } = boneFollower;
					const { worldX, worldY } = widget;
					const applied = bone.applied;
					this.worldToScreen(this.tempFollowBoneVector, applied.worldX + worldX, applied.worldY + worldY);

					if (Number.isNaN(this.tempFollowBoneVector.x)) continue;

					let x = this.tempFollowBoneVector.x - this.overflowLeftSize;
					let y = this.tempFollowBoneVector.y - this.overflowTopSize;

					if (this.appendedToBody) {
						x += window.scrollX;
						y += window.scrollY;
					}

					element.style.transform = `translate(calc(-50% + ${x.toFixed(2)}px),calc(-50% + ${y.toFixed(2)}px))`
						+ (followRotation ? ` rotate(${-applied.getWorldRotationX()}deg)` : "")
						+ (followScale ? ` scale(${applied.getWorldScaleX()}, ${applied.getWorldScaleY()})` : "")
						;

					element.style.display = ""

					const pose = slot.applied;
					if (followVisibility && !pose.attachment) {
						element.style.opacity = "0";
					} else if (followOpacity) {
						element.style.opacity = `${pose.color.a}`;
					}

				}
			}
		}

		const loop = () => {
			if (this.disposed || !this.isConnected || !this.visible) {
				this.running = false;
				return;
			};
			requestAnimationFrame(loop);
			if (!this.loaded) return;
			this.time.update();
			this.translateCanvas();
			updateWidgets();
			renderWidgets();
			updateBoneFollowers();
		}

		requestAnimationFrame(loop);
		this.running = true;

		const red = new Color(1, 0, 0, 1);
		const green = new Color(0, 1, 0, 1);
		const blue = new Color(0, 0, 1, 1);
		const transparentRed = new Color(1, 0, 0, .3);
	}

	public pointerCanvasX = 1;
	public pointerCanvasY = 1;
	public pointerWorldX = 1;
	public pointerWorldY = 1;

	private tempVector = new Vector3();
	private updatePointer (input: Point) {
		this.pointerCanvasX = input.x - window.scrollX;
		this.pointerCanvasY = input.y - window.scrollY;

		if (!this.appendedToBody) {
			const ref = this.parentElement.getBoundingClientRect();
			this.pointerCanvasX -= ref.left;
			this.pointerCanvasY -= ref.top;
		}

		const tempVector = this.tempVector;
		tempVector.set(this.pointerCanvasX, this.pointerCanvasY, 0);
		this.renderer.camera.screenToWorld(tempVector, this.canvas.clientWidth, this.canvas.clientHeight);

		if (Number.isNaN(tempVector.x) || Number.isNaN(tempVector.y)) return;
		this.pointerWorldX = tempVector.x;
		this.pointerWorldY = tempVector.y;
	}

	private updateWidgetPointer (widget: SpineWebComponentSkeleton): boolean {
		if (widget.worldX === Infinity) return false;

		widget.pointerWorldX = this.pointerWorldX - widget.worldX;
		widget.pointerWorldY = this.pointerWorldY - widget.worldY;

		return true;
	}

	private setupDragUtility (): Input {
		// TODO: we should use document - body might have some margin that offset the click events - Meanwhile I take event pageX/Y
		const inputManager = new Input(document.body, false)
		const inputPointTemp: Point = new Vector2();

		const getInput = (ev: MouseEvent | TouchEvent): Point => {
			const originalEvent = ev instanceof MouseEvent ? ev : ev.changedTouches[0];
			inputPointTemp.x = originalEvent.pageX + this.overflowLeftSize;
			inputPointTemp.y = originalEvent.pageY + this.overflowTopSize;
			return inputPointTemp;
		}

		let lastX = 0;
		let lastY = 0;
		inputManager.addListener({
			// moved is used to pass pointer position wrt to canvas and widget position and currently is EXPERIMENTAL
			moved: (x, y, ev: MouseEvent | TouchEvent) => {
				const input = getInput(ev);
				this.updatePointer(input);

				for (const widget of this.widgets) {
					if (!this.updateWidgetPointer(widget) || !widget.onScreen) continue;

					widget.pointerEventUpdate("move", ev);
				}
			},
			down: (x, y, ev: MouseEvent | TouchEvent) => {
				const input = getInput(ev);

				this.updatePointer(input);

				for (const widget of this.widgets) {
					if (!this.updateWidgetPointer(widget) || widget.isOffScreenAndWasMoved()) continue;

					widget.pointerEventUpdate("down", ev);

					if ((widget.interactive && widget.pointerInsideBounds) || (!widget.interactive && widget.isPointerInsideBounds())) {
						if (!widget.drag) continue;

						widget.dragging = true;
						ev?.preventDefault();
					}

				}
				lastX = input.x;
				lastY = input.y;
			},
			dragged: (x, y, ev: MouseEvent | TouchEvent) => {
				const input = getInput(ev);

				const dragX = input.x - lastX;
				const dragY = input.y - lastY;

				this.updatePointer(input);

				for (const widget of this.widgets) {
					if (!this.updateWidgetPointer(widget) || widget.isOffScreenAndWasMoved()) continue;

					widget.pointerEventUpdate("drag", ev);

					if (!widget.dragging) continue;

					const skeleton = widget.skeleton as Skeleton;
					widget.dragX += this.screenToWorldLength(dragX);
					widget.dragY -= this.screenToWorldLength(dragY);
					skeleton.physicsTranslate(dragX, -dragY);
					ev?.preventDefault();
					ev?.stopPropagation();
				}
				lastX = input.x;
				lastY = input.y;
			},
			up: (x, y, ev) => {
				for (const widget of this.widgets) {
					widget.dragging = false;

					if (widget.pointerInsideBounds) {
						widget.pointerEventUpdate("up", ev);
					}
				}
			}
		});

		return inputManager;
	}

	/*
	* Resize/scroll utilities
	*/

	private updateCanvasSize (onlyDiv = false) {
		const { width, height } = this.getViewportSize();

		// if the target width/height changes, resize the canvas.
		if (!onlyDiv && this.lastCanvasBaseWidth !== width || this.lastCanvasBaseHeight !== height) {
			this.lastCanvasBaseWidth = width;
			this.lastCanvasBaseHeight = height;
			this.overflowLeftSize = this.overflowLeft * width;
			this.overflowTopSize = this.overflowTop * height;

			const totalWidth = width * (1 + (this.overflowLeft + this.overflowRight));
			const totalHeight = height * (1 + (this.overflowTop + this.overflowBottom));

			this.canvas.style.width = `${totalWidth}px`;
			this.canvas.style.height = `${totalHeight}px`;
			this.resize(totalWidth, totalHeight);
		}

		// temporarely remove the div to get the page size without considering the div
		// this is necessary otherwise if the bigger element in the page is remove and the div
		// was the second bigger element, now it would be the div to determine the page size
		// this.div?.remove(); is it better width/height to zero?
		// this.div!.style.width = 0 + "px";
		// this.div!.style.height = 0 + "px";
		this.div.style.display = "none";
		if (this.appendedToBody) {
			const { width, height } = this.getPageSize();
			this.div.style.width = `${width}px`;
			this.div.style.height = `${height}px`;
		} else {
			if (this.hasCssTweakOff()) {
				// this case lags if scrolls or position fixed. Users should never use tweak off
				this.div.style.width = `${this.parentElement.clientWidth}px`;
				this.div.style.height = `${this.parentElement.clientHeight}px`;
				this.canvas.style.transform = `translate(${-this.overflowLeftSize}px,${-this.overflowTopSize}px)`;
			} else {
				this.div.style.width = `${this.parentElement.scrollWidth}px`;
				this.div.style.height = `${this.parentElement.scrollHeight}px`;
			}
		}
		this.div.style.display = "";
		// this.root.appendChild(this.div!);
	}

	private resize (width: number, height: number) {
		const canvas = this.canvas;
		canvas.width = Math.round(this.screenToWorldLength(width));
		canvas.height = Math.round(this.screenToWorldLength(height));
		this.renderer.context.gl.viewport(0, 0, canvas.width, canvas.height);
		this.renderer.camera.setViewport(canvas.width, canvas.height);
		this.renderer.camera.update();
	}

	// we need the bounding client rect otherwise decimals won't be returned
	// this means that during zoom it might occurs that the div would be resized
	// rounded 1px more making a scrollbar appear
	private getPageSize () {
		return document.documentElement.getBoundingClientRect();
	}

	private lastViewportWidth = 0;
	private lastViewportHeight = 0;
	private lastDPR = 0;
	private static readonly WIDTH_INCREMENT = 1.15;
	private static readonly HEIGHT_INCREMENT = 1.2;
	private static readonly MAX_CANVAS_WIDTH = 7000;
	private static readonly MAX_CANVAS_HEIGHT = 7000;

	// determine the target viewport width and height.
	// The target width/height won't change if the viewport shrink to avoid useless re render (especially re render bursts on mobile)
	private getViewportSize (): { width: number, height: number } {
		if (!this.appendedToBody) {
			return {
				width: this.parentElement.clientWidth,
				height: this.parentElement.clientHeight,
			}
		}

		const width = window.innerWidth;
		const height = window.innerHeight;

		const dpr = this.getDevicePixelRatio();
		if (dpr !== this.lastDPR) {
			this.lastDPR = dpr;
			this.lastViewportWidth = this.lastViewportWidth === 0 ? width : width * SpineWebComponentOverlay.WIDTH_INCREMENT;
			this.lastViewportHeight = height * SpineWebComponentOverlay.HEIGHT_INCREMENT;

			this.updateWidgetScales();
		} else {
			if (width > this.lastViewportWidth) this.lastViewportWidth = width * SpineWebComponentOverlay.WIDTH_INCREMENT;
			if (height > this.lastViewportHeight) this.lastViewportHeight = height * SpineWebComponentOverlay.HEIGHT_INCREMENT;
		}

		// if the resulting canvas width/height is too high, scale the DPI
		if (this.lastViewportHeight * (1 + this.overflowTop + this.overflowBottom) * dpr > SpineWebComponentOverlay.MAX_CANVAS_HEIGHT ||
			this.lastViewportWidth * (1 + this.overflowLeft + this.overflowRight) * dpr > SpineWebComponentOverlay.MAX_CANVAS_WIDTH) {
			this.dprScale += .5;
			return this.getViewportSize();
		}

		return {
			width: this.lastViewportWidth,
			height: this.lastViewportHeight,
		}
	}

	/**
	 * @internal
	 */
	public getDevicePixelRatio () {
		return window.devicePixelRatio / this.dprScale;
	}
	private dprScale = 1;

	private updateWidgetScales () {
		for (const widget of this.widgets) {
			// inside mode scale automatically to fit the skeleton within its parent
			if (widget.fit !== "origin" && widget.fit !== "none") continue;

			const skeleton = widget.skeleton;
			if (!skeleton) continue;

			// I'm not sure about this. With mode origin and fit none:
			// case 1) If I comment this scale code, the skeleton is never scaled and will be always at the same size and won't change size while zooming
			// case 2) Otherwise, the skeleton is loaded always at the same size, but changes size while zooming
			const scale = this.getDevicePixelRatio();
			skeleton.scaleX = skeleton.scaleX / widget.dprScale * scale;
			skeleton.scaleY = skeleton.scaleY / widget.dprScale * scale;
			widget.dprScale = scale;
		}
	}

	// this function is invoked each frame - pay attention to what you add here
	private translateCanvas () {
		let scrollPositionX = -this.overflowLeftSize;
		let scrollPositionY = -this.overflowTopSize;

		if (this.appendedToBody) {
			scrollPositionX += window.scrollX;
			scrollPositionY += window.scrollY;
		} else {

			// Ideally this should be the only appendedToBody case (no-auto-parent-transform not enabled or at least an ancestor has transform)
			// I'd like to get rid of the else case
			if (this.hasParentTransform) {
				scrollPositionX += this.parentElement.scrollLeft;
				scrollPositionY += this.parentElement.scrollTop;
			} else {
				const { left, top } = this.parentElement.getBoundingClientRect();
				scrollPositionX += left + window.scrollX;
				scrollPositionY += top + window.scrollY;

				let offsetParent = this.offsetParent;
				do {
					if (offsetParent === null || offsetParent === document.body) break;

					const htmlOffsetParentElement = offsetParent as HTMLElement;
					if (htmlOffsetParentElement.style.position === "fixed" || htmlOffsetParentElement.style.position === "sticky" || htmlOffsetParentElement.style.position === "absolute") {
						const parentRect = htmlOffsetParentElement.getBoundingClientRect();
						this.div.style.transform = `translate(${left - parentRect.left}px,${top - parentRect.top}px)`;
						return;
					}

					offsetParent = htmlOffsetParentElement.offsetParent;
				} while (offsetParent);

				this.div.style.transform = `translate(${scrollPositionX + this.overflowLeftSize}px,${scrollPositionY + this.overflowTopSize}px)`;
				return;
			}

		}

		this.canvas.style.transform = `translate(${scrollPositionX}px,${scrollPositionY}px)`;
	}

	private updateZIndexIfNecessary (element: HTMLElement) {
		let parent: HTMLElement | null = element;
		let zIndex: undefined | number;
		do {
			const currentZIndex = parseInt(getComputedStyle(parent).zIndex);

			// searching the shallowest z-index
			if (!Number.isNaN(currentZIndex)) zIndex = currentZIndex;
			parent = parent.parentElement;
		} while (parent && parent !== document.body)

		if (zIndex && (!this.zIndex || this.zIndex < zIndex)) {
			this.zIndex = zIndex;
			this.div.style.zIndex = `${this.zIndex}`;
		}
	}

	/*
	* Other utilities
	*/
	public screenToWorld (vec: Vector3, x: number, y: number) {
		vec.set(x, y, 0);
		// pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
		this.renderer.camera.screenToWorld(vec, this.canvas.clientWidth, this.canvas.clientHeight);
	}
	public worldToScreen (vec: Vector3, x: number, y: number) {
		vec.set(x, -y, 0);
		// pay attention that clientWidth/Height rounds the size - if we don't like it, we should use getBoundingClientRect as in getPagSize
		// this.renderer.camera.worldToScreen(vec, this.canvas.clientWidth, this.canvas.clientHeight);
		this.renderer.camera.worldToScreen(vec, this.worldToScreenLength(this.renderer.camera.viewportWidth), this.worldToScreenLength(this.renderer.camera.viewportHeight));
	}
	public screenToWorldLength (length: number) {
		return length * this.getDevicePixelRatio();
	}
	public worldToScreenLength (length: number) {
		return length / this.getDevicePixelRatio();
	}
}

customElements.define("spine-overlay", SpineWebComponentOverlay);
