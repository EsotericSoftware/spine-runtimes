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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spine {
	/// <summary>
	/// Batch drawing of lines and shapes that can be derrived from lines.
	/// 
	/// Call drawing methods in between Begin()/End()
	/// </summary>
	public class ShapeRenderer {
		GraphicsDevice device;		
		List<VertexPositionColor> vertices = new List<VertexPositionColor>();
		Color color = Color.White;
		BasicEffect effect;
		public BasicEffect Effect { get { return effect; } set { effect = value; } }

		public ShapeRenderer(GraphicsDevice device) {
			this.device = device;
			this.effect = new BasicEffect(device);
			effect.World = Matrix.Identity;
			effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
			effect.TextureEnabled = false;
			effect.VertexColorEnabled = true;
		}

		public void SetColor(Color color) {
			this.color = color;
		}

		public void Begin() {
			device.RasterizerState = new RasterizerState();
			device.BlendState = BlendState.AlphaBlend;
		}

		public void Line(float x1, float y1, float x2, float y2) {
			vertices.Add(new VertexPositionColor(new Vector3(x1, y1, 0), color));
			vertices.Add(new VertexPositionColor(new Vector3(x2, y2, 0), color));
		}

		/** Calls {@link #circle(float, float, float, int)} by estimating the number of segments needed for a smooth circle. */
		public void Circle(float x, float y, float radius) {
			Circle(x, y, radius, Math.Max(1, (int)(6 * (float)Math.Pow(radius, 1.0f / 3.0f))));
		}

		/** Draws a circle using {@link ShapeType#Line} or {@link ShapeType#Filled}. */
		public void Circle(float x, float y, float radius, int segments) {
			if (segments <= 0) throw new ArgumentException("segments must be > 0.");			
			float angle = 2 * MathUtils.PI / segments;
			float cos = MathUtils.Cos(angle);
			float sin = MathUtils.Sin(angle);
			float cx = radius, cy = 0;
			float temp = 0;
							
			for (int i = 0; i < segments; i++) {				
				vertices.Add(new VertexPositionColor(new Vector3(x + cx, y + cy, 0), color));
				temp = cx;
				cx = cos * cx - sin * cy;
				cy = sin * temp + cos * cy;				
				vertices.Add(new VertexPositionColor(new Vector3(x + cx, y + cy, 0), color));
			}
			vertices.Add(new VertexPositionColor(new Vector3(x + cx, y + cy, 0), color));

			temp = cx;
			cx = radius;
			cy = 0;
			vertices.Add(new VertexPositionColor(new Vector3(x + cx, y + cy, 0), color));
		}

		public void Triangle(float x1, float y1, float x2, float y2, float x3, float y3) {
			vertices.Add(new VertexPositionColor(new Vector3(x1, y1, 0), color));
			vertices.Add(new VertexPositionColor(new Vector3(x2, y2, 0), color));

			vertices.Add(new VertexPositionColor(new Vector3(x2, y2, 0), color));
			vertices.Add(new VertexPositionColor(new Vector3(x3, y3, 0), color));

			vertices.Add(new VertexPositionColor(new Vector3(x3, y3, 0), color));
			vertices.Add(new VertexPositionColor(new Vector3(x1, y1, 0), color));
		}

		public void X(float x, float y, float len) {
			Line(x + len, y + len, x - len, y - len);
			Line(x - len, y + len, x + len, y - len);
		}

		public void Polygon(float[] polygonVertices, int offset, int count) {
			if (count< 3) throw new ArgumentException("Polygon must contain at least 3 vertices");

			offset <<= 1;
			count <<= 1;

			var firstX = polygonVertices[offset];
			var firstY = polygonVertices[offset + 1];
			var last = offset + count;

			for (int i = offset, n = offset + count - 2; i<n; i += 2) {
				var x1 = polygonVertices[i];
				var y1 = polygonVertices[i + 1];

				var x2 = 0f;
				var y2 = 0f;

				if (i + 2 >= last) {
					x2 = firstX;
					y2 = firstY;
				} else {
					x2 = polygonVertices[i + 2];
					y2 = polygonVertices[i + 3];
				}

				Line(x1, y1, x2, y2);
			}
		}

		public void Rect(float x, float y, float width, float height) {
			Line(x, y, x + width, y);
			Line(x + width, y, x + width, y + height);
			Line(x + width, y + height, x, y + height);
			Line(x, y + height, x, y);
		}

		public void End() {
			if (vertices.Count == 0) return;
			var verticesArray = vertices.ToArray();

			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.LineList, verticesArray, 0, verticesArray.Length / 2);		
			}

			vertices.Clear();
		}
	}
}
