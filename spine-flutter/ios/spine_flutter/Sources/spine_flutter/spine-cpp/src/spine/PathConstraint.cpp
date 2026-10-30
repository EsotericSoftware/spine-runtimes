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

#include <spine/PathConstraint.h>

#include <spine/Bone.h>
#include <spine/BonePose.h>
#include <spine/PathAttachment.h>
#include <spine/PathConstraintData.h>
#include <spine/PathConstraintPose.h>
#include <spine/Skeleton.h>
#include <spine/Slot.h>
#include <spine/MathUtil.h>
#include <spine/Skin.h>
#include <spine/BoneData.h>
#include <spine/SlotData.h>
#include <spine/SkeletonData.h>

using namespace spine;

RTTI_IMPL(PathConstraint, Constraint)

const float PathConstraint::epsilon = 0.00001f;
const int PathConstraint::NONE = -1;
const int PathConstraint::BEFORE = -2;
const int PathConstraint::AFTER = -3;

PathConstraint::PathConstraint(PathConstraintData &data, Skeleton &skeleton) : PathConstraintBase(data) {

	_bones.ensureCapacity(data.getBones().size());
	for (size_t i = 0; i < data.getBones().size(); i++) {
		BoneData *boneData = data.getBones()[i];
		_bones.add(&skeleton._bones[boneData->getIndex()]->_constrainedPose);
	}

	_slot = skeleton._slots[data._slot->_index];
	_segments.setSize(10, 0);
}

PathConstraint &PathConstraint::copy(Skeleton &skeleton) {
	PathConstraint *copy = new (__FILE__, __LINE__) PathConstraint(_data, skeleton);
	copy->_pose.set(_pose);
	return *copy;
}

