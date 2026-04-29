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

#include <spine/VertexAttachment.h>

#include <spine/Slot.h>

#include <spine/Bone.h>
#include <spine/Skeleton.h>

using namespace spine;

RTTI_IMPL(VertexAttachment, Attachment)

VertexAttachment::VertexAttachment(const String &name) : Attachment(name), _worldVerticesLength(0), _id(getNextID()) {
}

VertexAttachment::~VertexAttachment() {
}


void VertexAttachment::computeWorldVertices(Skeleton &skeleton, Slot &slot, size_t start, size_t count, Array<float> &worldVertices, size_t offset,
											size_t stride) {
	computeWorldVertices(skeleton, slot, start, count, worldVertices.buffer(), offset, stride);
}

void VertexAttachment::computeWorldVertices(Skeleton &skeleton, Slot &slot, size_t start, size_t count, float *worldVertices, size_t offset,
											size_t stride) {
	count = offset + (count >> 1) * stride;
	Array<float> *deformArray = &slot.getAppliedPose().getDeform();
	Array<float> *vertices = &_vertices;
	Array<int> &bones = _bones;
	if (bones.size() == 0) {
		if (deformArray->size() > 0) vertices = deformArray;

		BonePose &bone = slot.getBone().getAppliedPose();
		float x = bone.getWorldX();
		float y = bone.getWorldY();
		float a = bone.getA(), b = bone.getB(), c = bone.getC(), d = bone.getD();
		for (size_t vv = start, w = offset; w < count; vv += 2, w += stride) {
			float vx = (*vertices)[vv];
			float vy = (*vertices)[vv + 1];
			worldVertices[w] = vx * a + vy * b + x;
			worldVertices[w + 1] = vx * c + vy * d + y;
		}
		return;
	}

	int v = 0, skip = 0;
	for (size_t i = 0; i < start; i += 2) {
		int n = (int) bones[v];
		v += n + 1;
		skip += n;
	}

	Array<Bone *> &skeletonBones = skeleton.getBones();
	if (deformArray->size() == 0) {
		for (size_t w = offset, b = skip * 3; w < count; w += stride) {
			float wx = 0, wy = 0;
			int n = (int) bones[v++];
			n += v;
			for (; v < n; v++, b += 3) {
				Bone *boneP = skeletonBones[bones[v]];
				BonePose &bonePose = boneP->getAppliedPose();
				float vx = (*vertices)[b];
				float vy = (*vertices)[b + 1];
				float weight = (*vertices)[b + 2];
				wx += (vx * bonePose.getA() + vy * bonePose.getB() + bonePose.getWorldX()) * weight;
				wy += (vx * bonePose.getC() + vy * bonePose.getD() + bonePose.getWorldY()) * weight;
			}
			worldVertices[w] = wx;
			worldVertices[w + 1] = wy;
		}
	} else {
		for (size_t w = offset, b = skip * 3, f = skip << 1; w < count; w += stride) {
			float wx = 0, wy = 0;
			int n = (int) bones[v++];
			n += v;
			for (; v < n; v++, b += 3, f += 2) {
				Bone *boneP = skeletonBones[bones[v]];
				BonePose &bonePose = boneP->getAppliedPose();
				float vx = (*vertices)[b] + (*deformArray)[f];
				float vy = (*vertices)[b + 1] + (*deformArray)[f + 1];
				float weight = (*vertices)[b + 2];
				wx += (vx * bonePose.getA() + vy * bonePose.getB() + bonePose.getWorldX()) * weight;
				wy += (vx * bonePose.getC() + vy * bonePose.getD() + bonePose.getWorldY()) * weight;
			}
			worldVertices[w] = wx;
			worldVertices[w + 1] = wy;
		}
	}
}

int VertexAttachment::getId() {
	return _id;
}

Array<int> &VertexAttachment::getBones() {
	return _bones;
}

void VertexAttachment::setBones(Array<int> &bones) {
	_bones.clearAndAddAll(bones);
}

Array<float> &VertexAttachment::getVertices() {
	return _vertices;
}

void VertexAttachment::setVertices(Array<float> &vertices) {
	_vertices.clearAndAddAll(vertices);
}

size_t VertexAttachment::getWorldVerticesLength() {
	return _worldVerticesLength;
}

void VertexAttachment::setWorldVerticesLength(size_t inValue) {
	_worldVerticesLength = inValue;
}

Attachment *VertexAttachment::getTimelineAttachment() {
	return Attachment::getTimelineAttachment();
}

void VertexAttachment::setTimelineAttachment(Attachment *attachment) {
	Attachment::setTimelineAttachment(attachment);
}

int VertexAttachment::getNextID() {
	static int nextID = 0;
	return nextID++;
}

void VertexAttachment::copyTo(VertexAttachment &other) {
	other._bones.clearAndAddAll(this->_bones);
	other._vertices.clearAndAddAll(this->_vertices);
	other._worldVerticesLength = this->_worldVerticesLength;
	other.setTimelineAttachment(this->getTimelineAttachment());
	other.setTimelineSlots(this->getTimelineSlots());
}
