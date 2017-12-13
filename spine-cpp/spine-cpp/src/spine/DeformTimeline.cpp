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

#include <spine/DeformTimeline.h>

#include <spine/Skeleton.h>
#include <spine/Event.h>

#include <spine/VertexAttachment.h>

#include <spine/Animation.h>
#include <spine/TimelineType.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>

namespace Spine {
    RTTI_IMPL(DeformTimeline, CurveTimeline);
    
    DeformTimeline::DeformTimeline(int frameCount) : CurveTimeline(frameCount), _slotIndex(0), _attachment(NULL) {
        _frames.reserve(frameCount);
        _frameVertices.reserve(frameCount);
        
        _frames.setSize(frameCount);
        
        for (int i = 0; i < frameCount; ++i) {
            Vector<float> vec;
            _frameVertices.push_back(vec);
        }
    }
    
    void DeformTimeline::apply(Skeleton& skeleton, float lastTime, float time, Vector<Event*>* pEvents, float alpha, MixPose pose, MixDirection direction) {
        Slot* slotP = skeleton._slots[_slotIndex];
        Slot& slot = *slotP;
        
        if (slot._attachment == NULL || !slot._attachment->getRTTI().derivesFrom(VertexAttachment::rtti)) {
            return;
        }
        
        VertexAttachment* vertexAttachment = static_cast<VertexAttachment*>(slot._attachment);
        if (!vertexAttachment->applyDeform(_attachment)) {
            return;
        }
        
        Vector<float>& vertices = slot._attachmentVertices;
        if (vertices.size() == 0) {
            alpha = 1;
        }
        
        int vertexCount = static_cast<int>(_frameVertices[0].size());
        
        if (time < _frames[0]) {
            switch (pose) {
                case MixPose_Setup:
                    vertices.clear();
                    return;
                case MixPose_Current:
                    if (alpha == 1) {
                        vertices.clear();
                        return;
                    }
                    
                    // Ensure size and preemptively set count.
                    vertices.reserve(vertexCount);
                    vertices.setSize(vertexCount);
                    
                    if (vertexAttachment->_bones.size() == 0) {
                        // Unweighted vertex positions.
                        Vector<float>& setupVertices = vertexAttachment->_vertices;
                        for (int i = 0; i < vertexCount; ++i) {
                            vertices[i] += (setupVertices[i] - vertices[i]) * alpha;
                        }
                    }
                    else {
                        // Weighted deform offsets.
                        alpha = 1 - alpha;
                        for (int i = 0; i < vertexCount; ++i) {
                            vertices[i] *= alpha;
                        }
                    }
                    return;
                default:
                    return;
            }
        }
        
        // Ensure size and preemptively set count.
        vertices.reserve(vertexCount);
        vertices.setSize(vertexCount);
        
        if (time >= _frames[_frames.size() - 1]) {
            // Time is after last frame.
            Vector<float>& lastVertices = _frameVertices[_frames.size() - 1];
            if (alpha == 1) {
                // Vertex positions or deform offsets, no alpha.
                vertices.clear();
                for (int i = 0; i < vertexCount; ++i) {
                    float vertex = lastVertices[i];
                    vertices.push_back(vertex);
                }
            }
            else if (pose == MixPose_Setup) {
                if (vertexAttachment->_bones.size() == 0) {
                    // Unweighted vertex positions, with alpha.
                    Vector<float>& setupVertices = vertexAttachment->_vertices;
                    for (int i = 0; i < vertexCount; i++) {
                        float setup = setupVertices[i];
                        vertices[i] = setup + (lastVertices[i] - setup) * alpha;
                    }
                }
                else {
                    // Weighted deform offsets, with alpha.
                    for (int i = 0; i < vertexCount; ++i) {
                        vertices[i] = lastVertices[i] * alpha;
                    }
                }
            }
            else {
                // Vertex positions or deform offsets, with alpha.
                for (int i = 0; i < vertexCount; ++i) {
                    vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
                }
            }
            return;
        }
        
        // Interpolate between the previous frame and the current frame.
        int frame = Animation::binarySearch(_frames, time);
        Vector<float>& prevVertices = _frameVertices[frame - 1];
        Vector<float>& nextVertices = _frameVertices[frame];
        float frameTime = _frames[frame];
        float percent = getCurvePercent(frame - 1, 1 - (time - frameTime) / (_frames[frame - 1] - frameTime));
        
        if (alpha == 1) {
            // Vertex positions or deform offsets, no alpha.
            for (int i = 0; i < vertexCount; ++i) {
                float prev = prevVertices[i];
                vertices[i] = prev + (nextVertices[i] - prev) * percent;
            }
        }
        else if (pose == MixPose_Setup) {
            if (vertexAttachment->_bones.size() == 0) {
                // Unweighted vertex positions, with alpha.
                Vector<float>& setupVertices = vertexAttachment->_vertices;
                for (int i = 0; i < vertexCount; ++i) {
                    float prev = prevVertices[i], setup = setupVertices[i];
                    vertices[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
                }
            }
            else {
                // Weighted deform offsets, with alpha.
                for (int i = 0; i < vertexCount; ++i) {
                    float prev = prevVertices[i];
                    vertices[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
                }
            }
        }
        else {
            // Vertex positions or deform offsets, with alpha.
            for (int i = 0; i < vertexCount; ++i) {
                float prev = prevVertices[i];
                vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
            }
        }
    }
    
    int DeformTimeline::getPropertyId() {
        assert(_attachment != NULL);
        
        return ((int)TimelineType_Deform << 24) + _attachment->_id + _slotIndex;
    }
    
    void DeformTimeline::setFrame(int frameIndex, float time, Vector<float>& vertices) {
        _frames[frameIndex] = time;
        _frameVertices[frameIndex] = vertices;
    }
    
    int DeformTimeline::getSlotIndex() {
        return _slotIndex;
    }
    
    void DeformTimeline::setSlotIndex(int inValue) {
        _slotIndex = inValue;
    }
    
    Vector<float>& DeformTimeline::getFrames() {
        return _frames;
    }
    
    void DeformTimeline::setFrames(Vector<float>& inValue) {
        _frames = inValue;
    }
    
    Vector< Vector<float> >& DeformTimeline::getVertices() {
        return _frameVertices;
    }
    
    void DeformTimeline::setVertices(Vector< Vector<float> >& inValue) {
        _frameVertices = inValue;
    }
    
    VertexAttachment* DeformTimeline::getAttachment() {
        return _attachment;
    }
    
    void DeformTimeline::setAttachment(VertexAttachment* inValue) {
        _attachment = inValue;
    }
}
