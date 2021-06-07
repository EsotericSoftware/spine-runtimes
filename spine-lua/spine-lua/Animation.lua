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

local setmetatable = setmetatable
local math_floor = math.floor
local math_abs = math.abs
local math_signum = utils.signum

local function zlen(array)
	return #array + 1
end

local Animation = {}

function Animation.new (name, timelines, duration)
	if not name then error("name cannot be nil", 2) end
	if not timelines then error("timelines cannot be nil", 2) end

	local self = {
		name = name,
		timelines = timelines,
		timelineIds = nil,
		duration = duration
	}

	function self:setTimelines (timelines)
		self.timelines = timelines

		self.timelineIds = {}
		for i,timeline in ipairs(self.timelines) do
			for _,id in ipairs(timeline.propertyIds) do
				self.timelineIds[id] = true
			end
		end
	end

	function self:hasTimeline (ids)
		for _,id in ipairs(ids) do
			if self.timelineIds[id] then return true end
		end
		return false
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

	self:setTimelines(timelines)
	return self
end

Animation.MixBlend = {
	setup = 0,
	first = 1,
	replace = 2,
	add = 3
}
local MixBlend = Animation.MixBlend

Animation.MixDirection = {
	mixIn = 0, mixOut = 1
}
local MixDirection = Animation.MixDirection

Animation.Property = {
	rotate = 0,
	x = 1,
	y = 2,
	scaleX = 3,
	scaleY = 4,
	shearX = 5,
	shearY = 6,

	rgb = 7,
	alpha = 8,
	rgb2 = 9,

	attachment = 10,
	deform = 11,

	event = 12,
	drawOrder = 13,

	ikConstraint = 14,
	transformConstraint = 15,

	pathConstraintPosition = 16,
	pathConstraintSpacing = 17,
	pathConstraintMix = 18
}
local Property = Animation.Property

Animation.TimelineType = {
	rotate = 0,
	translate = 1, translateX = 2, translateY = 3,
	scale = 4, scaleX = 5, scaleY = 6,
	shear = 7, shearX = 8, shearY = 9,
	rgba = 10, rgb = 11, alpha = 12, rgba2 = 13, rgb2 = 14,
	attachment = 15,
	deform = 16,
	event = 17,
	drawOrder = 18,
	ikConstraint = 19,
	transformConstraint = 20,
	pathConstraintPosition = 21, pathConstraintSpacing = 22, pathConstraintMix = 23
}
local TimelineType = Animation.TimelineType

Animation.Timeline = {}
function Animation.Timeline.new (timelineType, frameEntries, frameCount, propertyIds)
	local self = {
		timelineType = timelineType,
		propertyIds = propertyIds,
		frames = utils.newNumberArrayZero(frameCount * frameEntries)
	}

	function self:getFrameEntries ()
		return 1
	end
	
	function self:getFrameCount ()
		return math_floor(zlen(self.frames) / self:getFrameEntries())
	end
	
	function self:getDuration ()
		return self.frames[zlen(self.frames) - self:getFrameEntries()]
	end

	return self
end

local function search1 (frames, time)
	local n = zlen(frames)
	local i = 1
	while i < n do
		if frames[i] > time then return i - 1 end
		i = i + 1
	end
	return n - 1
end
Animation.Timeline.search1 = search1

local function search (frames, time, step)
	local n = zlen(frames)
	local i = step
	while i < n do
		if frames[i] > time then return i - step end
		i = i + step
	end
	return n - step
end

local LINEAR = 0
local STEPPED = 1
local BEZIER = 2
local BEZIER_SIZE = 18

Animation.CurveTimeline = {}
function Animation.CurveTimeline.new (timelineType, frameEntries, frameCount, bezierCount, propertyIds)
	local self = Animation.Timeline.new(timelineType, frameEntries, frameCount, propertyIds)
	self.curves = utils.newNumberArrayZero(frameCount + bezierCount * BEZIER_SIZE)
	self.curves[frameCount - 1] = STEPPED

	function self:setStepped (frame)
		self.curves[frame] = STEPPED
	end

	function self:setLinear (frame)
		self.curves[frame] = LINEAR
	end
	
	function self:shrink (bezierCount)
		utils.setArraySize(self.curves, self:getFrameCount() + bezierCount * BEZIER_SIZE)
	end

	function self:setBezier (bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2)
		local curves = self.curves
		local i = self:getFrameCount() + bezier * BEZIER_SIZE
		if value == 0 then curves[frame] = BEZIER + i end
		local tmpx = (time1 - cx1 * 2 + cx2) * 0.03
		local tmpy = (value1 - cy1 * 2 + cy2) * 0.03
		local dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006
		local dddy = ((cy1 - cy2) * 3 - value1 + value2) * 0.006
		local ddx = tmpx * 2 + dddx
		local ddy = tmpy * 2 + dddy
		local dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667
		local dy = (cy1 - value1) * 0.3 + tmpy + dddy * 0.16666667
		local x = time1 + dx
		local y = value1 + dy
		local n = i + BEZIER_SIZE
		while i < n do
			curves[i] = x
			curves[i + 1] = y
			dx = dx + ddx
			dy = dy + ddy
			ddx = ddx + dddx
			ddy = ddy + dddy
			x = x + dx
			y = y + dy
			i = i + 2
		end
	end

	function self:getBezierValue (time, frameIndex, valueOffset, i)
		local curves = self.curves
		if curves[i] > time then
			local x = self.frames[frameIndex]
			local y = self.frames[frameIndex + valueOffset]
			return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y)
		end
		local n = i + BEZIER_SIZE
		i = i + 2
		while i < n do
			if curves[i] >= time then
				local x = curves[i - 2]
				local y = curves[i - 1]
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y)
			end
			i = i + 2
		end
		frameIndex = frameIndex + self:getFrameEntries()
		local x = curves[n - 2]
		local y = curves[n - 1]
		return y + (time - x) / (self.frames[frameIndex] - x) * (self.frames[frameIndex + valueOffset] - y)
	end

	return self
end

