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

RTTI_IMPL(DeformTimeline, SlotCurveTimeline)

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


void DeformTimeline::apply(Skeleton &skeleton, float lastTime, float time, Array<Event *> *events, float alpha, MixFrom from, bool add, bool out,
						   bool appliedPose) {
	SP_UNUSED(lastTime);
	SP_UNUSED(events);
	SP_UNUSED(out);

	Array<Slot *> &slots = skeleton.getSlots();
	if (!_attachment->isTimelineActive(slots, getSlotIndex(), appliedPose)) return;
	Array<int> &timelineSlots = _attachment->getTimelineSlots();

	Array<float> &frames = _frames;
	if (time < frames[0]) {
		applyBeforeFirst(*slots[getSlotIndex()], appliedPose, alpha, from);
		for (size_t i = 0; i < timelineSlots.size(); ++i) applyBeforeFirst(*slots[timelineSlots[i]], appliedPose, alpha, from);
		return;
	}

	Array<float> *v1 = NULL;
	Array<float> *v2 = NULL;
	float percent = 0;
	if (time >= frames[frames.size() - 1]) {
		v1 = &_vertices[frames.size() - 1];
	} else {
		int frame = Animation::search(frames, time);
		percent = getCurvePercent(time, frame);
		v1 = &_vertices[frame];
		v2 = &_vertices[frame + 1];
	}

	size_t vertexCount = _vertices[0].size();
	applyToSlot(*slots[getSlotIndex()], appliedPose, *v1, v2, percent, vertexCount, alpha, from, add);
	for (size_t i = 0; i < timelineSlots.size(); ++i)
		applyToSlot(*slots[timelineSlots[i]], appliedPose, *v1, v2, percent, vertexCount, alpha, from, add);
}

void DeformTimeline::applyBeforeFirst(Slot &slot, bool appliedPose, float alpha, MixFrom from) {
	if (!slot.getBone().isActive()) return;
	SlotPose &pose = appliedPose ? slot.getAppliedPose() : slot.getPose();
	Attachment *attachment = pose.getAttachment();
	if (attachment == NULL || attachment->getTimelineAttachment() != _attachment) return;
	Array<float> &deformArray = pose.getDeform();
	if (deformArray.size() == 0) from = MixFrom_Setup;
	switch (from) {
		case MixFrom_Setup:
			deformArray.clear();
			break;
		case MixFrom_First: {
			if (alpha == 1) {
				deformArray.clear();
				return;
			}
			size_t vertexCount = _vertices[0].size();
			deformArray.setSize(vertexCount, 0);
			VertexAttachment *vertexAttachment = static_cast<VertexAttachment *>(attachment);
			if (vertexAttachment->getBones().size() == 0) {
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) deformArray[i] += (setupVertices[i] - deformArray[i]) * alpha;
			} else {
				float a = 1 - alpha;
				for (size_t i = 0; i < vertexCount; i++) deformArray[i] *= a;
			}
			break;
		}
		case MixFrom_Current:
			break;
	}
}

