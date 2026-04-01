/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#include <spine/DeformTimeline.h>

#include <spine/Event.h>
#include <spine/Skeleton.h>
#include <spine/VertexAttachment.h>
#include <spine/Animation.h>
#include <spine/Bone.h>
#include <spine/Property.h>
#include <spine/Slot.h>
#include <spine/SlotData.h>
#include <spine/SlotPose.h>

using namespace spine;

RTTI_IMPL(DeformTimeline, CurveTimeline)

DeformTimeline::DeformTimeline(size_t frameCount, size_t bezierCount, int slotIndex, VertexAttachment &attachment)
	: SlotCurveTimeline(frameCount, 1, bezierCount, slotIndex), _attachment(&attachment) {
	PropertyId ids[] = {((PropertyId) Property_Deform << 32) | ((slotIndex << 16 | attachment._id) & 0xffffffff)};
	setPropertyIds(ids, 1);
	_additive = true;

	_vertices.ensureCapacity(frameCount);
	for (size_t i = 0; i < frameCount; ++i) {
		Array<float> vec;
		_vertices.add(vec);
	}
}


void DeformTimeline::_apply(Slot &slot, SlotPose &pose, float time, float alpha, bool fromSetup, bool add) {
	SP_UNUSED(slot);
	Attachment *slotAttachment = pose._attachment;
	if (slotAttachment == NULL || !slotAttachment->getRTTI().instanceOf(VertexAttachment::rtti)) {
		return;
	}

	VertexAttachment *vertexAttachment = static_cast<VertexAttachment *>(slotAttachment);
	if (vertexAttachment->getTimelineAttachment() != _attachment) {
		return;
	}

	Array<float> &deformArray = pose._deform;
	if (deformArray.size() == 0) fromSetup = true;

	Array<Array<float>> &vertices = _vertices;
	size_t vertexCount = vertices[0].size();

	Array<float> &frames = _frames;
	if (time < frames[0]) {
		if (fromSetup) deformArray.clear();
		return;
	}

	deformArray.setSize(vertexCount, 0);
	Array<float> &deform = deformArray;

	if (time >= frames[frames.size() - 1]) {// Time is after last frame.
		Array<float> &lastVertices = vertices[frames.size() - 1];
		if (alpha == 1) {
			if (add && !fromSetup) {
				if (vertexAttachment->getBones().size() == 0) {
					// Unweighted vertex positions, no alpha.
					Array<float> &setupVertices = vertexAttachment->getVertices();
					for (size_t i = 0; i < vertexCount; i++) deform[i] += lastVertices[i] - setupVertices[i];
				} else {
					// Weighted deform offsets, no alpha.
					for (size_t i = 0; i < vertexCount; i++) deform[i] += lastVertices[i];
				}
			} else {
				// Vertex positions or deform offsets, no alpha.
				memcpy(deform.buffer(), lastVertices.buffer(), vertexCount * sizeof(float));
			}
		} else if (fromSetup) {
			if (vertexAttachment->getBones().size() == 0) {
				// Unweighted vertex positions, with alpha.
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) {
					float setup = setupVertices[i];
					deform[i] = setup + (lastVertices[i] - setup) * alpha;
				}
			} else {
				// Weighted deform offsets, with alpha.
				for (size_t i = 0; i < vertexCount; i++) deform[i] = lastVertices[i] * alpha;
			}
		} else if (add) {
			if (vertexAttachment->getBones().size() == 0) {
				// Unweighted vertex positions, with alpha.
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) deform[i] += (lastVertices[i] - setupVertices[i]) * alpha;
			} else {
				// Weighted deform offsets, with alpha.
				for (size_t i = 0; i < vertexCount; i++) deform[i] += lastVertices[i] * alpha;
			}
		} else {
			// Vertex positions or deform offsets, with alpha.
			for (size_t i = 0; i < vertexCount; i++) deform[i] += (lastVertices[i] - deform[i]) * alpha;
		}
		return;
	}

	// Interpolate between the previous frame and the current frame.
	int frame = Animation::search(frames, time);
	float percent = getCurvePercent(time, frame);
	Array<float> &prevVertices = vertices[frame];
	Array<float> &nextVertices = vertices[frame + 1];

	if (alpha == 1) {
		if (add && !fromSetup) {
			if (vertexAttachment->getBones().size() == 0) {
				// Unweighted vertex positions, no alpha.
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deform[i] += prev + (nextVertices[i] - prev) * percent - setupVertices[i];
				}
			} else {
				// Weighted deform offsets, no alpha.
				for (size_t i = 0; i < vertexCount; i++) {
					float prev = prevVertices[i];
					deform[i] += prev + (nextVertices[i] - prev) * percent;
				}
			}
		} else if (percent == 0) {
			memcpy(deform.buffer(), prevVertices.buffer(), vertexCount * sizeof(float));
		} else {
			// Vertex positions or deform offsets, no alpha.
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = prevVertices[i];
				deform[i] = prev + (nextVertices[i] - prev) * percent;
			}
		}
	} else if (fromSetup) {
		if (vertexAttachment->getBones().size() == 0) {
			// Unweighted vertex positions, with alpha.
			Array<float> &setupVertices = vertexAttachment->getVertices();
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = prevVertices[i], setup = setupVertices[i];
				deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha;
			}
		} else {
			// Weighted deform offsets, with alpha.
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = prevVertices[i];
				deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha;
			}
		}
	} else if (add) {
		if (vertexAttachment->getBones().size() == 0) {
			// Unweighted vertex positions, with alpha.
			Array<float> &setupVertices = vertexAttachment->getVertices();
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = prevVertices[i];
				deform[i] += (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha;
			}
		} else {
			// Weighted deform offsets, with alpha.
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = prevVertices[i];
				deform[i] += (prev + (nextVertices[i] - prev) * percent) * alpha;
			}
		}
	} else {
		// Vertex positions or deform offsets, with alpha.
		for (size_t i = 0; i < vertexCount; i++) {
			float prev = prevVertices[i];
			deform[i] += (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha;
		}
	}
}

