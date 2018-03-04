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

#include <spine/SkeletonBounds.h>

#include <spine/Skeleton.h>
#include <spine/BoundingBoxAttachment.h>

#include <spine/Slot.h>
#include <spine/MathUtil.h>

namespace Spine {
    SkeletonBounds::SkeletonBounds() : _minX(0), _minY(0), _maxX(0), _maxY(0) {
        // Empty
    }
    
    void SkeletonBounds::update(Skeleton& skeleton, bool updateAabb) {
        Vector<Slot*>& slots = skeleton._slots;
        int slotCount = static_cast<int>(slots.size());

        _boundingBoxes.clear();
        for (int i = 0, n = static_cast<int>(_polygons.size()); i < n; ++i) {
            _polygonPool.push_back(_polygons[i]);
        }

        _polygons.clear();

        for (int i = 0; i < slotCount; i++) {
            Slot* slot = slots[i];
            Attachment* attachment = slot->_attachment;
            if (attachment == NULL || !attachment->getRTTI().derivesFrom(BoundingBoxAttachment::rtti)) {
                continue;
            }
            BoundingBoxAttachment* boundingBox = static_cast<BoundingBoxAttachment*>(attachment);
            _boundingBoxes.push_back(boundingBox);

            Polygon* polygonP = NULL;
            int poolCount = static_cast<int>(_polygonPool.size());
            if (poolCount > 0) {
                polygonP = _polygonPool[poolCount - 1];
                _polygonPool.erase(poolCount - 1);
            }
            else {
                Polygon* polygonP = NEW(Polygon);
                new (polygonP) Polygon();
            }

            _polygons.push_back(polygonP);

            Polygon& polygon = *polygonP;

            int count = boundingBox->getWorldVerticesLength();
            polygon._count = count;
            if (polygon._vertices.size() < count) {
                polygon._vertices.reserve(count);
            }
            boundingBox->computeWorldVertices(*slot, polygon._vertices);
        }

        if (updateAabb) {
            aabbCompute();
        }
        else {
            _minX = std::numeric_limits<int>::min();
            _minY = std::numeric_limits<int>::min();
            _maxX = std::numeric_limits<int>::max();
            _maxY = std::numeric_limits<int>::max();
        }
    }
    
    bool SkeletonBounds::aabbcontainsPoint(float x, float y) {
        return x >= _minX && x <= _maxX && y >= _minY && y <= _maxY;
    }
    
    bool SkeletonBounds::aabbintersectsSegment(float x1, float y1, float x2, float y2) {
        float minX = _minX;
        float minY = _minY;
        float maxX = _maxX;
        float maxY = _maxY;
        
        if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY)) {
            return false;
        }
        
        float m = (y2 - y1) / (x2 - x1);
        float y = m * (minX - x1) + y1;
        if (y > minY && y < maxY) {
            return true;
        }
        y = m * (maxX - x1) + y1;
        if (y > minY && y < maxY) {
            return true;
        }
        float x = (minY - y1) / m + x1;
        if (x > minX && x < maxX) {
            return true;
        }
        x = (maxY - y1) / m + x1;
        if (x > minX && x < maxX) {
            return true;
        }
        return false;
    }
    
    bool SkeletonBounds::aabbIntersectsSkeleton(SkeletonBounds bounds) {
        return _minX < bounds._maxX && _maxX > bounds._minX && _minY < bounds._maxY && _maxY > bounds._minY;
    }
    
    bool SkeletonBounds::containsPoint(Polygon* polygon, float x, float y) {
        Vector<float>& vertices = polygon->_vertices;
        int nn = polygon->_count;

        int prevIndex = nn - 2;
        bool inside = false;
        for (int ii = 0; ii < nn; ii += 2) {
            float vertexY = vertices[ii + 1];
            float prevY = vertices[prevIndex + 1];
            if ((vertexY < y && prevY >= y) || (prevY < y && vertexY >= y)) {
                float vertexX = vertices[ii];
                if (vertexX + (y - vertexY) / (prevY - vertexY) * (vertices[prevIndex] - vertexX) < x) {
                    inside = !inside;
                }
            }
            prevIndex = ii;
        }
        return inside;
    }
    
    BoundingBoxAttachment* SkeletonBounds::containsPoint(float x, float y) {
        for (int i = 0, n = static_cast<int>(_polygons.size()); i < n; ++i) {
            if (containsPoint(_polygons[i], x, y)) {
                return _boundingBoxes[i];
            }
        }
        
        return NULL;
    }
    
    BoundingBoxAttachment* SkeletonBounds::intersectsSegment(float x1, float y1, float x2, float y2) {
        for (int i = 0, n = static_cast<int>(_polygons.size()); i < n; ++i) {
            if (intersectsSegment(_polygons[i], x1, y1, x2, y2)) {
                return _boundingBoxes[i];
            }
        }
        return NULL;
    }
    
    bool SkeletonBounds::intersectsSegment(Polygon* polygon, float x1, float y1, float x2, float y2) {
        Vector<float>& vertices = polygon->_vertices;
        int nn = polygon->_count;

        float width12 = x1 - x2, height12 = y1 - y2;
        float det1 = x1 * y2 - y1 * x2;
        float x3 = vertices[nn - 2], y3 = vertices[nn - 1];
        for (int ii = 0; ii < nn; ii += 2) {
            float x4 = vertices[ii], y4 = vertices[ii + 1];
            float det2 = x3 * y4 - y3 * x4;
            float width34 = x3 - x4, height34 = y3 - y4;
            float det3 = width12 * height34 - height12 * width34;
            float x = (det1 * width34 - width12 * det2) / det3;
            if (((x >= x3 && x <= x4) || (x >= x4 && x <= x3)) && ((x >= x1 && x <= x2) || (x >= x2 && x <= x1))) {
                float y = (det1 * height34 - height12 * det2) / det3;
                if (((y >= y3 && y <= y4) || (y >= y4 && y <= y3)) && ((y >= y1 && y <= y2) || (y >= y2 && y <= y1))) {
                    return true;
                }
            }
            x3 = x4;
            y3 = y4;
        }
        
        return false;
    }
    
    Polygon* SkeletonBounds::getPolygon(BoundingBoxAttachment* attachment) {
        int index = _boundingBoxes.indexOf(attachment);
        
        return index == -1 ? NULL : _polygons[index];
    }
    
    float SkeletonBounds::getWidth() {
        return _maxX - _minX;
    }
    
    float SkeletonBounds::getHeight() {
        return _maxY - _minY;
    }
    
    void SkeletonBounds::aabbCompute() {
        float minX = std::numeric_limits<int>::min();
        float minY = std::numeric_limits<int>::min();
        float maxX = std::numeric_limits<int>::max();
        float maxY = std::numeric_limits<int>::max();
        
        for (int i = 0, n = static_cast<int>(_polygons.size()); i < n; ++i) {
            Polygon* polygon = _polygons[i];
            Vector<float>& vertices = polygon->_vertices;
            for (int ii = 0, nn = polygon->_count; ii < nn; ii += 2) {
                float x = vertices[ii];
                float y = vertices[ii + 1];
                minX = MIN(minX, x);
                minY = MIN(minY, y);
                maxX = MAX(maxX, x);
                maxY = MAX(maxY, y);
            }
        }
        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;
    }
}

