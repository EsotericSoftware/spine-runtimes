-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.3
-- 
-- Copyright (c) 2013-2015, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to use, install, execute and perform the Spine
-- Runtimes Software (the "Software") and derivative works solely for personal
-- or internal use. Without the written permission of Esoteric Software (see
-- Section 2 of the Spine Software License Agreement), you may not (a) modify,
-- translate, adapt or otherwise create derivative works, improvements of the
-- Software or develop new applications using the Software or (b) remove,
-- delete, alter or obscure any trademarks or any copyright, trademark, patent
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local Bone = {}

function Bone.new (data, skeleton, parent)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		skeleton = skeleton,
		parent = parent,
		x = 0, y = 0,
		rotation = 0, rotationIK = 0,
		scaleX = 1, scaleY = 1,
		flipX = false, flipY = false,
		m00 = 0, m01 = 0, worldX = 0, -- a b x
		m10 = 0, m11 = 0, worldY = 0, -- c d y
		worldRotation = 0,
		worldScaleX = 1, worldScaleY = 1,
		worldFlipX = false, worldFlipY = false,
	}

	function self:updateWorldTransform (flipX, flipY)
		local parent = self.parent
		if parent then
			self.worldX = self.x * parent.m00 + self.y * parent.m01 + parent.worldX
			self.worldY = self.x * parent.m10 + self.y * parent.m11 + parent.worldY
			if (self.data.inheritScale) then
				 self.worldScaleX = parent.worldScaleX * self.scaleX
				 self.worldScaleY = parent.worldScaleY * self.scaleY
			else
				 self.worldScaleX = self.scaleX
				 self.worldScaleY = self.scaleY
			end
			if (self.data.inheritRotation) then
				 self.worldRotation = parent.worldRotation + self.rotationIK
			else
				 self.worldRotation = self.rotationIK
			end
			self.worldFlipX = parent.worldFlipX ~= self.flipX
			self.worldFlipY = parent.worldFlipY ~= self.flipY
		else
			local skeletonFlipX, skeletonFlipY = self.skeleton.flipX, self.skeleton.flipY
			if skeletonFlipX then
				self.worldX = -self.x
			else
				self.worldX = self.x
			end
			if skeletonFlipY then
				self.worldY = -self.y
			else
				self.worldY = self.y
			end
			self.worldScaleX = self.scaleX
			self.worldScaleY = self.scaleY
			self.worldRotation = self.rotationIK
			self.worldFlipX = skeletonFlipX ~= self.flipX
			self.worldFlipY = skeletonFlipY ~= self.flipY
		end
		local radians = math.rad(self.worldRotation)
		local cos = math.cos(radians)
		local sin = math.sin(radians)
		if self.worldFlipX then
			self.m00 = -cos * self.worldScaleX
			self.m01 = sin * self.worldScaleY
		else
			self.m00 = cos * self.worldScaleX
			self.m01 = -sin * self.worldScaleY
		end
		if self.worldFlipY then
			self.m10 = -sin * self.worldScaleX
			self.m11 = -cos * self.worldScaleY
		else
			self.m10 = sin * self.worldScaleX
			self.m11 = cos * self.worldScaleY
		end
	end

	function self:setToSetupPose ()
		local data = self.data
		self.x = data.x
		self.y = data.y
		self.rotation = data.rotation
		self.rotationIK = self.rotation
		self.scaleX = data.scaleX
		self.scaleY = data.scaleY
		self.flipX = data.flipX
		self.flipY = data.flipY
	end

	function self:worldToLocal (worldCoords)
		local dx = worldCoords[1] - self.worldX
		local dy = worldCoords[2] - self.worldY
		local m00 = self.m00
		local m10 = self.m10
		local m01 = self.m01
		local m11 = self.m11
		if self.worldFlipX ~= self.worldFlipY then
			m00 = -m00
			m11 = -m11
		end
		local invDet = 1 / (m00 * m11 - m01 * m10)
		worldCoords[1] = dx * m00 * invDet - dy * m01 * invDet
		worldCoords[2] = dy * m11 * invDet - dx * m10 * invDet
	end

	function self:localToWorld (localCoords)
		local localX = localCoords[1]
		local localY = localCoords[2]
		localCoords[1] = localX * self.m00 + localY * self.m01 + self.worldX
		localCoords[2] = localX * self.m10 + localY * self.m11 + self.worldY
	end

	self:setToSetupPose()
	return self
end
return Bone
