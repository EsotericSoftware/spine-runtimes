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

#ifndef Spine_MathUtil_h
#define Spine_MathUtil_h

#include <math.h>
#include <float.h>

#define SPINE_PI 3.1415927f
#define SPINE_PI_2 SPINE_PI * 2
#define RadDeg 180.0f / SPINE_PI
#define DegRad SPINE_PI / 180.0f
#define SIN_BITS 14 // 16KB. Adjust for accuracy.
#define SIN_MASK ~(-(1 << SIN_BITS))
#define SIN_COUNT SIN_MASK + 1
#define RadFull SPINE_PI * 2
#define DegFull 360
#define RadToIndex SIN_COUNT / RadFull
#define DegToIndex SIN_COUNT / DegFull
#define MAX(a, b) (((a) > (b)) ? (a) : (b))
#define MIN(a, b) (((a) < (b)) ? (a) : (b))

namespace Spine
{
    template <typename T>
    int sign(T val) {
        return (T(0) < val) - (val < T(0));
    }
    
    inline bool areFloatsPracticallyEqual(float A, float B, float maxDiff = 0.0000000000000001f, float maxRelDiff = FLT_EPSILON) {
        // Check if the numbers are really close -- needed
        // when comparing numbers near zero.
        float diff = fabs(A - B);
        if (diff <= maxDiff) {
            return true;
        }
        
        A = fabs(A);
        B = fabs(B);
        
        float largest = (B > A) ? B : A;
        
        if (diff <= largest * maxRelDiff) {
            return true;
        }
        
        return false;
    }
    
    inline float clamp(float x, float lower, float upper) {
        return fminf(upper, fmaxf(x, lower));
    }
    
    class MathUtil {
    public:
        MathUtil();
        
        /// Returns the sine in radians from a lookup table.
        static float sin(float radians);
        
        /// Returns the cosine in radians from a lookup table.
        static float cos(float radians);
        
        /// Returns the sine in radians from a lookup table.
        static float sinDeg(float degrees);
        
        /// Returns the cosine in radians from a lookup table.
        static float cosDeg(float degrees);
        
        /// Returns atan2 in radians, faster but less accurate than Math.Atan2. Average error of 0.00231 radians (0.1323
        /// degrees), largest error of 0.00488 radians (0.2796 degrees).
        static float atan2(float y, float x);
    
    private:
        static float SIN_TABLE[SIN_COUNT];
    };
}

#endif /* Spine_MathUtil_h */
