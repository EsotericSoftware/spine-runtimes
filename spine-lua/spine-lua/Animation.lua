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

-- FIXME
-- All the indexing in this file is zero based. We use zlen()
-- instead of the # operator. Initialization of number arrays
-- is performed via utils.newNumberArrayZero. This needs
-- to be rewritten using one-based indexing for better performance

local utils = require "spine-lua.utils"
local AttachmentType = require "spine-lua.attachments.AttachmentType"
local math_floor = math.floor
local math_abs = math.abs
local math_signum = utils.signum

local function zlen(array)
	return #array + 1
end

local Animation = {}
function Animation.new (name, timelines, duration)
	if not timelines then error("timelines cannot be nil", 2) end

	local self = {
		name = name,
		timelines = timelines,
		timelineIds = {},
		duration = duration
	}
	
	for i,timeline in ipairs(self.timelines) do
		self.timelineIds[timeline:getPropertyId()] = true
	end

	function self:hasTimeline(id)
		return self.timelineIds[id] == true
	end

	function self:apply (skeleton, lastTime, time, loop, events, alpha, blend, direction)
		if not skeleton then error("skeleton cannot be nil.", 2) end

		if loop and duration > 0 then
			time = time % self.duration
			if lastTime > 0 then lastTime = lastTime % self.duration end
		end

		for i,timeline in ipairs(self.timelines) do
			timeline:apply(skeleton, lastTime, time, events, alpha, blend, direction)
		end
	end

	return self
end

local function binarySearch (values, target, step)
	local low = 0
	local high = math.floor(zlen(values) / step - 2)
	if high == 0 then return step end
	local current = math.floor(high / 2)
	while true do
		if values[(current + 1) * step] <= target then
			low = current + 1
		else
			high = current
		end
		if low == high then return (low + 1) * step end
		current = math.floor((low + high) / 2)
	end
end
Animation.binarySearch = binarySearch

local function binarySearch1 (values, target)
	local low = 0
	local high = math.floor(zlen(values)	- 2)
	if high == 0 then return 1 end
	local current = math.floor(high / 2)
	while true do
		if values[current + 1] <= target then
			low = current + 1
		else
			high = current
		end
		if low == high then return low + 1 end
		current = math.floor((low + high) / 2)
	end
end

local function linearSearch (values, target, step)
	local i = 0
	local last = zlen(values) - step
	while i <= last do
		if (values[i] > target) then return i end
		i = i + step
	end
	return -1
end

Animation.MixBlend = {
	setup = 0,
	first = 1,
	replace = 2,
	add = 3
}
local MixBlend = Animation.MixBlend

Animation.MixDirection = {
	_in = 0, out = 1
}
local MixDirection = Animation.MixDirection

Animation.TimelineType = {
	rotate = 0, translate = 1, scale = 2, shear = 3,
	attachment = 4, color = 5, deform = 6,
	event = 7, drawOrder = 8,
	ikConstraint = 9, transformConstraint = 10,
	pathConstraintPosition = 11, pathConstraintSpacing = 12, pathConstraintMix = 13,
	twoColor = 14
}
local TimelineType = Animation.TimelineType
local SHL_24 = 16777216
local SHL_27 = 134217728

Animation.CurveTimeline = {}
function Animation.CurveTimeline.new (frameCount)
	local LINEAR = 0
	local STEPPED = 1
	local BEZIER = 2
	local BEZIER_SIZE = 10 * 2 - 1

	local self = {
		curves = utils.newNumberArrayZero((frameCount - 1) * BEZIER_SIZE) -- type, x, y, ...
	}

	function self:getFrameCount ()
		return math.floor(zlen(self.curves) / BEZIER_SIZE) + 1
	end

	function self:setStepped (frameIndex)
		self.curves[frameIndex * BEZIER_SIZE] = STEPPED
	end

	function self:getCurveType (frameIndex)
		local index = frameIndex * BEZIER_SIZE
		if index == zlen(self.curves) then return LINEAR end
		local type = self.curves[index]
		if type == LINEAR then return LINEAR end
		if type == STEPPED then return STEPPED end
		return BEZIER
	end

	function self:setCurve (frameIndex, cx1, cy1, cx2, cy2)
			local tmpx = (-cx1 * 2 + cx2) * 0.03
			local tmpy = (-cy1 * 2 + cy2) * 0.03
			local dddfx = ((cx1 - cx2) * 3 + 1) * 0.006
			local dddfy = ((cy1 - cy2) * 3 + 1) * 0.006
			local ddfx = tmpx * 2 + dddfx
			local ddfy = tmpy * 2 + dddfy
			local dfx = cx1 * 0.3 + tmpx + dddfx * 0.16666667
			local dfy = cy1 * 0.3 + tmpy + dddfy * 0.16666667

			local i = frameIndex * BEZIER_SIZE
			local curves = self.curves
			curves[i] = BEZIER
			i = i + 1

			local x = dfx
			local y = dfy
			local n = i + BEZIER_SIZE - 1
			while i < n do
				curves[i] = x
				curves[i + 1] = y
				dfx = dfx + ddfx
				dfy = dfy + ddfy
				ddfx = ddfx + dddfx
				ddfy = ddfy + dddfy
				x = x + dfx
				y = y + dfy
				i = i + 2
			end
	end

	function self:getCurvePercent (frameIndex, percent)
		percent = utils.clamp(percent, 0, 1)
		local curves = self.curves
		local i = frameIndex * BEZIER_SIZE
		local type = curves[i]
		if type == LINEAR then return percent end
		if type == STEPPED then return 0 end
		i = i + 1
		local x
		local n = i + BEZIER_SIZE - 1
		local start = i
		while i < n do
			x = curves[i]
			if x >= percent then
				local prevX, prevY
				if i == start then
					prevX = 0
					prevY = 0
				else
					prevX = curves[i - 2]
					prevY = curves[i - 1]
				end
				return prevY + (curves[i + 1] - prevY) * (percent - prevX) / (x - prevX)
			end
			i = i + 2
		end
		local y = curves[i - 1]
		return y + (1 - y) * (percent - x) / (1 - x) -- Last point is 1,1.
	end

	return self
