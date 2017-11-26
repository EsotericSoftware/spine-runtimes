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

#include <spine/SkeletonClipping.h>

#include <spine/Slot.h>
#include <spine/ClippingAttachment.h>

namespace Spine
{
    SkeletonClipping::SkeletonClipping() : _clipAttachment(NULL)
    {
        _clipOutput.reserve(128);
        _clippedVertices.reserve(128);
        _clippedTriangles.reserve(128);
        _clippedUVs.reserve(128);
    }
    
    int SkeletonClipping::clipStart(Slot& slot, ClippingAttachment* clip)
    {
        if (_clipAttachment != NULL)
        {
            return 0;
        }
        
        _clipAttachment = clip;

        int n = clip->getWorldVerticesLength();
        _clippingPolygon.reserve(n);
        clip->computeWorldVertices(slot, 0, n, _clippingPolygon, 0, 2);
        makeClockwise(_clippingPolygon);
        Vector< Vector<float>* > clippingPolygons = _triangulator.decompose(_clippingPolygon, _triangulator.triangulate(_clippingPolygon));
        
        _clippingPolygons = clippingPolygons;
        
        for (Vector<float>** i = _clippingPolygons.begin(); i != _clippingPolygons.end(); ++i)
        {
            Vector<float>* polygonP = (*i);
            Vector<float>& polygon = *polygonP;
            makeClockwise(polygon);
            polygon.push_back(polygon[0]);
            polygon.push_back(polygon[1]);
        }
        
        return static_cast<int>(_clippingPolygons.size());
    }
    
    void SkeletonClipping::clipEnd(Slot& slot)
    {
        if (_clipAttachment != NULL && _clipAttachment->_endSlot == &slot._data)
        {
            clipEnd();
        }
    }
    
    void SkeletonClipping::clipEnd()
    {
        if (_clipAttachment == NULL)
        {
            return;
        }
        
        _clipAttachment = NULL;
        _clippingPolygons.clear();
        _clippedVertices.clear();
        _clippedTriangles.clear();
        _clippingPolygon.clear();
    }
    
    void SkeletonClipping::clipTriangles(Vector<float>& vertices, int verticesLength, Vector<int>& triangles, int trianglesLength, Vector<float>& uvs)
    {
        Vector<float>& clipOutput = _clipOutput, clippedVertices = _clippedVertices;
        Vector<int>& clippedTriangles = _clippedTriangles;
        Vector< Vector<float>* >& polygons = _clippingPolygons;
        int polygonsCount = static_cast<int>(_clippingPolygons.size());

        int index = 0;
        clippedVertices.clear();
        _clippedUVs.clear();
        clippedTriangles.clear();
        
        for (int i = 0; i < trianglesLength; i += 3)
        {
            int vertexOffset = triangles[i] << 1;
            float x1 = vertices[vertexOffset], y1 = vertices[vertexOffset + 1];
            float u1 = uvs[vertexOffset], v1 = uvs[vertexOffset + 1];

            vertexOffset = triangles[i + 1] << 1;
            float x2 = vertices[vertexOffset], y2 = vertices[vertexOffset + 1];
            float u2 = uvs[vertexOffset], v2 = uvs[vertexOffset + 1];

            vertexOffset = triangles[i + 2] << 1;
            float x3 = vertices[vertexOffset], y3 = vertices[vertexOffset + 1];
            float u3 = uvs[vertexOffset], v3 = uvs[vertexOffset + 1];

            for (int p = 0; p < polygonsCount; p++)
            {
                int s = static_cast<int>(clippedVertices.size());
                if (clip(x1, y1, x2, y2, x3, y3, *polygons[p], clipOutput))
                {
                    int clipOutputLength = static_cast<int>(clipOutput.size());
                    if (clipOutputLength == 0)
                    {
                        continue;
                    }
                    float d0 = y2 - y3, d1 = x3 - x2, d2 = x1 - x3, d4 = y3 - y1;
                    float d = 1 / (d0 * d2 + d1 * (y1 - y3));

                    int clipOutputCount = clipOutputLength >> 1;
                    clippedVertices.reserve(s + clipOutputCount * 2);
                    _clippedUVs.reserve(s + clipOutputCount * 2);
                    for (int ii = 0; ii < clipOutputLength; ii += 2)
                    {
                        float x = clipOutput[ii], y = clipOutput[ii + 1];
                        clippedVertices[s] = x;
                        clippedVertices[s + 1] = y;
                        float c0 = x - x3, c1 = y - y3;
                        float a = (d0 * c0 + d1 * c1) * d;
                        float b = (d4 * c0 + d2 * c1) * d;
                        float c = 1 - a - b;
                        _clippedUVs[s] = u1 * a + u2 * b + u3 * c;
                        _clippedUVs[s + 1] = v1 * a + v2 * b + v3 * c;
                        s += 2;
                    }

                    s = static_cast<int>(clippedTriangles.size());
                    clippedTriangles.reserve(s + 3 * (clipOutputCount - 2));
                    clipOutputCount--;
                    for (int ii = 1; ii < clipOutputCount; ii++)
                    {
                        clippedTriangles[s] = index;
                        clippedTriangles[s + 1] = index + ii;
                        clippedTriangles[s + 2] = index + ii + 1;
                        s += 3;
                    }
                    index += clipOutputCount + 1;
                }
                else
                {
                    clippedVertices.reserve(s + 3 * 2);
                    _clippedUVs.reserve(s + 3 * 2);
                    clippedVertices[s] = x1;
                    clippedVertices[s + 1] = y1;
                    clippedVertices[s + 2] = x2;
                    clippedVertices[s + 3] = y2;
                    clippedVertices[s + 4] = x3;
                    clippedVertices[s + 5] = y3;

                    _clippedUVs[s] = u1;
                    _clippedUVs[s + 1] = v1;
                    _clippedUVs[s + 2] = u2;
                    _clippedUVs[s + 3] = v2;
                    _clippedUVs[s + 4] = u3;
                    _clippedUVs[s + 5] = v3;

                    s = static_cast<int>(clippedTriangles.size());
                    clippedTriangles.reserve(s + 3);
                    clippedTriangles[s] = index;
                    clippedTriangles[s + 1] = index + 1;
                    clippedTriangles[s + 2] = index + 2;
                    index += 3;
                    break;
                }
            }
        }
    }
    