Animation.CurveTimeline1 = {}
function Animation.CurveTimeline1.new (timelineType, frameCount, bezierCount, propertyId)
	local ENTRIES = 2
	local VALUE = 1

	local self = Animation.CurveTimeline.new(timelineType, ENTRIES, frameCount, bezierCount, { propertyId })

	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, value)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + VALUE] = value
	end

	function self:getCurveValue (time)
		local frames = self.frames
		local i = zlen(frames) - 2
		local ii = 2
		while ii <= i do
			if frames[ii] > time then
				i = ii - 2
				break
			end
			ii = ii + 2
		end
		local curveType = self.curves[i / 2]
		if curveType == LINEAR then
			local before = frames[i]
			local value = frames[i + VALUE]
			return value + (time - before) / (frames[i + ENTRIES] - before) * (frames[i + ENTRIES + VALUE] - value)
		elseif curveType == STEPPED then
			return frames[i + VALUE]
		end
		return self:getBezierValue(time, i, VALUE, curveType - BEZIER)
	end

	return self
end

Animation.CurveTimeline2 = {}
function Animation.CurveTimeline2.new (timelineType, frameCount, bezierCount, propertyId1, propertyId2)
	local ENTRIES = 3
	local VALUE1 = 1
	local VALUE2 = 2

	local self = Animation.CurveTimeline.new(timelineType, ENTRIES, frameCount, bezierCount, { propertyId1, propertyId2 })

	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, value1, value2)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + VALUE1] = value1
		self.frames[frame + VALUE2] = value2
	end

	return self
end

Animation.RotateTimeline = {}
function Animation.RotateTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.rotate, frameCount, bezierCount, Property.rotate.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.rotation = bone.data.rotation
			elseif blend == MixBlend.first then
				bone.rotation = bone.rotation + (bone.data.rotation - bone.rotation) * alpha
			end
			return
		end

		local r = self:getCurveValue(time)
		if blend == MixBlend.setup then
			bone.rotation = bone.data.rotation + r * alpha
		else
			if blend == MixBlend.first or blend == MixBlend.replace then
				r = r + bone.data.rotation - bone.rotation
			end
			bone.rotation = bone.rotation + r * alpha
		end
	end

	return self
end

Animation.TranslateTimeline = {}
function Animation.TranslateTimeline.new (frameCount, bezierCount, boneIndex)
	local ENTRIES = 3
	local VALUE1 = 1
	local VALUE2 = 2

	local self = Animation.CurveTimeline2.new(TimelineType.translate, frameCount, bezierCount,
		Property.x.."|"..boneIndex,
		Property.y.."|"..boneIndex
	)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
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
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[math_floor(i / ENTRIES)]
		if curveType == LINEAR then
			local before = frames[i]
			x = frames[i + VALUE1]
			y = frames[i + VALUE2]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			x = x + (frames[i + ENTRIES + VALUE1] - x) * t
			y = y + (frames[i + ENTRIES + VALUE2] - y) * t
		elseif curveType == STEPPED then
			x = frames[i + VALUE1]
			y = frames[i + VALUE2]
		else
			x = self:getBezierValue(time, i, VALUE1, curveType - BEZIER)
			y = self:getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER)
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

Animation.TranslateXTimeline = {}
function Animation.TranslateXTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.translateX, frameCount, bezierCount, Property.x.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.x = bone.data.x
			elseif blend == MixBlend.first then
				bone.x = bone.x + (bone.data.x - bone.x) * alpha
			end
			return
		end

		local x = self:getCurveValue(time)
		if blend == MixBlend.setup then
			bone.x = bone.data.x + x * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			bone.x = bone.x + (bone.data.x + x - bone.x) * alpha
		elseif blend == MixBlend.add then
			bone.x = bone.x + x * alpha
		end
	end

	return self
end

Animation.TranslateYTimeline = {}
function Animation.TranslateYTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.translateY, frameCount, bezierCount, Property.x.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.y = bone.data.y
			elseif blend == MixBlend.first then
				bone.y = bone.y + (bone.data.y - bone.y) * alpha
			end
			return
		end

		local y = self:getCurveValue(time)
		if blend == MixBlend.setup then
			bone.y = bone.data.y + y * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			bone.y = bone.y + (bone.data.y + y - bone.y) * alpha
		elseif blend == MixBlend.add then
			bone.y = bone.y + y * alpha
		end
	end

	return self
end

Animation.ScaleTimeline = {}
function Animation.ScaleTimeline.new (frameCount, bezierCount, boneIndex)
	local ENTRIES = 3
	local VALUE1 = 1
	local VALUE2 = 2

	local self = Animation.CurveTimeline2.new(TimelineType.scale, frameCount, bezierCount,
		Property.scaleX.."|"..boneIndex,
		Property.scaleY.."|"..boneIndex
	)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
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

		local x
		local y
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[math_floor(i / ENTRIES)]
		if curveType == LINEAR then
			local before = frames[i]
			x = frames[i + VALUE1]
			y = frames[i + VALUE2]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			x = x + (frames[i + ENTRIES + VALUE1] - x) * t
			y = y + (frames[i + ENTRIES + VALUE2] - y) * t
		elseif curveType == STEPPED then
			x = frames[i + VALUE1]
			y = frames[i + VALUE2]
		else
			x = self:getBezierValue(time, i, VALUE1, curveType - BEZIER)
			y = self:getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER)
		end
		x = x * bone.data.scaleX
		y = y * bone.data.scaleY

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
			if direction == MixDirection.mixOut then
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

Animation.ScaleXTimeline = {}
function Animation.ScaleXTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.scaleX, frameCount, bezierCount, Property.scaleX.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.scaleX = bone.data.scaleX
			elseif blend == MixBlend.first then
				bone.scaleX = bone.scaleX + (bone.data.scaleX - bone.scaleX) * alpha
			end
			return
		end

		local x = self:getCurveValue(time) * bone.data.scaleX
		if alpha == 1 then
			if blend == MixBlend.add then
				bone.scaleX = bone.scaleX + x - bone.data.scaleX
			else
				bone.scaleX = x
			end
		else
			local bx = 0
			if direction == MixDirection.mixOut then
				if blend == MixBlend.setup then
					bx = bone.data.scaleX
					bone.scaleX = bx + (math_abs(x) * math_signum(bx) - bx) * alpha
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					bx = bone.scaleX
					bone.scaleX = bx + (math_abs(x) * math_signum(bx) - bx) * alpha
				elseif blend == MixBlend.add then
					bx = bone.scaleX
					bone.scaleX = bx + (math_abs(x) * math_signum(bx) - bone.data.scaleX) * alpha
				end
			else
				if blend == MixBlend.setup then
					bx = math_abs(bone.data.scaleX) * math_signum(x)
					bone.scaleX = bx + (x - bx) * alpha
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					bx = math_abs(bone.scaleX) * math_signum(x)
					bone.scaleX = bx + (x - bx) * alpha
				elseif blend == MixBlend.add then
					bx = math_signum(x)
					bone.scaleX = math_abs(bone.scaleX) * bx + (x - math_abs(bone.data.scaleX) * bx) * alpha
				end
			end
		end
	end

	return self
