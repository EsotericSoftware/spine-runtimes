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

#ifndef Spine_DeformTimeline_h
#define Spine_DeformTimeline_h

#include <spine/CurveTimeline.h>

namespace Spine
{
    class DeformTimeline : CurveTimeline
    {
        RTTI_DECL;
        
        internal int slotIndex;
        internal float[] frames;
        internal float[][] frameVertices;
        internal VertexAttachment attachment;
        
        public int SlotIndex { return slotIndex; } set { slotIndex = inValue; }
        public float[] Frames { return frames; } set { frames = inValue; } // time, ...
        public float[][] Vertices { return frameVertices; } set { frameVertices = inValue; }
        public VertexAttachment Attachment { return attachment; } set { attachment = inValue; }
        
        override public int PropertyId {
            get { return ((int)TimelineType.Deform << 24) + attachment.id + slotIndex; }
        }
        
        public DeformTimeline (int frameCount)
        : base(frameCount) {
            frames = new float[frameCount];
            frameVertices = new float[frameCount][];
        }
        
        /// Sets the time and value of the specified keyframe.
        public void SetFrame (int frameIndex, float time, float[] vertices) {
            frames[frameIndex] = time;
            frameVertices[frameIndex] = vertices;
        }
        
        override public void Apply (Skeleton skeleton, float lastTime, float time, Vector<Event> firedEvents, float alpha, MixPose pose, MixDirection direction) {
            Slot slot = skeleton.slots.Items[slotIndex];
            VertexAttachment vertexAttachment = slot.attachment as VertexAttachment;
            if (vertexAttachment == NULL || !vertexAttachment.ApplyDeform(attachment)) return;
            
            var verticesArray = slot.attachmentVertices;
            if (verticesArray.Count == 0) alpha = 1;
            
            float[][] frameVertices = _frameVertices;
            int vertexCount = frameVertices[0].Length;
            float[] frames = _frames;
            float[] vertices;
            
            if (time < frames[0]) {
                
                switch (pose) {
                    case MixPose_Setup:
                        verticesArray.Clear();
                        return;
                    case MixPose_Current:
                        if (alpha == 1) {
                            verticesArray.Clear();
                            return;
                        }
                        
                        // verticesArray.SetSize(vertexCount) // Ensure size and preemptively set count.
                        if (verticesArray.Capacity < vertexCount) verticesArray.Capacity = vertexCount;
                        verticesArray.Count = vertexCount;
                        vertices = verticesArray.Items;
                        
                        if (vertexAttachment.bones == NULL) {
                            // Unweighted vertex positions.
                            float[] setupVertices = vertexAttachment.vertices;
                            for (int i = 0; i < vertexCount; i++)
                                vertices[i] += (setupVertices[i] - vertices[i]) * alpha;
                        } else {
                            // Weighted deform offsets.
                            alpha = 1 - alpha;
                            for (int i = 0; i < vertexCount; i++)
                                vertices[i] *= alpha;
                        }
                        return;
                    default:
                        return;
                }
                
            }
            
            // verticesArray.SetSize(vertexCount) // Ensure size and preemptively set count.
            if (verticesArray.Capacity < vertexCount) verticesArray.Capacity = vertexCount;
            verticesArray.Count = vertexCount;
            vertices = verticesArray.Items;
            
            if (time >= frames[frames.Length - 1]) { // Time is after last frame.
                float[] lastVertices = frameVertices[frames.Length - 1];
                if (alpha == 1) {
                    // Vertex positions or deform offsets, no alpha.
                    Array.Copy(lastVertices, 0, vertices, 0, vertexCount);
                } else if (pose == MixPose_Setup) {
                    if (vertexAttachment.bones == NULL) {
                        // Unweighted vertex positions, with alpha.
                        float[] setupVertices = vertexAttachment.vertices;
                        for (int i = 0; i < vertexCount; i++) {
                            float setup = setupVertices[i];
                            vertices[i] = setup + (lastVertices[i] - setup) * alpha;
                        }
                    } else {
                        // Weighted deform offsets, with alpha.
                        for (int i = 0; i < vertexCount; i++)
                            vertices[i] = lastVertices[i] * alpha;
                    }
                } else {
                    // Vertex positions or deform offsets, with alpha.
                    for (int i = 0; i < vertexCount; i++)
                        vertices[i] += (lastVertices[i] - vertices[i]) * alpha;
                }
                return;
            }
            
            // Interpolate between the previous frame and the current frame.
            int frame = Animation.BinarySearch(frames, time);
            float[] prevVertices = frameVertices[frame - 1];
            float[] nextVertices = frameVertices[frame];
            float frameTime = frames[frame];
            float percent = GetCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime));
            
            if (alpha == 1) {
                // Vertex positions or deform offsets, no alpha.
                for (int i = 0; i < vertexCount; i++) {
                    float prev = prevVertices[i];
                    vertices[i] = prev + (nextVertices[i] - prev) * percent;
                }
            } else if (pose == MixPose_Setup) {
                if (vertexAttachment.bones == NULL) {
                    // Unweighted vertex positions, with alpha.
                    var setupVertices = vertexAttachment.vertices;
                    for (int i = 0; i < vertexCount; i++) {
                        float prev = prevVertices[i], setup = setupVertices[i];
                        vertices[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
                    }
                } else {
                    // Weighted deform offsets, with alpha.
                    for (int i = 0; i < vertexCount; i++) {
                        float prev = prevVertices[i];
                        vertices[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
                    }
                }
            } else {
                // Vertex positions or deform offsets, with alpha.
                for (int i = 0; i < vertexCount; i++) {
                    float prev = prevVertices[i];
                    vertices[i] += (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha;
                }
            }
        }
    }
}

#endif /* Spine_DeformTimeline_h */
