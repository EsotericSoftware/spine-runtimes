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
local math_abs = math.abs
local math_pi = math.pi

local TransformMode = require "spine-lua.TransformMode"

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
		ax = 0, ay = 0, arotation = 0, ascaleX = 0, ascaleY = 0, ashearX = 0, ashearY = 0,
		appliedValid = false,

		a = 0, b = 0, worldX = 0, -- a b x
		c = 0, d = 0, worldY = 0, -- c d y
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
	self.ax = x
	self.ay = y
	self.arotation = rotation
	self.ascaleX = scaleX
	self.ascaleY = scaleY
	self.ashearX = shearX
	self.ashearY = shearY
	self.appliedValid = true

	local parent = self.parent
	if parent == nil then
		local rotationY = rotation + 90 + shearY
		local rotationRad = math_rad(rotation + shearX)
		local rotationYRad = math_rad(rotationY)
		local la = math_cos(rotationRad) * scaleX
		local lb = math_cos(rotationYRad) * scaleY
		local lc = math_sin(rotationRad) * scaleX
		local ld = math_sin(rotationYRad) * scaleY
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
		self.worldX = x + skeleton.x
		self.worldY = y + skeleton.y
		return
	end

	local pa = parent.a
	local pb = parent.b
	local pc = parent.c
	local pd = parent.d
	self.worldX = pa * x + pb * y + parent.worldX
	self.worldY = pc * x + pd * y + parent.worldY

	local transformMode = self.data.transformMode
	if transformMode == TransformMode.normal then
		local rotationY = rotation + 90 + shearY
		local la = math_cos(math_rad(rotation + shearX)) * scaleX
		local lb = math_cos(math_rad(rotationY)) * scaleY
		local lc = math_sin(math_rad(rotation + shearX)) * scaleX
		local ld = math_sin(math_rad(rotationY)) * scaleY
		self.a = pa * la + pb * lc
		self.b = pa * lb + pb * ld
		self.c = pc * la + pd * lc
		self.d = pc * lb + pd * ld
		return;
	elseif transformMode == TransformMode.onlyTranslation then
		local rotationY = rotation + 90 + shearY
		self.a = math_cos(math_rad(rotation + shearX)) * scaleX
		self.b = math_cos(math_rad(rotationY)) * scaleY
		self.c = math_sin(math_rad(rotation + shearX)) * scaleX
		self.d = math_sin(math_rad(rotationY)) * scaleY
	elseif transformMode == TransformMode.noRotationOrReflection then
		local s = pa * pa + pc * pc
		local prx = 0
		if s > 0.0001 then
			s = math_abs(pa * pd - pb * pc) / s
			pb = pc * s
			pd = pa * s
			prx = math_deg(math_atan2(pc, pa));
		else
			pa = 0;
			pc = 0;
			prx = 90 - math_deg(math_atan2(pd, pb));
		end
		local rx = rotation + shearX - prx
		local ry = rotation + shearY - prx + 90
		local la = math_cos(math_rad(rx)) * scaleX
		local lb = math_cos(math_rad(ry)) * scaleY
		local lc = math_sin(math_rad(rx)) * scaleX
		local ld = math_sin(math_rad(ry)) * scaleY
		self.a = pa * la - pb * lc
		self.b = pa * lb - pb * ld
		self.c = pc * la + pd * lc
		self.d = pc * lb + pd * ld	
	elseif transformMode == TransformMode.noScale or transformMode == TransformMode.noScaleOrReflection then
		local cos = math_cos(math_rad(rotation))
		local sin = math_sin(math_rad(rotation))
		local za = pa * cos + pb * sin
		local zc = pc * cos + pd * sin
		local s = math_sqrt(za * za + zc * zc)
		if s > 0.00001 then s = 1 / s end
		za = za * s
		zc = zc * s
		s = math_sqrt(za * za + zc * zc)
		local r = math_pi / 2 + math_atan2(zc, za)
		local zb = math_cos(r) * s
		local zd = math_sin(r) * s
		local la = math_cos(math_rad(shearX)) * scaleX;
		local lb = math_cos(math_rad(90 + shearY)) * scaleY;
		local lc = math_sin(math_rad(shearX)) * scaleX;
		local ld = math_sin(90 + shearY) * scaleY;
		local flip = self.skeleton.flipX ~= self.skeleton.flipY
		if transformMode ~= TransformMode.noScaleOrReflection then flip = pa * pd - pb * pc < 0 end
		if flip then
			zb = -zb
			zd = -zd
		end
		self.a = za * la + zb * lc
		self.b = za * lb + zb * ld
		self.c = zc * la + zd * lc
		self.d = zc * lb + zd * ld		
		return
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
	return math_sqrt(self.a * self.a + self.c * self.c)
end

function Bone:getWorldScaleY ()
	return math_sqrt(self.b * self.b + self.d * self.d)
end

function updateAppliedTransform ()
	local parent = self.parent
	if parent == nil then
		self.ax = self.worldX
		self.ay = self.worldY
		self.arotation = math_deg(math_atan2(self.c, self.a))
		self.ascaleX = math_sqrt(self.a * self.a + self.c * self.c)
		self.ascaleY = math_sqrt(self.b * self.b + self.d * self.d)
		self.ashearX = 0
		self.ashearY = math_deg(math_atan2(self.a * self.b + self.c * self.d, self.a * self.d - self.b * self.c))
		return
	end
	local pa = parent.a
	local pb = parent.b
	local pc = parent.c
	local pd = parent.d
	local pid = 1 / (pa * pd - pb * pc)
	local dx = self.worldX - parent.worldX
	local dy = self.worldY - parent.worldY
	self.ax = (dx * pd * pid - dy * pb * pid)
	self.ay = (dy * pa * pid - dx * pc * pid)
	local ia = pid * pd
	local id = pid * pa
	local ib = pid * pb
	local ic = pid * pc
	local ra = ia * self.a - ib * self.c
	local rb = ia * self.b - ib * self.d
	local rc = id * self.c - ic * self.a
	local rd = id * self.d - ic * self.b
	self.ashearX = 0
	self.ascaleX = math_sqrt(ra * ra + rc * rc)
	if self.ascaleX > 0.0001 then
		local det = ra * rd - rb * rc
		self.ascaleY = det / self.ascaleX
		self.ashearY = math_deg(math_atan2(ra * rb + rc * rd, det))
		self.arotation = math_deg(math_atan2(rc, ra))
	else
		self.ascaleX = 0
		self.ascaleY = math_sqrt(rb * rb + rd * rd)
		self.ashearY = 0
		self.arotation = 90 - math_deg(math_atan2(rd, rb))
	end
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

function Bone:worldToLocalRotation (worldRotation)
	local sin = math_sin(math_rad(worldRotation))
	local cos = math_cos(math_rad(worldRotation))
	return math_deg(math_atan2(self.a * sin - self.c * cos, self.d * cos - self.b * sin))
end

function Bone:localToWorldRotation (localRotation)
	local sin = math_sin(math_rad(localRotation))
	local cos = math_cos(math_rad(localRotation))
	return math_deg(math_atan2(cos * self.c + sin * self.d, cos * self.a + sin * self.b))
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
	self.appliedValid = false
end

return Bone
