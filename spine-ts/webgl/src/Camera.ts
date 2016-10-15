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
	export class OrthoCamera {
		position = new Vector3(0, 0, 0);
		direction = new Vector3(0, 0, -1);
		up = new Vector3(0, 1, 0);
		near = 0;
		far = 100;
		zoom = 1;
		viewportWidth = 0;
		viewportHeight = 0;
		projectionView = new Matrix4();
		inverseProjectionView = new Matrix4();
		projection = new Matrix4();
		view = new Matrix4();

		private tmp = new Vector3();

		constructor (viewportWidth: number, viewportHeight: number) {
			this.viewportWidth = viewportWidth;
			this.viewportHeight = viewportHeight;
			this.update();
		}

		update () {
			let projection = this.projection;
			let view = this.view;
			let projectionView = this.projectionView;
			let inverseProjectionView = this.inverseProjectionView;
			let zoom = this.zoom, viewportWidth = this.viewportWidth, viewportHeight = this.viewportHeight;
			projection.ortho(zoom * (-viewportWidth / 2), zoom * (viewportWidth / 2),
							 zoom * (-viewportHeight / 2), zoom * (viewportHeight / 2),
							 this.near, this.far);
			view.lookAt(this.position, this.direction, this.up);
			projectionView.set(projection.values);
			projectionView.multiply(view);
			inverseProjectionView.set(projectionView.values).invert();
		}

		screenToWorld (screenCoords: Vector3, screenWidth: number, screenHeight: number) {
			let x = screenCoords.x, y = screenHeight - screenCoords.y - 1;
			let tmp = this.tmp;
			tmp.x = (2 * x) / screenWidth - 1;
			tmp.y = (2 * y) / screenHeight - 1;
			tmp.z = (2 * screenCoords.z) - 1;
			tmp.project(this.inverseProjectionView);
			screenCoords.set(tmp.x, tmp.y, tmp.z);
			return screenCoords;
		}

		setViewport(viewportWidth: number, viewportHeight: number) {
			this.viewportWidth = viewportWidth;
			this.viewportHeight = viewportHeight;
		}
	}
}
