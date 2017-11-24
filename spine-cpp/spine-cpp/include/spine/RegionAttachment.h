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

#include <string>

#define NUM_UVS 8

namespace Spine
{
    class Bone;
    
    /// Attachment that displays a texture region.
    class RegionAttachment : public Attachment
    {
        SPINE_RTTI_DECL;
        
        friend class AtlasAttachmentLoader;
        
    public:
        RegionAttachment(std::string name);

        void updateOffset();
        
        void setUVs(float u, float v, float u2, float v2, bool rotate);
        
        /// Transforms the attachment's four vertices to world coordinates.
        /// @param bone The parent bone.
        /// @param worldVertices The output world vertices. Must have a length greater than or equal to offset + 8.
        /// @param offset The worldVertices index to begin writing values.
        /// @param stride The number of worldVertices entries between the value pairs written.
        void computeWorldVertices(Bone& bone, Vector<float>& worldVertices, int offset, int stride = 2);
        
        float getX();
        void setX(float inValue);
        float getY();
        void setY(float inValue);
        float getRotation();
        void setRotation(float inValue);
        float getScaleX();
        void setScaleX(float inValue);
        float getScaleY();
        void setScaleY(float inValue);
        float getWidth();
        void setWidth(float inValue);
        float getHeight();
        void setHeight(float inValue);
        
        float getR();
        void setR(float inValue);
        float getG();
        void setG(float inValue);
        float getB();
        void setB(float inValue);
        float getA();
        void setA(float inValue);
        
        std::string getPath();
        void setPath(std::string inValue);
        void* getRendererObject();
        void setRendererObject(void* inValue);
        float getRegionOffsetX();
        void setRegionOffsetX(float inValue);
        
        // Pixels stripped from the bottom left, unrotated.
        float getRegionOffsetY();
        void setRegionOffsetY(float inValue);
        float getRegionWidth();
        void setRegionWidth(float inValue);
        
        // Unrotated, stripped size.
        float getRegionHeight();
        void setRegionHeight(float inValue);
        float getRegionOriginalWidth();
        void setRegionOriginalWidth(float inValue);
        
        // Unrotated, unstripped size.
        float getRegionOriginalHeight();
        void setRegionOriginalHeight(float inValue);
        
        Vector<float>& getOffset();
        Vector<float>& getUVs();
        
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
        void* _rendererObject;
        std::string _path;
        float _regionU;
        float _regionV;
        float _regionU2;
        float _regionV2;
        float _r, _g, _b, _a;
    };
}

#endif /* Spine_RegionAttachment_h */