end

Animation.ScaleYTimeline = {}
function Animation.ScaleYTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.scaleY, frameCount, bezierCount, Property.scaleY.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.scaleY = bone.data.scaleY
			elseif blend == MixBlend.first then
				bone.scaleY = bone.scaleY + (bone.data.scaleY - bone.scaleY) * alpha
			end
			return
		end

		local y = self:getCurveValue(time) * bone.data.scaleY
		if alpha == 1 then
			if blend == MixBlend.add then
				bone.scaleY = bone.scaleY + y - bone.data.scaleY
			else
				bone.scaleY = y
			end
		else
			local by = 0
			if direction == MixDirection.mixOut then
				if blend == MixBlend.setup then
					by = bone.data.scaleY
					bone.scaleY = by + (math_abs(y) * math_signum(by) - by) * alpha
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					by = bone.scaleY
					bone.scaleY = by + (math_abs(y) * math_signum(by) - by) * alpha
				elseif blend == MixBlend.add then
					by = bone.scaleY
					bone.scaleY = by + (math_abs(y) * math_signum(by) - bone.data.scaleY) * alpha
				end
			else
				if blend == MixBlend.setup then
					by = math_abs(bone.data.scaleY) * math_signum(y)
					bone.scaleY = by + (y - by) * alpha
				elseif blend == MixBlend.first or blend == MixBlend.replace then
					by = math_abs(bone.scaleY) * math_signum(y)
					bone.scaleY = by + (y - by) * alpha
				elseif blend == MixBlend.add then
					by = math_signum(y)
					bone.scaleY = math_abs(bone.scaleY) * by + (y - math_abs(bone.data.scaleY) * by) * alpha
				end
			end
		end
	end

	return self
end

Animation.ShearTimeline = {}
function Animation.ShearTimeline.new (frameCount, bezierCount, boneIndex)
	local ENTRIES = 3
	local VALUE1 = 1
	local VALUE2 = 2

	local self = Animation.CurveTimeline2.new(TimelineType.shear, frameCount, bezierCount,
		Property.shearX.."|"..boneIndex,
		Property.shearY.."|"..boneIndex
	)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
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
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[math_floor(i / ENTRIES)]
		if curveType == LINEAR then
			local before = frames[i]
			x = frames[i + VALUE1]
			y = frames[i + VALUE2]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			x = x + (frames[i + ENTRIES + VALUE1] - x) * t
			y = y + (frames[i + ENTRIES + VALUE2] - y) * t
		elseif curveType == STEPPED then
			x = frames[i + VALUE1]
			y = frames[i + VALUE2]
		else
			x = self:getBezierValue(time, i, VALUE1, curveType - BEZIER)
			y = self:getBezierValue(time, i, VALUE2, curveType + BEZIER_SIZE - BEZIER)
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

Animation.ShearXTimeline = {}
function Animation.ShearXTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.shearX, frameCount, bezierCount, Property.shearX.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.shearX = bone.data.shearX
			elseif blend == MixBlend.first then
				bone.shearX = bone.shearX + (bone.data.shearX - bone.shearX) * alpha
			end
			return
		end

		local x = self:getCurveValue(time)
		if blend == MixBlend.setup then
			bone.shearX = bone.data.shearX + x * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			bone.shearX = bone.shearX + (bone.data.shearX + x - bone.shearX) * alpha
		elseif blend == MixBlend.add then
			bone.shearX = bone.shearX + x * alpha
		end
	end

	return self
end

Animation.ShearYTimeline = {}
function Animation.ShearYTimeline.new (frameCount, bezierCount, boneIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.shearY, frameCount, bezierCount, Property.shearY.."|"..boneIndex)
	self.boneIndex = boneIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local bone = skeleton.bones[self.boneIndex]
		if not bone.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				bone.shearY = bone.data.shearY
			elseif blend == MixBlend.first then
				bone.shearY = bone.shearX + (bone.data.shearY - bone.shearY) * alpha
			end
			return
		end

		local y = self:getCurveValue(time)
		if blend == MixBlend.setup then
			bone.shearY = bone.data.shearY + y * alpha
		elseif blend == MixBlend.first or blend == MixBlend.replace then
			bone.shearY = bone.shearY + (bone.data.shearY + y - bone.shearY) * alpha
		elseif blend == MixBlend.add then
			bone.shearY = bone.shearY + y * alpha
		end
	end

	return self
end

Animation.RGBATimeline = {}
function Animation.RGBATimeline.new (frameCount, bezierCount, slotIndex)
	local ENTRIES = 5
	local R = 1
	local G = 2
	local B = 3
	local A = 4

	local self = Animation.CurveTimeline.new(TimelineType.rgba, ENTRIES, frameCount, bezierCount, {
		Property.rgb.."|"..slotIndex,
		Property.alpha.."|"..slotIndex
	})
	self.slotIndex = slotIndex
	
	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, r, g, b, a)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + R] = r
		self.frames[frame + G] = g
		self.frames[frame + B] = b
		self.frames[frame + A] = a
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local frames = self.frames
		local color = slot.color
		if time < frames[0] then
			local setup = slot.data.color
			if blend == MixBlend.setup then
				color:setFrom(setup)
			elseif blend == MixBlend.first then
				color:add((setup.r - color.r) * alpha, (setup.g - color.g) * alpha, (setup.b - color.b) * alpha,
						(setup.a - color.a) * alpha)
			end
			return
		end

		local r, g, b, a
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[i / ENTRIES]
		if curveType == LINEAR then
			local before = frames[i]
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			a = frames[i + A]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			r = r + (frames[i + ENTRIES + R] - r) * t
			g = g + (frames[i + ENTRIES + G] - g) * t
			b = b + (frames[i + ENTRIES + B] - b) * t
			a = a + (frames[i + ENTRIES + A] - a) * t
		elseif curveType == STEPPED then
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			a = frames[i + A]
		else
			r = self:getBezierValue(time, i, R, curveType - BEZIER)
			g = self:getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER)
			b = self:getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER)
			a = self:getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER)
		end

		if alpha == 1 then
			color:set(r, g, b, a)
		else
			if blend == MixBlend.setup then color:setFrom(slot.data.color) end
			color:add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha)
		end
	end

	return self
