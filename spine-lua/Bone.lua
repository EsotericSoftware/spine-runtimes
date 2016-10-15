-------------------------------------------------------------------------------
-- Spine Runtimes Software License v2.5
--
-- Copyright (c) 2013-2016, Esoteric Software
-- All rights reserved.
--
-- You are granted a perpetual, non-exclusive, non-sublicensable, and
-- non-transferable license to use, install, execute, and perform the Spine
-- Runtimes software and derivative works solely for personal or internal
-- use. Without the written permission of Esoteric Software (see Section 2 of
-- the Spine Software License Agreement), you may not (a) modify, translate,
-- adapt, or develop new applications using the Spine Runtimes or otherwise
-- create derivative works or improvements of the Spine Runtimes or (b) remove,
-- delete, alter, or obscure any trademarks or any copyright, trademark, patent,
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
--
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
-- USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
-- IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
-- ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
-- POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local setmetatable = setmetatable
local math_rad = math.rad
local math_deg = math.deg
local math_sin = math.sin
local math_cos = math.cos
local math_atan2 = math.atan2
local math_sqrt = math.sqrt

function math.sign(x)
	if x<0 then
		return -1
	elseif x>0 then
		return 1
	else
		return 0
	end
end

local math_sign = math.sign

local Bone = {}
Bone.__index = Bone

function Bone.new (data, skeleton, parent)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		skeleton = skeleton,
		parent = parent,
		children = { },
		x = 0, y = 0, rotation = 0, scaleX = 1, scaleY = 1, shearX = 0, shearY = 0,
		appliedRotation = 0,

		a = 0, b = 0, worldX = 0, -- a b x
		c = 0, d = 0, worldY = 0, -- c d y
		worldSignX = 0, worldSignY = 0,
		sorted = false
	}
	setmetatable(self, Bone)

	self:setToSetupPose()
	return self
end

function Bone:update ()
	self:updateWorldTransformWith(self.x, self.y, self.rotation, self.scaleX, self.scaleY, self.shearX, self.shearY)
end

function Bone:updateWorldTransform ()
	self:updateWorldTransformWith(self.x, self.y, self.rotation, self.scaleX, self.scaleY, self.shearX, self.shearY)
end

function Bone:updateWorldTransformWith (x, y, rotation, scaleX, scaleY, shearX, shearY)
	self.appliedRotation = rotation

	local rotationY = rotation + 90 + shearY
	local rotationRad = math_rad(rotation + shearX)
	local rotationYRad = math_rad(rotationY)
	local la = math_cos(rotationRad) * scaleX
	local lb = math_cos(rotationYRad) * scaleY
	local lc = math_sin(rotationRad) * scaleX
	local ld = math_sin(rotationYRad) * scaleY

	local parent = self.parent
	if parent == nil then
		local skeleton = self.skeleton
		if skeleton.flipX then
			x = -x
			la = -la
			lb = -lb
		end
		if skeleton.flipY then
			y = -y
			lc = -lc
			ld = -ld
		end
		self.a = la
		self.b = lb
		self.c = lc
		self.d = ld
		self.worldX = x
		self.worldY = y
		self.worldSignX = math_sign(scaleX)
		self.worldSignY = math_sign(scaleY)
		return
	end

	local pa = parent.a
	local pb = parent.b
	local pc = parent.c
	local pd = parent.d
	self.worldX = pa * x + pb * y + parent.worldX
	self.worldY = pc * x + pd * y + parent.worldY
	self.worldSignX = parent.worldSignX * math_sign(scaleX)
	self.worldSignY = parent.worldSignY * math_sign(scaleY)

	if self.data.inheritRotation and self.data.inheritScale then
		self.a = pa * la + pb * lc
		self.b = pa * lb + pb * ld
		self.c = pc * la + pd * lc
		self.d = pc * lb + pd * ld
	else
		if self.data.inheritRotation then
			pa = 1
			pb = 0
			pc = 0
			pd = 1
			repeat
				local appliedRotationRad = math_rad(parent.appliedRotation)
				local cos = math_cos(appliedRotationRad)
				local sin = math_sin(appliedRotationRad)
				local temp = pa * cos + pb * sin
				pb = pb * cos - pa * sin
				pa = temp
				temp = pc * cos + pd * sin
				pd = pd * cos - pc * sin
				pc = temp

				if not parent.data.inheritRotation then break end
				parent = parent.parent
			until parent == nil
			self.a = pa * la + pb * lc
			self.b = pa * lb + pb * ld
			self.c = pc * la + pd * lc
			self.d = pc * lb + pd * ld
		elseif self.data.inheritScale then
			pa = 1
			pb = 0
			pc = 0
			pd = 1
			repeat
				local appliedRotationRad = math_rad(parent.appliedRotation)
				local cos = math_cos(appliedRotationRad)
				local sin = math_sin(appliedRotationRad)
				local psx = parent.scaleX
				local psy = parent.scaleY
				local za = cos * psx
				local zb = sin * psy
				local zc = sin * psx
				local zd = cos * psy
				local temp = pa * za + pb * zc
				pb = pb * zd - pa * zb
				pa = temp
				temp = pc * za + pd * zc
				pd = pd * zd - pc * zb
				pc = temp

				if psx >= 0 then sin = -sin end
				temp = pa * cos + pb * sin
				pb = pb * cos - pa * sin
				pa = temp
				temp = pc * cos + pd * sin
				pd = pd * cos - pc * sin
				pc = temp

				if not parent.data.inheritScale then break end
				parent = parent.parent
			until parent == nil
			self.a = pa * la + pb * lc
			self.b = pa * lb + pb * ld
			self.c = pc * la + pd * lc
			self.d = pc * lb + pd * ld
		else
			self.a = la
			self.b = lb
			self.c = lc
			self.d = ld
		end
		if self.skeleton.flipX then
			self.a = -self.a
			self.b = -self.b
		end
		if self.skeleton.flipY then
			self.c = -self.c
			self.d = -self.d
		end
	end
