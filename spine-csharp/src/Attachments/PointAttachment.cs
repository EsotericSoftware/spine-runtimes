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

namespace Spine {
	/// <summary>
	/// An attachment which is a single point and a rotation. This can be used to spawn projectiles, particles, etc. A bone can be
	/// used in similar ways, but a PointAttachment is slightly less expensive to compute and can be hidden, shown, and placed in a
	/// skin.
	/// <p>
	/// See <a href="http://esotericsoftware.com/spine-point-attachments">Point Attachments</a> in the Spine User Guide.
	/// </summary>
	public class PointAttachment : Attachment {
		internal float x, y, rotation;
		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Rotation { get { return rotation; } set { rotation = value; } }

		public PointAttachment (string name)
			: base(name) {
		}

		public void ComputeWorldPosition (Bone bone, out float ox, out float oy) {
			bone.LocalToWorld(this.x, this.y, out ox, out oy);
		}

		public float ComputeWorldRotation (Bone bone) {
			float cos = MathUtils.CosDeg(rotation), sin = MathUtils.SinDeg(rotation);
			float ix = cos * bone.a + sin * bone.b;
			float iy = cos * bone.c + sin * bone.d;
			return MathUtils.Atan2(iy, ix) * MathUtils.RadDeg;
		}
	}
}