end

Animation.RGBTimeline = {}
function Animation.RGBTimeline.new (frameCount, bezierCount, slotIndex)
	local ENTRIES = 4
	local R = 1
	local G = 2
	local B = 3

	local self = Animation.CurveTimeline.new(TimelineType.rgb, ENTRIES, frameCount, bezierCount, { Property.rgb.."|"..slotIndex })
	self.slotIndex = slotIndex
	
	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, r, g, b)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + R] = r
		self.frames[frame + G] = g
		self.frames[frame + B] = b
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local frames = self.frames
		local color = slot.color
		if time < frames[0] then
			local setup = slot.data.color
			if blend == MixBlend.setup then
				color.r = setup.r
				color.g = setup.g
				color.b = setup.b
			elseif blend == MixBlend.first then
				color.r = color.r + (setup.r - color.r) * alpha
				color.g = color.g + (setup.g - color.g) * alpha
				color.b = color.b + (setup.b - color.b) * alpha
			end
			return
		end

		local r, g, b
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[i / ENTRIES]
		if curveType == LINEAR then
			local before = frames[i]
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			r = r + (frames[i + ENTRIES + R] - r) * t
			g = g + (frames[i + ENTRIES + G] - g) * t
			b = b + (frames[i + ENTRIES + B] - b) * t
		elseif curveType == STEPPED then
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
		else
			r = self:getBezierValue(time, i, R, curveType - BEZIER)
			g = self:getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER)
			b = self:getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER)
		end

		if alpha == 1 then
			color.r = r
			color.g = g
			color.b = b
		else
			if blend == MixBlend.setup then
				local setup = slot.data.color
				color.r = setup.r
				color.g = setup.g
				color.b = setup.b
			end
			color.r = color.r + (r - color.r) * alpha
			color.g = color.g + (g - color.g) * alpha
			color.b = color.b + (b - color.b) * alpha
		end
	end

	return self
end

Animation.AlphaTimeline = {}
function Animation.AlphaTimeline.new (frameCount, bezierCount, slotIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.alpha, frameCount, bezierCount, Property.alpha.."|"..slotIndex)
	self.slotIndex = slotIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local color = slot.color
		if time < frames[0] then
			local setup = slot.data.color
			if blend == MixBlend.setup then
				color.a = setup.a
				return
			else
				color.a = color.a + (setup.a - color.a) * alpha
			end
			return
		end

		local a = self:getCurveValue(time)
		if alpha == 1 then
			color.a = a
		else
			if blend == MixBlend.setup then color.a = slot.data.color.a end
			color.a = color.a + (a - color.a) * alpha
		end
	end

	return self
end

Animation.RGBA2Timeline = {}
function Animation.RGBA2Timeline.new (frameCount, bezierCount, slotIndex)
	local ENTRIES = 8
	local R = 1
	local G = 2
	local B = 3
	local A = 4
	local R2 = 5
	local G2 = 6
	local B2 = 7

	local self = Animation.CurveTimeline.new(TimelineType.rgba2, ENTRIES, frameCount, bezierCount, {
		Property.rgb.."|"..slotIndex,
		Property.alpha.."|"..slotIndex,
		Property.rgb2.."|"..slotIndex
	})
	self.slotIndex = slotIndex
	
	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, r, g, b, a, r2, g2, b2)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + R] = r
		self.frames[frame + G] = g
		self.frames[frame + B] = b
		self.frames[frame + A] = a
		self.frames[frame + R2] = r2
		self.frames[frame + G2] = g2
		self.frames[frame + B2] = b2
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local frames = self.frames
		local light = slot.color
		local dark = slot.darkColor
		if time < frames[0] then
			local setupLight = slot.data.color
			local setupDark = slot.data.darkColor
			if blend == MixBlend.setup then
				light:setFrom(setupLight)
				dark.r = setupDark.r
				dark.g = setupDark.g
				dark.b = setupDark.b
			elseif blend == MixBlend.first then
				light:add((setupLight.r - light.r) * alpha, (setupLight.g - light.g) * alpha, (setupLight.b - light.b) * alpha,
					(setupLight.a - light.a) * alpha)
				dark.r = dark.r + (setupDark.r - dark.r) * alpha
				dark.g = dark.g + (setupDark.g - dark.g) * alpha
				dark.b = dark.b + (setupDark.b - dark.b) * alpha
			end
			return
		end

		local r, g, b, a, r2, g2, b2
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[math_floor(i / ENTRIES)]
		if curveType == LINEAR then
			local before = frames[i]
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			a = frames[i + A]
			r2 = frames[i + R2]
			g2 = frames[i + G2]
			b2 = frames[i + B2]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			r = r + (frames[i + ENTRIES + R] - r) * t
			g = g + (frames[i + ENTRIES + G] - g) * t
			b = b + (frames[i + ENTRIES + B] - b) * t
			a = a + (frames[i + ENTRIES + A] - a) * t
			r2 = r2 + (frames[i + ENTRIES + R2] - r2) * t
			g2 = g2 + (frames[i + ENTRIES + G2] - g2) * t
			b2 = b2 + (frames[i + ENTRIES + B2] - b2) * t
		elseif curveType == STEPPED then
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			a = frames[i + A]
			r2 = frames[i + R2]
			g2 = frames[i + G2]
			b2 = frames[i + B2]
		else
			r = self:getBezierValue(time, i, R, curveType - BEZIER)
			g = self:getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER)
			b = self:getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER)
			a = self:getBezierValue(time, i, A, curveType + BEZIER_SIZE * 3 - BEZIER)
			r2 = self:getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 4 - BEZIER)
			g2 = self:getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 5 - BEZIER)
			b2 = self:getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 6 - BEZIER)
		end

		if alpha == 1 then
			light:set(r, g, b, a)
			dark.r = r2
			dark.g = g2
			dark.b = b2
		else
			if blend == MixBlend.setup then
				light:setFrom(slot.data.color)
				local setupDark = slot.data.darkColor
				dark.r = setupDark.r
				dark.g = setupDark.g
				dark.b = setupDark.b
			end
			light:add((r - light.r) * alpha, (g - light.g) * alpha, (b - light.b) * alpha, (a - light.a) * alpha)
			dark.r = dark.r + (r2 - dark.r) * alpha
			dark.g = dark.g + (g2 - dark.g) * alpha
			dark.b = dark.b + (b2 - dark.b) * alpha
		end
	end

	return self
