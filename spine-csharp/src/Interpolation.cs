/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2026, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * https://esotericsoftware.com/spine-editor-license
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

namespace Spine {
	/// <summary>
	/// Takes a linear value in the range of 0-1 and outputs a (usually) non-linear, interpolated value.
	/// </summary>
	public abstract class Interpolation {
		/// <param name="a">Alpha value between 0 and 1.</param>
		abstract public float Apply (float a);

		/// <param name="a">Alpha value between 0 and 1.</param>
		public float Apply (float start, float end, float a) {
			return start + (end - start) * Apply(a);
		}

		public static readonly Interpolation Linear = new LinearInterpolation();
		/// <summary>Aka "smoothstep".</summary>
		public static readonly Interpolation Smooth = new SmoothInterpolation();
		public static readonly Interpolation Smooth2 = new Smooth2Interpolation();
		/// <summary>By Ken Perlin.</summary>
		public static readonly Interpolation Smoother = new SmootherInterpolation();
		public static readonly Interpolation Fade = Smoother;

		public static readonly PowInterpolation Pow2 = new PowInterpolation(2);
		/// <summary>Slow, then fast.</summary>
		public static readonly PowInInterpolation Pow2In = new PowInInterpolation(2);
		public static readonly PowInInterpolation SlowFast = Pow2In;
		/// <summary>Fast, then slow.</summary>
		public static readonly PowOutInterpolation Pow2Out = new PowOutInterpolation(2);
		public static readonly PowOutInterpolation FastSlow = Pow2Out;
		public static readonly Interpolation Pow2InInverse = new Pow2InInverseInterpolation();
		public static readonly Interpolation Pow2OutInverse = new Pow2OutInverseInterpolation();

		public static readonly PowInterpolation Pow3 = new PowInterpolation(3);
		public static readonly PowInInterpolation Pow3In = new PowInInterpolation(3);
		public static readonly PowOutInterpolation Pow3Out = new PowOutInterpolation(3);
		public static readonly Interpolation Pow3InInverse = new Pow3InInverseInterpolation();
		public static readonly Interpolation Pow3OutInverse = new Pow3OutInverseInterpolation();

		public static readonly PowInterpolation Pow4 = new PowInterpolation(4);
		public static readonly PowInInterpolation Pow4In = new PowInInterpolation(4);
		public static readonly PowOutInterpolation Pow4Out = new PowOutInterpolation(4);

		public static readonly PowInterpolation Pow5 = new PowInterpolation(5);
		public static readonly PowInInterpolation Pow5In = new PowInInterpolation(5);
		public static readonly PowOutInterpolation Pow5Out = new PowOutInterpolation(5);

		public static readonly Interpolation Sine = new SineInterpolation();
		public static readonly Interpolation SineIn = new SineInInterpolation();
		public static readonly Interpolation SineOut = new SineOutInterpolation();

		public static readonly ExpInterpolation Exp10 = new ExpInterpolation(2, 10);
		public static readonly ExpInInterpolation Exp10In = new ExpInInterpolation(2, 10);
		public static readonly ExpOutInterpolation Exp10Out = new ExpOutInterpolation(2, 10);

		public static readonly ExpInterpolation Exp5 = new ExpInterpolation(2, 5);
		public static readonly ExpInInterpolation Exp5In = new ExpInInterpolation(2, 5);
		public static readonly ExpOutInterpolation Exp5Out = new ExpOutInterpolation(2, 5);

		public static readonly Interpolation Circle = new CircleInterpolation();
		public static readonly Interpolation CircleIn = new CircleInInterpolation();
		public static readonly Interpolation CircleOut = new CircleOutInterpolation();

		public static readonly ElasticInterpolation Elastic = new ElasticInterpolation(2, 10, 7, 1);
		public static readonly ElasticInInterpolation ElasticIn = new ElasticInInterpolation(2, 10, 6, 1);
		public static readonly ElasticOutInterpolation ElasticOut = new ElasticOutInterpolation(2, 10, 7, 1);

		public static readonly SwingInterpolation Swing = new SwingInterpolation(1.5f);
		public static readonly SwingInInterpolation SwingIn = new SwingInInterpolation(2f);
		public static readonly SwingOutInterpolation SwingOut = new SwingOutInterpolation(2f);

		public static readonly BounceInterpolation Bounce = new BounceInterpolation(4);
		public static readonly BounceInInterpolation BounceIn = new BounceInInterpolation(4);
		public static readonly BounceOutInterpolation BounceOut = new BounceOutInterpolation(4);

		#region Implementation Classes
		class LinearInterpolation : Interpolation {
			public override float Apply (float a) {
				return a;
			}
		}

		class SmoothInterpolation : Interpolation {
			public override float Apply (float a) {
				return a * a * (3 - 2 * a);
			}
		}

