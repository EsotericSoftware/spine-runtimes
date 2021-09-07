-------------------------------------------------------------------------------
-- Spine Runtimes License Agreement
-- Last updated January 1, 2020. Replaces all prior versions.
--
-- Copyright (c) 2013-2020, Esoteric Software LLC
--
-- Integration of the Spine Runtimes into software or otherwise creating
-- derivative works of the Spine Runtimes is permitted under the terms and
-- conditions of Section 2 of the Spine Editor License Agreement:
-- http://esotericsoftware.com/spine-editor-license
--
-- Otherwise, it is permitted to integrate the Spine Runtimes into software
-- or otherwise create derivative works of the Spine Runtimes (collectively,
-- "Products"), provided that each user of the Products must obtain their own
-- Spine Editor license and redistribution of the Products in any form must
-- include this license and copyright notice.
--
-- THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
-- EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
-- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
-- DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
-- DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
-- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
-- BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
-- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
-- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
-- THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
local math_floor = math.floor

local TransformConstraint = {}
TransformConstraint.__index = TransformConstraint

function TransformConstraint.new (data, skeleton)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		bones = {},
		target = nil,
		mixRotate = data.mixRotate, mixX = data.mixX, mixY = data.mixY, mixScaleX = data.mixScaleX, mixScaleY = data.mixScaleY, mixShearY = data.mixShearY,
		temp = { 0, 0 },
		active = false
	}
	setmetatable(self, TransformConstraint)

	for _,bone in ipairs(data.bones) do
		table_insert(self.bones, skeleton:findBone(bone.name))
	end
	self.target = skeleton:findBone(data.target.name)

	return self
end

function TransformConstraint:update ()
	if self.mixRotate == 0 and self.mixX == 0 and self.mixY == 0 and self.mixScaleX == 0 and self.mixScaleX == 0 and self.mixShearY == 0 then return end

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
	local mixRotate = self.mixRotate
	local mixX = self.mixX
	local mixY = self.mixY
	local mixScaleX = self.mixScaleX
	local mixScaleY = self.mixScaleY
	local mixShearY = self.mixShearY
	local translate = mixX ~= 0 or mixY ~= 0

	local target = self.target
	local ta = target.a
	local tb = target.b
	local tc = target.c
	local td = target.d
	local degRadReflect = 0
	if ta * td - tb * tc > 0 then degRadReflect = utils.degRad else degRadReflect = -utils.degRad end
	local offsetRotation = self.data.offsetRotation * degRadReflect
	local offsetShearY = self.data.offsetShearY * degRadReflect

	local bones = self.bones
	for _, bone in ipairs(bones) do
		if mixRotate ~= 0 then
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
			r = r * mixRotate
			local cos = math_cos(r)
			local sin = math_sin(r)
			bone.a = cos * a - sin * c
			bone.b = cos * b - sin * d
			bone.c = sin * a + cos * c
			bone.d = sin * b + cos * d
		end

		if translate then
			local temp = self.temp
			temp[1] = self.data.offsetX
			temp[2] = self.data.offsetY
			target:localToWorld(temp)
			bone.worldX = bone.worldX + (temp[1] - bone.worldX) * mixX
			bone.worldY = bone.worldY + (temp[2] - bone.worldY) * mixY
		end

		if mixScaleX ~= 0 then
			local s = math_sqrt(bone.a * bone.a + bone.c * bone.c)
			if s ~= 0 then s = (s + (math_sqrt(ta * ta + tc * tc) - s + self.data.offsetScaleX) * mixScaleX) / s end
			bone.a = bone.a * s
			bone.c = bone.c * s
		end
		if mixScaleY ~= 0 then
			local s = math_sqrt(bone.b * bone.b + bone.d * bone.d)
			if s ~= 0 then s = (s + (math_sqrt(tb * tb + td * td) - s + self.data.offsetScaleY) * mixScaleY) / s end
			bone.b = bone.b * s
			bone.d = bone.d * s
		end

		if mixShearY > 0 then
			local b = bone.b
			local d = bone.d
			local by = math_atan2(d, b)
			local r = math_atan2(td, tb) - math_atan2(tc, ta) - (by - math_atan2(bone.c, bone.a))
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			r = by + (r + offsetShearY) * mixShearY
			local s = math_sqrt(b * b + d * d)
			bone.b = math_cos(r) * s
			bone.d = math_sin(r) * s
		end

		bone:updateAppliedTransform()
	end
end

