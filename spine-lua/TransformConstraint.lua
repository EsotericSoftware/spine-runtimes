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
local utils = require "spine-lua.utils"
local math_pi = math.pi
local math_pi2 = math.pi * 2
local math_atan2 = math.atan2
local math_sqrt = math.sqrt
local math_acos = math.acos
local math_sin = math.sin
local math_cos = math.cos
local table_insert = table.insert
local math_deg = math.deg
local math_rad = math.rad
local math_abs = math.abs

local TransformConstraint = {}
TransformConstraint.__index = TransformConstraint

function TransformConstraint.new (data, skeleton)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		bones = {},
		target = nil,
		rotateMix = data.rotateMix, translateMix = data.translateMix, scaleMix = data.scaleMix, shearMix = data.shearMix,
		temp = { 0, 0 }
	}
	setmetatable(self, TransformConstraint)

	for i,bone in ipairs(data.bones) do
		table_insert(self.bones, skeleton:findBone(bone.name))
	end
	self.target = skeleton:findBone(data.target.name)

	return self
end

function TransformConstraint:apply ()
	self:update()
end

function TransformConstraint:update ()
	if self.data.local_ then
		if self.data.relative then
			self:applyRelativeLocal()
		else
			self:applyAbsoluteLocal()
		end
	else
		if self.data.relative then
			self:applyRelativeWorld()
		else
			self:applyAbsoluteWorld()
		end
	end
end

function TransformConstraint:applyAbsoluteWorld ()
	local rotateMix = self.rotateMix
	local translateMix = self.translateMix
	local scaleMix = self.scaleMix
	local shearMix = self.shearMix
	local target = self.target
	local ta = target.a
	local tb = target.b
	local tc = target.c
	local td = target.d
	local degRadReflect = 0;
	if ta * td - tb * tc > 0 then degRadReflect = utils.degRad else degRadReflect = -utils.degRad end
	local offsetRotation = self.data.offsetRotation * degRadReflect
	local offsetShearY = self.data.offsetShearY * degRadReflect
	local bones = self.bones
	for i, bone in ipairs(bones) do
		local modified = false
		if rotateMix ~= 0 then
			local a = bone.a
			local b = bone.b
			local c = bone.c
			local d = bone.d
			local r = math_atan2(tc, ta) - math_atan2(c, a) + offsetRotation
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			r = r * rotateMix
			local cos = math_cos(r)
			local sin = math_sin(r)
			bone.a = cos * a - sin * c
			bone.b = cos * b - sin * d
			bone.c = sin * a + cos * c
			bone.d = sin * b + cos * d
			modified = true
		end

		if translateMix ~= 0 then
			local temp = self.temp
			temp[1] = self.data.offsetX
			temp[2] = self.data.offsetY
			target:localToWorld(temp)
			bone.worldX = bone.worldX + (temp[1] - bone.worldX) * translateMix
			bone.worldY = bone.worldY + (temp[2] - bone.worldY) * translateMix
			modified = true
		end

		if scaleMix > 0 then
			local s = math_sqrt(bone.a * bone.a + bone.c * bone.c)
			local ts = math_sqrt(ta * ta + tc * tc)
			if s > 0.00001 then
				s = (s + (ts - s + self.data.offsetScaleX) * scaleMix) / s
			end
			bone.a = bone.a * s
			bone.c = bone.c * s
			s = math_sqrt(bone.b * bone.b + bone.d * bone.d)
			ts = math_sqrt(tb * tb + td * td)
			if s > 0.00001 then
				s = (s + (ts - s + self.data.offsetScaleY) * scaleMix) / s
			end
			bone.b = bone.b * s
			bone.d = bone.d * s
			modified = true
		end

		if shearMix > 0 then
			local b = bone.b
			local d = bone.d
			local by = math_atan2(d, b)
			local r = math_atan2(td, tb) - math_atan2(tc, ta) - (by - math_atan2(bone.c, bone.a))
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			r = by + (r + offsetShearY) * shearMix
			local s = math_sqrt(b * b + d * d)
			bone.b = math_cos(r) * s
			bone.d = math_sin(r) * s
			modified = true
		end
		
		if modified then bone.appliedValid = false end
	end
end

