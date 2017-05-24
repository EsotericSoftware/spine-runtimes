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
local math_atan2 = math.atan2
local math_sqrt = math.sqrt
local math_acos = math.acos
local math_sin = math.sin
local math_cos = math.cos
local table_insert = table.insert
local math_deg = math.deg
local math_rad = math.rad
local math_abs = math.abs

local IkConstraint = {}
IkConstraint.__index = IkConstraint

function IkConstraint.new (data, skeleton)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		bones = {},
		target = nil,
		mix = data.mix,
		bendDirection = data.bendDirection,
	}
	setmetatable(self, IkConstraint)

	local self_bones = self.bones
	for i,boneData in ipairs(data.bones) do
		table_insert(self_bones, skeleton:findBone(boneData.name))
	end
	self.target = skeleton:findBone(data.target.name)

	return self
end

function IkConstraint:apply ()
	self:update()
end

function IkConstraint:update ()
	local target = self.target
	local bones = self.bones
	local boneCount = #bones
	if boneCount == 1 then
		self:apply1(bones[1], target.worldX, target.worldY, self.mix)
	elseif boneCount == 2 then
		self:apply2(bones[1], bones[2], target.worldX, target.worldY, self.bendDirection, self.mix)
	end
end

function IkConstraint:apply1 (bone, targetX, targetY, alpha)
	if not bone.appliedValid then bone:updateAppliedTransform() end
	local p = bone.parent
	local id = 1 / (p.a * p.d - p.b * p.c)
	local x = targetX - p.worldX
	local y = targetY - p.worldY
	local tx = (x * p.d - y * p.b) * id - bone.ax
	local ty = (y * p.a - x * p.c) * id - bone.ay
	local rotationIK = math_deg(math_atan2(ty, tx)) - bone.ashearX - bone.arotation
	if bone.ascaleX < 0 then rotationIK = rotationIK + 180 end
	if rotationIK > 180 then
		rotationIK = rotationIK - 360
	elseif (rotationIK < -180) then
		rotationIK = rotationIK + 360
	end
	bone:updateWorldTransformWith(bone.ax, bone.ay, bone.arotation + rotationIK * alpha, bone.ascaleX, bone.ascaleY, bone.ashearX, bone.ashearY)
end

function IkConstraint:apply2 (parent, child, targetX, targetY, bendDir, alpha)
	if alpha == 0 then
		child:updateWorldTransform()
		return
	end
	if not parent.appliedValid then parent:updateAppliedTransform() end
	if not child.appliedValid then child:updateAppliedTransform() end
	local px = parent.ax
	local py = parent.ay
	local psx = parent.ascaleX
	local psy = parent.ascaleY
	local csx = child.ascaleX
	local os1 = 0
	local os2 = 0
	local s2 = 0
	if psx < 0 then
		psx = -psx
		os1 = 180
		s2 = -1
	else
		os1 = 0
		s2 = 1
	end
	if psy < 0 then
		psy = -psy
		s2 = -s2
	end
	if csx < 0 then
		csx = -csx
		os2 = 180
	else
		os2 = 0
	end
	local cx = child.ax
	local cy = 0
	local cwx = 0
	local cwy = 0
	local a = parent.a
	local b = parent.b
	local c = parent.c
	local d = parent.d
	local u = math_abs(psx - psy) <= 0.0001
	if not u then
		cy = 0
		cwx = a * cx + parent.worldX
		cwy = c * cx + parent.worldY
	else
		cy = child.ay
		cwx = a * cx + b * cy + parent.worldX
		cwy = c * cx + d * cy + parent.worldY
	end
	local pp = parent.parent
	a = pp.a
	b = pp.b
	c = pp.c
	d = pp.d
	local id = 1 / (a * d - b * c)
	local x = targetX - pp.worldX
	local y = targetY - pp.worldY
	local tx = (x * d - y * b) * id - px
	local ty = (y * a - x * c) * id - py
	x = cwx - pp.worldX
	y = cwy - pp.worldY
	local dx = (x * d - y * b) * id - px
	local dy = (y * a - x * c) * id - py
	local l1 = math_sqrt(dx * dx + dy * dy)
	local l2 = child.data.length * csx
	local a1 = 0
	local a2 = 0

	if u then
		l2 = l2 * psx
		local cos = (tx * tx + ty * ty - l1 * l1 - l2 * l2) / (2 * l1 * l2)
		if cos < -1 then
			cos = -1
		elseif cos > 1 then
			cos = 1
		end
		a2 = math_acos(cos) * bendDir
		a = l1 + l2 * cos
		b = l2 * math_sin(a2)
		a1 = math_atan2(ty * a - tx * b, tx * a + ty * b)
	else
		local skip = false
		a = psx * l2
		b = psy * l2
		local aa = a * a
		local bb = b * b
		local dd = tx * tx + ty * ty
		local ta = math_atan2(ty, tx);
		c = bb * l1 * l1 + aa * dd - aa * bb
		local c1 = -2 * bb * l1
		local c2 = bb - aa
		d = c1 * c1 - 4 * c2 * c
		if d >= 0 then
			local q = math_sqrt(d);
			if (c1 < 0) then q = -q end
			q = -(c1 + q) / 2
			local r0 = q / c2
			local r1 = c / q
			local r = r1
			if math_abs(r0) < math_abs(r1) then r = r0 end
			if r * r <= dd then
				y = math_sqrt(dd - r * r) * bendDir
				a1 = ta - math_atan2(y, r)
				a2 = math_atan2(y / psy, (r - l1) / psx)
				skip = true
			end
		end
		if not skip then
			local minAngle = math_pi
			local minX = l1 - a
			local minDist = minX * minX
			local minY = 0;
			local maxAngle = 0
			local maxX = l1 + a
			local maxDist = maxX * maxX
			local maxY = 0
			c = -a * l1 / (aa - bb)
			if (c >= -1 and c <= 1) then
				c = math_acos(c)
				x = a * math_cos(c) + l1
				y = b * math_sin(c)
				d = x * x + y * y
				if d < minDist then
					minAngle = c
					minDist = d
					minX = x
					minY = y
				end
				if d > maxDist then
					maxAngle = c
					maxDist = d
					maxX = x
					maxY = y
				end
			end
			if dd <= (minDist + maxDist) / 2 then
				a1 = ta - math_atan2(minY * bendDir, minX)
				a2 = minAngle * bendDir
			else
				a1 = ta - math_atan2(maxY * bendDir, maxX)
				a2 = maxAngle * bendDir
			end
		end
	end
	local os = math_atan2(cy, cx) * s2
	local rotation = parent.arotation
	a1 = math_deg(a1 - os) + os1 - rotation
	if a1 > 180 then
		a1 = a1 - 360
	elseif a1 < -180 then
		a1 = a1 + 360
	end
	parent:updateWorldTransformWith(px, py, rotation + a1 * alpha, parent.ascaleX, parent.ascaleY, 0, 0)
	rotation = child.rotation
	a2 = (math_deg(a2 + os) - child.ashearX) * s2 + os2 - rotation
	if a2 > 180 then
		a2 = a2 - 360
	elseif a2 < -180 then
		a2 = a2 + 360
	end
	child:updateWorldTransformWith(cx, cy, rotation + a2 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
end

return IkConstraint
