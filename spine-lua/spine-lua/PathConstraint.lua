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

-- FIXME the logic in this file uses 0-based indexing. Each array
-- access adds 1 to the calculated index. We should switch the logic
-- to 1-based indexing eventually.

local setmetatable = setmetatable
local AttachmentType = require "spine-lua.attachments.AttachmentType"
local PathConstraintData = require "spine-lua.PathConstraintData"
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
local math_max = math.max

local PathConstraint = {}
PathConstraint.__index = PathConstraint

PathConstraint.NONE = -1
PathConstraint.BEFORE = -2
PathConstraint.AFTER = -3
PathConstraint.epsilon = 0.00001

function PathConstraint.new (data, skeleton)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		bones = {},
		target = skeleton:findSlot(data.target.name),
		position = data.position,
		spacing = data.spacing,
		rotateMix = data.rotateMix,
		translateMix = data.translateMix,
		spaces = {},
		positions = {},
		world = {},
		curves = {},
		lengths = {},
		segments = {},
		active = false
	}
	setmetatable(self, PathConstraint)

	for _,boneData in ipairs(data.bones) do
		table_insert(self.bones, skeleton:findBone(boneData.name))
	end

	return self
end

function PathConstraint:apply ()
	self:update()
end

function PathConstraint:update ()
	local attachment = self.target.attachment
	if not attachment or not (attachment.type == AttachmentType.path) then return end

	local rotateMix = self.rotateMix
	local translateMix = self.translateMix
	local translate = translateMix > 0
	local rotate = rotateMix > 0
	if not translate and not rotate then return end

	local data = self.data;
	local percentSpacing = data.spacingMode == PathConstraintData.SpacingMode.percent
	local rotateMode = data.rotateMode
	local tangents = rotateMode == PathConstraintData.RotateMode.tangent
	local scale = rotateMode == PathConstraintData.RotateMode.chainscale
	local bones = self.bones
	local boneCount = #bones
	local spacesCount = boneCount + 1
	if tangents then spacesCount = boneCount end
	local spaces = utils.setArraySize(self.spaces, spacesCount)
	local lengths = nil
	local spacing = self.spacing
	if scale or not percentSpacing then
		if scale then lengths = utils.setArraySize(self.lengths, boneCount) end
		local lengthSpacing = data.spacingMode == PathConstraintData.SpacingMode.length
		local i = 0
		local n = spacesCount - 1
		while i < n do
			local bone = bones[i + 1];
			local setupLength = bone.data.length
			if setupLength < PathConstraint.epsilon then
				if scale then lengths[i + 1] = 0 end
				i = i + 1
				spaces[i + 1] = 0
			elseif percentSpacing then
				if scale then
					local x = setupLength * bone.a
					local y = setupLength * bone.c
					local length = math_sqrt(x * x + y * y)
					lengths[i + 1] = length
				end
				i = i + 1
				spaces[i + 1] = spacing
			else
	 			local x = setupLength * bone.a
				local y = setupLength * bone.c
				local length = math_sqrt(x * x + y * y)
				if scale then lengths[i + 1] = length end
				i = i + 1
				if lengthSpacing then
					spaces[i + 1] = (setupLength + spacing) * length / setupLength
				else
					spaces[i + 1] = spacing * length / setupLength
				end
			end
		end
	else
		local i = 1
		while i < spacesCount do
			spaces[i + 1] = spacing
			i = i + 1
		end
	end

	local positions = self:computeWorldPositions(attachment, spacesCount, tangents, data.positionMode == PathConstraintData.PositionMode.percent, percentSpacing)
	local boneX = positions[1]
	local boneY = positions[2]
	local offsetRotation = data.offsetRotation
	local tip = false;
	if offsetRotation == 0 then
			tip = rotateMode == PathConstraintData.RotateMode.chain
	else
		tip = false;
		local p = self.target.bone;
		if p.a * p.d - p.b * p.c > 0 then
			offsetRotation = offsetRotation * utils.degRad
		else
			offsetRotation = offsetRotation * -utils.degRad
		end
	end

	local i = 0
	local p = 3
	while i < boneCount do
		local bone = bones[i + 1]
		bone.worldX = bone.worldX + (boneX - bone.worldX) * translateMix
		bone.worldY = bone.worldY + (boneY - bone.worldY) * translateMix
		local x = positions[p + 1]
		local y = positions[p + 2]
		local dx = x - boneX
		local dy = y - boneY
		if scale then
			local length = lengths[i + 1]
			if length ~= 0 then
				local s = (math_sqrt(dx * dx + dy * dy) / length - 1) * rotateMix + 1
				bone.a = bone.a * s
				bone.c = bone.c * s
			end
		end
		boneX = x
		boneY = y
		if rotate then
			local a = bone.a
			local b = bone.b
			local c = bone.c
			local d = bone.d
			local r = 0
			local cos = 0
			local sin = 0
			if tangents then
				r = positions[p - 1 + 1]
			elseif spaces[i + 1 + 1] == 0 then
				r = positions[p + 2 + 1]
			else
				r = math_atan2(dy, dx)
			end
			r = r - math_atan2(c, a)
			if tip then
				cos = math_cos(r)
				sin = math_sin(r)
				local length = bone.data.length
				boneX = boneX + (length * (cos * a - sin * c) - dx) * rotateMix;
				boneY = boneY + (length * (sin * a + cos * c) - dy) * rotateMix;
			else
				r = r + offsetRotation
			end
			if r > math_pi then
				r = r - math_pi2
			elseif r < -math_pi then
				r = r + math_pi2
			end
			r = r * rotateMix
			cos = math_cos(r)
			sin = math.sin(r)
			bone.a = cos * a - sin * c
			bone.b = cos * b - sin * d
			bone.c = sin * a + cos * c
			bone.d = sin * b + cos * d
		end
		bone.appliedValid = false
		i = i + 1
		p = p + 3
	end