end

Animation.RGB2Timeline = {}
function Animation.RGB2Timeline.new (frameCount, bezierCount, slotIndex)
	local ENTRIES = 7
	local R = 1
	local G = 2
	local B = 3
	local R2 = 4
	local G2 = 5
	local B2 = 6

	local self = Animation.CurveTimeline.new(TimelineType.rgb2, ENTRIES, frameCount, bezierCount, {
		Property.rgb.."|"..slotIndex,
		Property.rgb2.."|"..slotIndex
	})
	self.slotIndex = slotIndex

	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, r, g, b, r2, g2, b2)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + R] = r
		self.frames[frame + G] = g
		self.frames[frame + B] = b
		self.frames[frame + R2] = r2
		self.frames[frame + G2] = g2
		self.frames[frame + B2] = b2
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local frames = self.frames
		local light = slot.color
		local dark = slot.darkColor
		if time < frames[0] then
			local setupLight = slot.data.color
			local setupDark = slot.data.darkColor
			if blend == MixBlend.setup then
				light.r = setupLight.r
				light.g = setupLight.g
				light.b = setupLight.b
				dark.r = setupDark.r
				dark.g = setupDark.g
				dark.b = setupDark.b
			elseif blend == MixBlend.first then
				light.r = light.r + (setupLight.r - light.r) * alpha
				light.g = light.g + (setupLight.g - light.g) * alpha
				light.b = light.b + (setupLight.b - light.b) * alpha
				dark.r = dark.r + (setupDark.r - dark.r) * alpha
				dark.g = dark.g + (setupDark.g - dark.g) * alpha
				dark.b = dark.b + (setupDark.b - dark.b) * alpha
			end
			return
		end

		local r, g, b, r2, g2, b2
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[math_floor(i / ENTRIES)]
		if curveType == LINEAR then
			local before = frames[i]
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			r2 = frames[i + R2]
			g2 = frames[i + G2]
			b2 = frames[i + B2]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			r = r + (frames[i + ENTRIES + R] - r) * t
			g = g + (frames[i + ENTRIES + G] - g) * t
			b = b + (frames[i + ENTRIES + B] - b) * t
			r2 = r2 + (frames[i + ENTRIES + R2] - r2) * t
			g2 = g2 + (frames[i + ENTRIES + G2] - g2) * t
			b2 = b2 + (frames[i + ENTRIES + B2] - b2) * t
		elseif curveType == STEPPED then
			r = frames[i + R]
			g = frames[i + G]
			b = frames[i + B]
			r2 = frames[i + R2]
			g2 = frames[i + G2]
			b2 = frames[i + B2]
		else
			r = self:getBezierValue(time, i, R, curveType - BEZIER)
			g = self:getBezierValue(time, i, G, curveType + BEZIER_SIZE - BEZIER)
			b = self:getBezierValue(time, i, B, curveType + BEZIER_SIZE * 2 - BEZIER)
			r2 = self:getBezierValue(time, i, R2, curveType + BEZIER_SIZE * 4 - BEZIER)
			g2 = self:getBezierValue(time, i, G2, curveType + BEZIER_SIZE * 5 - BEZIER)
			b2 = self:getBezierValue(time, i, B2, curveType + BEZIER_SIZE * 6 - BEZIER)
		end

		if alpha == 1 then
			light.r = r
			light.g = g
			light.b = b
			dark.r = r2
			dark.g = g2
			dark.b = b2
		else
			if blend == MixBlend.setup then
				local setupLight = slot.data.color
				local setupDark = slot.data.darkColor
				light.r = setupLight.r
				light.g = setupLight.g
				light.b = setupLight.b
				dark.r = setupDark.r
				dark.g = setupDark.g
				dark.b = setupDark.b
			end
			light.r = light.r + (r - light.r) * alpha
			light.g = light.g + (g - light.g) * alpha
			light.b = light.b + (b - light.b) * alpha
			dark.r = dark.r + (r2 - dark.r) * alpha
			dark.g = dark.g + (g2 - dark.g) * alpha
			dark.b = dark.b + (b2 - dark.b) * alpha
		end
	end

	return self
end

Animation.AttachmentTimeline = {}
function Animation.AttachmentTimeline.new (frameCount, bezierCount, slotIndex)
	local self = Animation.Timeline.new(TimelineType.attachment, 1, frameCount, { Property.attachment.."|"..slotIndex })
	self.slotIndex = slotIndex
	self.attachmentNames = {}

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frame, time, attachmentName)
		self.frames[frame] = time
		self.attachmentNames[frame] = attachmentName
	end

	function self:setAttachment(skeleton, slot, attachmentName)
		attachmentName = slot.data.attachmentName
		if not attachmentName then
			slot:setAttachment(nil)
		else
			slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
		end
	end

	local function setAttachment (skeleton, slot, attachmentName)
		local attachmentName = self.attachmentNames[frameIndex]
		if not attachmentName then
			slot:setAttachment(nil)
		else
			slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
		end
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		if direction == MixDirection.mixOut then
			if blend == MixBlend.setup then
				self:setAttachment(skeleton, slot, slot.data.attachmentName)
			end
			return
		end

		if time < self.frames[0] then
			if blend == MixBlend.setup or blend == MixBlend.first then
				self:setAttachment(skeleton, slot, slot.data.attachmentName)
			end
			return
		end

		setAttachment(skeleton, slot, self.attachmentNames[search1(self.frames, time)])
	end

	return self
end