end

Animation.RotateTimeline = {}
Animation.RotateTimeline.ENTRIES = 2
Animation.RotateTimeline.PREV_TIME = -2
Animation.RotateTimeline.PREV_ROTATION = -1
Animation.RotateTimeline.ROTATION = 1
function Animation.RotateTimeline.new (frameCount)
	local ENTRIES = Animation.RotateTimeline.ENTRIES
	local PREV_TIME = Animation.RotateTimeline.PREV_TIME
	local PREV_ROTATION = Animation.RotateTimeline.PREV_ROTATION
	local ROTATION = Animation.RotateTimeline.ROTATION

	local self = Animation.CurveTimeline.new(frameCount)
	self.boneIndex = -1
	self.frames = utils.newNumberArrayZero(frameCount * 2)
	self.type = TimelineType.rotate

	function self:getPropertyId ()
		return TimelineType.rotate * SHL_24 + self.boneIndex
	end

	function self:setFrame (frameIndex, time, degrees)
		frameIndex = frameIndex * 2
		self.frames[frameIndex] = time
		self.frames[frameIndex + ROTATION] = degrees
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.rotation = bone.data.rotation
			elseif blend == MixBlend.first then
				local r = bone.data.rotation - bone.rotation
				bone.rotation = bone.rotation + (r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360) * alpha
			end
			return
		end

		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			local r = frames[zlen(frames) + PREV_ROTATION]
			if blend == MixBlend.setup then
				bone.rotation = bone.data.rotation + r * alpha
			elseif blend == MixBlend.first or blend == MixBlend.replace then
				r = r + bone.data.rotation - bone.rotation
				r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360 -- Wrap within -180 and 180.
				bone.rotation = bone.rotation + r * alpha;
			elseif blend == MixBlend.add then
				bone.rotation = bone.rotation + r * alpha;
			end
			return;
		end

		-- Interpolate between the last frame and the current frame.
		local frame = binarySearch(frames, time, ENTRIES)
		local prevRotation = frames[frame + PREV_ROTATION]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent((math.floor(frame / 2)) - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

		local r = frames[frame + ROTATION] - prevRotation
		r = prevRotation + (r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360) * percent
		if blend == MixBlend.setup then
			bone.rotation = bone.data.rotation + (r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360) * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			r = r + bone.data.rotation - bone.rotation;
			bone.rotation = bone.rotation + (r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360) * alpha
		elseif blend == MixBlend.add then
			bone.rotation = bone.rotation + (r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360) * alpha
		end
	end

	return self
end

Animation.TranslateTimeline = {}
Animation.TranslateTimeline.ENTRIES = 3
function Animation.TranslateTimeline.new (frameCount)
	local ENTRIES = Animation.TranslateTimeline.ENTRIES
	local PREV_TIME = -3
	local PREV_X = -2
	local PREV_Y = -1
	local X = 1
	local Y = 2

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.boneIndex = -1
	self.type = TimelineType.translate

	function self:getPropertyId ()
		return TimelineType.translate * SHL_24 + self.boneIndex
	end

	function self:setFrame (frameIndex, time, x, y)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + X] = x
		self.frames[frameIndex + Y] = y
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.x = bone.data.x
				bone.y = bone.data.y
			elseif blend == MixBlend.first then
				bone.x = bone.x + (bone.data.x - bone.x) * alpha
				bone.y = bone.y + (bone.data.y - bone.y) * alpha
			end
			return
		end

		local x = 0
		local y = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- // Time is after last frame.
			x = frames[zlen(frames) + PREV_X];
			y = frames[zlen(frames) + PREV_Y];
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			x = frames[frame + PREV_X]
			y = frames[frame + PREV_Y]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			x = x + (frames[frame + X] - x) * percent
			y = y + (frames[frame + Y] - y) * percent
		end
		if blend == MixBlend.setup then
			bone.x = bone.data.x + x * alpha
			bone.y = bone.data.y + y * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			bone.x = bone.x + (bone.data.x + x - bone.x) * alpha
			bone.y = bone.y + (bone.data.y + y - bone.y) * alpha
		elseif blend == MixBlend.add then
			bone.x = bone.x + x * alpha
			bone.y = bone.y + y * alpha
		end
	end

	return self
end

