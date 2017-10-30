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

#ifndef Spine_MeshAttachment_h
#define Spine_MeshAttachment_h

#include <spine/VertexAttachment.h>

namespace Spine
{
    /// Attachment that displays a texture region using a mesh.
    class MeshAttachment : public VertexAttachment
    {
        RTTI_DECL;
        
    public:
        public int HullLength { get { return _hulllength; } set { _hulllength = value; } }
        public float[] RegionUVs { get { return _regionUVs; } set { _regionUVs = value; } }
        /// The UV pair for each vertex, normalized within the entire texture. <seealso cref="MeshAttachment.updateUVs"/>
        public float[] UVs { get { return _uvs; } set { _uvs = value; } }
        public int[] Triangles { get { return _triangles; } set { _triangles = value; } }
        
        public float R { get { return r; } set { r = value; } }
        public float G { get { return g; } set { g = value; } }
        public float B { get { return b; } set { b = value; } }
        public float A { get { return a; } set { a = value; } }
        
        public string Path { get; set; }
        public object RendererObject; //public Object RendererObject { get; set; }
        public float RegionU { get; set; }
        public float RegionV { get; set; }
        public float RegionU2 { get; set; }
        public float RegionV2 { get; set; }
        public bool RegionRotate { get; set; }
        public float RegionOffsetX { get { return _regionOffsetX; } set { _regionOffsetX = value; } }
        public float RegionOffsetY { get { return _regionOffsetY; } set { _regionOffsetY = value; } } // Pixels stripped from the bottom left, unrotated.
        public float RegionWidth { get { return _regionWidth; } set { _regionWidth = value; } }
        public float RegionHeight { get { return _regionHeight; } set { _regionHeight = value; } } // Unrotated, stripped size.
        public float RegionOriginalWidth { get { return _regionOriginalWidth; } set { _regionOriginalWidth = value; } }
        public float RegionOriginalHeight { get { return _regionOriginalHeight; } set { _regionOriginalHeight = value; } } // Unrotated, unstripped size.
        
        public bool InheritDeform { get { return _inheritDeform; } set { _inheritDeform = value; } }
        
        public MeshAttachment ParentMesh {
            get { return _parentMesh; }
            set {
                _parentMesh = value;
                if (value != null) {
                    bones = value.bones;
                    vertices = value.vertices;
                    worldVerticesLength = value.worldVerticesLength;
                    _regionUVs = value._regionUVs;
                    _triangles = value._triangles;
                    HullLength = value.HullLength;
                    Edges = value.Edges;
                    Width = value.Width;
                    Height = value.Height;
                }
            }
        }
        
        // Nonessential.
        public int[] Edges { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        
        public MeshAttachment (string name) : VertexAttachment(name)
        {
            // Empty
        }
        
        public void updateUVs()
        {
            float u = RegionU, v = RegionV, width = RegionU2 - RegionU, height = RegionV2 - RegionV;
            if (_uvs == null || _uvs.Length != _regionUVs.Length)
            {
                _uvs = new float[_regionUVs.Length];
            }
            
            float[] _uvs = _uvs;
            if (_regionRotate)
            {
                for (int i = 0, n = _uvs.Length; i < n; i += 2)
                {
                    _uvs[i] = u + _regionUVs[i + 1] * width;
                    _uvs[i + 1] = v + height - _regionUVs[i] * height;
                }
            }
            else
            {
                for (int i = 0, n = _uvs.Length; i < n; i += 2)
                {
                    _uvs[i] = u + _regionUVs[i] * width;
                    _uvs[i + 1] = v + _regionUVs[i + 1] * height;
                }
            }
        }
        
        virtual bool applyDeform(VertexAttachment* sourceAttachment)
        {
            return this == sourceAttachment || (_inheritDeform && _parentMesh == sourceAttachment);
        }
        
    private:
        float _regionOffsetX, _regionOffsetY, _regionWidth, _regionHeight, _regionOriginalWidth, _regionOriginalHeight;
        MeshAttachment* _parentMesh;
        float[] _uvs, _regionUVs;
        int[] _triangles;
        float r = 1, g = 1, b = 1, a = 1;
        int _hulllength;
        bool _inheritDeform;
        bool _regionRotate;
    }
}

#endif /* Spine_MeshAttachment_h */