Animation.DeformTimeline = {}
function Animation.DeformTimeline.new (frameCount, bezierCount, slotIndex, attachment)
	local self = Animation.CurveTimeline.new(TimelineType.deform, 1, frameCount, bezierCount, { Property.deform.."|"..slotIndex.."|"..attachment.id })
	self.slotIndex = slotIndex
	self.attachment = attachment
	self.vertices = {}

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frame, time, vertices)
		self.frames[frame] = time
		self.vertices[frame] = vertices
	end

	function self:setBezier (bezier, frame, value, time1, value1, cx1, cy1, cx2, cy2, time2, value2)
		local curves = self.curves
		local i = self:getFrameCount() + bezier * BEZIER_SIZE
		if value == 0 then curves[frame] = BEZIER + i end
		local tmpx = (time1 - cx1 * 2 + cx2) * 0.03
		local tmpy = cy2 * 0.03 - cy1 * 0.06
		local dddx = ((cx1 - cx2) * 3 - time1 + time2) * 0.006
		local dddy = (cy1 - cy2 + 0.33333333) * 0.018
		local ddx = tmpx * 2 + dddx
		local ddy = tmpy * 2 + dddy
		local dx = (cx1 - time1) * 0.3 + tmpx + dddx * 0.16666667
		local dy = cy1 * 0.3 + tmpy + dddy * 0.16666667
		local x = time1 + dx
		local y = dy
		local n = i + BEZIER_SIZE
		while i < n do
			curves[i] = x
			curves[i + 1] = y
			dx = dx + ddx
			dy = dy + ddy
			ddx = ddx + dddx
			ddy = ddy + dddy
			x = x + dx
			y = y + dy
			i = i + 2
		end
	end

	function self:getCurvePercent (time, frame)
		local curves = self.curves
		local i = curves[frame]
		if i == LINEAR then
			local x = self.frames[frame]
			return (time - x) / (self.frames[frame + self:getFrameEntries()] - x)
		elseif i == STEPPED then
			return 0
		end
		i = i - BEZIER
		if curves[i] > time then
			local x = self.frames[frame]
			return curves[i + 1] * (time - x) / (curves[i] - x)
		end
		local n = i + BEZIER_SIZE
		i = i + 2
		while i < n do
			if curves[i] >= time then
				local x = curves[i - 2]
				local y = curves[i - 1]
				return y + (time - x) / (curves[i] - x) * (curves[i + 1] - y)
			end
			i = i + 2
		end
		local x = curves[n - 2]
		local y = curves[n - 1]
		return y + (1 - y) * (time - x) / (self.frames[frame + self:getFrameEntries()] - x)
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local slot = skeleton.slots[self.slotIndex]
		if not slot.bone.active then return end

		local vertexAttachment = slot.attachment
		if not vertexAttachment or not vertexAttachment.isVertexAttachment or vertexAttachment.deformAttachment ~= self.attachment then return end

		local frames = self.frames
		local deform = slot.deform
		if #deform == 0 then blend = MixBlend.setup end

		local vertices = self.vertices
		local vertexCount = #(vertices[0])

		if time < frames[0] then
			if blend == MixBlend.setup then
				slot.deform = {}
				return
			elseif blend == MixBlend.first then
				if alpha == 1 then
					slot.deform = {}
					return
				end
				utils.setArraySize(deform, vertexCount)
				if not vertexAttachment.bones then
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

		utils.setArraySize(deform, vertexCount)
		if time >= frames[#frames] then -- Time is after last frame.
			local lastVertices = vertices[#frames]
			if alpha == 1 then
				if blend == MixBlend.add then
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
			return
		end

		-- Interpolate between the previous frame and the current frame.
		local frame = search1(frames, time)
		local percent = self:getCurvePercent(time, frame)
		local prevVertices = vertices[frame]
		local nextVertices = vertices[frame + 1]

		if alpha == 1 then
			if blend == MixBlend.add then
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
local eventPropertyIds = { Property.event }
function Animation.EventTimeline.new (frameCount)
	local self = Animation.Timeline.new(TimelineType.event, 1, frameCount, eventPropertyIds)
	self.events = {}

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frame, event)
		self.frames[frame] = event.time
		self.events[frame] = event
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

		local i
		if lastTime < frames[0] then
			i = 0
		else
			i = search1(frames, lastTime) + 1
			local i = frames[i]
			while i > 0 do -- Fire multiple events with the same frame.
				if frames[i - 1] ~= i then break end
				i = i - 1
			end
		end
		while i < frameCount and time >= frames[i] do
			table.insert(firedEvents, self.events[i])
			i = i + 1
		end
	end

	return self
end

Animation.DrawOrderTimeline = {}
local drawOrderPropertyIds = { Property.drawOrder }
function Animation.DrawOrderTimeline.new (frameCount)
	local self = Animation.Timeline.new(TimelineType.drawOrder, 1, frameCount, drawOrderPropertyIds)
	self.drawOrders = {}

	function self:getFrameCount ()
		return zlen(self.frames)
	end

	function self:setFrame (frame, time, drawOrder)
		self.frames[frame] = time
		self.drawOrders[frame] = drawOrder
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local drawOrder = skeleton.drawOrder
		local slots = skeleton.slots

		if direction == MixDirection.mixOut then
			if blend == MixBlend.setup then
				for i,slot in ipairs(slots) do
					drawOrder[i] = slots[i]
				end
			end
			return
		end

		if time < self.frames[0] then
			if blend == MixBlend.setup or blend == MixBlend.first then
				for i,slot in ipairs(slots) do
					drawOrder[i] = slots[i]
				end
			end
			return
		end

		local drawOrderToSetupIndex = self.drawOrders[search1(self.frames, time)]
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
function Animation.IkConstraintTimeline.new (frameCount, bezierCount, ikConstraintIndex)
	local ENTRIES = 6
	local MIX = 1
	local SOFTNESS = 2
	local BEND_DIRECTION = 3
	local COMPRESS = 4
	local STRETCH = 5

	local self = Animation.CurveTimeline.new(TimelineType.ikConstraint, ENTRIES, frameCount, bezierCount, { Property.ikConstraint.."|"..ikConstraintIndex })
	self.ikConstraintIndex = ikConstraintIndex

	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, mix, softness, bendDirection, compress, stretch)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + MIX] = mix
		self.frames[frame + SOFTNESS] = softness
		self.frames[frame + BEND_DIRECTION] = bendDirection
		if compress then
			self.frames[frame + COMPRESS] = 1
		else
			self.frames[frame + COMPRESS] = 0
		end
		if stretch then
			self.frames[frame + STRETCH] = 1
		else
			self.frames[frame + STRETCH] = 0
		end
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local constraint = skeleton.ikConstraints[self.ikConstraintIndex]
		if not constraint.active then return end
		
		local frames = self.frames
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

		local mix = 0
		local softness = 0
		local i = search(frames, time, ENTRIES)
		local curveType = this.curves[i / ENTRIES]
		if curveType == LINEAR then
			local before = frames[i]
			mix = frames[i + MIX]
			softness = frames[i + SOFTNESS]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			mix = mix + (frames[i + ENTRIES + MIX] - mix) * t
			softness = softness + (frames[i + ENTRIES + SOFTNESS] - softness) * t
		elseif curveType == STEPPED then
			mix = frames[i + MIX]
			softness = frames[i + SOFTNESS]
		else
			mix = self:getBezierValue(time, i, MIX, curveType - BEZIER)
			softness = self:getBezierValue(time, i, SOFTNESS, curveType + BEZIER_SIZE - BEZIER)
		end

		if blend == MixBlend.setup then
			constraint.mix = constraint.data.mix + (mix - constraint.data.mix) * alpha
			constraint.softness = constraint.data.softness + (softness - constraint.data.softness) * alpha
			if direction == MixDirection.mixOut then
				constraint.bendDirection = constraint.data.bendDirection
				constraint.compress = constraint.data.compress
				constraint.stretch = constraint.data.stretch
			else
				constraint.bendDirection = math_floor(frames[i + BEND_DIRECTION])
				if math_floor(frames[i + COMPRESS]) == 1 then constraint.compress = true else constraint.compress = false end
				if math_floor(frames[i + STRETCH]) == 1 then constraint.stretch = true else constraint.stretch = false end
			end
		else
			constraint.mix = constraint.mix + (mix - constraint.mix) * alpha
			constraint.softness = constraint.softness + (softness - constraint.softness) * alpha
			if direction == MixDirection.mixIn then
				constraint.bendDirection = math_floor(frames[i + BEND_DIRECTION])
				if math_floor(frames[i + COMPRESS]) == 1 then constraint.compress = true else constraint.compress = false end
				if math_floor(frames[i + STRETCH]) == 1 then constraint.stretch = true else constraint.stretch = false end
			end
		end
	end

	return self