Animation.ScaleTimeline = {}
Animation.ScaleTimeline.ENTRIES = Animation.TranslateTimeline.ENTRIES
function Animation.ScaleTimeline.new (frameCount)
	local ENTRIES = Animation.ScaleTimeline.ENTRIES
	local PREV_TIME = -3
	local PREV_X = -2
	local PREV_Y = -1
	local X = 1
	local Y = 2

	local self = Animation.TranslateTimeline.new(frameCount)
	self.type = TimelineType.scale

	function self:getPropertyId ()
		return TimelineType.scale * SHL_24 + self.boneIndex
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.scaleX = bone.data.scaleX
				bone.scaleY = bone.data.scaleY
			elseif blend == MixBlend.first then
				bone.scaleX = bone.scaleX + (bone.data.scaleX - bone.scaleX) * alpha
				bone.scaleY = bone.scaleY + (bone.data.scaleY - bone.scaleY) * alpha
			end
			return
		end

		local x = 0
		local y = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			x = frames[zlen(frames) + PREV_X] * bone.data.scaleX
			y = frames[zlen(frames) + PREV_Y] * bone.data.scaleY
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			x = frames[frame + PREV_X]
			y = frames[frame + PREV_Y]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			x = (x + (frames[frame + X] - x) * percent) * bone.data.scaleX
			y = (y + (frames[frame + Y] - y) * percent) * bone.data.scaleY
		end
		if alpha == 1 then
			if blend == MixBlend.add then
				bone.scaleX = bone.scaleX + x - bone.data.scaleX
				bone.scaleY = bone.scaleY + y - bone.data.scaleY
			else
				bone.scaleX = x
				bone.scaleY = y
			end
		else
			local bx = 0
			local by = 0
			if direction == MixDirection.out then
				if blend == MixBlend.setup then
					bx = bone.data.scaleX
					by = bone.data.scaleY
					bone.scaleX = bx + (math_abs(x) * math_signum(bx) - bx) * alpha
					bone.scaleY = by + (math_abs(y) * math_signum(by) - by) * alpha
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					bx = bone.scaleX
					by = bone.scaleY
					bone.scaleX = bx + (math_abs(x) * math_signum(bx) - bx) * alpha
					bone.scaleY = by + (math_abs(y) * math_signum(by) - by) * alpha
				elseif blend == MixBlend.add then
					bx = bone.scaleX
					by = bone.scaleY
					bone.scaleX = bx + (math_abs(x) * math_signum(bx) - bone.data.scaleX) * alpha
					bone.scaleY = by + (math_abs(y) * math_signum(by) - bone.data.scaleY) * alpha
				end
			else
				if blend == MixBlend.setup then
					bx = math_abs(bone.data.scaleX) * math_signum(x)
					by = math_abs(bone.data.scaleY) * math_signum(y)
					bone.scaleX = bx + (x - bx) * alpha
					bone.scaleY = by + (y - by) * alpha
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					bx = math_abs(bone.scaleX) * math_signum(x)
					by = math_abs(bone.scaleY) * math_signum(y)
					bone.scaleX = bx + (x - bx) * alpha
					bone.scaleY = by + (y - by) * alpha
				elseif blend == MixBlend.add then
					bx = math_signum(x)
					by = math_signum(y)
					bone.scaleX = math_abs(bone.scaleX) * bx + (x - math_abs(bone.data.scaleX) * bx) * alpha
					bone.scaleY = math_abs(bone.scaleY) * by + (y - math_abs(bone.data.scaleY) * by) * alpha
				end
			end
		end
	end

	return self
end

Animation.ShearTimeline = {}
Animation.ShearTimeline.ENTRIES = Animation.TranslateTimeline.ENTRIES
function Animation.ShearTimeline.new (frameCount)
	local ENTRIES = Animation.ShearTimeline.ENTRIES
	local PREV_TIME = -3
	local PREV_X = -2
	local PREV_Y = -1
	local X = 1
	local Y = 2

	local self = Animation.TranslateTimeline.new(frameCount)
	self.type = TimelineType.shear

	function self:getPropertyId ()
		return TimelineType.shear * SHL_24 + self.boneIndex
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.shearX = bone.data.shearX
				bone.shearY = bone.data.shearY
			elseif blend == MixBlend.first then
				bone.shearX = bone.shearX + (bone.data.shearX - bone.shearX) * alpha
				bone.shearY = bone.shearX + (bone.data.shearY - bone.shearY) * alpha
			end
			return
		end

		local x = 0
		local y = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- // Time is after last frame.
			x = frames[zlen(frames) + PREV_X]
			y = frames[zlen(frames) + PREV_Y]
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			x = frames[frame + PREV_X]
			y = frames[frame + PREV_Y]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			x = x + (frames[frame + X] - x) * percent
			y = y + (frames[frame + Y] - y) * percent
		end
		if blend == MixBlend.setup then
			bone.shearX = bone.data.shearX + x * alpha
			bone.shearY = bone.data.shearY + y * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			bone.shearX = bone.shearX + (bone.data.shearX + x - bone.shearX) * alpha
			bone.shearY = bone.shearY + (bone.data.shearY + y - bone.shearY) * alpha
		elseif blend == MixBlend.add then
			bone.shearX = bone.shearX + x * alpha
			bone.shearY = bone.shearY + y * alpha
		end
	end

	return self
end

Animation.ColorTimeline = {}
Animation.ColorTimeline.ENTRIES = 5
function Animation.ColorTimeline.new (frameCount)
	local ENTRIES = Animation.ColorTimeline.ENTRIES
	local PREV_TIME = -5
	local PREV_R = -4
	local PREV_G = -3
	local PREV_B = -2
	local PREV_A = -1
	local R = 1
	local G = 2
	local B = 3
	local A = 4

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.slotIndex = -1
	self.type = TimelineType.color

	function self:getPropertyId ()
		return TimelineType.color * SHL_24 + self.slotIndex
	end

	function self:setFrame (frameIndex, time, r, g, b, a)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + R] = r
		self.frames[frameIndex + G] = g
		self.frames[frameIndex + B] = b
		self.frames[frameIndex + A] = a
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end
		if time < frames[0] then
			if blend == MixBlend.setup then
				slot.color:setFrom(slot.data.color)
			elseif blend == MixBlend.first then
				local color = slot.color
				local setup = slot.data.color
				color:add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha)
			end
			return
		end

		local r, g, b, a
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			local i = zlen(frames)
			r = frames[i + PREV_R]
			g = frames[i + PREV_G]
			b = frames[i + PREV_B]
			a = frames[i + PREV_A]
		else
			-- Interpolate between the last frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			r = frames[frame + PREV_R]
			g = frames[frame + PREV_G]
			b = frames[frame + PREV_B]
			a = frames[frame + PREV_A]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math.floor(frame / ENTRIES) - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			r = r + (frames[frame + R] - r) * percent
			g = g + (frames[frame + G] - g) * percent
			b = b + (frames[frame + B] - b) * percent
			a = a + (frames[frame + A] - a) * percent
		end
		if alpha == 1 then
			slot.color:set(r, g, b, a)
		else
			local color = slot.color
			if blend == MixBlend.setup then color:setFrom(slot.data.color) end
			color:add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha)
		end
	end

	return self
end

