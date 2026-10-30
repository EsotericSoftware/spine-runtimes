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

#include <spine/Slot.h>

#include <spine/Bone.h>
#include <spine/Skeleton.h>
#include <spine/SlotData.h>
#include <spine/SlotPose.h>
#include <spine/Color.h>

using namespace spine;

Slot::Slot(SlotData &data, Skeleton &skeleton)
	: PosedGeneric<SlotData, SlotPose, SlotPose>(data), _skeleton(skeleton), _bone(*skeleton.getBones()[data._boneData._index]), _attachmentState(0) {

	if (data.getSetupPose().hasDarkColor()) {
		_pose._hasDarkColor = true;
		_constrainedPose._hasDarkColor = true;
	}
	setupPose();
}

Bone &Slot::getBone() {
	return _bone;
}

void Slot::setupPose() {
	_pose._color.set(_data._setupPose._color);
	if (_pose._hasDarkColor) _pose._darkColor.set(_data._setupPose._darkColor);
	_pose._sequenceIndex = _data._setupPose._sequenceIndex;
	if (_data._attachmentName.isEmpty())
		_pose.setAttachment(NULL);
	else {
		_pose._attachment = NULL;
		_pose.setAttachment(_skeleton.getAttachment(_data._index, _data._attachmentName));
	}
}