end

Animation.TransformConstraintTimeline = {}
function Animation.TransformConstraintTimeline.new (frameCount, bezierCount, transformConstraintIndex)
	local ENTRIES = 7
	local ROTATE = 1
	local X = 2
	local Y = 3
	local SCALEX = 4
	local SCALEY = 5
	local SHEARY = 6

	local self = Animation.CurveTimeline.new(TimelineType.transformConstraint, ENTRIES, frameCount, bezierCount, { Property.transformConstraint.."|"..transformConstraintIndex })
	self.transformConstraintIndex = transformConstraintIndex

	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY)
		frame = frame * ENTRIES
		self.frames[frame] = time
		self.frames[frame + ROTATE] = mixRotate
		self.frames[frame + X] = mixX
		self.frames[frame + Y] = mixY
		self.frames[frame + SCALEX] = mixScaleX
		self.frames[frame + SCALEY] = mixScaleY
		self.frames[frame + SHEARY] = mixShearY
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local constraint = skeleton.transformConstraints[self.transformConstraintIndex]
		if not constraint.active then return end

		local frames = self.frames
		if time < frames[0] then
			local data = constraint.data
			if blend == MixBlend.setup then
				constraint.mixRotate = data.mixRotate
				constraint.mixX = data.mixX
				constraint.mixY = data.mixY
				constraint.mixScaleX = data.mixScaleX
				constraint.mixScaleY = data.mixScaleY
				constraint.mixShearY = data.mixShearY
			elseif blend == MixBlend.first then
				constraint.mixRotate = constraint.mixRotate + (data.mixRotate - constraint.mixRotate) * alpha
				constraint.mixX = constraint.mixX + (data.mixX - constraint.mixX) * alpha
				constraint.mixY = constraint.mixY + (data.mixY - constraint.mixY) * alpha
				constraint.mixScaleX = constraint.mixScaleX + (data.mixScaleX - constraint.mixScaleX) * alpha
				constraint.mixScaleY = constraint.mixScaleY + (data.mixScaleY - constraint.mixScaleY) * alpha
				constraint.mixShearY = constraint.mixShearY + (data.mixShearY - constraint.mixShearY) * alpha
			end
			return
		end

		local rotate
		local x
		local y
		local scaleX
		local scaleY
		local shearY
		local i = search(frames, time, ENTRIES)
		local curveType = this.curves[i / ENTRIES]
		if curveType == LINEAR then
			local before = frames[i]
			rotate = frames[i + ROTATE]
			x = frames[i + X]
			y = frames[i + Y]
			scaleX = frames[i + SCALEX]
			scaleY = frames[i + SCALEY]
			shearY = frames[i + SHEARY]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			rotate = rotate + (frames[i + ENTRIES + ROTATE] - rotate) * t
			x = x + (frames[i + ENTRIES + X] - x) * t
			y = y + (frames[i + ENTRIES + Y] - y) * t
			scaleX = scaleX + (frames[i + ENTRIES + SCALEX] - scaleX) * t
			scaleY = scaleY + (frames[i + ENTRIES + SCALEY] - scaleY) * t
			shearY = shearY + (frames[i + ENTRIES + SHEARY] - shearY) * t
		elseif curveType == STEPPED then
			rotate = frames[i + ROTATE]
			x = frames[i + X]
			y = frames[i + Y]
			scaleX = frames[i + SCALEX]
			scaleY = frames[i + SCALEY]
			shearY = frames[i + SHEARY]
		else
			rotate = self:getBezierValue(time, i, ROTATE, curveType - BEZIER)
			x = self:getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER)
			y = self:getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER)
			scaleX = self:getBezierValue(time, i, SCALEX, curveType + BEZIER_SIZE * 3 - BEZIER)
			scaleY = self:getBezierValue(time, i, SCALEY, curveType + BEZIER_SIZE * 4 - BEZIER)
			shearY = self:getBezierValue(time, i, SHEARY, curveType + BEZIER_SIZE * 5 - BEZIER)
		end

		if blend == MixBlend.setup then
			local data = constraint.data
			constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha
			constraint.mixX = data.mixX + (x - data.mixX) * alpha
			constraint.mixY = data.mixY + (y - data.mixY) * alpha
			constraint.mixScaleX = data.mixScaleX + (scaleX - data.mixScaleX) * alpha
			constraint.mixScaleY = data.mixScaleY + (scaleY - data.mixScaleY) * alpha
			constraint.mixShearY = data.mixShearY + (shearY - data.mixShearY) * alpha
		else
			constraint.mixRotate = constraint.mixRotate + (rotate - constraint.mixRotate) * alpha
			constraint.mixX = constraint.mixX + (x - constraint.mixX) * alpha
			constraint.mixY = constraint.mixY + (y - constraint.mixY) * alpha
			constraint.mixScaleX = constraint.mixScaleX + (scaleX - constraint.mixScaleX) * alpha
			constraint.mixScaleY = constraint.mixScaleY + (scaleY - constraint.mixScaleY) * alpha
			constraint.mixShearY = constraint.mixShearY + (shearY - constraint.mixShearY) * alpha
		end
	end

	return self