end

function PathConstraint:computeWorldPositions (path, spacesCount, tangents, percentPosition, percentSpacing)
	local target = self.target
	local position = self.position
	local spaces = self.spaces
	local out = utils.setArraySize(self.positions, spacesCount * 3 + 2)
	local world = nil
	local closed = path.closed
	local verticesLength = path.worldVerticesLength
	local curveCount = verticesLength / 6
	local prevCurve = PathConstraint.NONE
	local i = 0

	if not path.constantSpeed then
		local lengths = path.lengths
		if closed then curveCount = curveCount - 1 else curveCount = curveCount - 2 end
		local pathLength = lengths[curveCount + 1];
		if percentPosition then position = position * pathLength end
		if percentSpacing then
			i = 1
			while i < spacesCount do
				spaces[i + 1] = spaces[i + 1] * pathLength
				i = i + 1
			end
		end
		world = utils.setArraySize(self.world, 8);
		i = 0
		local o = 0
		local curve = 0
		while i < spacesCount do
			local space = spaces[i + 1];
			position = position + space
			local p = position

			local skip = false
			if closed then
				p = p % pathLength
				if p < 0 then p = p + pathLength end
				curve = 0
			elseif p < 0 then
				if prevCurve ~= PathConstraint.BEFORE then
					prevCurve = PathConstraint.BEFORE
					path:computeWorldVertices(target, 2, 4, world, 0, 2)
				end
				self:addBeforePosition(p, world, 0, out, o)
				skip = true
			elseif p > pathLength then
				if prevCurve ~= PathConstraint.AFTER then
					prevCurve = PathConstraint.AFTER
					path:computeWorldVertices(target, verticesLength - 6, 4, world, 0, 2)
				end
				self:addAfterPosition(p - pathLength, world, 0, out, o)
				skip = true
			end

			if not skip then
				-- Determine curve containing position.
				while true do
					local length = lengths[curve + 1]
					if p <= length then
						if curve == 0 then
							p = p / length
						else
							local prev = lengths[curve - 1 + 1]
							p = (p - prev) / (length - prev)
						end
						break
					end
					curve = curve + 1
				end
				if curve ~= prevCurve then
					prevCurve = curve
					if closed and curve == curveCount then
						path:computeWorldVertices(target, verticesLength - 4, 4, world, 0, 2)
						path:computeWorldVertices(target, 0, 4, world, 4, 2)
					else
						path:computeWorldVertices(target, curve * 6 + 2, 8, world, 0, 2)
					end
				end
				self:addCurvePosition(p, world[1], world[2], world[3], world[4], world[5], world[6], world[7], world[8], out, o, tangents or (i > 0 and space == 0))
			end

			i = i + 1
			o = o + 3
		end
		return out
	end

	-- World vertices.
	if closed then
		verticesLength = verticesLength + 2
		world = utils.setArraySize(self.world, verticesLength)
		path:computeWorldVertices(target, 2, verticesLength - 4, world, 0, 2)
		path:computeWorldVertices(target, 0, 2, world, verticesLength - 4, 2)
		world[verticesLength - 2 + 1] = world[0 + 1]
		world[verticesLength - 1 + 1] = world[1 + 1]
	else
		curveCount = curveCount - 1
		verticesLength = verticesLength - 4;
		world = utils.setArraySize(self.world, verticesLength)
		path:computeWorldVertices(target, 2, verticesLength, world, 0, 2)
	end

	-- Curve lengths.
	local curves = utils.setArraySize(self.curves, curveCount)
	local pathLength = 0;
	local x1 = world[0 + 1]
	local y1 = world[1 + 1]
	local cx1 = 0
	local cy1 = 0
	local cx2 = 0
	local cy2 = 0
	local x2 = 0
	local y2 = 0
	local tmpx = 0
	local tmpy = 0
	local dddfx = 0
	local dddfy = 0
	local ddfx = 0
	local ddfy = 0
	local dfx = 0
	local dfy = 0
	local w = 2
	while i < curveCount do
		cx1 = world[w + 1]
		cy1 = world[w + 2]
		cx2 = world[w + 3]
		cy2 = world[w + 4]
		x2 = world[w + 5]
		y2 = world[w + 6]
		tmpx = (x1 - cx1 * 2 + cx2) * 0.1875
		tmpy = (y1 - cy1 * 2 + cy2) * 0.1875
		dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375
		dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375
		ddfx = tmpx * 2 + dddfx
		ddfy = tmpy * 2 + dddfy
		dfx = (cx1 - x1) * 0.75 + tmpx + dddfx * 0.16666667
		dfy = (cy1 - y1) * 0.75 + tmpy + dddfy * 0.16666667
		pathLength = pathLength + math_sqrt(dfx * dfx + dfy * dfy)
		dfx = dfx + ddfx
		dfy = dfy + ddfy
		ddfx = ddfx + dddfx
		ddfy = ddfy + dddfy
		pathLength = pathLength + math_sqrt(dfx * dfx + dfy * dfy)
		dfx = dfx + ddfx
		dfy = dfy + ddfy
		pathLength = pathLength + math_sqrt(dfx * dfx + dfy * dfy)
		dfx = dfx + ddfx + dddfx
		dfy = dfy + ddfy + dddfy
		pathLength = pathLength + math_sqrt(dfx * dfx + dfy * dfy)
		curves[i + 1] = pathLength
		x1 = x2
		y1 = y2
		i = i + 1
		w = w + 6
	end
	if percentPosition then
		position = position * pathLength
	else
		position = position * pathLength / path.lengths[curveCount];
	end
	if percentSpacing then
		i = 1
		while i < spacesCount do
			spaces[i + 1] = spaces[i + 1] * pathLength
			i = i + 1
		end
	end

	local segments = self.segments
	local curveLength = 0
	i = 0
	local o = 0
	local curve = 0
	local segment = 0
	while i < spacesCount do
		local space = spaces[i + 1]
		position = position + space
		local p = position

		local skip = false
		if closed then
			p = p % pathLength
			if p < 0 then p = p + pathLength end
			curve = 0
		elseif p < 0 then
			self:addBeforePosition(p, world, 0, out, o)
			skip = true
		elseif p > pathLength then
			self:addAfterPosition(p - pathLength, world, verticesLength - 4, out, o)
			skip = true
		end

		if not skip then
			-- Determine curve containing position.
			while true do
				local length = curves[curve + 1]
				if p <= length then
					if curve == 0 then
						p = p / length
					else
						local prev = curves[curve - 1 + 1]
						p = (p - prev) / (length - prev)
					end
					break
				end
				curve = curve + 1
			end

			-- Curve segment lengths.
			if curve ~= prevCurve then
				prevCurve = curve
				local ii = curve * 6
				x1 = world[ii + 1]
				y1 = world[ii + 2]
				cx1 = world[ii + 3]
				cy1 = world[ii + 4]
				cx2 = world[ii + 5]
				cy2 = world[ii + 6]
				x2 = world[ii + 7]
				y2 = world[ii + 8]
				tmpx = (x1 - cx1 * 2 + cx2) * 0.03
				tmpy = (y1 - cy1 * 2 + cy2) * 0.03
				dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006
				dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006
				ddfx = tmpx * 2 + dddfx
				ddfy = tmpy * 2 + dddfy
				dfx = (cx1 - x1) * 0.3 + tmpx + dddfx * 0.16666667
				dfy = (cy1 - y1) * 0.3 + tmpy + dddfy * 0.16666667
				curveLength = math_sqrt(dfx * dfx + dfy * dfy)
				segments[1] = curveLength
				ii = 1
				while ii < 8 do
					dfx = dfx + ddfx
					dfy = dfy + ddfy
					ddfx = ddfx + dddfx
					ddfy = ddfy + dddfy
					curveLength = curveLength + math_sqrt(dfx * dfx + dfy * dfy)
					segments[ii + 1] = curveLength
					ii = ii + 1
				end
				dfx = dfx + ddfx
				dfy = dfy + ddfy
				curveLength = curveLength + math_sqrt(dfx * dfx + dfy * dfy)
				segments[9] = curveLength
				dfx = dfx + ddfx + dddfx
				dfy = dfy + ddfy + dddfy
				curveLength = curveLength + math_sqrt(dfx * dfx + dfy * dfy)
				segments[10] = curveLength
				segment = 0
			end

			-- Weight by segment length.
			p = p * curveLength
			while true do
				local length = segments[segment + 1]
				if p <= length then
					if segment == 0 then
						p = p / length
					else
						local prev = segments[segment - 1 + 1]
						p = segment + (p - prev) / (length - prev)
					end
					break;
				end
				segment = segment + 1
			end
			self:addCurvePosition(p * 0.1, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents or (i > 0 and space == 0))
		end

		i = i + 1
		o = o + 3
	end
	return out
