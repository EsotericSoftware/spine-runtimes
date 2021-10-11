/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
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

//#define USE_FAST_SIN_COS_ATAN2_APPROXIMATIONS

using System;

namespace Spine {
	public static class MathUtils {
		public const float PI = 3.1415927f;
		public const float PI2 = PI * 2;
		public const float RadDeg = 180f / PI;
		public const float DegRad = PI / 180;

		static Random random = new Random();

#if USE_FAST_SIN_COS_ATAN2_APPROXIMATIONS
		const int SIN_BITS = 14; // 16KB. Adjust for accuracy.
		const int SIN_MASK = ~(-1 << SIN_BITS);
		const int SIN_COUNT = SIN_MASK + 1;
		const float RadFull = PI * 2;
		const float DegFull = 360;
		const float RadToIndex = SIN_COUNT / RadFull;
		const float DegToIndex = SIN_COUNT / DegFull;
		static float[] sin = new float[SIN_COUNT];

		static MathUtils () {
			for (int i = 0; i < SIN_COUNT; i++)
				sin[i] = (float)Math.Sin((i + 0.5f) / SIN_COUNT * RadFull);
			for (int i = 0; i < 360; i += 90)
				sin[(int)(i * DegToIndex) & SIN_MASK] = (float)Math.Sin(i * DegRad);
		}

		/// <summary>Returns the sine of a given angle in radians from a lookup table.</summary>
		static public float Sin (float radians) {
			return sin[(int)(radians * RadToIndex) & SIN_MASK];
		}

		/// <summary>Returns the cosine of a given angle in radians from a lookup table.</summary>
		static public float Cos (float radians) {
			return sin[(int)((radians + PI / 2) * RadToIndex) & SIN_MASK];
		}

		/// <summary>Returns the sine of a given angle in degrees from a lookup table.</summary>
		static public float SinDeg (float degrees) {
			return sin[(int)(degrees * DegToIndex) & SIN_MASK];
		}

		/// <summary>Returns the cosine of a given angle in degrees from a lookup table.</summary>
		static public float CosDeg (float degrees) {
			return sin[(int)((degrees + 90) * DegToIndex) & SIN_MASK];
		}

		/// <summary>Returns atan2 in radians, faster but less accurate than Math.Atan2. Average error of 0.00231 radians (0.1323
		/// degrees), largest error of 0.00488 radians (0.2796 degrees).</summary>
		static public float Atan2 (float y, float x) {
			if (x == 0f) {
				if (y > 0f) return PI / 2;
				if (y == 0f) return 0f;
				return -PI / 2;
			}
			float atan, z = y / x;
			if (Math.Abs(z) < 1f) {
				atan = z / (1f + 0.28f * z * z);
				if (x < 0f) return atan + (y < 0f ? -PI : PI);
				return atan;
			}
			atan = PI / 2 - z / (z * z + 0.28f);
			return y < 0f ? atan - PI : atan;
		}
#else
		/// <summary>Returns the sine of a given angle in radians.</summary>
		static public float Sin (float radians) {
			return (float)Math.Sin(radians);
		}

		/// <summary>Returns the cosine of a given angle in radians.</summary>
		static public float Cos (float radians) {
			return (float)Math.Cos(radians);
		}

		/// <summary>Returns the sine of a given angle in degrees.</summary>
		static public float SinDeg (float degrees) {
			return (float)Math.Sin(degrees * DegRad);
		}

		/// <summary>Returns the cosine of a given angle in degrees.</summary>
		static public float CosDeg (float degrees) {
			return (float)Math.Cos(degrees * DegRad);
		}

		/// <summary>Returns the atan2 using Math.Atan2.</summary>
		static public float Atan2 (float y, float x) {
			return (float)Math.Atan2(y, x);
		}
#endif
		static public float Clamp (float value, float min, float max) {
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}

		static public float RandomTriangle (float min, float max) {
			return RandomTriangle(min, max, (min + max) * 0.5f);
		}

		static public float RandomTriangle (float min, float max, float mode) {
			float u = (float)random.NextDouble();
			float d = max - min;
			if (u <= (mode - min) / d) return min + (float)Math.Sqrt(u * d * (mode - min));
			return max - (float)Math.Sqrt((1 - u) * d * (max - mode));
		}
	}

	public abstract class IInterpolation {
		public static IInterpolation Pow2 = new Pow(2);
		public static IInterpolation Pow2Out = new PowOut(2);

		protected abstract float Apply (float a);

		public float Apply (float start, float end, float a) {
			return start + (end - start) * Apply(a);
		}
	}

	public class Pow : IInterpolation {
		public float Power { get; set; }

		public Pow (float power) {
			Power = power;
		}

		protected override float Apply (float a) {
			if (a <= 0.5f) return (float)Math.Pow(a * 2, Power) / 2;
			return (float)Math.Pow((a - 1) * 2, Power) / (Power % 2 == 0 ? -2 : 2) + 1;
		}
	}

	public class PowOut : Pow {
		public PowOut (float power) : base(power) {
		}

		protected override float Apply (float a) {
			return (float)Math.Pow(a - 1, Power) * (Power % 2 == 0 ? -1 : 1) + 1;
		}
	}
}
