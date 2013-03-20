 -------------------------------------------------------------------------------
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms, with or without
 -- modification, are permitted provided that the following conditions are met:
 -- 
 -- 1. Redistributions of source code must retain the above copyright notice, this
 --    list of conditions and the following disclaimer.
 -- 2. Redistributions in binary form must reproduce the above copyright notice,
 --    this list of conditions and the following disclaimer in the documentation
 --    and/or other materials provided with the distribution.
 -- 
 -- THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 -- ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 -- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 -- DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 -- ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 -- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 -- LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 -- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 -- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 -- SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ------------------------------------------------------------------------------

local Bone = {}
function Bone.new (data, parent)
	if not data then error("data cannot be nil", 2) end
	
	local self = {
		data = data,
		parent = parent
	}

	function self:updateWorldTransform (flipX, flipY)
		local parent = self.parent
		if parent then
			self.worldX = self.x * parent.m00 + self.y * parent.m01 + parent.worldX
			self.worldY = self.x * parent.m10 + self.y * parent.m11 + parent.worldY
			self.worldScaleX = parent.worldScaleX * self.scaleX
			self.worldScaleY = parent.worldScaleY * self.scaleY
			self.worldRotation = parent.worldRotation + self.rotation
		else
			self.worldX = self.x
			self.worldY = self.y
			self.worldScaleX = self.scaleX
			self.worldScaleY = self.scaleY
			self.worldRotation = self.rotation
		end
		local radians = math.rad(self.worldRotation)
		local cos = math.cos(radians)
		local sin = math.sin(radians)
		self.m00 = cos * self.worldScaleX
		self.m10 = sin * self.worldScaleX
		self.m01 = -sin * self.worldScaleY
		self.m11 = cos * self.worldScaleY
		if flipX then
			self.m00 = -self.m00
			self.m01 = -self.m01
		end
		if flipY then
			self.m10 = -self.m10
			self.m11 = -self.m11
		end
	end

	function self:setToBindPose ()
		local data = self.data
		self.x = data.x
		self.y = data.y
		self.rotation = data.rotation
		self.scaleX = data.scaleX
		self.scaleY = data.scaleY
	end
	
	return self
end
return Bone
