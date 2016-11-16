/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

module spine.webgl {
	export class Input {
		element: HTMLElement;
		lastX = 0;
		lastY = 0;
		buttonDown = false;
		currTouch: Touch = null;
		touchesPool = new Pool<spine.webgl.Touch>(() => {
			return new spine.webgl.Touch(0, 0, 0);
		});

		private listeners = new Array<InputListener>();
		constructor (element: HTMLElement) {
			this.element = element;
			this.setupCallbacks(element);
		}

		private setupCallbacks(element: HTMLElement) {
			element.addEventListener("mousedown", (ev: UIEvent) => {
				if (ev instanceof MouseEvent) {
					let rect = element.getBoundingClientRect();
					let x = ev.clientX - rect.left;
					let y = ev.clientY - rect.top;

					let listeners = this.listeners;
					for (let i = 0; i < listeners.length; i++) {
						listeners[i].down(x, y);
					}

					this.lastX = x;
					this.lastY = y;
					this.buttonDown = true;
				}
			}, true);
			element.addEventListener("mousemove", (ev: UIEvent) => {
				if (ev instanceof MouseEvent) {
					let rect = element.getBoundingClientRect();
					let x = ev.clientX - rect.left;
					let y = ev.clientY - rect.top;

					let listeners = this.listeners;
					for (let i = 0; i < listeners.length; i++) {
						if (this.buttonDown) {
							listeners[i].dragged(x, y);
						} else {
							listeners[i].moved(x, y);
						}
					}

					this.lastX = x;
					this.lastY = y;
				}
			}, true);
			element.addEventListener("mouseup", (ev: UIEvent) => {
				if (ev instanceof MouseEvent) {
					let rect = element.getBoundingClientRect();
					let x = ev.clientX - rect.left;
					let y = ev.clientY - rect.top;

					let listeners = this.listeners;
					for (let i = 0; i < listeners.length; i++) {
						listeners[i].up(x, y);
					}

					this.lastX = x;
					this.lastY = y;
					this.buttonDown = false;
				}
			}, true);
			element.addEventListener("touchstart", (ev: TouchEvent) => {
				if (this.currTouch != null) return;

				var touches = ev.changedTouches;
				for (var i = 0; i < touches.length; i++) {
					var touch = touches[i];
					let rect = element.getBoundingClientRect();
					let x = touch.clientX - rect.left;
					let y = touch.clientY - rect.top;
					this.currTouch = this.touchesPool.obtain();
					this.currTouch.identifier = touch.identifier;
					this.currTouch.x = x;
					this.currTouch.y = y;
					break;
				}

				let listeners = this.listeners;
				for (let i = 0; i < listeners.length; i++) {
					listeners[i].down(this.currTouch.x, this.currTouch.y);
				}
				console.log("Start " + this.currTouch.x + ", " + this.currTouch.y);
				this.lastX = this.currTouch.x;
				this.lastY = this.currTouch.y;
				this.buttonDown = true;
				ev.preventDefault();
			}, false);
			element.addEventListener("touchend", (ev: TouchEvent) => {
				var touches = ev.changedTouches;
				for (var i = 0; i < touches.length; i++) {
					var touch = touches[i];
					if (this.currTouch.identifier === touch.identifier) {
						let rect = element.getBoundingClientRect();
						let x = this.currTouch.x = touch.clientX - rect.left;
						let y = this.currTouch.y = touch.clientY - rect.top;
						this.touchesPool.free(this.currTouch);
						let listeners = this.listeners;
						for (let i = 0; i < listeners.length; i++) {
							listeners[i].up(x, y);
						}
						console.log("End " + x + ", " + y);
						this.lastX = x;
						this.lastY = y;
						this.buttonDown = false;
						this.currTouch = null;
						break;
					}
				}
				ev.preventDefault();
			}, false);
			element.addEventListener("touchcancel", (ev: TouchEvent) => {
				var touches = ev.changedTouches;
				for (var i = 0; i < touches.length; i++) {
					var touch = touches[i];
					if (this.currTouch.identifier === touch.identifier) {
						let rect = element.getBoundingClientRect();
						let x = this.currTouch.x = touch.clientX - rect.left;
						let y = this.currTouch.y = touch.clientY - rect.top;
						this.touchesPool.free(this.currTouch);
						let listeners = this.listeners;
						for (let i = 0; i < listeners.length; i++) {
							listeners[i].up(x, y);
						}
						console.log("End " + x + ", " + y);
						this.lastX = x;
						this.lastY = y;
						this.buttonDown = false;
						this.currTouch = null;
						break;
					}
				}
				ev.preventDefault();
			}, false);
			element.addEventListener("touchmove", (ev: TouchEvent) => {
				if (this.currTouch == null) return;

				var touches = ev.changedTouches;
				for (var i = 0; i < touches.length; i++) {
					var touch = touches[i];
					if (this.currTouch.identifier === touch.identifier) {
						let rect = element.getBoundingClientRect();
						let x = touch.clientX - rect.left;
						let y = touch.clientY - rect.top;

						let listeners = this.listeners;
						for (let i = 0; i < listeners.length; i++) {
							listeners[i].dragged(x, y);
						}
						console.log("Drag " + x + ", " + y);
						this.lastX = this.currTouch.x = x;
						this.lastY = this.currTouch.y = y;
						break;
					}
				}
				ev.preventDefault();
			}, false);
		}

		addListener(listener: InputListener) {
			this.listeners.push(listener);
		}

		removeListener(listener: InputListener) {
			let idx = this.listeners.indexOf(listener);
			if (idx > -1) {
				this.listeners.splice(idx, 1);
			}
		}
	}

	export class Touch {
		constructor(public identifier: number, public x: number, public y: number) {
		}
	}

	export interface InputListener {
		down(x: number, y: number): void;
		up(x: number, y: number): void;
		moved(x: number, y: number): void;
		dragged(x: number, y: number): void;
	}
}