Animation.TwoColorTimeline = {}
Animation.TwoColorTimeline.ENTRIES = 8
function Animation.TwoColorTimeline.new (frameCount)
	local ENTRIES = Animation.TwoColorTimeline.ENTRIES
	local PREV_TIME = -8
	local PREV_R = -7
	local PREV_G = -6
	local PREV_B = -5
	local PREV_A = -4
	local PREV_R2 = -3
	local PREV_G2 = -2
	local PREV_B2 = -1
	local R = 1
	local G = 2
	local B = 3
	local A = 4
	local R2 = 5
	local G2 = 6
	local B2 = 7

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.slotIndex = -1
	self.type = TimelineType.twoColor

	function self:getPropertyId ()
		return TimelineType.twoColor * SHL_24 + self.slotIndex
	end

	function self:setFrame (frameIndex, time, r, g, b, a, r2, g2, b2)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + R] = r
		self.frames[frameIndex + G] = g
		self.frames[frameIndex + B] = b
		self.frames[frameIndex + A] = a
		self.frames[frameIndex + R2] = r2
		self.frames[frameIndex + G2] = g2
		self.frames[frameIndex + B2] = b2
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		if time < frames[0] then
			if blend == MixBlend.setup then
				slot.color:setFrom(slot.data.color)
				slot.darkColor:setFrom(slot.data.darkColor)
			elseif blend == MixBlend.first then
				local light = slot.color
				local dark = slot.darkColor
				local setupLight = slot.data.color
				local setupDark = slot.data.darkColor
				light:add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
					(setupLight.a - light.a) * alpha)
				dark:add((setupDark.r - dark.r) * alpha, (setupDark.g - dark.g) * alpha, (setupDark.b - dark.b) * alpha, 0)
			end
			return
		end

		local r, g, b, a, r2, g2, b2
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			local i = zlen(frames)
			r = frames[i + PREV_R]
			g = frames[i + PREV_G]
			b = frames[i + PREV_B]
			a = frames[i + PREV_A]
			r2 = frames[i + PREV_R2]
			g2 = frames[i + PREV_G2]
			b2 = frames[i + PREV_B2]
		else
			-- Interpolate between the last frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			r = frames[frame + PREV_R]
			g = frames[frame + PREV_G]
			b = frames[frame + PREV_B]
			a = frames[frame + PREV_A]
			r2 = frames[frame + PREV_R2]
			g2 = frames[frame + PREV_G2]
			b2 = frames[frame + PREV_B2]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math.floor(frame / ENTRIES) - 1,
					1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			r = r + (frames[frame + R] - r) * percent
			g = g + (frames[frame + G] - g) * percent
			b = b + (frames[frame + B] - b) * percent
			a = a + (frames[frame + A] - a) * percent
			r2 = r2 + (frames[frame + R2] - r2) * percent
			g2 = g2 + (frames[frame + G2] - g2) * percent
			b2 = b2 + (frames[frame + B2] - b2) * percent
		end
		if alpha == 1 then
			slot.color:set(r, g, b, a)
			slot.darkColor:set(r2, g2, b2, 1)
		else
			local light = slot.color
			local dark = slot.darkColor
			if blend == MixBlend.setup then
				light:setFrom(slot.data.color)
				dark:setFrom(slot.data.darkColor)
			end
			light:add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha)
			dark:add((r2 - dark.r) * alpha, (g2 - dark.g) * alpha, (b2 - dark.b) * alpha, 0)
		end
	end

	return self
end

Animation.AttachmentTimeline = {}
function Animation.AttachmentTimeline.new (frameCount)
	local self = {
		frames = utils.newNumberArrayZero(frameCount), -- time, ...
		attachmentNames = {},
		slotIndex = -1,
		type = TimelineType.attachment
	}

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frameIndex, time, attachmentName)
		self.frames[frameIndex] = time
		self.attachmentNames[frameIndex] = attachmentName
	end

	function self:getPropertyId ()
		return TimelineType.attachment * SHL_24 + self.slotIndex
	end

	function self:setAttachment(skeleton, slot, attachmentName)
		attachmentName = slot.data.attachmentName
		if not attachmentName then
			slot:setAttachment(nil)
		else
			slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
		end
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end
		local attachmentName
		if direction == MixDirection.out then
			if blend == MixBlend.setup then
				self:setAttachment(skeleton, slot, slot.data.attachmentName)
			end
			return;
		end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup or blend == MixBlend.first then
				self:setAttachment(skeleton, slot, slot.data.attachmentName)
			end
			return
		end

		local frameIndex = 0
		if time >= frames[zlen(frames) - 1] then
			frameIndex = zlen(frames) - 1
		else
			frameIndex = binarySearch(frames, time, 1) - 1
		end

		attachmentName = self.attachmentNames[frameIndex]
		if not attachmentName then
			slot:setAttachment(nil)
		else
			slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
		end
	end

	return self
end