void PathConstraint::update(Skeleton &skeleton, Physics physics) {
	Attachment *baseAttachment = _slot->_appliedPose->_attachment;
	if (baseAttachment == NULL || !baseAttachment->getRTTI().instanceOf(PathAttachment::rtti)) {
		return;
	}
	PathAttachment *pathAttachment = static_cast<PathAttachment *>(baseAttachment);

	PathConstraintPose &p = *_appliedPose;
	float mixRotate = p._mixRotate, mixX = p._mixX, mixY = p._mixY;
	if (mixRotate == 0 && mixX == 0 && mixY == 0) return;

	PathConstraintData &data = _data;
	bool tangents = data._rotateMode == RotateMode_Tangent, scale = data._rotateMode == RotateMode_ChainScale;
	size_t boneCount = _bones.size();
	size_t spacesCount = tangents ? boneCount : boneCount + 1;
	BonePose **bones = _bones.buffer();
	_spaces.setSize(spacesCount, 0);
	float *spaces = _spaces.buffer();
	float *lengths = NULL;
	if (scale) {
		_lengths.setSize(boneCount, 0);
		lengths = _lengths.buffer();
	}
	float spacing = p._spacing;

	switch (data._spacingMode) {
		case SpacingMode_Percent: {
			if (scale) {
				for (size_t i = 0, n = spacesCount - 1; i < n; i++) {
					BonePose *bone = bones[i];
					float setupLength = bone->_bone->getData().getLength();
					float x = setupLength * bone->_a;
					float y = setupLength * bone->_c;
					lengths[i] = MathUtil::sqrt(x * x + y * y);
				}
			}
			for (size_t i = 1; i < spacesCount; i++) {
				spaces[i] = spacing;
			}
			break;
		}
		case SpacingMode_Proportional: {
			float sum = 0;
			for (size_t i = 0, n = spacesCount - 1; i < n;) {
				BonePose *bone = bones[i];
				float setupLength = bone->_bone->getData().getLength();
				if (setupLength < epsilon) {
					if (scale) lengths[i] = 0;
					spaces[++i] = spacing;
				} else {
					float x = setupLength * bone->_a, y = setupLength * bone->_c;
					float length = MathUtil::sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = length;
					sum += length;
				}
			}
			if (sum > 0) {
				sum = spacesCount / sum * spacing;
				for (size_t i = 1; i < spacesCount; i++) {
					spaces[i] *= sum;
				}
			}
			break;
		}
		default: {
			bool lengthSpacing = data._spacingMode == SpacingMode_Length;
			for (size_t i = 0, n = spacesCount - 1; i < n;) {
				BonePose *bone = bones[i];
				float setupLength = bone->_bone->getData().getLength();
				if (setupLength < epsilon) {
					if (scale) lengths[i] = 0;
					spaces[++i] = spacing;
				} else {
					float x = setupLength * bone->_a, y = setupLength * bone->_c;
					float length = MathUtil::sqrt(x * x + y * y);
					if (scale) lengths[i] = length;
					spaces[++i] = (lengthSpacing ? MathUtil::max(0.0f, setupLength + spacing) : spacing) * length / setupLength;
				}
			}
		}
	}

	Array<float> &positions = computeWorldPositions(skeleton, *pathAttachment, (int) spacesCount, tangents);
	float *positionsBuffer = positions.buffer();
	float boneX = positionsBuffer[0], boneY = positionsBuffer[1], offsetRotation = data._offsetRotation;
	bool tip;
	if (offsetRotation == 0)
		tip = data._rotateMode == RotateMode_Chain;
	else {
		tip = false;
		BonePose &bone = _slot->getBone().getAppliedPose();
		offsetRotation *= bone._a * bone._d - bone._b * bone._c > 0 ? MathUtil::Deg_Rad : -MathUtil::Deg_Rad;
	}
	for (size_t i = 0, ip = 3; i < boneCount; i++, ip += 3) {
		BonePose *bone = bones[i];
		bone->modifyWorld(skeleton);
		bone->_worldX += (boneX - bone->_worldX) * mixX;
		bone->_worldY += (boneY - bone->_worldY) * mixY;
		float x = positionsBuffer[ip], y = positionsBuffer[ip + 1], dx = x - boneX, dy = y - boneY;
		if (scale) {
			float length = lengths[i];
			if (length >= epsilon) {
				float s = (MathUtil::sqrt(dx * dx + dy * dy) / length - 1) * mixRotate + 1;
				bone->_a *= s;
				bone->_c *= s;
			}
		}
		boneX = x;
		boneY = y;
		if (mixRotate > 0) {
			float a = bone->_a, b = bone->_b, c = bone->_c, d = bone->_d, r, cos, sin;
			if (tangents)
				r = positionsBuffer[ip - 1];
			else if (spaces[i + 1] < epsilon)
				r = positionsBuffer[ip + 2];
			else
				r = MathUtil::atan2(dy, dx);
			r -= MathUtil::atan2(c, a);
			if (tip) {
				cos = MathUtil::cos(r);
				sin = MathUtil::sin(r);
				float length = bone->_bone->getData().getLength();
				boneX += (length * (cos * a - sin * c) - dx) * mixRotate;
				boneY += (length * (sin * a + cos * c) - dy) * mixRotate;
			} else
				r += offsetRotation;
			if (r > MathUtil::Pi)
				r -= MathUtil::Pi_2;
			else if (r < -MathUtil::Pi)
				r += MathUtil::Pi_2;
			r *= mixRotate;
			cos = MathUtil::cos(r);
			sin = MathUtil::sin(r);
			bone->_a = cos * a - sin * c;
			bone->_b = cos * b - sin * d;
			bone->_c = sin * a + cos * c;
			bone->_d = sin * b + cos * d;
		}
	}
}


void PathConstraint::sort(Skeleton &skeleton) {
	int slotIndex = _slot->getData().getIndex();
	Bone &slotBone = _slot->getBone();
	if (skeleton.getSkin() != NULL) sortPathSlot(skeleton, *skeleton.getSkin(), slotIndex, slotBone);
	if (skeleton.getData().getDefaultSkin() != NULL && skeleton.getData().getDefaultSkin() != skeleton.getSkin())
		sortPathSlot(skeleton, *skeleton.getData().getDefaultSkin(), slotIndex, slotBone);
	sortPath(skeleton, _slot->_pose._attachment, slotBone);
	BonePose **bones = _bones.buffer();
	size_t boneCount = _bones.size();
	for (size_t i = 0; i < boneCount; i++) {
		Bone *bone = bones[i]->_bone;
		skeleton.sortBone(bone);
		skeleton.constrained(*bone);
	}
	skeleton._updateCache.add(this);
	for (size_t i = 0; i < boneCount; i++) skeleton.sortReset(bones[i]->_bone->getChildren());
	for (size_t i = 0; i < boneCount; i++) bones[i]->_bone->_sorted = true;
}