end

function Bone:setToSetupPose ()
	local data = self.data
	self.x = data.x
	self.y = data.y
	self.rotation = data.rotation
	self.scaleX = data.scaleX
	self.scaleY = data.scaleY
	self.shearX = data.shearX
	self.shearY = data.shearY
end

function Bone:getWorldRotationX ()
	return math_deg(math_atan2(self.c, self.a))
end

function Bone:getWorldRotationY ()
	return math_deg(math_atan2(self.d, self.b))
end

function Bone:getWorldScaleX ()
	return math_sqrt(self.a * self.a + self.b * self.b) * self.worldSignX
end

function Bone:getWorldScaleY ()
	return math_sqrt(self.c * self.c + self.d * self.d) * self.worldSignY
end

function Bone:worldToLocalRotationX ()
	local parent = self.parent
	if parent == nil then return self.rotation end
	local pa = parent.a
	local pb = parent.b
	local pc = parent.c
	local pd = parent.d
	local a = self.a
	local c = self.c
	return math_deg(math_atan2(pa * c - pc * a, pd * a - pb * c))
end

function Bone:worldToLocalRotationY ()
	local parent = self.parent
	if parent == nil then return self.rotation end
	local pa = parent.a
	local pb = parent.b
	local pc = parent.c
	local pd = parent.d
	local b = self.b
	local d = self.d
	return math_deg(math_atan2(pa * d - pc * b, pd * b - pb * d))
end

function Bone:rotateWorld (degrees)
	local a = self.a
	local b = self.b
	local c = self.c
	local d = self.d
	local degreesRad = math_rad(degrees)
	local cos = math_cos(degreesRad)
	local sin = math_sin(degreesRad)
	self.a = cos * a - sin * c
	self.b = cos * b - sin * d
	self.c = sin * a + cos * c
	self.d = sin * b + cos * d
end

function updateLocalTransform ()
	local parent = self.parent
	if parent == nil then
		self.x = self.worldX
		self.y = self.worldY
		self.rotation = math_deg(math_atan2(self.c, self.a))
		self.scaleX = math_sqrt(self.a * self.a + self.c * self.c)
		self.scaleY = math_sqrt(self.b * self.b + self.d * self.d)
		local det = self.a * self.d - self.b * self.c
		self.shearX = 0
		self.shearY = math_deg(math_atan2(self.a * self.b + self.c * self.d, det))
		return
	end
	local pa = parent.a
	local pb = parent.b
	local pc = parent.c
	local pd = parent.d
	local pid = 1 / (pa * pd - pb * pc)
	local dx = self.worldX - parent.worldX
	local dy = self.worldY - parent.worldY
	self.x = (dx * pd * pid - dy * pb * pid)
	self.y = (dy * pa * pid - dx * pc * pid)
	local ia = pid * pd
	local id = pid * pa
	local ib = pid * pb
	local ic = pid * pc
	local ra = ia * self.a - ib * self.c
	local rb = ia * self.b - ib * self.d
	local rc = id * self.c - ic * self.a
	local rd = id * self.d - ic * self.b
	self.shearX = 0
	self.scaleX = math_sqrt(ra * ra + rc * rc)
	if self.scaleX > 0.0001 then
		local det = ra * rd - rb * rc
		self.scaleY = det / self.scaleX
		self.shearY = math_deg(math_atan2(ra * rb + rc * rd, det))
		self.rotation = math_deg(math_atan2(rc, ra))
	else
		self.scaleX = 0
		self.scaleY = math_sqrt(rb * rb + rd * rd)
		self.shearY = 0
		self.rotation = 90 - math_deg(math_atan2(rd, rb))
	end
	self.appliedRotation = self.rotation
end

function Bone:worldToLocal (world)
	local a = self.a
	local b = self.b
	local c = self.c
	local d = self.d
	local invDet = 1 / (a * d - b * c)
	local x = world[1] - self.worldX
	local y = world[2] - self.worldY
	world[1] = (x * d * invDet - y * b * invDet)
	world[2] = (y * a * invDet - x * c * invDet)
	return world
end

function Bone:localToWorld (localCoords)
	local x = localCoords[1]
	local y = localCoords[2]
	localCoords[1] = x * self.a + y * self.b + self.worldX
	localCoords[2] = x * self.c + y * self.d + self.worldY
	return localCoords
end

return Bone