function TransformConstraint:applyRelativeWorld ()
	local rotateMix = self.rotateMix
	local translateMix = self.translateMix
	local scaleMix = self.scaleMix
	local shearMix = self.shearMix
	local target = self.target
	local ta = target.a
	local tb = target.b
	local tc = target.c
	local td = target.d
	local degRadReflect = 0;
	if ta * td - tb * tc > 0 then degRadReflect = utils.degRad else degRadReflect = -utils.degRad end
	local offsetRotation = self.data.offsetRotation * degRadReflect
	local offsetShearY = self.data.offsetShearY * degRadReflect
	local bones = self.bones
	for i, bone in ipairs(bones) do
		local modified = false
		
		if rotateMix ~= 0 then
			local a = bone.a
			local b = bone.b
			local c = bone.c
			local d = bone.d
			local r = math_atan2(tc, ta) + offsetRotation
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			r = r * rotateMix
			local cos = math_cos(r)
			local sin = math_sin(r)
			bone.a = cos * a - sin * c
			bone.b = cos * b - sin * d
			bone.c = sin * a + cos * c
			bone.d = sin * b + cos * d
			modified = true
		end

		if translateMix ~= 0 then
			local temp = self.temp
			temp[1] = self.data.offsetX
			temp[2] = self.data.offsetY
			target:localToWorld(temp)
			bone.worldX = bone.worldX + temp[1] * translateMix
			bone.worldY = bone.worldY + temp[2] * translateMix
			modified = true
		end

		if scaleMix > 0 then
			local s = (math_sqrt(ta * ta + tc * tc) - 1 + self.data.offsetScaleX) * scaleMix + 1
			bone.a = bone.a * s
			bone.c = bone.c * s
			local s = (math_sqrt(tb * tb + td * td) - 1 + self.data.offsetScaleY) * scaleMix + 1
			bone.b = bone.b * s
			bone.d = bone.d * s
			modified = true
		end

		if shearMix > 0 then
			local r = math_atan2(td, tb) - math_atan2(tc, ta)
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			local b = bone.b
			local d = bone.d
			r = math_atan2(d, b) + (r - math_pi / 2 + offsetShearY) * shearMix;
			local s = math_sqrt(b * b + d * d)
			bone.b = math_cos(r) * s
			bone.d = math_sin(r) * s
			modified = true
		end
		
		if modified then bone.appliedValid = false end
	end
end

function TransformConstraint:applyAbsoluteLocal ()
	local rotateMix = self.rotateMix
	local translateMix = self.translateMix
	local scaleMix = self.scaleMix
	local shearMix = self.shearMix
	local target = self.target
	if not target.appliedValid then target:updatedAppliedTransform() end
	local bones = self.bones
	for i, bone in ipairs(bones) do
		local modified = false
		if not bone.appliedValid then bone:updateAppliedTransform() end
		
		local rotation = bone.arotation
		if rotateMix ~= 0 then
			local r = target.arotation - rotation + self.data.offsetRotation
			r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
			rotation = rotation + r * rotateMix
		end

		local x = bone.ax
		local y = bone.ay
		if translateMix ~= 0 then
			x = x + (target.ax - x + self.data.offsetX) * translateMix
			y = x + (target.ay - y + self.data.offsetY) * translateMix
		end

		local scaleX = bone.ascaleX
		local scaleY = bone.ascaleY
		if scaleMix > 0 then
			if scaleX > 0.00001 then
				scaleX = (scaleX + (target.ascaleX - scaleX + self.data.offsetScaleX) * scaleMix) / scaleX
			end
			if scaleY > 0.00001 then
				scaleY = (scaleY + (target.ascaleY - scaleY + self.data.offsetScaleY) * scaleMix) / scaleY
			end
		end

		local shearY = bone.ashearY
		if shearMix > 0 then
			local r = target.ashearY - shearY + self.data.offsetShearY
			r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
			bone.shearY = bone.shearY + r * shearMix
		end

		bone:updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY);
	end
end

function TransformConstraint:applyRelativeLocal ()
	local rotateMix = self.rotateMix
	local translateMix = self.translateMix
	local scaleMix = self.scaleMix
	local shearMix = self.shearMix	
	local target = self.target
	if not target.appliedValid then target:updateAppliedTransform() end
	local bones = self.bones
	for i, bone in ipairs(bones) do
		if not bone.appliedValid then bone:updateAppliedTransform() end

		local rotation = bone.arotation
		if rotateMix ~= 0 then rotation = rotation + (target.arotation + self.data.offsetRotation) * rotateMix end

		local x = bone.ax
		local y = bone.ay
		if translateMix ~= 0 then
			x = x + (target.ax + self.data.offsetX) * translateMix
			y = y + (target.ay + self.data.offsetY) * translateMix
		end

		local scaleX = bone.ascaleX
		local scaleY = bone.ascaleY
		if scaleMix > 0 then
			if scaleX > 0.00001 then
				scaleX = scaleX * (((target.ascaleX - 1 + self.data.offsetScaleX) * scaleMix) + 1)
			end
			if scaleY > 0.00001 then
				scaleY = scaleY * (((target.ascaleY - 1 + this.data.offsetScaleY) * scaleMix) + 1)
			end
		end

		local shearY = bone.ashearY
		if shearMix > 0 then shearY = shearY + (target.ashearY + self.data.offsetShearY) * shearMix end

		bone:updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY)
	end
end

return TransformConstraint
