/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.3
 * 
 * Copyright (c) 2013-2015, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use, install, execute and perform the Spine
 * Runtimes Software (the "Software") and derivative works solely for personal
 * or internal use. Without the written permission of Esoteric Software (see
 * Section 2 of the Spine Software License Agreement), you may not (a) modify,
 * translate, adapt or otherwise create derivative works, improvements of the
 * Software or develop new applications using the Software or (b) remove,
 * delete, alter or obscure any trademarks or any copyright, trademark, patent
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package spine.starling {
import flash.display3D.Context3D;
import flash.display3D.Context3DProgramType;
import flash.display3D.Context3DTextureFormat;
import flash.display3D.Context3DVertexBufferFormat;
import flash.display3D.IndexBuffer3D;
import flash.display3D.Program3D;
import flash.display3D.VertexBuffer3D;
import flash.events.Event;
import flash.geom.Matrix;
import flash.geom.Point;
import flash.utils.Dictionary;

import spine.BlendMode;
import spine.flash.SkeletonSprite;

import starling.core.RenderSupport;
import starling.core.Starling;
import starling.display.BlendMode;
import starling.textures.Texture;
import starling.textures.TextureSmoothing;
import starling.utils.MatrixUtil;
import starling.utils.VertexData;

internal class PolygonBatch {
	static private var _tempPoint:Point = new Point();
	static private var _renderAlpha:Vector.<Number> = new <Number>[1.0, 1.0, 1.0, 1.0];
	static private var _programNameCache:Dictionary = new Dictionary();

	private var _capacity:int;
	public var maxCapacity:int = 2000;
	public var smoothing:String = TextureSmoothing.BILINEAR;

	private var _texture:Texture;
	private var _support:RenderSupport;
	private var _programBits:uint;
	private var _blendModeNormal:String;
	private var _blendMode:spine.BlendMode;
	private var _alpha:Number;

	private var _verticesCount:int;
	private var _vertices:Vector.<Number> = new <Number>[];
	private var _verticesBuffer:VertexBuffer3D;

	private var _trianglesCount:int;
	private var _triangles:Vector.<uint> = new <uint>[];
	private var _trianglesBuffer:IndexBuffer3D;

	public function PolygonBatch () {
		resize(32);
		Starling.current.stage3D.addEventListener(Event.CONTEXT3D_CREATE, onContextCreated, false, 0, true);
	}

	public function dispose () : void {
		Starling.current.stage3D.removeEventListener(Event.CONTEXT3D_CREATE, onContextCreated);
		if (_verticesBuffer) _verticesBuffer.dispose();
		if (_trianglesBuffer) _trianglesBuffer.dispose();
	}

	public function begin (support:RenderSupport, alpha:Number, blendMode:String) : void {
		_support = support;
		_alpha = alpha;
		_programBits = 0xffffffff;
		_blendMode = null;

		support.finishQuadBatch();
		support.blendMode = blendMode;
		_blendModeNormal = support.blendMode;

		var context:Context3D = Starling.context;
		context.setProgramConstantsFromMatrix(Context3DProgramType.VERTEX, 1, support.mvpMatrix3D, true);

		var verticesBuffer:VertexBuffer3D = _verticesBuffer;
		if (verticesBuffer) {
			context.setVertexBufferAt(0, verticesBuffer, VertexData.POSITION_OFFSET, Context3DVertexBufferFormat.FLOAT_2); 
			context.setVertexBufferAt(1, verticesBuffer, VertexData.COLOR_OFFSET, Context3DVertexBufferFormat.FLOAT_4);		
			context.setVertexBufferAt(2, verticesBuffer, VertexData.TEXCOORD_OFFSET, Context3DVertexBufferFormat.FLOAT_2);
		}
	}

	public function end () : void {
		flush();
		var context:Context3D = Starling.context;
		context.setTextureAt(0, null);
		context.setVertexBufferAt(2, null);
		context.setVertexBufferAt(1, null);
		context.setVertexBufferAt(0, null);
	}

	public function add (texture:Texture, vertices:Vector.<Number>, vl:int, uvs:Vector.<Number>, triangles:Vector.<uint>,
						 r:Number, g:Number, b:Number, a:Number, blendMode:spine.BlendMode, matrix:Matrix) : void {
		if (blendMode != _blendMode) {
			_blendMode = blendMode;
			flush();
			if (blendMode == spine.BlendMode.normal)
				_support.blendMode = _blendModeNormal;
			else
				_support.blendMode = spine.starling.SkeletonSprite.blendModes[blendMode.ordinal];
			_support.applyBlendMode(true);
		}

		if (!_texture || texture.base != _texture.base) {
			flush();
			_texture = texture;
		}

		var tl:int = triangles.length;
		var vc:int = _verticesCount, tc:int = _trianglesCount;
		var firstVertex:int = vc >> 3;
		if (firstVertex + (vl >> 1) > _capacity) resize(firstVertex + (vl >> 1) - _capacity);
		if (tc + tl > _triangles.length) resize((tc + tl - _triangles.length) / 3);

		var i:int, t:Vector.<uint> = _triangles;
		for (i = 0; i < tl; i += 3, tc += 3) {
			t[tc] = firstVertex + triangles[i];
			t[int(tc + 1)] = firstVertex + triangles[int(i + 1)];
			t[int(tc + 2)] = firstVertex + triangles[int(i + 2)];
		}
		_trianglesCount = tc;

		var v:Vector.<Number> = _vertices;
		if (matrix) {
			var point:Point = _tempPoint;
			for (i = 0; i < vl; i += 2, vc += 8) {
				MatrixUtil.transformCoords(matrix, vertices[i], vertices[int(i + 1)], point);
				v[vc] = point.x;
				v[int(vc + 1)] = point.y;
				v[int(vc + 2)] = r;
				v[int(vc + 3)] = g;
				v[int(vc + 4)] = b;
				v[int(vc + 5)] = a;
				v[int(vc + 6)] = uvs[i];
				v[int(vc + 7)] = uvs[int(i + 1)];
			}
		} else {
			for (i = 0; i < vl; i += 2, vc += 8) {
				v[vc] = vertices[i];
				v[int(vc + 1)] = vertices[int(i + 1)];
				v[int(vc + 2)] = r;
				v[int(vc + 3)] = g;
				v[int(vc + 4)] = b;
				v[int(vc + 5)] = a;
				v[int(vc + 6)] = uvs[i];
				v[int(vc + 7)] = uvs[int(i + 1)];
			}
		}
		_verticesCount = vc;
	}

	private function resize (additional:int) : void {
		var newCapacity:int = _capacity + additional;
		if (newCapacity > maxCapacity) {
			flush();
			newCapacity = additional;
			if (newCapacity < _capacity) return;
			if (newCapacity > maxCapacity) throw new ArgumentError("Too many vertices: " + newCapacity + " > " + maxCapacity);
		}
		_capacity = newCapacity;		
		_vertices.length = newCapacity << 3;
		_triangles.length = newCapacity * 3;
		_verticesBuffer = null;
		_trianglesBuffer = null;	
	}

	public function flush () : void {
		if (!_verticesCount) return;

		var context:Context3D = Starling.context;

		if (!_verticesBuffer) {
			_verticesBuffer = context.createVertexBuffer(_capacity, 8);
			var count:int = _verticesCount >> 3;
			_verticesBuffer.uploadFromVector(_vertices, 0, count);
			var verticesTemp:Vector.<Number> = new <Number>[]; // Buffer must be filled completely once.
			verticesTemp.length = (_capacity << 3) - _verticesCount;
			_verticesBuffer.uploadFromVector(verticesTemp, count, _capacity - count);
			verticesTemp = null;

			_trianglesBuffer = context.createIndexBuffer(_capacity * 3);
			_trianglesBuffer.uploadFromVector(_triangles, 0, _trianglesCount);
			var trianglesTemp:Vector.<uint> = new <uint>[]; // Buffer must be filled completely once.
			trianglesTemp.length = _capacity * 3 - _trianglesCount;
			_trianglesBuffer.uploadFromVector(trianglesTemp, _trianglesCount, trianglesTemp.length);
			trianglesTemp = null;

			context.setVertexBufferAt(0, _verticesBuffer, VertexData.POSITION_OFFSET, Context3DVertexBufferFormat.FLOAT_2); 
			context.setVertexBufferAt(1, _verticesBuffer, VertexData.COLOR_OFFSET, Context3DVertexBufferFormat.FLOAT_4);		
			context.setVertexBufferAt(2, _verticesBuffer, VertexData.TEXCOORD_OFFSET, Context3DVertexBufferFormat.FLOAT_2);
		} else {
			_verticesBuffer.uploadFromVector(_vertices, 0, _verticesCount >> 3);
			_trianglesBuffer.uploadFromVector(_triangles, 0, _trianglesCount);
		}

		var pma:Boolean = _texture ? _texture.premultipliedAlpha : true;
		_renderAlpha[0] = _renderAlpha[1] = _renderAlpha[2] = pma ? _alpha : 1.0;
		_renderAlpha[3] = _alpha;

		_support.applyBlendMode(pma);
		context.setProgramConstantsFromVector(Context3DProgramType.VERTEX, 0, _renderAlpha, 1);

		setProgram(context);
		context.setTextureAt(0, _texture.base);
		context.drawTriangles(_trianglesBuffer, 0, _trianglesCount / 3);

		_verticesCount = 0;
		_trianglesCount = 0;

		_support.raiseDrawCount();
	}

	private function onContextCreated (event:Event) : void {
		_verticesBuffer = null;
		_trianglesBuffer = null;
	}

	private function setProgram (context:Context3D) : void {
		var bits:uint = 0;
		var texture:Texture = _texture;
		if (texture.mipMapping) bits |= 1 << 1;
		if (texture.repeat) bits |= 1 << 2;
		if (smoothing != TextureSmoothing.BILINEAR) bits |= 1 << (smoothing == TextureSmoothing.TRILINEAR ? 3 : 4);
		if (texture.format != Context3DTextureFormat.BGRA) bits |= 1 << (texture.format == "compressedAlpha" ? 5 : 6);
		if (bits == _programBits) return;
		_programBits = bits;

		var name:String = _programNameCache[bits];
		if (name == null) {
			name = "PB_i." + bits.toString(16);
			_programNameCache[bits] = name;
		}

		var program:Program3D = Starling.current.getProgram(name);
		if (!program) {
			// va0 -> position
			// va1 -> color
			// va2 -> texCoords
			// vc0 -> alpha
			// vc1 -> mvpMatrix
			// fs0 -> texture
			var vertexShader:String = 
				"m44 op, va0, vc1 \n" + // 4x4 matrix transform to output clipspace
				"mul v0, va1, vc0 \n" + // multiply alpha (vc0) with color (va1)
				"mov v1, va2 \n"; // pass texture coordinates to fragment program
			var flags:String = RenderSupport.getTextureLookupFlags(texture.format, texture.mipMapping, texture.repeat, smoothing);
			var fragmentShader:String = 
				"tex ft1, v1, fs0 " + flags + " \n" + // sample texture 0
				"mul oc, ft1, v0 \n"; // multiply color with texel color
			program = Starling.current.registerProgramFromSource(name, vertexShader, fragmentShader);
		}
		context.setProgram(program);
	}
}
}