Animation.DeformTimeline = {}
function Animation.DeformTimeline.new (frameCount)
	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount)
	self.frameVertices = utils.newNumberArrayZero(frameCount)
	self.slotIndex = -1
	self.attachment = nil
	self.type = TimelineType.deform

	function self:getPropertyId ()
		return TimelineType.deform * SHL_27 + self.attachment.id + self.slotIndex
	end

	function self:setFrame (frameIndex, time, vertices)
		self.frames[frameIndex] = time
		self.frameVertices[frameIndex] = vertices
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local slotAttachment = slot.attachment
		if not slotAttachment then return end
		if not (slotAttachment.type == AttachmentType.mesh or slotAttachment.type == AttachmentType.linkedmesh or slotAttachment.type == AttachmentType.path or slotAttachment.type == AttachmentType.boundingbox) then return end
		if slotAttachment.deformAttachment ~= self.attachment then return end

		local frames = self.frames
		local deformArray = slot.deform
		if #(deformArray) == 0 then blend = MixBlend.setup end

		local frameVertices = self.frameVertices
		local vertexCount = #(frameVertices[0])

		if time < frames[0] then
			local vertexAttachment = slotAttachment;
			if blend == MixBlend.setup then
				slot.deform = {}
				return;
			elseif blend == MixBlend.first then
				if (alpha == 1) then
					slot.deform = {}
					return;
				end

				local deform = utils.setArraySize(deformArray, vertexCount)
				if (vertexAttachment.bones == nil) then
					local setupVertices = vertexAttachment.vertices
					local i = 1
					while i <= vertexCount do
						deform[i] = deform[i] + (setupVertices[i] - deform[i]) * alpha
						i = i + 1
					end
				else
					alpha = 1 - alpha
					local i = 1
					while i <= vertexCount do
						deform[i] = deform[i] * alpha
						i = i + 1
					end
				end
			end
			return
		end

		local deform = utils.setArraySize(deformArray, vertexCount)
		if time >= frames[zlen(frames) - 1] then -- Time is after last frame.
			local lastVertices = frameVertices[zlen(frames) - 1]
			if alpha == 1 then
				if blend == MixBlend.add then
					local vertexAttachment = slotAttachment
					if vertexAttachment.bones == nil then
						-- Unweighted vertex positions, with alpha.
						local setupVertices = vertexAttachment.vertices
						local i = 1
						while i <= vertexCount do
							deform[i] = deform[i] + lastVertices[i] - setupVertices[i]
							i = i + 1
						end
					else
						-- Weighted deform offsets, with alpha.
						local i = 1
						while i <= vertexCount do
							deform[i] = deform[i] + lastVertices[i]
							i = i + 1
						end
					end
				else
					local i = 1
					while i <= vertexCount do
						deform[i] = lastVertices[i]
						i = i + 1
					end
				end
			else
				if blend == MixBlend.setup then
					local vertexAttachment = slotAttachment
					if vertexAttachment.bones == nil then
						-- Unweighted vertex positions, with alpha.
						local setupVertices = vertexAttachment.vertices
						local i = 1
						while i <= vertexCount do
							local setup = setupVertices[i]
							deform[i] = setup + (lastVertices[i] - setup) * alpha
							i = i + 1
						end
					else
						-- Weighted deform offsets, with alpha.
						local i = 1
						while i <= vertexCount do
							deform[i] = lastVertices[i] * alpha
							i = i + 1
						end
					end
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					local i = 1
					while i <= vertexCount do
						deform[i] = deform[i] + (lastVertices[i] - deform[i]) * alpha
						i = i + 1
					end
					local vertexAttachment = slotAttachment
					if vertexAttachment.bones == nil then
						local setupVertices = vertexAttachment.vertices
						local i = 1
						while i <= vertexCount do
							deform[i] = deform[i] + (lastVertices[i] - setupVertices[i]) * alpha
							i = i + 1
						end
					else
						-- Weighted deform offsets, with alpha.
						local i = 1
						while i <= vertexCount do
							deform[i] = deform[i] + lastVertices[i] * alpha
							i = i + 1
						end
					end
				elseif blend == MixBlend.add then
					local vertexAttachment = slotAttachment
					if vertexAttachment.bones == nil then
						local setupVertices = vertexAttachment.vertices
						local i = 1
						while i <= vertexCount do
							deform[i] = deform[i] + (lastVertices[i] - setupVertices[i]) * alpha
							i = i + 1
						end
					else
						-- Weighted deform offsets, with alpha.
						local i = 1
						while i <= vertexCount do
							deform[i] = deform[i] + lastVertices[i] * alpha
							i = i + 1
						end
					end
				end
			end
			return;
		end

		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, 1)
		local prevVertices = frameVertices[frame - 1]
		local nextVertices = frameVertices[frame]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime))

		if alpha == 1 then
			if blend == MixBlend.add then
				local vertexAttachment = slotAttachment
				if vertexAttachment.bones == nil then
					-- Unweighted vertex positions, with alpha.
					local setupVertices = vertexAttachment.vertices
					local i = 1
					while i <= vertexCount do
						local prev = prevVertices[i]
						deform[i] = deform[i] + prev + (nextVertices[i] - prev) * precent - setupVertices[i]
						i = i + 1
					end
				else
					-- Weighted deform offsets, with alpha.
					local i = 1
					while i <= vertexCount do
						local prev = prevVertices[i]
						deform[i] = deform[i] + prev + (nextVertices[i] - prev) * percent
						i = i + 1
					end
				end
			else
				local i = 1
				while i <= vertexCount do
					local prev = prevVertices[i]
					deform[i] = prev + (nextVertices[i] - prev) * percent
					i = i + 1
				end
			end
		else
			if blend == MixBlend.setup then
				local vertexAttachment = slotAttachment
				if vertexAttachment.bones == nil then
					-- Unweighted vertex positions, with alpha.
					local setupVertices = vertexAttachment.vertices
					local i = 1
					while i <= vertexCount do
						local prev = prevVertices[i]
						local setup = setupVertices[i]
						deform[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha
						i = i + 1
					end
				else
					-- Weighted deform offsets, with alpha.
					local i = 1
					while i <= vertexCount do
						local prev = prevVertices[i]
						deform[i] = (prev + (nextVertices[i] - prev) * percent) * alpha
						i = i + 1
					end
				end
			elseif blend == MixBlend.first or blend == MixBlend.replace then
				local i = 1
				while i <= vertexCount do
					local prev = prevVertices[i]
					deform[i] = deform[i] + (prev + (nextVertices[i] - prev) * percent - deform[i]) * alpha
					i = i + 1
				end
			elseif blend == MixBlend.add then
				local vertexAttachment = slotAttachment
				if vertexAttachment.bones == nil then
					local setupVertices = vertexAttachment.vertices
					local i = 1
					while i <= vertexCount do
						local prev = prevVertices[i]
						deform[i] = deform[i] + (prev + (nextVertices[i] - prev) * percent - setupVertices[i]) * alpha
						i = i + 1
					end
				else
					-- Weighted deform offsets, with alpha.
					local i = 1
					while i <= vertexCount do
						local prev = prevVertices[i]
						deform[i] = deform[i] + (prev + (nextVertices[i] - prev) * percent) * alpha
						i = i + 1
					end
				end
			end
		end
	end

	return self
end

Animation.EventTimeline = {}
function Animation.EventTimeline.new (frameCount)
	local self = {
		frames = utils.newNumberArrayZero(frameCount),
		events = {},
		type = TimelineType.event
	}

	function self:getPropertyId ()
		return TimelineType.event * SHL_24
	end

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frameIndex, event)
		self.frames[frameIndex] = event.time
		self.events[frameIndex] = event
	end

	-- Fires events for frames > lastTime and <= time.
	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		if not firedEvents then return end

		local frames = self.frames
		local frameCount = zlen(frames)

		if lastTime > time then -- Fire events after last time for looped animations.
			self:apply(skeleton, lastTime, 999999, firedEvents, alpha, blend, direction)
			lastTime = -1
		elseif lastTime >= frames[frameCount - 1] then -- Last time is after last frame.
			return
		end
		if time < frames[0] then return end -- Time is before first frame.

		local frame
		if lastTime < frames[0] then
			frame = 0
		else
			frame = binarySearch1(frames, lastTime)
			local frame = frames[frame]
			while frame > 0 do -- Fire multiple events with the same frame.
				if frames[frame - 1] ~= frame then break end
				frame = frame - 1
			end
		end
		local events = self.events
		while frame < frameCount and time >= frames[frame] do
			table.insert(firedEvents, events[frame])
			frame = frame + 1
		end
	end

	return self
