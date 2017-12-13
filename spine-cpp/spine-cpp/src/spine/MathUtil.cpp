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

namespace Spine {
    float MathUtil::SIN_TABLE[SIN_COUNT] = {0.0f};
    
    MathUtil::MathUtil() {
        for (int i = 0; i < SIN_COUNT; ++i) {
            SIN_TABLE[i] = (float)sin((i + 0.5f) / SIN_COUNT * RadFull);
        }
        
        for (int i = 0; i < 360; i += 90) {
            SIN_TABLE[(int)(i * DegToIndex) & SIN_MASK] = (float)sin(i * DegRad);
        }
    }
    
    /// Returns the sine in radians from a lookup table.
    float MathUtil::sin(float radians) {
        return SIN_TABLE[(int)(radians * RadToIndex) & SIN_MASK];
    }
    
    /// Returns the cosine in radians from a lookup table.
    float MathUtil::cos(float radians) {
        return SIN_TABLE[(int)((radians + SPINE_PI / 2) * RadToIndex) & SIN_MASK];
    }
    
    /// Returns the sine in radians from a lookup table.
    float MathUtil::sinDeg(float degrees) {
        return SIN_TABLE[(int)(degrees * DegToIndex) & SIN_MASK];
    }
    
    /// Returns the cosine in radians from a lookup table.
    float MathUtil::cosDeg(float degrees) {
        return SIN_TABLE[(int)((degrees + 90) * DegToIndex) & SIN_MASK];
    }
    
    /// Returns atan2 in radians, faster but less accurate than Math.Atan2. Average error of 0.00231 radians (0.1323
    /// degrees), largest error of 0.00488 radians (0.2796 degrees).
    float MathUtil::atan2(float y, float x) {
        if (areFloatsPracticallyEqual(x, 0.0f)) {
            if (y > 0.0f) {
                return SPINE_PI / 2;
            }
            
            if (areFloatsPracticallyEqual(y, 0.0f)) {
                return 0.0f;
            }
            
            return -SPINE_PI / 2;
        }
        
        float atan, z = y / x;
        
        if (fabs(z) < 1.0f) {
            atan = z / (1.0f + 0.28f * z * z);
            if (x < 0.0f) {
                return atan + (y < 0.0f ? -SPINE_PI : SPINE_PI);
            }
            
            return atan;
        }
        
        atan = SPINE_PI / 2 - z / (z * z + 0.28f);
        
        return y < 0.0f ? atan - SPINE_PI : atan;
    }
}
