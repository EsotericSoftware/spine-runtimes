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

#ifndef SPINE_COLOR_H
#define SPINE_COLOR_H

#include <spine/MathUtil.h>
#include <string.h>
#include <stdlib.h>

namespace spine {
	class SP_API Color : public SpineObject {
	public:
		Color() : r(0), g(0), b(0), a(0) {
		}

		Color(float r, float g, float b, float a) : r(r), g(g), b(b), a(a) {
			clamp();
		}

		inline Color &set(float _r, float _g, float _b, float _a) {
			this->r = _r;
			this->g = _g;
			this->b = _b;
			this->a = _a;
			clamp();
			return *this;
		}

		inline Color &set(float _r, float _g, float _b) {
			this->r = _r;
			this->g = _g;
			this->b = _b;
			clamp();
			return *this;
		}

		inline Color &set(const Color &other) {
			r = other.r;
			g = other.g;
			b = other.b;
			a = other.a;
			clamp();
			return *this;
		}

		inline Color &add(float _r, float _g, float _b, float _a) {
			this->r += _r;
			this->g += _g;
			this->b += _b;
			this->a += _a;
			clamp();
			return *this;
		}

		inline Color &add(float _r, float _g, float _b) {
			this->r += _r;
			this->g += _g;
			this->b += _b;
			clamp();
			return *this;
		}

		inline Color &add(const Color &other) {
			r += other.r;
			g += other.g;
			b += other.b;
			a += other.a;
			clamp();
			return *this;
		}

		inline Color &clamp() {
			r = MathUtil::clamp(this->r, 0, 1);
			g = MathUtil::clamp(this->g, 0, 1);
			b = MathUtil::clamp(this->b, 0, 1);
			a = MathUtil::clamp(this->a, 0, 1);
			return *this;
		}

		// Parse hex color string like "ff0000ff" (RRGGBBAA) or "ff0000" (RRGGBB)
		static Color valueOf(const char *hexString) {
			Color color;
			valueOf(hexString, color);
			return color;
		}

		// Parse hex color string into existing Color object
		static void valueOf(const char *hexString, Color &color) {
			size_t len = strlen(hexString);
			if (len >= 6) {
				color.r = parseHex(hexString, 0);
				color.g = parseHex(hexString, 1);
				color.b = parseHex(hexString, 2);
				color.a = len >= 8 ? parseHex(hexString, 3) : 1.0f;
			}
		}

		static float parseHex(const char *value, size_t index) {
			char digits[3];
			digits[0] = value[index * 2];
			digits[1] = value[index * 2 + 1];
			digits[2] = '\0';
			return strtoul(digits, NULL, 16) / 255.0f;
		}

		// Convert packed RGBA8888 integer to Color
		static void rgba8888ToColor(Color &color, int value) {
			unsigned int rgba = (unsigned int) value;
			color.r = ((rgba & 0xff000000) >> 24) / 255.0f;
			color.g = ((rgba & 0x00ff0000) >> 16) / 255.0f;
			color.b = ((rgba & 0x0000ff00) >> 8) / 255.0f;
			color.a = (rgba & 0x000000ff) / 255.0f;
		}

		// Convert packed RGB888 integer to Color (no alpha)
		static void rgb888ToColor(Color &color, int value) {
			unsigned int rgb = (unsigned int) value;
			color.r = ((rgb & 0xff0000) >> 16) / 255.0f;
			color.g = ((rgb & 0x00ff00) >> 8) / 255.0f;
			color.b = (rgb & 0x0000ff) / 255.0f;
			color.a = 1.0f;
		}

		float r, g, b, a;
	};
}


#endif//SPINE_COLOR_H