    bool SkeletonClipping::isClipping()
    {
        return _clipAttachment != NULL;
    }
    
    bool SkeletonClipping::clip(float x1, float y1, float x2, float y2, float x3, float y3, Vector<float>& clippingArea, Vector<float>& output)
    {
        Vector<float> originalOutput = output;
        bool clipped = false;

        // Avoid copy at the end.
        Vector<float> input;
        if (clippingArea.size() % 4 >= 2)
        {
            input = output;
            output = _scratch;
        }
        else
        {
            input = _scratch;
        }

        input.clear();
        input.push_back(x1);
        input.push_back(y1);
        input.push_back(x2);
        input.push_back(y2);
        input.push_back(x3);
        input.push_back(y3);
        input.push_back(x1);
        input.push_back(y1);
        output.clear();

        Vector<float> clippingVertices = clippingArea;
        int clippingVerticesLast = static_cast<int>(clippingArea.size()) - 4;
        for (int i = 0; ; i += 2)
        {
            float edgeX = clippingVertices[i], edgeY = clippingVertices[i + 1];
            float edgeX2 = clippingVertices[i + 2], edgeY2 = clippingVertices[i + 3];
            float deltaX = edgeX - edgeX2, deltaY = edgeY - edgeY2;

            Vector<float> inputVertices = input;
            int inputVerticesLength = static_cast<int>(input.size()) - 2, outputStart = static_cast<int>(output.size());
            for (int ii = 0; ii < inputVerticesLength; ii += 2)
            {
                float inputX = inputVertices[ii], inputY = inputVertices[ii + 1];
                float inputX2 = inputVertices[ii + 2], inputY2 = inputVertices[ii + 3];
                bool side2 = deltaX * (inputY2 - edgeY2) - deltaY * (inputX2 - edgeX2) > 0;
                if (deltaX * (inputY - edgeY2) - deltaY * (inputX - edgeX2) > 0)
                {
                    if (side2)
                    {
                        // v1 inside, v2 inside
                        output.push_back(inputX2);
                        output.push_back(inputY2);
                        continue;
                    }
                    // v1 inside, v2 outside
                    float c0 = inputY2 - inputY, c2 = inputX2 - inputX;
                    float ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / (c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY));
                    output.push_back(edgeX + (edgeX2 - edgeX) * ua);
                    output.push_back(edgeY + (edgeY2 - edgeY) * ua);
                }
                else if (side2)
                {
                    // v1 outside, v2 inside
                    float c0 = inputY2 - inputY, c2 = inputX2 - inputX;
                    float ua = (c2 * (edgeY - inputY) - c0 * (edgeX - inputX)) / (c0 * (edgeX2 - edgeX) - c2 * (edgeY2 - edgeY));
                    output.push_back(edgeX + (edgeX2 - edgeX) * ua);
                    output.push_back(edgeY + (edgeY2 - edgeY) * ua);
                    output.push_back(inputX2);
                    output.push_back(inputY2);
                }
                clipped = true;
            }

            if (outputStart == output.size())
            {
                // All edges outside.
                originalOutput.clear();
                return true;
            }

            output.push_back(output[0]);
            output.push_back(output[1]);

            if (i == clippingVerticesLast)
            {
                break;
            }
            Vector<float> temp = output;
            output = input;
            output.clear();
            input = temp;
        }

        if (originalOutput != output)
        {
            originalOutput.clear();
            for (int i = 0, n = static_cast<int>(output.size()) - 2; i < n; ++i)
            {
                originalOutput.push_back(output[i]);
            }
        }
        else
        {
            originalOutput.reserve(originalOutput.size() - 2);
        }

        return clipped;
    }
    
    void SkeletonClipping::makeClockwise(Vector<float>& polygon)
    {
        int verticeslength = static_cast<int>(polygon.size());

        float area = polygon[verticeslength - 2] * polygon[1] - polygon[0] * polygon[verticeslength - 1], p1x, p1y, p2x, p2y;
        
        for (int i = 0, n = verticeslength - 3; i < n; i += 2)
        {
            p1x = polygon[i];
            p1y = polygon[i + 1];
            p2x = polygon[i + 2];
            p2y = polygon[i + 3];
            area += p1x * p2y - p2x * p1y;
        }
        
        if (area < 0)
        {
            return;
        }

        for (int i = 0, lastX = verticeslength - 2, n = verticeslength >> 1; i < n; i += 2)
        {
            float x = polygon[i], y = polygon[i + 1];
            int other = lastX - i;
            polygon[i] = polygon[other];
            polygon[i + 1] = polygon[other + 1];
            polygon[other] = x;
            polygon[other + 1] = y;
        }
    }
}
