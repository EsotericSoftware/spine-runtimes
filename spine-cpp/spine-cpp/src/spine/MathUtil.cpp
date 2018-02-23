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

#include <spine/MathUtil.h>
#include <math.h>

namespace Spine {
    float MathUtil::abs(float v) {
        return ((v) < 0? -(v): (v));
    }

    int MathUtil::sign(float v) {
        return ((v) < 0? -1: (v) > 0 ? 1 : 0);
    }

    float MathUtil::clamp(float x, float min, float max) {
        return((x) < (min) ? (min) : ((x) > (max) ? (max) : (x)));
    }

    float MathUtil::fmod(float a, float b) {
        return ::fmod(a, b);
    }

    /// Returns atan2 in radians, faster but less accurate than Math.Atan2. Average error of 0.00231 radians (0.1323
    /// degrees), largest error of 0.00488 radians (0.2796 degrees).
    float MathUtil::atan2(float y, float x) {
        return ::atan2(y, x);
    }

    /// Returns the cosine in radians from a lookup table.
    float MathUtil::cos(float radians) {
        return ::cos(radians);
    }

    /// Returns the sine in radians from a lookup table.
    float MathUtil::sin(float radians) {
        return ::sin(radians);
    }

    float MathUtil::sqrt(float v) {
        return ::sqrt(v);
    }

    float MathUtil::acos(float v) {
        return ::acos(v);
    }
    
    /// Returns the sine in radians from a lookup table.
    float MathUtil::sinDeg(float degrees) {
        return ::sin(degrees * DEG_RAD);
    }
    
    /// Returns the cosine in radians from a lookup table.
    float MathUtil::cosDeg(float degrees) {
        return ::cos(degrees * DEG_RAD);
    }

    /* Need to pass 0 as an argument, so VC++ doesn't error with C2124 */
    static bool _isNan(float value, float zero) {
        float _nan =  (float)0.0 / zero;
        return 0 == memcmp((void*)&value, (void*)&_nan, sizeof(value));
    }

    bool MathUtil::isNan(float v) {
        return _isNan(v, 0);
    }
}
