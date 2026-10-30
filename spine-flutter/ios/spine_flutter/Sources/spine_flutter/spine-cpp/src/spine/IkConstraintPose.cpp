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

#include <spine/IkConstraintPose.h>

using namespace spine;

IkConstraintPose::IkConstraintPose() : _bendDirection(0), _compress(false), _stretch(false), _mix(0), _softness(0) {
}

IkConstraintPose::~IkConstraintPose() {
}

void IkConstraintPose::set(IkConstraintPose &pose) {
	_mix = pose._mix;
	_softness = pose._softness;
	_bendDirection = pose._bendDirection;
	_compress = pose._compress;
	_stretch = pose._stretch;
}

float IkConstraintPose::getMix() {
	return _mix;
}

void IkConstraintPose::setMix(float mix) {
	_mix = mix;
}

float IkConstraintPose::getSoftness() {
	return _softness;
}

void IkConstraintPose::setSoftness(float softness) {
	_softness = softness;
}

int IkConstraintPose::getBendDirection() {
	return _bendDirection;
}

void IkConstraintPose::setBendDirection(int bendDirection) {
	_bendDirection = bendDirection;
}

bool IkConstraintPose::getCompress() {
	return _compress;
}

void IkConstraintPose::setCompress(bool compress) {
	_compress = compress;
}

bool IkConstraintPose::getStretch() {
	return _stretch;
}

void IkConstraintPose::setStretch(bool stretch) {
	_stretch = stretch;
}