end

Animation.DrawOrderTimeline = {}
function Animation.DrawOrderTimeline.new (frameCount)
	local self = {
		frames = utils.newNumberArrayZero(frameCount),
		drawOrders = {},
		type = TimelineType.drawOrder
	}

	function self:getPropertyId ()
		return TimelineType.drawOrder * SHL_24
	end

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frameIndex, time, drawOrder)
		self.frames[frameIndex] = time
		self.drawOrders[frameIndex] = drawOrder
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local drawOrder = skeleton.drawOrder
		local slots = skeleton.slots
		if direction == MixDirection.out then
			if blend == MixBlend.setup then
				for i,slot in ipairs(slots) do
					drawOrder[i] = slots[i]
				end
			end
			return;
		end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup or blend == MixBlend.first then
				for i,slot in ipairs(slots) do
					drawOrder[i] = slots[i]
				end
			end
			return
		end

		local frame
		if time >= frames[zlen(frames) - 1] then -- Time is after last frame.
			frame = zlen(frames) - 1
		else
			frame = binarySearch1(frames, time) - 1
		end

		local drawOrderToSetupIndex = self.drawOrders[frame]
		if not drawOrderToSetupIndex then
			for i,slot in ipairs(slots) do
				drawOrder[i] = slots[i]
			end
		else
			for i,setupIndex in ipairs(drawOrderToSetupIndex) do
				drawOrder[i] = skeleton.slots[setupIndex]
			end
		end
	end

	return self
end

Animation.IkConstraintTimeline = {}
Animation.IkConstraintTimeline.ENTRIES = 6
function Animation.IkConstraintTimeline.new (frameCount)
	local ENTRIES = Animation.IkConstraintTimeline.ENTRIES
	local PREV_TIME = -6
	local PREV_MIX = -5
	local PREV_SOFTNESS = -4
	local PREV_BEND_DIRECTION = -3
	local PREV_COMPRESS = -2
	local PREV_STRETCH = -1
	local MIX = 1
	local SOFTNESS = 2
	local BEND_DIRECTION = 3
	local COMPRESS = 4
	local STRETCH = 5

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES) -- time, mix, softness, bendDirection, compress, stretch, ...
	self.ikConstraintIndex = -1
	self.type = TimelineType.ikConstraint

	function self:getPropertyId ()
		return TimelineType.ikConstraint * SHL_24 + self.ikConstraintIndex
	end

	function self:setFrame (frameIndex, time, mix, softness, bendDirection, compress, stretch)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + MIX] = mix
		self.frames[frameIndex + SOFTNESS] = softness
		self.frames[frameIndex + BEND_DIRECTION] = bendDirection
		if (compress) then
			self.frames[frameIndex + COMPRESS] = 1
		else
			self.frames[frameIndex + COMPRESS] = 0
		end
		if (stretch) then
			self.frames[frameIndex + STRETCH] = 1
		else
			self.frames[frameIndex + STRETCH] = 0
		end
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local constraint = skeleton.ikConstraints[self.ikConstraintIndex]
		if not constraint.active then return end
		if time < frames[0] then
			if blend == MixBlend.setup then
				constraint.mix = constraint.data.mix
				constraint.softness = constraint.data.softness
				constraint.bendDirection = constraint.data.bendDirection
				constraint.compress = constraint.data.compress
				constraint.stretch = constraint.data.stretch
			elseif blend == MixBlend.first then
				constraint.mix = constraint.mix + (constraint.data.mix - constraint.mix) * alpha
				constraint.softness = constraint.softness + (constraint.data.softness - constraint.softness) * alpha
				constraint.bendDirection = constraint.data.bendDirection
				constraint.compress = constraint.data.compress
				constraint.stretch = constraint.data.stretch
			end
			return
		end

		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			if blend == MixBlend.setup then
				constraint.mix = constraint.data.mix + (frames[zlen(frames) + PREV_MIX] - constraint.data.mix) * alpha
				constraint.softness = constraint.data.softness + (frames[zlen(frames) + PREV_SOFTNESS] - constraint.data.softness) * alpha
				if direction == MixDirection.out then
					constraint.bendDirection = constraint.data.bendDirection
					constraint.compress = constraint.data.compress
					constraint.stretch = constraint.data.stretch
				else
					constraint.bendDirection = math_floor(frames[zlen(frames) + PREV_BEND_DIRECTION]);
					if (math_floor(frames[zlen(frames) + PREV_COMPRESS]) == 1) then constraint.compress = true else constraint.compress = false end
					if (math_floor(frames[zlen(frames) + PREV_STRETCH]) == 1) then constraint.stretch = true else constraint.stretch = false end
				end
			else
				constraint.mix = constraint.mix + (frames[zlen(frames) + PREV_MIX] - constraint.mix) * alpha
				constraint.softness = constraint.softness + (frames[zlen(frames) + PREV_SOFTNESS] - constraint.softness) * alpha
				if direction == MixDirection._in then
					constraint.bendDirection = math_floor(frames[zlen(frames) + PREV_BEND_DIRECTION])
					if (math_floor(frames[zlen(frames) + PREV_COMPRESS]) == 1) then constraint.compress = true else constraint.compress = false end
					if (math_floor(frames[zlen(frames) + PREV_STRETCH]) == 1) then constraint.stretch = true else constraint.stretch = false end
				end
			end
			return
		end

		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, ENTRIES)
		local mix = frames[frame + PREV_MIX]
		local softness = frames[frame + PREV_SOFTNESS]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math.floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

		if blend == MixBlend.setup then
			constraint.mix = constraint.data.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.data.mix) * alpha
			constraint.softness = constraint.data.softness + (softness + (frames[frame + SOFTNESS] - softness) * percent - constraint.data.softness) * alpha
			if direction == MixDirection.out then
				constraint.bendDirection = constraint.data.bendDirection
				constraint.compress = constraint.data.compress
				constraint.stretch = constraint.data.stretch
			else
				constraint.bendDirection = math_floor(frames[frame + PREV_BEND_DIRECTION])
				if (math_floor(frames[frame + PREV_COMPRESS]) == 1) then constraint.compress = true else constraint.compress = false end
				if (math_floor(frames[frame + PREV_STRETCH]) == 1) then constraint.stretch = true else constraint.stretch = false end
			end
		else
			constraint.mix = constraint.mix + (mix + (frames[frame + MIX] - mix) * percent - constraint.mix) * alpha
			constraint.softness = constraint.softness + (softness + (frames[frame + SOFTNESS] - softness) * percent - constraint.softness) * alpha
			if direction == MixDirection._in then
				constraint.bendDirection = math_floor(frames[frame + PREV_BEND_DIRECTION])
				if (math_floor(frames[frame + PREV_COMPRESS]) == 1) then constraint.compress = true else constraint.compress = false end
				if (math_floor(frames[frame + PREV_STRETCH]) == 1) then constraint.stretch = true else constraint.stretch = false end
			end
		end
	end

	return self