end

Animation.PathConstraintPositionTimeline = {}
function Animation.PathConstraintPositionTimeline.new (frameCount, bezierCount, pathConstraintIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.pathConstraintPosition, frameCount, bezierCount, Property.pathConstraintPosition.."|"..pathConstraintIndex)
	self.pathConstraintIndex = pathConstraintIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
		if not constraint.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				constraint.position = constraint.data.position
			elseif blend == MixBlend.first then
				constraint.position = constraint.position + (constraint.data.position - constraint.position) * alpha
			end
			return
		end

		local position = self:getCurveValue(time)
		if blend == MixBlend.setup then
			constraint.position = constraint.data.position + (position - constraint.data.position) * alpha
		else
			constraint.position = constraint.position + (position - constraint.position) * alpha
		end
	end

	return self
end

Animation.PathConstraintSpacingTimeline = {}
function Animation.PathConstraintSpacingTimeline.new (frameCount, bezierCount, pathConstraintIndex)
	local self = Animation.CurveTimeline1.new(TimelineType.pathConstraintSpacing, frameCount, bezierCount, Property.pathConstraintSpacing.."|"..pathConstraintIndex)
	self.pathConstraintIndex = pathConstraintIndex

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
		if not constraint.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				constraint.spacing = constraint.data.spacing
			elseif blend == MixBlend.first then
				constraint.spacing = constraint.spacing + (constraint.data.spacing - constraint.spacing) * alpha
			end
			return
		end

		local spacing = self:getCurveValue(time)
		if blend == MixBlend.setup then
			constraint.spacing = constraint.data.spacing + (spacing - constraint.data.spacing) * alpha
		else
			constraint.spacing = constraint.spacing + (spacing - constraint.spacing) * alpha
		end
	end

	return self
end

Animation.PathConstraintMixTimeline = {}
function Animation.PathConstraintMixTimeline.new (frameCount, bezierCount, pathConstraintIndex)
	local ENTRIES = 4
	local ROTATE = 1
	local X = 2
	local Y = 3

	local self = Animation.CurveTimeline.new(TimelineType.pathConstraintMix, ENTRIES, frameCount, bezierCount, Property.pathConstraintMix.."|"..pathConstraintIndex)
	self.pathConstraintIndex = pathConstraintIndex

	function self:getFrameEntries ()
		return ENTRIES
	end

	function self:setFrame (frame, time, mixRotate, mixX, mixY)
		local frames = self.frames
		frame = frame * ENTRIES
		frames[frame] = time
		frames[frame + ROTATE] = mixRotate
		frames[frame + X] = mixX
		frames[frame + Y] = mixY
	end

	function self:apply (skeleton, lastTime, time, events, alpha, blend, direction)
		local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
		if not constraint.active then return end

		local frames = self.frames
		if time < frames[0] then
			if blend == MixBlend.setup then
				constraint.mixRotate = constraint.data.mixRotate
				constraint.mixX = constraint.data.mixX
				constraint.mixY = constraint.data.mixY
			elseif blend == MixBlend.first then
				constraint.mixRotate = constraint.mixRotate + (constraint.data.mixRotate - constraint.mixRotate) * alpha
				constraint.mixX = constraint.mixX + (constraint.data.mixX - constraint.mixX) * alpha
				constraint.mixY = constraint.mixY + (constraint.data.mixY - constraint.mixY) * alpha
			end
			return
		end

		local rotate
		local x
		local y
		local i = search(frames, time, ENTRIES)
		local curveType = self.curves[math_floor(i / 4)]
		if curveType == LINEAR then
			local before = frames[i]
			rotate = frames[i + ROTATE]
			x = frames[i + X]
			y = frames[i + Y]
			local t = (time - before) / (frames[i + ENTRIES] - before)
			rotate = rotate + (frames[i + ENTRIES + ROTATE] - rotate) * t
			x = x + (frames[i + ENTRIES + X] - x) * t
			y = y + (frames[i + ENTRIES + Y] - y) * t
		elseif curveType == STEPPED then
			rotate = frames[i + ROTATE]
			x = frames[i + X]
			y = frames[i + Y]
		else
			rotate = this.getBezierValue(time, i, ROTATE, curveType - BEZIER)
			x = this.getBezierValue(time, i, X, curveType + BEZIER_SIZE - BEZIER)
			y = this.getBezierValue(time, i, Y, curveType + BEZIER_SIZE * 2 - BEZIER)
		end

		if blend == MixBlend.setup then
			local data = constraint.data
			constraint.mixRotate = data.mixRotate + (rotate - data.mixRotate) * alpha
			constraint.mixX = data.mixX + (x - data.mixX) * alpha
			constraint.mixY = data.mixY + (y - data.mixY) * alpha
		else
			constraint.mixRotate = constraint.mixRotate + (rotate - constraint.mixRotate) * alpha
			constraint.mixX = constraint.mixX + (x - constraint.mixX) * alpha
			constraint.mixY = constraint.mixY + (y - constraint.mixY) * alpha
		end
	end

	return self
end

return Animation
