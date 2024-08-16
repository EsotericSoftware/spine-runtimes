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

export class Input {
	element: HTMLElement;
	mouseX = 0;
	mouseY = 0;
	buttonDown = false;
	touch0: Touch | null = null;
	touch1: Touch | null = null;
	initialPinchDistance = 0;
	private listeners = new Array<InputListener>();
	private preventDefault: boolean;

	constructor (element: HTMLElement, preventDefault = true) {
		this.element = element;
		this.preventDefault = preventDefault;
		this.setupCallbacks(element);
	}

	private setupCallbacks (element: HTMLElement) {
		let mouseDown = (ev: UIEvent) => {
			if (ev instanceof MouseEvent) {
				let rect = element.getBoundingClientRect();
				this.mouseX = ev.clientX - rect.left;;
				this.mouseY = ev.clientY - rect.top;
				this.buttonDown = true;
				this.listeners.map((listener) => { if (listener.down) listener.down(this.mouseX, this.mouseY, ev); });

				document.addEventListener("mousemove", mouseMove);
				document.addEventListener("mouseup", mouseUp);
			}
		}

		let mouseMove = (ev: UIEvent) => {
			if (ev instanceof MouseEvent) {
				let rect = element.getBoundingClientRect();
				this.mouseX = ev.clientX - rect.left;
				this.mouseY = ev.clientY - rect.top;

				this.listeners.map((listener) => {
					if (this.buttonDown) {
						if (listener.dragged) listener.dragged(this.mouseX, this.mouseY, ev);
					} else {
						if (listener.moved) listener.moved(this.mouseX, this.mouseY);
					}
				});
			}
		};

		let mouseUp = (ev: UIEvent) => {
			if (ev instanceof MouseEvent) {
				let rect = element.getBoundingClientRect();
				this.mouseX = ev.clientX - rect.left;;
				this.mouseY = ev.clientY - rect.top;
				this.buttonDown = false;
				this.listeners.map((listener) => { if (listener.up) listener.up(this.mouseX, this.mouseY); });

				document.removeEventListener("mousemove", mouseMove);
				document.removeEventListener("mouseup", mouseUp);
			}
		}

		let mouseWheel = (ev: WheelEvent) => {
			if (this.preventDefault) ev.preventDefault();
			let deltaY = ev.deltaY;
			if (ev.deltaMode == WheelEvent.DOM_DELTA_LINE) deltaY *= 8;
			if (ev.deltaMode == WheelEvent.DOM_DELTA_PAGE) deltaY *= 24;
			this.listeners.map((listener) => { if (listener.wheel) listener.wheel(ev.deltaY); });
		};

		element.addEventListener("mousedown", mouseDown, true);
		element.addEventListener("mousemove", mouseMove, true);
		element.addEventListener("mouseup", mouseUp, true);
		element.addEventListener("wheel", mouseWheel, true);


		element.addEventListener("touchstart", (ev: TouchEvent) => {
			if (!this.touch0 || !this.touch1) {
				var touches = ev.changedTouches;
				let nativeTouch = touches.item(0);
				if (!nativeTouch) return;
				let rect = element.getBoundingClientRect();
				let x = nativeTouch.clientX - rect.left;
				let y = nativeTouch.clientY - rect.top;
				let touch = new Touch(nativeTouch.identifier, x, y);
				this.mouseX = x;
				this.mouseY = y;
				this.buttonDown = true;

				if (!this.touch0) {
					this.touch0 = touch;
					this.listeners.map((listener) => { if (listener.down) listener.down(touch.x, touch.y, ev) })
				} else if (!this.touch1) {
					this.touch1 = touch;
					let dx = this.touch1.x - this.touch0.x;
					let dy = this.touch1.x - this.touch0.x;
					this.initialPinchDistance = Math.sqrt(dx * dx + dy * dy);
					this.listeners.map((listener) => { if (listener.zoom) listener.zoom(this.initialPinchDistance, this.initialPinchDistance) });
				}
			}
			if (this.preventDefault) ev.preventDefault();
		}, { passive: this.preventDefault });

		element.addEventListener("touchmove", (ev: TouchEvent) => {
			if (this.touch0) {
				var touches = ev.changedTouches;
				let rect = element.getBoundingClientRect();
				for (var i = 0; i < touches.length; i++) {
					var nativeTouch = touches[i];
					let x = nativeTouch.clientX - rect.left;
					let y = nativeTouch.clientY - rect.top;

					if (this.touch0.identifier === nativeTouch.identifier) {
						this.touch0.x = this.mouseX = x;
						this.touch0.y = this.mouseY = y;
						this.listeners.map((listener) => { if (listener.dragged) listener.dragged(x, y, ev) });
					}
					if (this.touch1 && this.touch1.identifier === nativeTouch.identifier) {
						this.touch1.x = this.mouseX = x;
						this.touch1.y = this.mouseY = y;
					}
				}
				if (this.touch0 && this.touch1) {
					let dx = this.touch1.x - this.touch0.x;
					let dy = this.touch1.x - this.touch0.x;
					let distance = Math.sqrt(dx * dx + dy * dy);
					this.listeners.map((listener) => { if (listener.zoom) listener.zoom(this.initialPinchDistance, distance) });
				}
			}
			if (this.preventDefault) ev.preventDefault();
		}, { passive: this.preventDefault });

		let touchEnd = (ev: TouchEvent) => {
			if (this.touch0) {
				var touches = ev.changedTouches;
				let rect = element.getBoundingClientRect();

				for (var i = 0; i < touches.length; i++) {
					var nativeTouch = touches[i];
					let x = nativeTouch.clientX - rect.left;
					let y = nativeTouch.clientY - rect.top;

					if (this.touch0.identifier === nativeTouch.identifier) {
						this.touch0 = null;
						this.mouseX = x;
						this.mouseY = y;
						this.listeners.map((listener) => { if (listener.up) listener.up(x, y) });

						if (!this.touch1) {
							this.buttonDown = false;
							break;
						} else {
							this.touch0 = this.touch1;
							this.touch1 = null;
							this.mouseX = this.touch0.x;
							this.mouseX = this.touch0.x;
							this.buttonDown = true;
							this.listeners.map((listener) => { if (listener.down) listener.down(this.touch0!.x, this.touch0!.y) });
						}
					}

					if (this.touch1 && this.touch1.identifier) {
						this.touch1 = null;
					}
				}
			}
			if (this.preventDefault) ev.preventDefault();
		};
		element.addEventListener("touchend", touchEnd, false);
		element.addEventListener("touchcancel", touchEnd);
	}

	addListener (listener: InputListener) {
		this.listeners.push(listener);
	}

	removeListener (listener: InputListener) {
		let idx = this.listeners.indexOf(listener);
		if (idx > -1) {
			this.listeners.splice(idx, 1);
		}
	}
}

export class Touch {
	constructor (public identifier: number, public x: number, public y: number) {
	}
}

export interface InputListener {
	down?(x: number, y: number, ev?: MouseEvent | TouchEvent): void;
	up?(x: number, y: number): void;
	moved?(x: number, y: number): void;
	dragged?(x: number, y: number, ev?: MouseEvent | TouchEvent): void;
	wheel?(delta: number): void;
	zoom?(initialDistance: number, distance: number): void;
}
