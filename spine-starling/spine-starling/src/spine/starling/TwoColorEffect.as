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

package spine.starling {
	import flash.display3D.Context3DProgramType;
	import starling.rendering.Program;

	import flash.display3D.Context3D;

	import starling.rendering.VertexDataFormat;
	import starling.rendering.MeshEffect;

	public class TwoColorEffect extends MeshEffect {
		public static const VERTEX_FORMAT : VertexDataFormat = TwoColorMeshStyle.VERTEX_FORMAT;
		private static const VECTOR_ONES:Vector.<Number> = Vector.<Number>([1, 1, 1, 1]);

		override protected function createProgram() : Program {
			// v0 -> tex coords
			// v1 -> color plus alpha
			// v2 -> dark color
			var vertexShader : String = [
			"m44 op, va0, vc0", // 4x4 matrix transform to output clip-space
			"mov v0, va1     ", // pass texture coordinates to fragment program
			"mul v1, va2, vc4", // multiply alpha (vc4) with color (va2), pass to fp
			"mov v2, va3     " // pass dark color to fp
			].join("\n");

			var fragmentShader : String = [
			tex("ft0", "v0", 0, texture), // ft0 = texture2d(texCoords)
			"mul ft1, ft0, v1", // ft1 = texColor * light
			"sub ft3.xyz, ft0.www, fc0.xyz", // ft3 = texColor.a - 1
			"sub ft2.xyz, fc0.xyz, ft0.xyz", // ft2.xyz = (1 - texColor.rgb)
			"add ft2.xyz, ft2.xyz, ft3.xyz", // ft2.xyz = ((texColor.a - 1.0) + 1.0 - texColor.rgb)
			"mul ft2.xyz, ft2.xyz, v2.xyz", // ft2.xyz = ((texColor.a - 1.0) + 1.0 - texColor.rgb) * dark.rgb
			"add ft2.xyz, ft2.xyz, ft1.xyz", // ft2.xyz = ((texColor.a - 1.0) + 1.0 - texColor.rgb) * dark.rgb + texColor.rgb * light.rgb
			"mov ft2.w, ft1.w", // ft2.w = alpha
			"mov oc, ft2"
			].join("\n");

			return Program.fromSource(vertexShader, fragmentShader);
		}

		override public function get vertexFormat() : VertexDataFormat {
			return VERTEX_FORMAT;
		}

		override protected function beforeDraw(context : Context3D) : void {
			super.beforeDraw(context);
			vertexFormat.setVertexBufferAt(3, vertexBuffer, "color2");

			// fc0 -> (1, 1, 1, 1)
			context.setProgramConstantsFromVector(Context3DProgramType.FRAGMENT, 0, VECTOR_ONES);
		}

		override protected function afterDraw(context : Context3D) : void {
			context.setVertexBufferAt(3, null);
			super.afterDraw(context);
		}
	}
}
