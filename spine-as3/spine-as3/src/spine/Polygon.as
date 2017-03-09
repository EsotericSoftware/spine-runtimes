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

package spine {
	public class Polygon {
		public var vertices : Vector.<Number> = new Vector.<Number>();

		public function Polygon() {
		}

		/** Returns true if the polygon contains the point. */
		public function containsPoint(x : Number, y : Number) : Boolean {
			var nn : int = vertices.length;

			var prevIndex : int = nn - 2;
			var inside : Boolean = false;
			for (var ii : int = 0; ii < nn; ii += 2) {
				var vertexY : Number = vertices[ii + 1];
				var prevY : Number = vertices[prevIndex + 1];
				if ((vertexY < y && prevY >= y) || (prevY < y && vertexY >= y)) {
					var vertexX : Number = vertices[ii];
					if (vertexX + (y - vertexY) / (prevY - vertexY) * (vertices[prevIndex] - vertexX) < x) inside = !inside;
				}
				prevIndex = ii;
			}

			return inside;
		}

		/** Returns true if the polygon contains the line segment. */
		public function intersectsSegment(x1 : Number, y1 : Number, x2 : Number, y2 : Number) : Boolean {
			var nn : int = vertices.length;

			var width12 : Number = x1 - x2, height12 : Number = y1 - y2;
			var det1 : Number = x1 * y2 - y1 * x2;
			var x3 : Number = vertices[nn - 2], y3 : Number = vertices[nn - 1];
			for (var ii : int = 0; ii < nn; ii += 2) {
				var x4 : Number = vertices[ii], y4 : Number = vertices[ii + 1];
				var det2 : Number = x3 * y4 - y3 * x4;
				var width34 : Number = x3 - x4, height34 : Number = y3 - y4;
				var det3 : Number = width12 * height34 - height12 * width34;
				var x : Number = (det1 * width34 - width12 * det2) / det3;
				if (((x >= x3 && x <= x4) || (x >= x4 && x <= x3)) && ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))) {
					var y : Number = (det1 * height34 - height12 * det2) / det3;
					if (((y >= y3 && y <= y4) || (y >= y4 && y <= y3)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))) return true;
				}
				x3 = x4;
				y3 = y4;
			}
			return false;
		}
	}
}