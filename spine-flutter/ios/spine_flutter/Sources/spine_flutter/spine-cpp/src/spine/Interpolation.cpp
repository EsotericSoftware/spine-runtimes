/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
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

#include <spine/Interpolation.h>
#include <spine/MathUtil.h>

using namespace spine;

namespace {
	class LinearInterpolation : public Interpolation {
	public:
		float apply(float a) override {
			return a;
		}
	};

	class SmoothInterpolation : public Interpolation {
	public:
		float apply(float a) override {
			return a * a * (3 - 2 * a);
		}
	};

	class SlowFastInterpolation : public Interpolation {
	public:
		float apply(float a) override {
			return a * a;
		}
	};

	class FastSlowInterpolation : public Interpolation {
	public:
		float apply(float a) override {
			return (a - 1) * (a - 1) * -1 + 1;
		}
	};

	class CircleInterpolation : public Interpolation {
	public:
		float apply(float a) override {
			if (a <= 0.5f) {
				a *= 2;
				return (1 - MathUtil::sqrt(1 - a * a)) / 2;
			}
			a--;
			a *= 2;
			return (MathUtil::sqrt(1 - a * a) + 1) / 2;
		}
	};
}

Interpolation::~Interpolation() {
}

float Interpolation::apply(float a) {
	return a;
}

float Interpolation::apply(float start, float end, float a) {
	return start + (end - start) * apply(a);
}

Interpolation &Interpolation::linear() {
	static LinearInterpolation interpolation;
	return interpolation;
}

Interpolation &Interpolation::smooth() {
	static SmoothInterpolation interpolation;
	return interpolation;
}

Interpolation &Interpolation::slowFast() {
	static SlowFastInterpolation interpolation;
	return interpolation;
}

Interpolation &Interpolation::fastSlow() {
	static FastSlowInterpolation interpolation;
	return interpolation;
}

Interpolation &Interpolation::circle() {
	static CircleInterpolation interpolation;
	return interpolation;
}
