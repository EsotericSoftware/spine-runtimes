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
	export class LoadingScreen {
		static FADE_SECONDS = 1;

		private static loaded = 0;
		private static spinnerImg: HTMLImageElement = null;
		private static logoImg: HTMLImageElement = null;

		private renderer: SceneRenderer;
		private logo: GLTexture = null;
		private spinner: GLTexture = null;
		private angle = 0;
		private fadeOut = 0;
		private timeKeeper = new spine.TimeKeeper();
		backgroundColor = new spine.Color(0.135, 0.135, 0.135, 1);
		private tempColor = new spine.Color();
		private firstDraw = 0;

		private static SPINNER_DATA = "data:image/png;base64,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";

		private static SPINE_LOGO_DATA = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAFIAAAAZCAYAAACis3k0AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAtNJREFUaN7tmT2I1EAUxwN+oWgRT0HFKo0WCkJ6ObmAWFwZbCxsXGysLNJaiCyIoDaSwk4ETzvhmnBaCRbBWoQ01ho4PwotjP8cE337mMy8TLK757mBH3fLTWbe/PbN53neNniqZW8FvAVvQAqugwvgDDgO9niLRyTyJagM/ACPF6bsIl9ZRDac/Cc6tLn5xQdRQ496QlKPLxD5QCDxO9jtGM8QfYoIgUlgCipGCRJL5VvlyOdCU09iEXkCfLSIfCrs7Fab6nOsiafu06iDwES9w/uU1QnDC+ekkVS9vEaDsgVeB0d+z1VDtOGxRaYPboP3Gokb4GgXkZp4chZPJKgvZ3U0XkriK/TIt9YUDllFgTAjGwoaoHqfBhMI58yD4BQ4V6/aHYdfxToftvw9F2SiVroawU2/Cv5C4Thv0KB9S5nxlOd4STxjwUjzSdYlgrYijw2BsEfgsaFcM09lhiys94xXQQwugcvgJrgFLjrEE7WUiTuWCQzt/ZXN7FfqGwuGClyVy2xZAFmfDQvNtwFFSspMDGsD+UTWqu1KoVmVooFEJgKRXw0if85RpISEzwsjzeqWzkjkC4PIJ3MUmQgITAHlQwTFhnZhELkEntfZRwR+AvfAgXmJHOqU02XligWT8ppg67NXbdCXeq7afUQ6L8C2DalEZNt2YyQ94Qy8/ekjMpBMbfyl5iTjG7YAI8cNecROAb4kJmTjaXAF3AGvwQewOiuRxEtlSaT4j2h2lMsUueQEoMlIKpTvAmKhxPMtC876jEX6rE8l8TNx/KVbn6xlWU9NWcSDUsO4NGWpQOTZFpHPOooMXcswmW2XFk3ixb2v0Nq+XVKP00QNaffBLyWwBI/AkTlfMYZDXMf12kc6yjwEjoFdO/5me5oi/6tnyhlZX6OtgmX1c2Uh0k3khmbB2b9TRfpd/jfTUeRDJvHdYg5wE7kPXAN3wQ1weDvH+xufEgpi5qIl3QAAAABJRU5ErkJggg=="

		constructor (renderer: SceneRenderer) {
			this.renderer = renderer;

			this.timeKeeper.maxDelta = 9;

			if (LoadingScreen.logoImg === null) {
				// thank you Apple Inc.
				let isSafari = navigator.userAgent.indexOf("Safari") > -1;

				LoadingScreen.logoImg = new Image();
				LoadingScreen.logoImg.src = LoadingScreen.SPINE_LOGO_DATA;
				if (!isSafari) LoadingScreen.logoImg.crossOrigin = "anonymous";
				LoadingScreen.logoImg.onload = (ev) => {
					LoadingScreen.loaded++;
				}

				LoadingScreen.spinnerImg = new Image();
				LoadingScreen.spinnerImg.src = LoadingScreen.SPINNER_DATA;
				if (!isSafari) LoadingScreen.spinnerImg.crossOrigin = "anonymous";
				LoadingScreen.spinnerImg.onload = (ev) => {
					LoadingScreen.loaded++;
				}
			}
		}

		draw (complete = false) {
			if (complete && this.fadeOut > LoadingScreen.FADE_SECONDS) return;

			this.timeKeeper.update();
			let a = Math.abs(Math.sin(this.timeKeeper.totalTime + 0.75));
			this.angle -= this.timeKeeper.delta * 360 * (1 + 1.5 * Math.pow(a, 5));

			let renderer = this.renderer;
			let canvas = renderer.canvas;
			let gl = renderer.context.gl;

			let oldX = renderer.camera.position.x, oldY = renderer.camera.position.y;
			renderer.camera.position.set(canvas.width / 2, canvas.height / 2, 0);
			renderer.camera.viewportWidth = canvas.width;
			renderer.camera.viewportHeight = canvas.height;
			renderer.resize(ResizeMode.Stretch);

			if (!complete) {
				gl.clearColor(this.backgroundColor.r, this.backgroundColor.g, this.backgroundColor.b, this.backgroundColor.a);
				gl.clear(gl.COLOR_BUFFER_BIT);
				this.tempColor.a = 1;
			} else {
				this.fadeOut += this.timeKeeper.delta * (this.timeKeeper.totalTime < 1 ? 2 : 1);
				if (this.fadeOut > LoadingScreen.FADE_SECONDS) {
					renderer.camera.position.set(oldX, oldY, 0);
					return;
				}
				a = 1 - this.fadeOut / LoadingScreen.FADE_SECONDS;
				this.tempColor.setFromColor(this.backgroundColor);
				this.tempColor.a = 1 - (a - 1) * (a - 1);
				renderer.begin();
				renderer.quad(true, 0, 0, canvas.width, 0, canvas.width, canvas.height, 0, canvas.height,
					this.tempColor, this.tempColor, this.tempColor, this.tempColor);
				renderer.end();
			}
			this.tempColor.set(1, 1, 1, this.tempColor.a);

			if (LoadingScreen.loaded != 2) return;
			if (this.logo === null) {
				this.logo = new GLTexture(renderer.context, LoadingScreen.logoImg);
				this.spinner = new GLTexture(renderer.context, LoadingScreen.spinnerImg);
			}
			this.logo.update(false);
			this.spinner.update(false);

			let logoWidth = this.logo.getImage().width;
			let logoHeight = this.logo.getImage().height;
			let spinnerWidth = this.spinner.getImage().width;
			let spinnerHeight = this.spinner.getImage().height;

			renderer.batcher.setBlendMode(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
			renderer.begin();
			renderer.drawTexture(this.logo, (canvas.width - logoWidth) / 2, (canvas.height - logoHeight) / 2, logoWidth, logoHeight, this.tempColor);
			renderer.drawTextureRotated(this.spinner, (canvas.width - spinnerWidth) / 2, (canvas.height - spinnerHeight) / 2, spinnerWidth, spinnerHeight, spinnerWidth / 2, spinnerHeight / 2, this.angle, this.tempColor);
			renderer.end();

			renderer.camera.position.set(oldX, oldY, 0);
		}
	}
}
