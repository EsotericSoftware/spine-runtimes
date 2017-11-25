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

#include <spine/MathUtil.h>

#include <assert.h>

namespace Spine
{
    RTTI_IMPL(RegionAttachment, Attachment);
    
    const int RegionAttachment::BLX = 0;
    const int RegionAttachment::BLY = 1;
    const int RegionAttachment::ULX = 2;
    const int RegionAttachment::ULY = 3;
    const int RegionAttachment::URX = 4;
    const int RegionAttachment::URY = 5;
    const int RegionAttachment::BRX = 6;
    const int RegionAttachment::BRY = 7;
    
    RegionAttachment::RegionAttachment(std::string name) : Attachment(name)
    {
        _offset.reserve(NUM_UVS);
        _uvs.reserve(NUM_UVS);
    }
    
    void RegionAttachment::updateOffset()
    {
        float regionScaleX = _width / _regionOriginalWidth * _scaleX;
        float regionScaleY = _height / _regionOriginalHeight * _scaleY;
        float localX = -_width / 2 * _scaleX + _regionOffsetX * regionScaleX;
        float localY = -_height / 2 * _scaleY + _regionOffsetY * regionScaleY;
        float localX2 = localX + _regionWidth * regionScaleX;
        float localY2 = localY + _regionHeight * regionScaleY;
        float cos = MathUtil::cosDeg(_rotation);
        float sin = MathUtil::sinDeg(_rotation);
        float localXCos = localX * cos + _x;
        float localXSin = localX * sin;
        float localYCos = localY * cos + _y;
        float localYSin = localY * sin;
        float localX2Cos = localX2 * cos + _x;
        float localX2Sin = localX2 * sin;
        float localY2Cos = localY2 * cos + _y;
        float localY2Sin = localY2 * sin;
        
        _offset[BLX] = localXCos - localYSin;
        _offset[BLY] = localYCos + localXSin;
        _offset[ULX] = localXCos - localY2Sin;
        _offset[ULY] = localY2Cos + localXSin;
        _offset[URX] = localX2Cos - localY2Sin;
        _offset[URY] = localY2Cos + localX2Sin;
        _offset[BRX] = localX2Cos - localYSin;
        _offset[BRY] = localYCos + localX2Sin;
    }
    
    void RegionAttachment::setUVs(float u, float v, float u2, float v2, bool rotate)
    {
        if (rotate)
        {
            _uvs[URX] = u;
            _uvs[URY] = v2;
            _uvs[BRX] = u;
            _uvs[BRY] = v;
            _uvs[BLX] = u2;
            _uvs[BLY] = v;
            _uvs[ULX] = u2;
            _uvs[ULY] = v2;
        }
        else
        {
            _uvs[ULX] = u;
            _uvs[ULY] = v2;
            _uvs[URX] = u;
            _uvs[URY] = v;
            _uvs[BRX] = u2;
            _uvs[BRY] = v;
            _uvs[BLX] = u2;
            _uvs[BLY] = v2;
        }
    }
    
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
    
    float RegionAttachment::getX()
    {
        return _x;
    }
    
    void RegionAttachment::setX(float inValue)
    {
        _x = inValue;
    }
    
    float RegionAttachment::getY()
    {
        return _y;
    }
    
    void RegionAttachment::setY(float inValue)
    {
        _y = inValue;
    }
    
    float RegionAttachment::getRotation()
    {
        return _rotation;
    }
    
    void RegionAttachment::setRotation(float inValue)
    {
        _rotation = inValue;
    }
    
    float RegionAttachment::getScaleX()
    {
        return _scaleX;
    }
    
    void RegionAttachment::setScaleX(float inValue)
    {
        _scaleX = inValue;
    }
    
    float RegionAttachment::getScaleY()
    {
        return _scaleY;
    }
    
    void RegionAttachment::setScaleY(float inValue)
    {
        _scaleY = inValue;
    }
    
    float RegionAttachment::getWidth()
    {
        return _width;
    }
    
    void RegionAttachment::setWidth(float inValue)
    {
        _width = inValue;
    }
    
    float RegionAttachment::getHeight()
    {
        return _height;
    }
    
    void RegionAttachment::setHeight(float inValue)
    {
        _height = inValue;
    }
    
    float RegionAttachment::getR()
    {
        return _r;
    }
    
    void RegionAttachment::setR(float inValue)
    {
        _r = inValue;
    }
    
    float RegionAttachment::getG()
    {
        return _g;
    }
    
    void RegionAttachment::setG(float inValue)
    {
        _g = inValue;
    }
    
    float RegionAttachment::getB()
    {
        return _b;
    }
    
    void RegionAttachment::setB(float inValue)
    {
        _b = inValue;
    }
    
    float RegionAttachment::getA()
    {
        return _a;
    }
    
    void RegionAttachment::setA(float inValue)
    {
        _a = inValue;
    }
    
    std::string RegionAttachment::getPath()
    {
        return _path;
    }
    
    void RegionAttachment::setPath(std::string inValue)
    {
        _path = inValue;
    }
    
    void* RegionAttachment::getRendererObject()
    {
        return _rendererObject;
    }
    
    void RegionAttachment::setRendererObject(void* inValue)
    {
        _rendererObject = inValue;
    }
    
    float RegionAttachment::getRegionOffsetX()
    {
        return _regionOffsetX;
    }
    
    void RegionAttachment::setRegionOffsetX(float inValue)
    {
        _regionOffsetX = inValue;
    }
    
    float RegionAttachment::getRegionOffsetY()
    {
        return _regionOffsetY;
    }
    
    void RegionAttachment::setRegionOffsetY(float inValue)
    {
        _regionOffsetY = inValue;
    }
    
    float RegionAttachment::getRegionWidth()
    {
        return _regionWidth;
    }
    
    void RegionAttachment::setRegionWidth(float inValue)
    {
        _regionWidth = inValue;
    }
    
    float RegionAttachment::getRegionHeight()
    {
        return _regionHeight;
    }
    
    void RegionAttachment::setRegionHeight(float inValue)
    {
        _regionHeight = inValue;
    }
    
    float RegionAttachment::getRegionOriginalWidth()
    {
        return _regionOriginalWidth;
    }
    
    void RegionAttachment::setRegionOriginalWidth(float inValue)
    {
        _regionOriginalWidth = inValue;
    }
    
    float RegionAttachment::getRegionOriginalHeight()
    {
        return _regionOriginalHeight;
    }
    
    void RegionAttachment::setRegionOriginalHeight(float inValue)
    {
        _regionOriginalHeight = inValue;
    }
    
    Vector<float>& RegionAttachment::getOffset()
    {
        return _offset;
    }
    
    Vector<float>& RegionAttachment::getUVs()
    {
        return _uvs;
    }
}
