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
