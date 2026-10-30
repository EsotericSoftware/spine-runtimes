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

#include <spine/TransformConstraintPose.h>

using namespace spine;

TransformConstraintPose::TransformConstraintPose() : _mixRotate(0), _mixX(0), _mixY(0), _mixScaleX(0), _mixScaleY(0), _mixShearY(0) {
}

TransformConstraintPose::~TransformConstraintPose() {
}

void TransformConstraintPose::set(TransformConstraintPose &pose) {
	_mixRotate = pose._mixRotate;
	_mixX = pose._mixX;
	_mixY = pose._mixY;
	_mixScaleX = pose._mixScaleX;
	_mixScaleY = pose._mixScaleY;
	_mixShearY = pose._mixShearY;
}

float TransformConstraintPose::getMixRotate() {
	return _mixRotate;
}

void TransformConstraintPose::setMixRotate(float mixRotate) {
	this->_mixRotate = mixRotate;
}

float TransformConstraintPose::getMixX() {
	return _mixX;
}

void TransformConstraintPose::setMixX(float mixX) {
	this->_mixX = mixX;
}

float TransformConstraintPose::getMixY() {
	return _mixY;
}

void TransformConstraintPose::setMixY(float mixY) {
	this->_mixY = mixY;
}

float TransformConstraintPose::getMixScaleX() {
	return _mixScaleX;
}

void TransformConstraintPose::setMixScaleX(float mixScaleX) {
	this->_mixScaleX = mixScaleX;
}

float TransformConstraintPose::getMixScaleY() {
	return _mixScaleY;
}

void TransformConstraintPose::setMixScaleY(float mixScaleY) {
	this->_mixScaleY = mixScaleY;
}

float TransformConstraintPose::getMixShearY() {
	return _mixShearY;
}

void TransformConstraintPose::setMixShearY(float mixShearY) {
	this->_mixShearY = mixShearY;
}