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

#include <spine/RTTI.h>
#include <spine/SpineString.h>

using namespace spine;

RTTI::RTTI(const char *className) : _className(className), _pBaseRTTI(NULL), _interfaceCount(0) {
	_interfaces[0] = NULL;
	_interfaces[1] = NULL;
	_interfaces[2] = NULL;
}

RTTI::RTTI(const char *className, const RTTI &baseRTTI) : _className(className), _pBaseRTTI(&baseRTTI), _interfaceCount(0) {
	_interfaces[0] = NULL;
	_interfaces[1] = NULL;
	_interfaces[2] = NULL;
}

RTTI::RTTI(const char *className, const RTTI &baseRTTI, const RTTI *interface1, const RTTI *interface2, const RTTI *interface3)
	: _className(className), _pBaseRTTI(&baseRTTI), _interfaceCount(0) {
	_interfaces[0] = interface1;
	_interfaces[1] = interface2;
	_interfaces[2] = interface3;

	if (interface1) _interfaceCount++;
	if (interface2) _interfaceCount++;
	if (interface3) _interfaceCount++;
}

const char *RTTI::getClassName() const {
	return _className;
}

bool RTTI::isExactly(const RTTI &rtti) const {
	return !strcmp(this->_className, rtti._className);
}

bool RTTI::instanceOf(const RTTI &rtti) const {
	// Check the main inheritance chain
	const RTTI *pCompare = this;
	while (pCompare) {
		if (!strcmp(pCompare->_className, rtti._className)) return true;

		// Check interfaces at this level of the hierarchy
		for (int i = 0; i < pCompare->_interfaceCount; i++) {
			if (pCompare->_interfaces[i] && !strcmp(pCompare->_interfaces[i]->_className, rtti._className)) {
				return true;
			}
		}

		pCompare = pCompare->_pBaseRTTI;
	}

	return false;
}