end

function PathConstraint:addBeforePosition (p, temp, i, out, o)
	local x1 = temp[i + 1]
	local y1 = temp[i + 2]
	local dx = temp[i + 3] - x1
	local dy = temp[i + 4] - y1
	local r = math_atan2(dy, dx)
	out[o + 1] = x1 + p * math_cos(r)
	out[o + 2] = y1 + p * math_sin(r)
	out[o + 3] = r
end

function PathConstraint:addAfterPosition(p, temp, i, out, o)
	local x1 = temp[i + 3]
	local y1 = temp[i + 4]
	local dx = x1 - temp[i + 1]
	local dy = y1 - temp[i + 2]
	local r = math_atan2(dy, dx)
	out[o + 1] = x1 + p * math_cos(r)
	out[o + 2] = y1 + p * math_sin(r)
	out[o + 3] = r
end

function PathConstraint:addCurvePosition(p, x1, y1, cx1, cy1, cx2, cy2, x2, y2, out, o, tangents)
	if p == 0 or (p ~= p) then
		out[o + 1] = x1
		out[o + 2] = y1
		out[o + 3] = math_atan2(cy1 - y1, cx1 - x1)
		return;
	end
	local tt = p * p
	local ttt = tt * p
	local u = 1 - p
	local uu = u * u
	local uuu = uu * u
	local ut = u * p
	local ut3 = ut * 3
	local uut3 = u * ut3
	local utt3 = ut3 * p
	local x = x1 * uuu + cx1 * uut3 + cx2 * utt3 + x2 * ttt
	local y = y1 * uuu + cy1 * uut3 + cy2 * utt3 + y2 * ttt
	out[o + 1] = x
	out[o + 2] = y
	if tangents then
		if p < 0.001 then
			out[o + 3] = math_atan2(cy1 - y1, cx1 - x1)
		else
			out[o + 3] = math_atan2(y - (y1 * uu + cy1 * ut * 2 + cy2 * tt), x - (x1 * uu + cx1 * ut * 2 + cx2 * tt))
		end
	end
end

return PathConstraint
