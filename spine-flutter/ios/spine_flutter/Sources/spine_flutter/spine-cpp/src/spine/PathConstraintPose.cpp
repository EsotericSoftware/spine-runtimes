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

#include <spine/PathConstraintPose.h>

using namespace spine;

PathConstraintPose::PathConstraintPose() : Pose<PathConstraintPose>(), _position(0), _spacing(0), _mixRotate(0), _mixX(0), _mixY(0) {
}

PathConstraintPose::~PathConstraintPose() {
}

void PathConstraintPose::set(PathConstraintPose &pose) {
	_position = pose._position;
	_spacing = pose._spacing;
	_mixRotate = pose._mixRotate;
	_mixX = pose._mixX;
	_mixY = pose._mixY;
}

float PathConstraintPose::getPosition() {
	return _position;
}

void PathConstraintPose::setPosition(float position) {
	_position = position;
}

float PathConstraintPose::getSpacing() {
	return _spacing;
}

void PathConstraintPose::setSpacing(float spacing) {
	_spacing = spacing;
}

float PathConstraintPose::getMixRotate() {
	return _mixRotate;
}

void PathConstraintPose::setMixRotate(float mixRotate) {
	_mixRotate = mixRotate;
}

float PathConstraintPose::getMixX() {
	return _mixX;
}

void PathConstraintPose::setMixX(float mixX) {
	_mixX = mixX;
}

float PathConstraintPose::getMixY() {
	return _mixY;
}

void PathConstraintPose::setMixY(float mixY) {
	_mixY = mixY;
}