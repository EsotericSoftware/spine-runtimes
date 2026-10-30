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

#include <spine/DrawOrder.h>

#include <spine/Slot.h>

using namespace spine;

DrawOrder::DrawOrder(Array<Slot *> &setupPose) : _setupPose(setupPose), _pose(), _constrainedPose(), _appliedPose(&_pose) {
}

void DrawOrder::setupPose() {
	_pose.clear();
	_pose.setSize(_setupPose.size(), NULL);
	for (size_t i = 0, n = _setupPose.size(); i < n; ++i) {
		_pose[i] = _setupPose[i];
	}
}

Array<Slot *> &DrawOrder::getPose() {
	return _pose;
}

Array<Slot *> &DrawOrder::getAppliedPose() {
	return *_appliedPose;
}

void DrawOrder::unconstrained() {
	_appliedPose = &_pose;
}

void DrawOrder::constrained() {
	_appliedPose = &_constrainedPose;
}

void DrawOrder::reset() {
	_constrainedPose.clear();
	_constrainedPose.setSize(_pose.size(), NULL);
	for (size_t i = 0, n = _pose.size(); i < n; ++i) {
		_constrainedPose[i] = _pose[i];
	}
}
