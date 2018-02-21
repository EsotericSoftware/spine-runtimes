//
// Created by Mario Zechner on 2/20/18.
//

#ifndef SPINE_COLOR_H
#define SPINE_COLOR_H

#include <spine/MathUtil.h>

namespace Spine {
	class Color {
	public:
		Color() : _r(0), _g(0), _b(0), _a(0) {
		}

		Color(float r, float g, float b, float a) : _r(r), _g(g), _b(b), _a(a) {
			clamp();
		}

		inline Color& set(float r, float g, float b, float a) {
			_r = r;
			_g = g;
			_b = b;
			_a = a;
			clamp();
			return *this;
		}

		inline Color& set(const Color& other) {
			_r = other._r;
			_g = other._g;
			_b = other._b;
			_a = other._a;
			clamp();
			return *this;
		}

		inline Color& add(float r, float g, float b, float a) {
			_r += r;
			_g += g;
			_b += b;
			_a += a;
			clamp();
			return *this;
		}

		inline Color& add(const Color& other) {
			_r += other._r;
			_g += other._g;
			_b += other._b;
			_a += other._a;
			clamp();
			return *this;
		}

		inline Color& clamp() {
			_r = MathUtil::clamp(this->_r, 0, 1);
			_g = MathUtil::clamp(this->_g, 0, 1);
			_b = MathUtil::clamp(this->_b, 0, 1);
			_a = MathUtil::clamp(this->_a, 0, 1);
			return *this;
		}

		float _r, _g, _b, _a;
	};
}


#endif //SPINE_COLOR_H
