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

#include <spine/RegionAttachment.h>

#include <spine/Bone.h>

namespace Spine
{
    const int RegionAttachment::BLX = 0;
    const int RegionAttachment::BLY = 1;
    const int RegionAttachment::ULX = 2;
    const int RegionAttachment::ULY = 3;
    const int RegionAttachment::URX = 4;
    const int RegionAttachment::URY = 5;
    const int RegionAttachment::BRX = 6;
    const int RegionAttachment::BRY = 7;
    
    void RegionAttachment::computeWorldVertices(Bone& bone, Vector<float>& worldVertices, int offset, int stride)
    {
        assert(worldVertices.size() >= (offset + 8));
        
        float bwx = bone._worldX, bwy = bone._worldY;
        float a = bone._a, b = bone._b, c = bone._c, d = bone._d;
        float offsetX, offsetY;
        
        offsetX = _offset[BRX]; // 0
        offsetY = _offset[BRY]; // 1
        worldVertices[offset] = offsetX * a + offsetY * b + bwx; // bl
        worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
        offset += stride;
        
        offsetX = _offset[BLX]; // 2
        offsetY = _offset[BLY]; // 3
        worldVertices[offset] = offsetX * a + offsetY * b + bwx; // ul
        worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
        offset += stride;
        
        offsetX = _offset[ULX]; // 4
        offsetY = _offset[ULY]; // 5
        worldVertices[offset] = offsetX * a + offsetY * b + bwx; // ur
        worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
        offset += stride;
        
        offsetX = _offset[URX]; // 6
        offsetY = _offset[URY]; // 7
        worldVertices[offset] = offsetX * a + offsetY * b + bwx; // br
        worldVertices[offset + 1] = offsetX * c + offsetY * d + bwy;
    }
    
    RTTI_IMPL(RegionAttachment, Attachment);
}