function TransformConstraint:applyRelativeWorld ()
	local mixRotate = self.mixRotate
	local mixX = self.mixX
	local mixY = self.mixY
	local mixScaleX = self.mixScaleX
	local mixScaleY = self.mixScaleY
	local mixShearY = self.mixShearY
	local translate = mixX ~= 0 or mixY ~= 0

	local target = self.target
	local ta = target.a
	local tb = target.b
	local tc = target.c
	local td = target.d
	local degRadReflect = 0
	if ta * td - tb * tc > 0 then degRadReflect = utils.degRad else degRadReflect = -utils.degRad end
	local offsetRotation = self.data.offsetRotation * degRadReflect
	local offsetShearY = self.data.offsetShearY * degRadReflect

	local bones = self.bones
	for _, bone in ipairs(bones) do
		local modified = false

		if mixRotate ~= 0 then
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
			r = r * mixRotate
			local cos = math_cos(r)
			local sin = math_sin(r)
			bone.a = cos * a - sin * c
			bone.b = cos * b - sin * d
			bone.c = sin * a + cos * c
			bone.d = sin * b + cos * d
		end

		if translate then
			local temp = self.temp
			temp[1] = self.data.offsetX
			temp[2] = self.data.offsetY
			target:localToWorld(temp)
			bone.worldX = bone.worldX + temp[1] * mixX
			bone.worldY = bone.worldY + temp[2] * mixY
		end

		if mixScaleX ~= 0 then
			local s = (math_sqrt(ta * ta + tc * tc) - 1 + self.data.offsetScaleX) * mixScaleX + 1
			bone.a = bone.a * s
			bone.c = bone.c * s
		end
		if mixScaleY ~= 0 then
			local s = (math_sqrt(tb * tb + td * td) - 1 + self.data.offsetScaleY) * mixScaleY + 1
			bone.b = bone.b * s
			bone.d = bone.d * s
		end

		if mixShearY > 0 then
			local r = math_atan2(td, tb) - math_atan2(tc, ta)
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			local b = bone.b
			local d = bone.d
			r = math_atan2(d, b) + (r - math_pi / 2 + offsetShearY) * mixShearY
			local s = math_sqrt(b * b + d * d)
			bone.b = math_cos(r) * s
			bone.d = math_sin(r) * s
		end

		bone:updateAppliedTransform()
	end
end

function TransformConstraint:applyAbsoluteLocal ()
	local mixRotate = self.mixRotate
	local mixX = self.mixX
	local mixY = self.mixY
	local mixScaleX = self.mixScaleX
	local mixScaleY = self.mixScaleY
	local mixShearY = self.mixShearY

	local target = self.target
	if not target.appliedValid then target:updatedAppliedTransform() end
	local bones = self.bones
	for _, bone in ipairs(bones) do
		local rotation = bone.arotation
		if mixRotate ~= 0 then
			local r = target.arotation - rotation + self.data.offsetRotation
			r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
			rotation = rotation + r * mixRotate
		end

		local x = bone.ax
		local y = bone.ay
		x = x + (target.ax - x + self.data.offsetX) * mixX
		y = x + (target.ay - y + self.data.offsetY) * mixX

		local scaleX = bone.ascaleX
		local scaleY = bone.ascaleY
		if mixScaleX ~= 0 and scaleX ~= 0 then
			scaleX = (scaleX + (target.ascaleX - scaleX + self.data.offsetScaleX) * mixScaleX) / scaleX
		end
		if mixScaleY ~= 0 and scaleY ~= 0 then
			scaleY = (scaleY + (target.ascaleY - scaleY + self.data.offsetScaleY) * mixScaleY) / scaleY
		end

		local shearY = bone.ashearY
		if mixShearY ~= 0 then
			local r = target.ashearY - shearY + self.data.offsetShearY
			r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
			bone.shearY = bone.shearY + r * mixShearY
		end

		bone:updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY)
	end
end

function TransformConstraint:applyRelativeLocal ()
	local mixRotate = self.mixRotate
	local mixX = self.mixX
	local mixY = self.mixY
	local mixScaleX = self.mixScaleX
	local mixScaleY = self.mixScaleY
	local mixShearY = self.mixShearY

	local target = self.target

	local bones = self.bones
	for _, bone in ipairs(bones) do
		local rotation = bone.arotation + (target.arotation + self.data.offsetRotation) * mixRotate
		local x = bone.ax + (target.ax + self.data.offsetX) * mixX
		local y = bone.ay + (target.ay + self.data.offsetY) * mixY
		local scaleX = bone.ascaleX * (((target.ascaleX - 1 + self.data.offsetScaleX) * mixScaleX) + 1)
		local scaleY = bone.ascaleY * (((target.ascaleY - 1 + self.data.offsetScaleY) * mixScaleY) + 1)
		local shearY = bone.ashearY + (target.ashearY + self.data.offsetShearY) * mixShearY
		bone:updateWorldTransformWith(x, y, rotation, scaleX, scaleY, bone.ashearX, shearY)
	end
end

return TransformConstraint
