/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
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

#include <spine/SlotPose.h>
#include <spine/Attachment.h>
#include <spine/VertexAttachment.h>

using namespace spine;

SlotPose::SlotPose() : _color(1, 1, 1, 1), _darkColor(0, 0, 0, 0), _hasDarkColor(false), _attachment(nullptr), _sequenceIndex(0) {
}

SlotPose::~SlotPose() {
}

void SlotPose::set(SlotPose &pose) {
	_color.set(pose._color);
	if (pose._hasDarkColor) _darkColor.set(pose._darkColor);
	_hasDarkColor = pose._hasDarkColor;
	_attachment = pose._attachment;
	_sequenceIndex = pose._sequenceIndex;
	_deform.clear();
	_deform.addAll(pose._deform);
}

Color &SlotPose::getColor() {
	return _color;
}

Color &SlotPose::getDarkColor() {
	return _darkColor;
}

bool SlotPose::hasDarkColor() {
	return _hasDarkColor;
}

void SlotPose::setHasDarkColor(bool hasDarkColor) {
	_hasDarkColor = hasDarkColor;
}

Attachment *SlotPose::getAttachment() {
	return _attachment;
}

void SlotPose::setAttachment(Attachment *attachment) {
	if (_attachment == attachment) return;

	// Check if we need to clear deform based on timeline attachment.
	if (!attachment || !_attachment || attachment->getTimelineAttachment() != _attachment->getTimelineAttachment()) _deform.clear();
	_attachment = attachment;
	_sequenceIndex = -1;
}

int SlotPose::getSequenceIndex() {
	return _sequenceIndex;
}

void SlotPose::setSequenceIndex(int sequenceIndex) {
	_sequenceIndex = sequenceIndex;
}

Array<float> &SlotPose::getDeform() {
	return _deform;
}