bool PathConstraint::isSourceActive() {
	return _slot->getBone().isActive();
}

Array<BonePose *> &PathConstraint::getBones() {
	return _bones;
}

Slot &PathConstraint::getSlot() {
	return *_slot;
}

void PathConstraint::setSlot(Slot &slot) {
	_slot = &slot;
}

Array<float> &PathConstraint::computeWorldPositions(Skeleton &skeleton, PathAttachment &path, int spacesCount, bool tangents) {
	float position = _appliedPose->_position;
	float *spaces = _spaces.buffer();
	_positions.setSize(spacesCount * 3 + 2, 0);
	Array<float> &out = _positions;
	Array<float> &world = _world;
	bool closed = path.getClosed();
	int verticesLength = (int) path.getWorldVerticesLength();
	int curveCount = verticesLength / 6;
	int prevCurve = NONE;

	float pathLength;
	if (!path.getConstantSpeed()) {
		Array<float> &lengths = path.getLengths();
		float *lengthsBuffer = lengths.buffer();
		curveCount -= closed ? 1 : 2;
		pathLength = lengthsBuffer[curveCount];
		if (_data._positionMode == PositionMode_Percent) position *= pathLength;

		float multiplier = 0;
		switch (_data._spacingMode) {
			case SpacingMode_Percent:
				multiplier = pathLength;
				break;
			case SpacingMode_Proportional:
				multiplier = pathLength / spacesCount;
				break;
			default:
				multiplier = 1;
		}

		world.setSize(8, 0);
		float *worldBuffer = world.buffer();
		for (int i = 0, o = 0, curve = 0; i < spacesCount; i++, o += 3) {
			float space = spaces[i] * multiplier;
			position += space;
			float p = position;

			if (closed) {
				p = MathUtil::fmod(p, pathLength);
				if (p < 0) p += pathLength;
				curve = 0;
			} else if (p < 0) {
				if (prevCurve != BEFORE) {
					prevCurve = BEFORE;
					path.computeWorldVertices(skeleton, *_slot, 2, 4, world, 0, 2);
				}
				addBeforePosition(p, world, 0, out, o);
				continue;
			} else if (p > pathLength) {
				if (prevCurve != AFTER) {
					prevCurve = AFTER;
					path.computeWorldVertices(skeleton, *_slot, verticesLength - 6, 4, world, 0, 2);
				}
				addAfterPosition(p - pathLength, world, 0, out, o);
				continue;
			}

			// Determine curve containing position.
			for (;; curve++) {
				float length = lengthsBuffer[curve];
				if (p > length) continue;
				if (curve == 0)
					p /= length;
				else {
					float prev = lengthsBuffer[curve - 1];
					p = (p - prev) / (length - prev);
				}
				break;
			}
			if (curve != prevCurve) {
				prevCurve = curve;
				if (closed && curve == curveCount) {
					path.computeWorldVertices(skeleton, *_slot, verticesLength - 4, 4, world, 0, 2);
					path.computeWorldVertices(skeleton, *_slot, 0, 4, world, 4, 2);
				} else
					path.computeWorldVertices(skeleton, *_slot, curve * 6 + 2, 8, world, 0, 2);
			}
			addCurvePosition(p, worldBuffer[0], worldBuffer[1], worldBuffer[2], worldBuffer[3], worldBuffer[4], worldBuffer[5], worldBuffer[6],
							 worldBuffer[7], out, o, tangents || (i > 0 && space < epsilon));
		}
		return out;
	}

	// World vertices.
	if (closed) {
		verticesLength += 2;
		world.setSize(verticesLength, 0);
		float *worldBuffer = world.buffer();
		path.computeWorldVertices(skeleton, *_slot, 2, verticesLength - 4, world, 0, 2);
		path.computeWorldVertices(skeleton, *_slot, 0, 2, world, verticesLength - 4, 2);
		worldBuffer[verticesLength - 2] = worldBuffer[0];
		worldBuffer[verticesLength - 1] = worldBuffer[1];
	} else {
		curveCount--;
		verticesLength -= 4;
		world.setSize(verticesLength, 0);
		path.computeWorldVertices(skeleton, *_slot, 2, verticesLength, world, 0, 2);
	}
	float *worldBuffer = world.buffer();

	// Curve lengths.
	_curves.setSize(curveCount, 0);
	float *curvesBuffer = _curves.buffer();
	pathLength = 0;
	float x1 = worldBuffer[0], y1 = worldBuffer[1], cx1 = 0, cy1 = 0, cx2 = 0, cy2 = 0, x2 = 0, y2 = 0;
	float tmpx, tmpy, dddfx, dddfy, ddfx, ddfy, dfx, dfy;
	for (int i = 0, w = 2; i < curveCount; i++, w += 6) {
		cx1 = worldBuffer[w];
		cy1 = worldBuffer[w + 1];
		cx2 = worldBuffer[w + 2];
		cy2 = worldBuffer[w + 3];
		x2 = worldBuffer[w + 4];
		y2 = worldBuffer[w + 5];
		tmpx = (x1 - cx1 * 2 + cx2) * 0.1875f;
		tmpy = (y1 - cy1 * 2 + cy2) * 0.1875f;
		dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375f;
		dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375f;
		ddfx = tmpx * 2 + dddfx;
		ddfy = tmpy * 2 + dddfy;
		dfx = (cx1 - x1) * 0.75f + tmpx + dddfx * 0.16666667f;
		dfy = (cy1 - y1) * 0.75f + tmpy + dddfy * 0.16666667f;
		pathLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
		dfx += ddfx;
		dfy += ddfy;
		ddfx += dddfx;
		ddfy += dddfy;
		pathLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
		dfx += ddfx;
		dfy += ddfy;
		pathLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
		dfx += ddfx + dddfx;
		dfy += ddfy + dddfy;
		pathLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
		curvesBuffer[i] = pathLength;
		x1 = x2;
		y1 = y2;
	}

	if (_data._positionMode == PositionMode_Percent) position *= pathLength;

	float multiplier = 0;
	switch (_data._spacingMode) {
		case SpacingMode_Percent:
			multiplier = pathLength;
			break;
		case SpacingMode_Proportional:
			multiplier = pathLength / spacesCount;
			break;
		default:
			multiplier = 1;
	}

	float curveLength = 0;
	for (int i = 0, o = 0, curve = 0, segment = 0; i < spacesCount; i++, o += 3) {
		float space = spaces[i] * multiplier;
		position += space;
		float p = position;

		if (closed) {
			p = MathUtil::fmod(p, pathLength);
			if (p < 0) p += pathLength;
			curve = 0;
			segment = 0;
		} else if (p < 0) {
			addBeforePosition(p, world, 0, out, o);
			continue;
		} else if (p > pathLength) {
			addAfterPosition(p - pathLength, world, verticesLength - 4, out, o);
			continue;
		}

		// Determine curve containing position.
		for (;; curve++) {
			float length = curvesBuffer[curve];
			if (p > length) continue;
			if (curve == 0)
				p /= length;
			else {
				float prev = curvesBuffer[curve - 1];
				p = (p - prev) / (length - prev);
			}
			break;
		}

		// Curve segment lengths.
		if (curve != prevCurve) {
			prevCurve = curve;
			int ii = curve * 6;
			x1 = worldBuffer[ii];
			y1 = worldBuffer[ii + 1];
			cx1 = worldBuffer[ii + 2];
			cy1 = worldBuffer[ii + 3];
			cx2 = worldBuffer[ii + 4];
			cy2 = worldBuffer[ii + 5];
			x2 = worldBuffer[ii + 6];
			y2 = worldBuffer[ii + 7];
			tmpx = (x1 - cx1 * 2 + cx2) * 0.03f;
			tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
			dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f;
			dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
			ddfx = tmpx * 2 + dddfx;
			ddfy = tmpy * 2 + dddfy;
			dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f;
			dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
			curveLength = MathUtil::sqrt(dfx * dfx + dfy * dfy);
			_segments[0] = curveLength;
			for (ii = 1; ii < 8; ii++) {
				dfx += ddfx;
				dfy += ddfy;
				ddfx += dddfx;
				ddfy += dddfy;
				curveLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
				_segments[ii] = curveLength;
			}
			dfx += ddfx;
			dfy += ddfy;
			curveLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
			_segments[8] = curveLength;
			dfx += ddfx + dddfx;
			dfy += ddfy + dddfy;
			curveLength += MathUtil::sqrt(dfx * dfx + dfy * dfy);
			_segments[9] = curveLength;
			segment = 0;
		}

		// Weight by segment length.
		p *= curveLength;
		for (;; segment++) {
			float length = _segments[segment];
			if (p > length) continue;
			if (segment == 0)
				p /= length;
			else {
				float prev = _segments[segment - 1];
				p = segment + (p - prev) / (length - prev);
			}
			break;
		}
		addCurvePosition(p * 0.1f, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents || (i > 0 && space < epsilon));
	}

	return out;
}