void DeformTimeline::applyToPose(SlotPose &pose, Array<float> &v1, Array<float> *v2, float percent, size_t vertexCount, float alpha, MixFrom from,
								 bool add) {
	Attachment *slotAttachment = pose.getAttachment();
	if (slotAttachment == NULL || !slotAttachment->getRTTI().instanceOf(VertexAttachment::rtti)) return;

	VertexAttachment *vertexAttachment = static_cast<VertexAttachment *>(slotAttachment);
	if (vertexAttachment->getTimelineAttachment() != _attachment) return;

	Array<float> &deformArray = pose.getDeform();
	if (deformArray.size() == 0) from = MixFrom_Setup;
	bool fromSetup = from == MixFrom_Setup;
	deformArray.setSize(vertexCount, 0);
	Array<float> &deform = deformArray;

	if (v2 == NULL) {
		if (alpha == 1) {
			if (add && !fromSetup) {
				if (vertexAttachment->getBones().size() == 0) {
					Array<float> &setupVertices = vertexAttachment->getVertices();
					for (size_t i = 0; i < vertexCount; i++) deform[i] += v1[i] - setupVertices[i];
				} else {
					for (size_t i = 0; i < vertexCount; i++) deform[i] += v1[i];
				}
			} else {
				memcpy(deform.buffer(), v1.buffer(), vertexCount * sizeof(float));
			}
		} else if (fromSetup) {
			if (vertexAttachment->getBones().size() == 0) {
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) {
					float setup = setupVertices[i];
					deform[i] = setup + (v1[i] - setup) * alpha;
				}
			} else {
				for (size_t i = 0; i < vertexCount; i++) deform[i] = v1[i] * alpha;
			}
		} else if (add) {
			if (vertexAttachment->getBones().size() == 0) {
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) deform[i] += (v1[i] - setupVertices[i]) * alpha;
			} else {
				for (size_t i = 0; i < vertexCount; i++) deform[i] += v1[i] * alpha;
			}
		} else {
			for (size_t i = 0; i < vertexCount; i++) deform[i] += (v1[i] - deform[i]) * alpha;
		}
		return;
	}

	if (alpha == 1) {
		if (add && !fromSetup) {
			if (vertexAttachment->getBones().size() == 0) {
				Array<float> &setupVertices = vertexAttachment->getVertices();
				for (size_t i = 0; i < vertexCount; i++) {
					float prev = v1[i];
					deform[i] += prev + ((*v2)[i] - prev) * percent - setupVertices[i];
				}
			} else {
				for (size_t i = 0; i < vertexCount; i++) {
					float prev = v1[i];
					deform[i] += prev + ((*v2)[i] - prev) * percent;
				}
			}
		} else if (percent == 0) {
			memcpy(deform.buffer(), v1.buffer(), vertexCount * sizeof(float));
		} else {
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = v1[i];
				deform[i] = prev + ((*v2)[i] - prev) * percent;
			}
		}
	} else if (fromSetup) {
		if (vertexAttachment->getBones().size() == 0) {
			Array<float> &setupVertices = vertexAttachment->getVertices();
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = v1[i], setup = setupVertices[i];
				deform[i] = setup + (prev + ((*v2)[i] - prev) * percent - setup) * alpha;
			}
		} else {
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = v1[i];
				deform[i] = (prev + ((*v2)[i] - prev) * percent) * alpha;
			}
		}
	} else if (add) {
		if (vertexAttachment->getBones().size() == 0) {
			Array<float> &setupVertices = vertexAttachment->getVertices();
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = v1[i];
				deform[i] += (prev + ((*v2)[i] - prev) * percent - setupVertices[i]) * alpha;
			}
		} else {
			for (size_t i = 0; i < vertexCount; i++) {
				float prev = v1[i];
				deform[i] += (prev + ((*v2)[i] - prev) * percent) * alpha;
			}
		}
	} else {
		for (size_t i = 0; i < vertexCount; i++) {
			float prev = v1[i];
			deform[i] += (prev + ((*v2)[i] - prev) * percent - deform[i]) * alpha;
		}
	}
}

void DeformTimeline::applyToSlot(Slot &slot, bool appliedPose, Array<float> &v1, Array<float> *v2, float percent, size_t vertexCount, float alpha,
								 MixFrom from, bool add) {
	if (!slot.getBone().isActive()) return;
	SlotPose &pose = appliedPose ? slot.getAppliedPose() : slot.getPose();
	Attachment *attachment = pose.getAttachment();
	if (attachment == NULL || attachment->getTimelineAttachment() != _attachment) return;
	applyToPose(pose, v1, v2, percent, vertexCount, alpha, from, add);
}

void DeformTimeline::_apply(Slot &slot, SlotPose &pose, float time, float alpha, MixFrom from, bool add) {
	Array<float> &frames = _frames;
	if (time < frames[0]) {
		Array<float> &deformArray = pose.getDeform();
		if (deformArray.size() == 0) from = MixFrom_Setup;
		switch (from) {
			case MixFrom_Setup:
				deformArray.clear();
				break;
			case MixFrom_First: {
				if (alpha == 1) {
					deformArray.clear();
					return;
				}
				size_t vertexCount = _vertices[0].size();
				deformArray.setSize(vertexCount, 0);
				VertexAttachment *vertexAttachment = static_cast<VertexAttachment *>(pose.getAttachment());
				if (vertexAttachment->getBones().size() == 0) {
					Array<float> &setupVertices = vertexAttachment->getVertices();
					for (size_t i = 0; i < vertexCount; i++) deformArray[i] += (setupVertices[i] - deformArray[i]) * alpha;
				} else {
					float a = 1 - alpha;
					for (size_t i = 0; i < vertexCount; i++) deformArray[i] *= a;
				}
				break;
			}
			case MixFrom_Current:
				break;
		}
		return;
	}

	Array<float> *v1 = NULL;
	Array<float> *v2 = NULL;
	float percent = 0;
	if (time >= frames[frames.size() - 1]) {
		v1 = &_vertices[frames.size() - 1];
	} else {
		int frame = Animation::search(frames, time);
		percent = getCurvePercent(time, frame);
		v1 = &_vertices[frame];
		v2 = &_vertices[frame + 1];
	}

	applyToPose(pose, *v1, v2, percent, _vertices[0].size(), alpha, from, add);
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
