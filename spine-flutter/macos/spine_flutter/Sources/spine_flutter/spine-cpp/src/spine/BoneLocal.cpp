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

#include <spine/BoneLocal.h>

using namespace spine;

BoneLocal::BoneLocal() : _x(0), _y(0), _rotation(0), _scaleX(1), _scaleY(1), _shearX(0), _shearY(0), _inherit(Inherit_Normal) {
}

BoneLocal::~BoneLocal() {
}

void BoneLocal::set(BoneLocal &pose) {
	_x = pose._x;
	_y = pose._y;
	_rotation = pose._rotation;
	_scaleX = pose._scaleX;
	_scaleY = pose._scaleY;
	_shearX = pose._shearX;
	_shearY = pose._shearY;
	_inherit = pose._inherit;
}

float BoneLocal::getX() {
	return _x;
}

void BoneLocal::setX(float x) {
	_x = x;
}

float BoneLocal::getY() {
	return _y;
}

void BoneLocal::setY(float y) {
	_y = y;
}

void BoneLocal::setPosition(float x, float y) {
	_x = x;
	_y = y;
}

float BoneLocal::getRotation() {
	return _rotation;
}

void BoneLocal::setRotation(float rotation) {
	_rotation = rotation;
}

float BoneLocal::getScaleX() {
	return _scaleX;
}

void BoneLocal::setScaleX(float scaleX) {
	_scaleX = scaleX;
}

float BoneLocal::getScaleY() {
	return _scaleY;
}

void BoneLocal::setScaleY(float scaleY) {
	_scaleY = scaleY;
}

void BoneLocal::setScale(float scaleX, float scaleY) {
	_scaleX = scaleX;
	_scaleY = scaleY;
}

void BoneLocal::setScale(float scale) {
	_scaleX = scale;
	_scaleY = scale;
}

float BoneLocal::getShearX() {
	return _shearX;
}

void BoneLocal::setShearX(float shearX) {
	_shearX = shearX;
}

float BoneLocal::getShearY() {
	return _shearY;
}

void BoneLocal::setShearY(float shearY) {
	_shearY = shearY;
}

Inherit BoneLocal::getInherit() {
	return _inherit;
}

void BoneLocal::setInherit(Inherit inherit) {
	_inherit = inherit;
}