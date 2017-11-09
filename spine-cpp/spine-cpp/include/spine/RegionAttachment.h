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

#ifndef Spine_RegionAttachment_h
#define Spine_RegionAttachment_h

#include <spine/Attachment.h>

#include <spine/Vector.h>
#include <spine/MathUtil.h>

#include <assert.h>

#define NUM_UVS 8

namespace Spine
{
    class Bone;
    
    /// Attachment that displays a texture region.
    class RegionAttachment : public Attachment
    {
        RTTI_DECL;
        
    public:
//        float X { get { return x; } set { x = value; } }
//        float Y { get { return y; } set { y = value; } }
//        float Rotation { get { return _rotation; } set { _rotation = value; } }
//        float ScaleX { get { return scaleX; } set { scaleX = value; } }
//        float ScaleY { get { return scaleY; } set { scaleY = value; } }
//        float Width { get { return width; } set { width = value; } }
//        float Height { get { return height; } set { height = value; } }
//        
//        float R { get { return r; } set { r = value; } }
//        float G { get { return g; } set { g = value; } }
//        float B { get { return b; } set { b = value; } }
//        float A { get { return a; } set { a = value; } }
//        
//        std::string Path { get; set; }
//        void* RendererObject; //object RendererObject { get; set; }
//        float RegionOffsetX { get { return _regionOffsetX; } set { _regionOffsetX = value; } }
//        float RegionOffsetY { get { return _regionOffsetY; } set { _regionOffsetY = value; } } // Pixels stripped from the bottom left, unrotated.
//        float RegionWidth { get { return _regionWidth; } set { _regionWidth = value; } }
//        float RegionHeight { get { return _regionHeight; } set { _regionHeight = value; } } // Unrotated, stripped size.
//        float RegionOriginalWidth { get { return _regionOriginalWidth; } set { _regionOriginalWidth = value; } }
//        float RegionOriginalHeight { get { return _regionOriginalHeight; } set { _regionOriginalHeight = value; } } // Unrotated, unstripped size.
//        
//        Vector<float>& Offset { get { return _offset; } }
//        Vector<float>& UVs { get { return _uvs; } }
//        
        RegionAttachment(std::string name) : Attachment(name)
        {
            _offset.reserve(NUM_UVS);
            _uvs.reserve(NUM_UVS);
        }
//
//        void updateOffset()
//        {
//            float regionScaleX = _width / _regionOriginalWidth * _scaleX;
//            float regionScaleY = _height / _regionOriginalHeight * _scaleY;
//            float localX = -_width / 2 * _scaleX + _regionOffsetX * regionScaleX;
//            float localY = -_height / 2 * _scaleY + _regionOffsetY * regionScaleY;
//            float localX2 = localX + _regionWidth * regionScaleX;
//            float localY2 = localY + _regionHeight * regionScaleY;
//            float cos = MathUtil::cosDeg(_rotation);
//            float sin = MathUtil::sinDeg(_rotation);
//            float localXCos = localX * cos + _x;
//            float localXSin = localX * sin;
//            float localYCos = localY * cos + _y;
//            float localYSin = localY * sin;
//            float localX2Cos = localX2 * cos + _x;
//            float localX2Sin = localX2 * sin;
//            float localY2Cos = localY2 * cos + _y;
//            float localY2Sin = localY2 * sin;
//            
//            _offset[BLX] = localXCos - localYSin;
//            _offset[BLY] = localYCos + localXSin;
//            _offset[ULX] = localXCos - localY2Sin;
//            _offset[ULY] = localY2Cos + localXSin;
//            _offset[URX] = localX2Cos - localY2Sin;
//            _offset[URY] = localY2Cos + localX2Sin;
//            _offset[BRX] = localX2Cos - localYSin;
//            _offset[BRY] = localYCos + localX2Sin;
//        }
//        
//        void setUVs(float u, float v, float u2, float v2, bool rotate)
//        {
//            if (rotate)
//            {
//                _uvs[URX] = u;
//                _uvs[URY] = v2;
//                _uvs[BRX] = u;
//                _uvs[BRY] = v;
//                _uvs[BLX] = u2;
//                _uvs[BLY] = v;
//                _uvs[ULX] = u2;
//                _uvs[ULY] = v2;
//            }
//            else
//            {
//                _uvs[ULX] = u;
//                _uvs[ULY] = v2;
//                _uvs[URX] = u;
//                _uvs[URY] = v;
//                _uvs[BRX] = u2;
//                _uvs[BRY] = v;
//                _uvs[BLX] = u2;
//                _uvs[BLY] = v2;
//            }
//        }
//        
        /// Transforms the attachment's four vertices to world coordinates.
        /// @param bone The parent bone.
        /// @param worldVertices The output world vertices. Must have a length greater than or equal to offset + 8.
        /// @param offset The worldVertices index to begin writing values.
        /// @param stride The number of worldVertices entries between the value pairs written.
        void computeWorldVertices(Bone& bone, Vector<float>& worldVertices, int offset, int stride = 2);
        
    private:
        static const int BLX;
        static const int BLY;
        static const int ULX;
        static const int ULY;
        static const int URX;
        static const int URY;
        static const int BRX;
        static const int BRY;
        
        float _x, _y, _rotation, _scaleX = 1, _scaleY = 1, _width, _height;
        float _regionOffsetX, _regionOffsetY, _regionWidth, _regionHeight, _regionOriginalWidth, _regionOriginalHeight;
        Vector<float> _offset;
        Vector<float> _uvs;
        float r = 1, g = 1, b = 1, a = 1;
    };
}

#endif /* Spine_RegionAttachment_h */