end

Animation.TransformConstraintTimeline = {}
Animation.TransformConstraintTimeline.ENTRIES = 5
function Animation.TransformConstraintTimeline.new (frameCount)
	local ENTRIES = Animation.TransformConstraintTimeline.ENTRIES
	local PREV_TIME = -5
	local PREV_ROTATE = -4
	local PREV_TRANSLATE = -3
	local PREV_SCALE = -2
	local PREV_SHEAR = -1
	local ROTATE = 1
	local TRANSLATE = 2
	local SCALE = 3
	local SHEAR = 4

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.transformConstraintIndex = -1
	self.type = TimelineType.transformConstraint

	function self:getPropertyId ()
		return TimelineType.transformConstraint * SHL_24 + self.transformConstraintIndex
	end

	function self:setFrame (frameIndex, time, rotateMix, translateMix, scaleMix, shearMix)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + ROTATE] = rotateMix
		self.frames[frameIndex + TRANSLATE] = translateMix
		self.frames[frameIndex + SCALE] = scaleMix
		self.frames[frameIndex + SHEAR] = shearMix
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local constraint = skeleton.transformConstraints[self.transformConstraintIndex]
		if not constraint.active then return end

		if time < frames[0] then
			local data = constraint.data
			if blend == MixBlend.setup then
				constraint.rotateMix = data.rotateMix
				constraint.translateMix = data.translateMix
				constraint.scaleMix = data.scaleMix
				constraint.shearMix = data.shearMix
			elseif blend == MixBlend.first then
				constraint.rotateMix = constraint.rotateMix + (data.rotateMix - constraint.rotateMix) * alpha
				constraint.translateMix = constraint.translateMix + (data.translateMix - constraint.translateMix) * alpha
				constraint.scaleMix = constraint.scaleMix + (data.scaleMix - constraint.scaleMix) * alpha
				constraint.shearMix = constraint.shearMix + (data.shearMix - constraint.shearMix) * alpha
			end
			return
		end

		local rotate = 0
		local translate = 0
		local scale = 0
		local shear = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			local i = zlen(frames)
			rotate = frames[i + PREV_ROTATE]
			translate = frames[i + PREV_TRANSLATE]
			scale = frames[i + PREV_SCALE]
			shear = frames[i + PREV_SHEAR]
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			rotate = frames[frame + PREV_ROTATE]
			translate = frames[frame + PREV_TRANSLATE]
			scale = frames[frame + PREV_SCALE]
			shear = frames[frame + PREV_SHEAR]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

			rotate = rotate + (frames[frame + ROTATE] - rotate) * percent
			translate = translate + (frames[frame + TRANSLATE] - translate) * percent
			scale = scale + (frames[frame + SCALE] - scale) * percent
			shear = shear + (frames[frame + SHEAR] - shear) * percent
		end
		if blend == MixBlend.setup then
			local data = constraint.data
			constraint.rotateMix = data.rotateMix + (rotate - data.rotateMix) * alpha
			constraint.translateMix = data.translateMix + (translate - data.translateMix) * alpha
			constraint.scaleMix = data.scaleMix + (scale - data.scaleMix) * alpha
			constraint.shearMix = data.shearMix + (shear - data.shearMix) * alpha
		else
			constraint.rotateMix = constraint.rotateMix + (rotate - constraint.rotateMix) * alpha
			constraint.translateMix = constraint.translateMix + (translate - constraint.translateMix) * alpha
			constraint.scaleMix = constraint.scaleMix + (scale - constraint.scaleMix) * alpha
			constraint.shearMix = constraint.shearMix + (shear - constraint.shearMix) * alpha
		end
	end

	return self
