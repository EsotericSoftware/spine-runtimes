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

package spine.vertexeffects {
	import spine.interpolation.Pow;
	import spine.MathUtils;
	import spine.Interpolation;
	import spine.Skeleton;
	import spine.Vertex;
	import spine.VertexEffect;
	
	public class SwirlEffect implements VertexEffect {
		private var worldX : Number, worldY : Number, _radius : Number = 0, _angle : Number = 0;
		private var _interpolation : Interpolation;
		private var _centerX : Number = 0, _centerY : Number = 0;
		
		public function SwirlEffect(radius : Number) {
			this._interpolation = new Pow(2);;
			this._radius = radius;
		}
		
		public function begin(skeleton : Skeleton) : void {
			worldX = skeleton.x + _centerX;
			worldY = skeleton.y + _centerY;			
		}

		public function transform(vertex : Vertex) : void {
			var x : Number = vertex.x - worldX;
			var y : Number = vertex.y - worldY;
			var dist : Number = Math.sqrt(x * x + y * y);
			if (dist < radius) {
				var theta : Number = interpolation.apply(0, angle, (radius - dist) / radius);
				var cos : Number = Math.cos(theta), sin : Number = Math.sin(theta);
				vertex.x = cos * x - sin * y + worldX;
				vertex.y = sin * x + cos * y + worldY;
			}
		}

		public function end() : void {
		}
		
		public function get radius () : Number { return _radius; }
		public function set radius (radius : Number) : void { _radius = radius; }
		
		public function get angle () : Number { return _angle; }
		public function set angle (angle : Number) : void { _angle = angle * MathUtils.degRad; }
		
		public function get centerX () : Number { return _centerX; }
		public function set centerX (centerX : Number) : void { _centerX = centerX; }
		
		public function get centerY () : Number { return _centerY; }
		public function set centerY (centerY : Number) : void { _centerY = centerY; }
		
		public function get interpolation () : Interpolation { return _interpolation; }
		public function set interpolation (interpolation : Interpolation) : void { _interpolation = interpolation; }
	}
}