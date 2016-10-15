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
	local rotateMix = self.rotateMix
	local translateMix = self.translateMix
	local scaleMix = self.scaleMix
	local shearMix = self.shearMix
	local target = self.target
	local ta = target.a
	local tb = target.b
	local tc = target.c
	local td = target.d
	local bones = self.bones
	for i, bone in ipairs(bones) do
		if rotateMix > 0 then
			local a = bone.a
			local b = bone.b
			local c = bone.c
			local d = bone.d
			local r = math_atan2(tc, ta) - math_atan2(c, a) + math_rad(self.data.offsetRotation);
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
		end

		if translateMix > 0 then
			local temp = self.temp
			temp[1] = self.data.offsetX
			temp[2] = self.data.offsetY
			target:localToWorld(temp)
			bone.worldX = bone.worldX + (temp[1] - bone.worldX) * translateMix
			bone.worldY = bone.worldY + (temp[2] - bone.worldY) * translateMix
		end

		if scaleMix > 0 then
			local bs = math_sqrt(bone.a * bone.a + bone.c * bone.c)
			local ts = math_sqrt(ta * ta + tc * tc)
			local s = 0
			if bs > 0.00001 then
				s = (bs + (ts - bs + self.data.offsetScaleX) * scaleMix) / bs
			end
			bone.a = bone.a * s
			bone.c = bone.c * s
			bs = math_sqrt(bone.b * bone.b + bone.d * bone.d)
			ts = math_sqrt(tb * tb + td * td)
			s = 0
			if bs > 0.00001 then
				s = (bs + (ts - bs + self.data.offsetScaleY) * scaleMix) / bs
			end
			bone.b = bone.b * s
			bone.d = bone.d * s
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
			r = by + (r + math_rad(self.data.offsetShearY)) * shearMix
			local s = math_sqrt(b * b + d * d)
			bone.b = math_cos(r) * s
			bone.d = math_sin(r) * s
		end
	end
end

return TransformConstraint