end

Animation.PathConstraintPositionTimeline = {}
Animation.PathConstraintPositionTimeline.ENTRIES = 2
function Animation.PathConstraintPositionTimeline.new (frameCount)
	local ENTRIES = Animation.PathConstraintPositionTimeline.ENTRIES
	local PREV_TIME = -2
	local PREV_VALUE = -1
	local VALUE = 1

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.pathConstraintIndex = -1
	self.type = TimelineType.pathConstraintPosition

	function self:getPropertyId ()
		return TimelineType.pathConstraintPosition * SHL_24 + self.pathConstraintIndex
	end

	function self:setFrame (frameIndex, time, value)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + VALUE] = value
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
		if not constraint.active then return end

		if (time < frames[0]) then
			if blend == MixBlend.setup then
				constraint.position = constraint.data.position
			elseif blend == MixBlend.first then
				constraint.position = constraint.position + (constraint.data.position - constraint.position) * alpha
			end
			return
		end

		local position = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			position = frames[zlen(frames) + PREV_VALUE]
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			position = frames[frame + PREV_VALUE]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			position = position + (frames[frame + VALUE] - position) * percent
		end
		if blend == MixBlend.setup then
			constraint.position = constraint.data.position + (position - constraint.data.position) * alpha
		else
			constraint.position = constraint.position + (position - constraint.position) * alpha
		end
	end

	return self
end

Animation.PathConstraintSpacingTimeline = {}
Animation.PathConstraintSpacingTimeline.ENTRIES = 2
function Animation.PathConstraintSpacingTimeline.new (frameCount)
	local ENTRIES = Animation.PathConstraintSpacingTimeline.ENTRIES
	local PREV_TIME = -2
	local PREV_VALUE = -1
	local VALUE = 1

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.pathConstraintIndex = -1
	self.type = TimelineType.pathConstraintSpacing

	function self:getPropertyId ()
		return TimelineType.pathConstraintSpacing * SHL_24 + self.pathConstraintIndex
	end

	function self:setFrame (frameIndex, time, value)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + VALUE] = value
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
		if not constraint.active then return end

		if (time < frames[0]) then
			if blend == MixBlend.setup then
				constraint.spacing = constraint.data.spacing
			elseif blend == MixBlend.first then
				constraint.spacing = constraint.spacing + (constraint.data.spacing - constraint.spacing) * alpha
			end
			return
		end

		local spacing = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			spacing = frames[zlen(frames) + PREV_VALUE]
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			spacing = frames[frame + PREV_VALUE]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			spacing = spacing + (frames[frame + VALUE] - spacing) * percent
		end

		if blend == MixBlend.setup then
			constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha
		else
			constraint.spacing = constraint.spacing + (spacing - constraint.spacing) * alpha
		end
	end

	return self
end

Animation.PathConstraintMixTimeline = {}
Animation.PathConstraintMixTimeline.ENTRIES = 3
function Animation.PathConstraintMixTimeline.new (frameCount)
	local ENTRIES = Animation.PathConstraintMixTimeline.ENTRIES
	local PREV_TIME = -3
	local PREV_ROTATE = -2
	local PREV_TRANSLATE = -1
	local ROTATE = 1
	local TRANSLATE = 2

	local self = Animation.CurveTimeline.new(frameCount)
	self.frames = utils.newNumberArrayZero(frameCount * ENTRIES)
	self.pathConstraintIndex = -1
	self.type = TimelineType.pathConstraintMix

	function self:getPropertyId ()
		return TimelineType.pathConstraintMix * SHL_24 + self.pathConstraintIndex
	end

	function self:setFrame (frameIndex, time, rotateMix, translateMix)
		frameIndex = frameIndex * ENTRIES
		self.frames[frameIndex] = time
		self.frames[frameIndex + ROTATE] = rotateMix
		self.frames[frameIndex + TRANSLATE] = translateMix
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha, blend, direction)
		local frames = self.frames

		local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
		if not constraint.active then return end

		if (time < frames[0]) then
			if blend == MixBlend.setup then
				constraint.rotateMix = constraint.data.rotateMix
				constraint.translateMix = constraint.data.translateMix
			elseif blend == MixBlend.first then
				constraint.rotateMix = constraint.rotateMix + (constraint.data.rotateMix - constraint.rotateMix) * alpha
				constraint.translateMix = constraint.translateMix + (constraint.data.translateMix - constraint.translateMix) * alpha
			end
			return
		end

		local rotate = 0
		local translate = 0
		if time >= frames[zlen(frames) - ENTRIES] then -- Time is after last frame.
			rotate = frames[zlen(frames) + PREV_ROTATE]
			translate = frames[zlen(frames) + PREV_TRANSLATE]
		else
			-- Interpolate between the previous frame and the current frame.
			local frame = binarySearch(frames, time, ENTRIES)
			rotate = frames[frame + PREV_ROTATE]
			translate = frames[frame + PREV_TRANSLATE]
			local frameTime = frames[frame]
			local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

			rotate = rotate + (frames[frame + ROTATE] - rotate) * percent
			translate = translate + (frames[frame + TRANSLATE] - translate) * percent
		end

		if blend == MixBlend.setup then
			constraint.rotateMix = constraint.data.rotateMix + (rotate - constraint.data.rotateMix) * alpha
			constraint.translateMix = constraint.data.translateMix + (translate - constraint.data.translateMix) * alpha
		else
			constraint.rotateMix = constraint.rotateMix + (rotate - constraint.rotateMix) * alpha
			constraint.translateMix = constraint.translateMix + (translate - constraint.translateMix) * alpha
		end
	end

	return self
end

return Animation