void PathConstraint::addBeforePosition(float p, Array<float> &temp, int i, Array<float> &output, int o) {
	float x1 = temp[i], y1 = temp[i + 1], dx = temp[i + 2] - x1, dy = temp[i + 3] - y1, r = MathUtil::atan2(dy, dx);
	output[o] = x1 + p * MathUtil::cos(r);
	output[o + 1] = y1 + p * MathUtil::sin(r);
	output[o + 2] = r;
}

void PathConstraint::addAfterPosition(float p, Array<float> &temp, int i, Array<float> &output, int o) {
	float x1 = temp[i + 2], y1 = temp[i + 3], dx = x1 - temp[i], dy = y1 - temp[i + 1], r = MathUtil::atan2(dy, dx);
	output[o] = x1 + p * MathUtil::cos(r);
	output[o + 1] = y1 + p * MathUtil::sin(r);
	output[o + 2] = r;
}

void PathConstraint::addCurvePosition(float p, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2,
									  Array<float> &output, int o, bool tangents) {
	if (p < epsilon || MathUtil::isNan(p)) {
		output[o] = x1;
		output[o + 1] = y1;
		output[o + 2] = MathUtil::atan2(cy1 - y1, cx1 - x1);
		return;
	}
	float tt = p * p, ttt = tt * p, u = 1 - p, uu = u * u, uuu = uu * u;
	float ut = u * p, ut3 = ut * 3, uut3 = u * ut3, utt3 = ut3 * p;
	float x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt, y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt;
	output[o] = x;
	output[o + 1] = y;
	if (tangents) {
		if (p < 0.001f)
			output[o + 2] = MathUtil::atan2(cy1 - y1, cx1 - x1);
		else
			output[o + 2] = MathUtil::atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt));
	}
}

void PathConstraint::sortPathSlot(Skeleton &skeleton, Skin &skin, int slotIndex, Bone &slotBone) {
	Skin::AttachmentMap::Entries entries = skin.getAttachments();
	while (entries.hasNext()) {
		Skin::AttachmentMap::Entry &entry = entries.next();
		if (entry._slotIndex == (size_t) slotIndex) sortPath(skeleton, entry._attachment, slotBone);
	}
}

void PathConstraint::sortPath(Skeleton &skeleton, Attachment *attachment, Bone &slotBone) {
	if (attachment == NULL || !attachment->getRTTI().instanceOf(PathAttachment::rtti)) return;
	PathAttachment *pathAttachment = static_cast<PathAttachment *>(attachment);
	Array<int> &pathBones = pathAttachment->getBones();
	if (pathBones.size() == 0)
		skeleton.sortBone(&slotBone);
	else {
		Array<Bone *> &bones = skeleton._bones;
		for (size_t i = 0, n = pathBones.size(); i < n;) {
			int nn = pathBones[i++];
			nn += i;
			while (i < (size_t) nn) skeleton.sortBone(bones[pathBones[i++]]);
		}
	}
}