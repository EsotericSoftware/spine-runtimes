/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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