		class Smooth2Interpolation : Interpolation {
			public override float Apply (float a) {
				a = a * a * (3 - 2 * a);
				return a * a * (3 - 2 * a);
			}
		}

		/// <summary>By Ken Perlin.</summary>
		class SmootherInterpolation : Interpolation {
			public override float Apply (float a) {
				return a * a * a * (a * (a * 6 - 15) + 10);
			}
		}

		public class PowInterpolation : Interpolation {
			protected readonly int power;

			public PowInterpolation (int power) {
				this.power = power;
			}

			public override float Apply (float a) {
				if (a <= 0.5f) return (float)Math.Pow(a * 2, power) / 2;
				return (float)Math.Pow((a - 1) * 2, power) / (power % 2 == 0 ? -2 : 2) + 1;
			}
		}

		public class PowInInterpolation : PowInterpolation {
			public PowInInterpolation (int power) : base(power) {
			}

			public override float Apply (float a) {
				return (float)Math.Pow(a, power);
			}
		}

		public class PowOutInterpolation : PowInterpolation {
			public PowOutInterpolation (int power) : base(power) {
			}

			public override float Apply (float a) {
				return (float)Math.Pow(a - 1, power) * (power % 2 == 0 ? -1 : 1) + 1;
			}
		}

		class Pow2InInverseInterpolation : Interpolation {
			public override float Apply (float a) {
				if (a < MathUtils.FloatRoundingError) return 0;
				return (float)Math.Sqrt(a);
			}
		}

		class Pow2OutInverseInterpolation : Interpolation {
			public override float Apply (float a) {
				if (a < MathUtils.FloatRoundingError) return 0;
				if (a > 1) return 1;
				return 1 - (float)Math.Sqrt(-(a - 1));
			}
		}

		class Pow3InInverseInterpolation : Interpolation {
			public override float Apply (float a) {
				return MathUtils.Cbrt(a);
			}
		}

		class Pow3OutInverseInterpolation : Interpolation {
			public override float Apply (float a) {
				return 1 - MathUtils.Cbrt(-(a - 1));
			}
		}

		class SineInterpolation : Interpolation {
			public override float Apply (float a) {
				return (1 - MathUtils.Cos(a * MathUtils.PI)) / 2;
			}
		}

		class SineInInterpolation : Interpolation {
			public override float Apply (float a) {
				return 1 - MathUtils.Cos(a * MathUtils.HalfPi);
			}
		}

		class SineOutInterpolation : Interpolation {
			public override float Apply (float a) {
				return MathUtils.Sin(a * MathUtils.HalfPi);
			}
		}

		public class ExpInterpolation : Interpolation {
			protected readonly float value, power, min, scale;

			public ExpInterpolation (float value, float power) {
				this.value = value;
				this.power = power;
				min = (float)Math.Pow(value, -power);
				scale = 1 / (1 - min);
			}

			public override float Apply (float a) {
				if (a <= 0.5f) return ((float)Math.Pow(value, power * (a * 2 - 1)) - min) * scale / 2;
				return (2 - ((float)Math.Pow(value, -power * (a * 2 - 1)) - min) * scale) / 2;
			}
		}

		public class ExpInInterpolation : ExpInterpolation {
			public ExpInInterpolation (float value, float power) : base(value, power) {
			}

			public override float Apply (float a) {
				return ((float)Math.Pow(value, power * (a - 1)) - min) * scale;
			}
		}

		public class ExpOutInterpolation : ExpInterpolation {
			public ExpOutInterpolation (float value, float power) : base(value, power) {
			}

			public override float Apply (float a) {
				return 1 - ((float)Math.Pow(value, -power * a) - min) * scale;
			}
		}

		class CircleInterpolation : Interpolation {
			public override float Apply (float a) {
				if (a <= 0.5f) {
					a *= 2;
					return (1 - (float)Math.Sqrt(1 - a * a)) / 2;
				}

				a--;
				a *= 2;
				return ((float)Math.Sqrt(1 - a * a) + 1) / 2;
			}
		}

		class CircleInInterpolation : Interpolation {
			public override float Apply (float a) {
				return 1 - (float)Math.Sqrt(1 - a * a);
			}
		}

		class CircleOutInterpolation : Interpolation {
			public override float Apply (float a) {
				a--;
				return (float)Math.Sqrt(1 - a * a);
			}
		}

		public class ElasticInterpolation : Interpolation {
			protected readonly float value, power, scale, bounces;

			public ElasticInterpolation (float value, float power, int bounces, float scale) {
				this.value = value;
				this.power = power;
				this.scale = scale;
				this.bounces = bounces * MathUtils.PI * (bounces % 2 == 0 ? 1 : -1);
			}

			public override float Apply (float a) {
				if (a <= 0.5f) {
					a *= 2;
					return (float)Math.Pow(value, power * (a - 1)) * MathUtils.Sin(a * bounces) * scale / 2;
				}
				a = 1 - a;
				a *= 2;
				return 1 - (float)Math.Pow(value, power * (a - 1)) * MathUtils.Sin(a * bounces) * scale / 2;
			}
		}

