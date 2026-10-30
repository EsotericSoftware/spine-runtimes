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

#ifndef Spine_CurveTimeline_h
#define Spine_CurveTimeline_h

#include <spine/Timeline.h>
#include <spine/Array.h>

namespace spine {
	/// Base class for frames that use an interpolation bezier curve.
	class SP_API CurveTimeline : public Timeline {
		RTTI_DECL

	public:
		explicit CurveTimeline(size_t frameCount, size_t frameEntries, size_t bezierCount);

		virtual ~CurveTimeline();

		void setLinear(size_t frame);

		void setStepped(size_t frame);

		virtual void setBezier(size_t bezier, size_t frame, float value, float time1, float value1, float cx1, float cy1, float cx2, float cy2,
							   float time2, float value2);

		float getBezierValue(float time, size_t frame, size_t valueOffset, size_t i);

		Array<float> &getCurves();

	protected:
		static const int LINEAR = 0;
		static const int STEPPED = 1;
		static const int BEZIER = 2;
		static const int BEZIER_SIZE = 18;

		Array<float> _curves;// type, x, y, ...
	};

	/// The base class for a CurveTimeline that sets one property.
	class SP_API CurveTimeline1 : public CurveTimeline {
		RTTI_DECL

	public:
		/// @param frameCount The number of frames for this timeline.
		/// @param bezierCount The maximum number of Bezier curves.
		explicit CurveTimeline1(size_t frameCount, size_t bezierCount);

		virtual ~CurveTimeline1();

		/// Sets the time and value for the specified frame.
		/// @param frame Between 0 and frameCount, inclusive.
		/// @param time The frame time in seconds.
		void setFrame(size_t frame, float time, float value);

		/// Returns the interpolated value for the specified time.
		float getCurveValue(float time);

		float getRelativeValue(float time, float alpha, MixFrom from, bool add, float current, float setup);

		float getAbsoluteValue(float time, float alpha, MixFrom from, bool add, float current, float setup);

		float getAbsoluteValue(float time, float alpha, MixFrom from, bool add, float current, float setup, float value);

		float getScaleValue(float time, float alpha, MixFrom from, bool add, bool out, float current, float setup);

		static float beforeFirstKey(MixFrom from, float alpha, float current, float setup);

	protected:
		static const int ENTRIES = 2;
		static const int VALUE = 1;
	};
}

#endif /* Spine_CurveTimeline_h */
