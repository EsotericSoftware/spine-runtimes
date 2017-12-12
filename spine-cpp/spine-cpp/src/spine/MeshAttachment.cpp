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

#include <spine/MeshAttachment.h>

namespace Spine
{
    RTTI_IMPL(MeshAttachment, VertexAttachment);
    
    MeshAttachment::MeshAttachment(std::string name) : VertexAttachment(name),
    _regionOffsetX(0),
    _regionOffsetY(0),
    _regionWidth(0),
    _regionHeight(0),
    _regionOriginalWidth(0),
    _regionOriginalHeight(0),
    _parentMesh(NULL),
    _rendererObject(NULL),
    _path(),
    _regionU(0),
    _regionV(0),
    _regionU2(0),
    _regionV2(0),
    _width(0),
    _height(0),
    _r(1),
    _g(1),
    _b(1),
    _a(1),
    _hullLength(0),
    _inheritDeform(false),
    _regionRotate(false) {
        // Empty
    }
    
    void MeshAttachment::updateUVs() {
        float u = _regionU, v = _regionV, width = _regionU2 - _regionU, height = _regionV2 - _regionV;
        if (_uvs.size() != _regionUVs.size()) {
            _uvs.reserve(_regionUVs.size());
            _uvs.setSize(_regionUVs.size());
        }
        
        if (_regionRotate) {
            for (size_t i = 0, n = _uvs.size(); i < n; i += 2) {
                _uvs[i] = u + _regionUVs[i + 1] * width;
                _uvs[i + 1] = v + height - _regionUVs[i] * height;
            }
        }
        else {
            for (size_t i = 0, n = _uvs.size(); i < n; i += 2) {
                _uvs[i] = u + _regionUVs[i] * width;
                _uvs[i + 1] = v + _regionUVs[i + 1] * height;
            }
        }
    }
    
    bool MeshAttachment::applyDeform(VertexAttachment* sourceAttachment) {
        return this == sourceAttachment || (_inheritDeform && _parentMesh == sourceAttachment);
    }
    
    int MeshAttachment::getHullLength() {
        return _hullLength;
    }
    
    void MeshAttachment::setHullLength(float inValue) {
        _hullLength = inValue;
    }
    
    Vector<float>& MeshAttachment::getRegionUVs() {
        return _regionUVs;
    }
    
    void MeshAttachment::setRegionUVs(Vector<float>& inValue) {
        _regionUVs = inValue;
    }
    
    Vector<float>& MeshAttachment::getUVs() {
        return _uvs;
    }
    
    void MeshAttachment::setUVs(Vector<float>& inValue) {
        _uvs = inValue;
    }
    
    Vector<short>& MeshAttachment::getTriangles() {
        return _triangles;
    }
    
    void MeshAttachment::setTriangles(Vector<short>& inValue) {
        _triangles = inValue;
    }
    
    float MeshAttachment::getR() {
        return _r;
    }
    
    void MeshAttachment::setR(float inValue) {
        _r = inValue;
    }
    
    float MeshAttachment::getG() {
        return _g;
    }
    
    void MeshAttachment::setG(float inValue) {
        _g = inValue;
    }
    
    float MeshAttachment::getB() {
        return _b;
    }
    
    void MeshAttachment::setB(float inValue) {
        _b = inValue;
    }
    
    float MeshAttachment::getA() {
        return _a;
    }
    
    void MeshAttachment::setA(float inValue) {
        _a = inValue;
    }
    
    std::string MeshAttachment::getPath() {
        return _path;
    }
    
    void MeshAttachment::setPath(std::string inValue) {
        _path = inValue;
    }
    
    void* MeshAttachment::getRendererObject() {
        return _rendererObject;
    }
    
    void MeshAttachment::setRendererObject(void* inValue) {
        _rendererObject = inValue;
    }
    
    float MeshAttachment::getRegionU() {
        return _regionU;
    }
    
    void MeshAttachment::setRegionU(float inValue) {
        _regionU = inValue;
    }
    
    float MeshAttachment::getRegionV() {
        return _regionV;
    }
    
    void MeshAttachment::setRegionV(float inValue) {
        _regionV = inValue;
    }
    
    float MeshAttachment::getRegionU2() {
        return _regionU2;
    }
    
    void MeshAttachment::setRegionU2(float inValue) {
        _regionU2 = inValue;
    }
    
    float MeshAttachment::getRegionV2() {
        return _regionV2;
    }
    
    void MeshAttachment::setRegionV2(float inValue) {
        _regionV2 = inValue;
    }
    
    bool MeshAttachment::getRegionRotate() {
        return _regionRotate;
    }
    
    void MeshAttachment::setRegionRotate(float inValue) {
        _regionRotate = inValue;
    }
    
    float MeshAttachment::getRegionOffsetX() {
        return _regionOffsetX;
    }
    
    void MeshAttachment::setRegionOffsetX(float inValue) {
        _regionOffsetX = inValue;
    }
    
    float MeshAttachment::getRegionOffsetY() {
        return _regionOffsetY;
    }
    
    void MeshAttachment::setRegionOffsetY(float inValue) {
        _regionOffsetY = inValue;
    }
    
    float MeshAttachment::getRegionWidth() {
        return _regionWidth;
    }
    
    void MeshAttachment::setRegionWidth(float inValue) {
        _regionWidth = inValue;
    }
    
    float MeshAttachment::getRegionHeight() {
        return _regionHeight;
    }
    
    void MeshAttachment::setRegionHeight(float inValue) {
        _regionHeight = inValue;
    }
    
    float MeshAttachment::getRegionOriginalWidth() {
        return _regionOriginalWidth;
    }
    
    void MeshAttachment::setRegionOriginalWidth(float inValue) {
        _regionOriginalWidth = inValue;
    }
    
    float MeshAttachment::getRegionOriginalHeight() {
        return _regionOriginalHeight;
    }
    
    void MeshAttachment::setRegionOriginalHeight(float inValue) {
        _regionOriginalHeight = inValue;
    }
    
    bool MeshAttachment::getInheritDeform() {
        return _inheritDeform;
    }
    
    void MeshAttachment::setInheritDeform(bool inValue) {
        _inheritDeform = inValue;
    }
    
    MeshAttachment* MeshAttachment::getParentMesh() {
        return _parentMesh;
    }
    
    void MeshAttachment::setParentMesh(MeshAttachment* inValue) {
        _parentMesh = inValue;
        if (inValue != NULL) {
            _bones = inValue->_bones;
            _vertices = inValue->_vertices;
            _worldVerticesLength = inValue->_worldVerticesLength;
            _regionUVs = inValue->_regionUVs;
            _triangles = inValue->_triangles;
            _hullLength = inValue->_hullLength;
            _edges = inValue->_edges;
            _width = inValue->_width;
            _height = inValue->_height;
        }
    }
    
    Vector<short>& MeshAttachment::getEdges() {
        return _edges;
    }
    
    void MeshAttachment::setEdges(Vector<short>& inValue) {
        _edges = inValue;
    }
    
    float MeshAttachment::getWidth() {
        return _width;
    }
    
    void MeshAttachment::setWidth(float inValue) {
        _width = inValue;
    }
    
    float MeshAttachment::getHeight() {
        return _height;
    }
    
    void MeshAttachment::setHeight(float inValue) {
        _height = inValue;
    }
}