		public class ElasticInInterpolation : ElasticInterpolation {
			public ElasticInInterpolation (float value, float power, int bounces, float scale)
				: base(value, power, bounces, scale) {
			}

			public override float Apply (float a) {
				if (a >= 0.99) return 1;
				return (float)Math.Pow(value, power * (a - 1)) * MathUtils.Sin(a * bounces) * scale;
			}
		}

		public class ElasticOutInterpolation : ElasticInterpolation {
			public ElasticOutInterpolation (float value, float power, int bounces, float scale)
				: base(value, power, bounces, scale) {
			}

			public override float Apply (float a) {
				if (a == 0) return 0;
				a = 1 - a;
				return 1 - (float)Math.Pow(value, power * (a - 1)) * MathUtils.Sin(a * bounces) * scale;
			}
		}

		public class SwingInterpolation : Interpolation {
			readonly float scale;

			public SwingInterpolation (float scale) {
				this.scale = scale * 2;
			}

			public override float Apply (float a) {
				if (a <= 0.5f) {
					a *= 2;
					return a * a * ((scale + 1) * a - scale) / 2;
				}
				a--;
				a *= 2;
				return a * a * ((scale + 1) * a + scale) / 2 + 1;
			}
		}

		public class SwingOutInterpolation : Interpolation {
			readonly float scale;

			public SwingOutInterpolation (float scale) {
				this.scale = scale;
			}

			public override float Apply (float a) {
				a--;
				return a * a * ((scale + 1) * a + scale) + 1;
			}
		}

		public class SwingInInterpolation : Interpolation {
			readonly float scale;

			public SwingInInterpolation (float scale) {
				this.scale = scale;
			}

			public override float Apply (float a) {
				return a * a * ((scale + 1) * a - scale);
			}
		}

		public class BounceOutInterpolation : Interpolation {
			protected readonly float[] widths, heights;

			public BounceOutInterpolation (float[] widths, float[] heights) {
				if (widths.Length != heights.Length)
					throw new ArgumentException("Must be the same number of widths and heights.");
				this.widths = widths;
				this.heights = heights;
			}

			public BounceOutInterpolation (int bounces) {
				if (bounces < 2 || bounces > 5) throw new ArgumentException("bounces cannot be < 2 or > 5: " + bounces);
				widths = new float[bounces];
				heights = new float[bounces];
				heights[0] = 1;
				switch (bounces) {
				case 2:
					widths[0] = 0.6f;
					widths[1] = 0.4f;
					heights[1] = 0.33f;
					break;
				case 3:
					widths[0] = 0.4f;
					widths[1] = 0.4f;
					widths[2] = 0.2f;
					heights[1] = 0.33f;
					heights[2] = 0.1f;
					break;
				case 4:
					widths[0] = 0.34f;
					widths[1] = 0.34f;
					widths[2] = 0.2f;
					widths[3] = 0.15f;
					heights[1] = 0.26f;
					heights[2] = 0.11f;
					heights[3] = 0.03f;
					break;
				case 5:
					widths[0] = 0.3f;
					widths[1] = 0.3f;
					widths[2] = 0.2f;
					widths[3] = 0.1f;
					widths[4] = 0.1f;
					heights[1] = 0.45f;
					heights[2] = 0.3f;
					heights[3] = 0.15f;
					heights[4] = 0.06f;
					break;
				}
				widths[0] *= 2;
			}

			public override float Apply (float a) {
				if (a == 1) return 1;
				a += widths[0] / 2;
				float width = 0, height = 0;
				for (int i = 0, n = widths.Length; i < n; i++) {
					width = widths[i];
					if (a <= width) {
						height = heights[i];
						break;
					}
					a -= width;
				}
				a /= width;
				float z = 4 / width * height * a;
				return 1 - (z - z * a) * width;
			}
		}

		public class BounceInterpolation : BounceOutInterpolation {
			public BounceInterpolation (float[] widths, float[] heights) : base(widths, heights) {
			}

			public BounceInterpolation (int bounces) : base(bounces) {
			}

			float Out (float a) {
				float test = a + widths[0] / 2;
				if (test < widths[0]) return test / (widths[0] / 2) - 1;
				return base.Apply(a);
			}

			public override float Apply (float a) {
				if (a <= 0.5f) return (1 - Out(1 - a * 2)) / 2;
				return Out(a * 2 - 1) / 2 + 0.5f;
			}
		}

		public class BounceInInterpolation : BounceOutInterpolation {
			public BounceInInterpolation (float[] widths, float[] heights) : base(widths, heights) {
			}

			public BounceInInterpolation (int bounces) : base(bounces) {
			}

			public override float Apply (float a) {
				return 1 - base.Apply(1 - a);
			}
		}
		#endregion
	}
}