void DeformTimeline::setBezier(size_t bezier, size_t frame, float value, float time1, float value1, float cx1, float cy1, float cx2, float cy2,
							   float time2, float value2) {
	SP_UNUSED(value1);
	SP_UNUSED(value2);
	Array<float> &curves = _curves;
	size_t i = getFrameCount() + bezier * BEZIER_SIZE;
	if (value == 0) curves[frame] = BEZIER + (float) i;
	float tmpx = (time1 - cx1 * 2 + cx2) * 0.03f, tmpy = cy2 * 0.03f - cy1 * 0.06f;
	float dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006f, dddy = (cy1 - cy2 + 0.33333333f) * 0.018f;
	float ddx = tmpx * 2 + dddx, ddy = tmpy * 2 + dddy;
	float dx = (cx1 - time1) * 0.3f + tmpx + dddx * 0.16666667f, dy = cy1 * 0.3f + tmpy + dddy * 0.16666667f;
	float x = time1 + dx, y = dy;
	for (size_t n = i + BEZIER_SIZE; i < n; i += 2) {
		curves[i] = x;
		curves[i + 1] = y;
		dx += ddx;
		dy += ddy;
		ddx += dddx;
		ddy += dddy;
		x += dx;
		y += dy;
	}
}

float DeformTimeline::getCurvePercent(float time, int frame) {
	Array<float> &curves = _curves;
	int i = (int) curves[frame];
	switch (i) {
		case LINEAR: {
			float x = _frames[frame];
			return (time - x) / (_frames[frame + getFrameEntries()] - x);
		}
		case STEPPED: {
			return 0;
		}
	}
	i -= BEZIER;
	if (curves[i] > time) {
		float x = _frames[frame];
		return curves[i + 1] * (time - x) / (curves[i] - x);
	}
	int n = i + BEZIER_SIZE;
	for (i += 2; i < n; i += 2) {
		if (curves[i] >= time) {
			float x = curves[i - 2], y = curves[i - 1];
			return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y);
		}
	}
	float x = curves[n - 2], y = curves[n - 1];
	return y + (1 - y) * (time - x) / (_frames[frame + getFrameEntries()] - x);
}

void DeformTimeline::setFrame(int frame, float time, Array<float> &vertices) {
	_frames[frame] = time;
	_vertices[frame].clear();
	_vertices[frame].addAll(vertices);
}

Array<Array<float>> &DeformTimeline::getVertices() {
	return _vertices;
}

VertexAttachment &DeformTimeline::getAttachment() {
	return *_attachment;
}

void DeformTimeline::setAttachment(VertexAttachment &inValue) {
	_attachment = &inValue;
}
