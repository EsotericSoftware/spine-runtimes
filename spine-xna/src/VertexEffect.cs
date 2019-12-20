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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spine {
	public interface IVertexEffect {
		void Begin(Skeleton skeleton);
		void Transform(ref VertexPositionColorTextureColor vertex);
		void End();
	}

	public class JitterEffect : IVertexEffect {
		public float JitterX { get; set; }
		public float JitterY { get; set; }

		public JitterEffect(float jitterX, float jitterY) {
			JitterX = jitterX;
			JitterY = jitterY;
		}

		public void Begin(Skeleton skeleton) {
		}

		public void End() {
		}

		public void Transform(ref VertexPositionColorTextureColor vertex) {
			vertex.Position.X += MathUtils.RandomTriangle(-JitterX, JitterY);
			vertex.Position.Y += MathUtils.RandomTriangle(-JitterX, JitterY);
		}
	}

	public class SwirlEffect : IVertexEffect {
		private float worldX, worldY, angle;

		public float Radius { get; set; }
		public float Angle { get { return angle; } set { angle = value * MathUtils.DegRad; } }
		public float CenterX { get; set; }
		public float CenterY { get; set; }
		public IInterpolation Interpolation { get; set; }

		public SwirlEffect(float radius) {
			Radius = radius;
			Interpolation = IInterpolation.Pow2;
		}

		public void Begin(Skeleton skeleton) {
			worldX = skeleton.X + CenterX;
			worldY = skeleton.Y + CenterY;
		}

		public void End() {
		}

		public void Transform(ref VertexPositionColorTextureColor vertex) {
			float x = vertex.Position.X - worldX;
			float y = vertex.Position.Y - worldY;
			float dist = (float)Math.Sqrt(x * x + y * y);
			if (dist < Radius) {
				float theta = Interpolation.Apply(0, angle, (Radius - dist) / Radius);
				float cos = MathUtils.Cos(theta), sin = MathUtils.Sin(theta);
				vertex.Position.X = cos * x - sin * y + worldX;
				vertex.Position.Y = sin * x + cos * y + worldY;
			}
		}
	}
}
