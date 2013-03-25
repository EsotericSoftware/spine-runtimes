/*******************************************************************************
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

#include <spine-cocos2dx/SkeletonJson.h>
#include <stdexcept>
#include <spine-cocos2dx/AtlasAttachmentLoader.h>
#include "platform/CCFileUtils.h"

using cocos2d::CCFileUtils;
using std::runtime_error;

namespace spine {

SkeletonJson::SkeletonJson (BaseAttachmentLoader *attachmentLoader) :
				BaseSkeletonJson(attachmentLoader) {
}

SkeletonJson::SkeletonJson (Atlas *atlas) :
				BaseSkeletonJson(new AtlasAttachmentLoader(atlas)) {
}

SkeletonData* SkeletonJson::readSkeletonData (const std::string &path) const {
	unsigned long size;
	char *data = reinterpret_cast<char*>(CCFileUtils::sharedFileUtils()->getFileData(
			CCFileUtils::sharedFileUtils()->fullPathForFilename(path.c_str()).c_str(), "r", &size));
	if (!data) throw runtime_error("Error reading skeleton file: " + path);
	return BaseSkeletonJson::readSkeletonData(data, data + size);
}

Animation* SkeletonJson::readAnimation (const std::string &path, const SkeletonData *skeletonData) const {
	unsigned long size;
	char *data = reinterpret_cast<char*>(CCFileUtils::sharedFileUtils()->getFileData(
			CCFileUtils::sharedFileUtils()->fullPathForFilename(path.c_str()).c_str(), "r", &size));
	if (!data) throw runtime_error("Error reading animation file: " + path);
	return BaseSkeletonJson::readAnimation(data, data + size, skeletonData);
}

} /* namespace spine */
