/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
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

#if UNITY_5_3_OR_NEWER
#define IS_UNITY
#endif

#if !IS_UNITY
namespace Spine {
	/// <summary>
	/// 32 bit floating point color to be used with XNA/Monogame.
	/// </summary>
	public struct Color32F {
		public float r, g, b, a;

		public Color32F (float r, float g, float b, float a = 1.0f) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color32F (Microsoft.Xna.Framework.Color xnaColor)
			: this(xnaColor.R / 255f, xnaColor.G / 255f, xnaColor.B / 255f, xnaColor.A / 255f) {
		}

		public static implicit operator Color32F (Microsoft.Xna.Framework.Color xnaColor) {
			return new Color32F(xnaColor);
		}

		public static implicit operator Microsoft.Xna.Framework.Color (Color32F c) {
			return new Microsoft.Xna.Framework.Color(
				(byte)(c.r * 255),
				(byte)(c.g * 255),
				(byte)(c.b * 255),
				(byte)(c.a * 255)
			);
		}

		public override string ToString () {
			return string.Format("RGBA({0}, {1}, {2}, {3})", r, g, b, a);
		}

		public static Color32F operator + (Color32F c1, Color32F c2) {
			return new Color32F(c1.r + c2.r, c1.g + c2.g, c1.b + c2.b, c1.a + c2.a);
		}

		public static Color32F operator - (Color32F c1, Color32F c2) {
			return new Color32F(c1.r - c2.r, c1.g - c2.g, c1.b - c2.b, c1.a - c2.a);
		}

		public static Color32F operator * (Color32F c1, Color32F c2) {
			return new Color32F(c1.r * c2.r, c1.g * c2.g, c1.b * c2.b, c1.a * c2.a);
		}
	}

	static class ColorExtensions {
		public static Color32F Clamp (this Color32F color) {
			color.r = MathUtils.Clamp(color.r, 0, 1);
			color.g = MathUtils.Clamp(color.g, 0, 1);
			color.b = MathUtils.Clamp(color.b, 0, 1);
			color.a = MathUtils.Clamp(color.a, 0, 1);
			return color;
		}

		public static Color32F ClampRGB (this Color32F color) {
			color.r = MathUtils.Clamp(color.r, 0, 1);
			color.g = MathUtils.Clamp(color.g, 0, 1);
			color.b = MathUtils.Clamp(color.b, 0, 1);
			return color;
		}

		public static Color32F RGBA8888ToColor (this uint rgba8888) {
			float r = ((rgba8888 & 0xff000000) >> 24) / 255f;
			float g = ((rgba8888 & 0x00ff0000) >> 16) / 255f;
			float b = ((rgba8888 & 0x0000ff00) >> 8) / 255f;
			float a = ((rgba8888 & 0x000000ff)) / 255f;
			return new Color32F(r, g, b, a);
		}

		public static Color32F XRGB888ToColor (this uint xrgb888) {
			float r = ((xrgb888 & 0x00ff0000) >> 16) / 255f;
			float g = ((xrgb888 & 0x0000ff00) >> 8) / 255f;
			float b = ((xrgb888 & 0x000000ff)) / 255f;
			return new Color32F(r, g, b);
		}
	}
}
#endif
