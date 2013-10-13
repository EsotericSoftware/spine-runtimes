------------------------------------------------------------------------------
 -- Spine Runtime Software License - Version 1.0
 -- 
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms in whole or in part, with
 -- or without modification, are permitted provided that the following conditions
 -- are met:
 -- 
 -- 1. A Spine Essential, Professional, Enterprise, or Education License must
 --    be purchased from Esoteric Software and the license must remain valid:
 --    http://esotericsoftware.com/
 -- 2. Redistributions of source code must retain this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer.
 -- 3. Redistributions in binary form must reproduce this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer, in the documentation and/or other materials provided with the
 --    distribution.
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
		parent = parent,
		x = 0, y = 0,
		rotation = 0,
		scaleX = 1, scaleY = 1,
		m00 = 0, m01 = 0, worldX = 0, -- a b x
		m10 = 0, m11 = 0, worldY = 0, -- c d y
		worldRotation = 0,
		worldScaleX = 1, worldScaleY = 1,
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
				 self.worldRotation = parent.worldRotation + self.rotation
			else
				 self.worldRotation = self.rotation
			end
		else
			if flipX then
				self.worldX = -self.x
			else
				self.worldX = self.x
			end
			if flipY then
				self.worldY = -self.y
			else
				self.worldY = self.y
			end
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

	function self:setToSetupPose ()
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
