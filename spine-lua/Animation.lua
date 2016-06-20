-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.3
-- 
-- Copyright (c) 2013-2015, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to use, install, execute and perform the Spine
-- Runtimes Software (the "Software") and derivative works solely for personal
-- or internal use. Without the written permission of Esoteric Software (see
-- Section 2 of the Spine Software License Agreement), you may not (a) modify,
-- translate, adapt or otherwise create derivative works, improvements of the
-- Software or develop new applications using the Software or (b) remove,
-- delete, alter or obscure any trademarks or any copyright, trademark, patent
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local Animation = {}
function Animation.new (name, timelines, duration)
	if not timelines then error("timelines cannot be nil", 2) end

	local self = {
		name = name,
		timelines = timelines,
		duration = duration
	}

	function self:apply (skeleton, lastTime, time, loop, events)
		if not skeleton then error("skeleton cannot be nil.", 2) end

		if loop and duration > 0 then
			time = time % self.duration
			lastTime = lastTime % self.duration
		end

		for i,timeline in ipairs(self.timelines) do
			timeline:apply(skeleton, lastTime, time, events, 1)
		end
	end

	function self:mix (skeleton, lastTime, time, loop, events, alpha)
		if not skeleton then error("skeleton cannot be nil.", 2) end

		if loop and duration > 0 then
			time = time % self.duration
			lastTime = lastTime % self.duration
		end

		for i,timeline in ipairs(self.timelines) do
			timeline:apply(skeleton, lastTime, time, events, alpha)
		end
	end

	return self
end

local function binarySearch (values, target, step)
	local low = 0
	local high = math.floor((#values + 1) / step - 2)
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

local function binarySearch1 (values, target)
	local low = 0
	local high = math.floor(#values - 1)
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
	for i = 0, #values, step do
		if (values[i] > target) then return i end
	end
	return -1
end

Animation.CurveTimeline = {}
function Animation.CurveTimeline.new ()
	local LINEAR = 0
	local STEPPED = 1
	local BEZIER = 2;
	local BEZIER_SEGMENTS = 10
	local BEZIER_SIZE = BEZIER_SEGMENTS * 2 - 1

	local self = {
		curves = {} -- type, x, y, ...
	}

	function self:setLinear (frameIndex)
		self.curves[frameIndex * BEZIER_SIZE] = LINEAR
	end

	function self:setStepped (frameIndex)
		self.curves[frameIndex * BEZIER_SIZE] = STEPPED
	end

	function self:setCurve (frameIndex, cx1, cy1, cx2, cy2)
		local subdiv1 = 1 / BEZIER_SEGMENTS
		local subdiv2 = subdiv1 * subdiv1
		local subdiv3 = subdiv2 * subdiv1;
		local pre1 = 3 * subdiv1
		local pre2 = 3 * subdiv2
		local pre4 = 6 * subdiv2
		local pre5 = 6 * subdiv3
		local tmp1x = -cx1 * 2 + cx2
		local tmp1y = -cy1 * 2 + cy2
		local tmp2x = (cx1 - cx2) * 3 + 1
		local tmp2y = (cy1 - cy2) * 3 + 1
		local dfx = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv3
		local dfy = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv3
		local ddfx = tmp1x * pre4 + tmp2x * pre5
		local ddfy = tmp1y * pre4 + tmp2y * pre5;
		local dddfx = tmp2x * pre5
		local dddfy = tmp2y * pre5

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
function Animation.RotateTimeline.new ()
	local PREV_FRAME_TIME = -2
	local FRAME_VALUE = 1

	local self = Animation.CurveTimeline.new()
	self.frames = {}
	self.boneIndex = -1

	function self:getDuration ()
		return self.frames[#self.frames - 1]
	end

	function self:getFrameCount ()
		return (#self.frames + 1) / 2
	end

	function self:setFrame (frameIndex, time, value)
		frameIndex = frameIndex * 2
		self.frames[frameIndex] = time
		self.frames[frameIndex + 1] = value
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local bone = skeleton.bones[self.boneIndex]

		if time >= frames[#frames - 1] then -- Time is after last frame.
			local amount = bone.data.rotation + frames[#frames] - bone.rotation
			while amount > 180 do
				amount = amount - 360
			end
			while amount < -180 do
				amount = amount + 360
			end
			bone.rotation = bone.rotation + amount * alpha
			return
		end

		-- Interpolate between the last frame and the current frame.
		local frameIndex = binarySearch(frames, time, 2)
		local prevFrameValue = frames[frameIndex - 1]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 2 - 1, percent)

		local amount = frames[frameIndex + FRAME_VALUE] - prevFrameValue
		while amount > 180 do
			amount = amount - 360
		end
		while amount < -180 do
			amount = amount + 360
		end
		amount = bone.data.rotation + (prevFrameValue + amount * percent) - bone.rotation
		while amount > 180 do
			amount = amount - 360
		end
		while amount < -180 do
			amount = amount + 360
		end
		bone.rotation = bone.rotation + amount * alpha
	end

	return self
end

Animation.TranslateTimeline = {}
function Animation.TranslateTimeline.new ()
	local PREV_FRAME_TIME = -3
	local FRAME_X = 1
	local FRAME_Y = 2

	local self = Animation.CurveTimeline.new()
	self.frames = {}
	self.boneIndex = -1

	function self:getDuration ()
		return self.frames[#self.frames - 2]
	end

	function self:getFrameCount ()
		return (#self.frames + 1) / 3
	end

	function self:setFrame (frameIndex, time, x, y)
		frameIndex = frameIndex * 3
		self.frames[frameIndex] = time
		self.frames[frameIndex + 1] = x
		self.frames[frameIndex + 2] = y
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local bone = skeleton.bones[self.boneIndex]
		
		if time >= frames[#frames - 2] then -- Time is after last frame.
			bone.x = bone.x + (bone.data.x + frames[#frames - 1] - bone.x) * alpha
			bone.y = bone.y + (bone.data.y + frames[#frames] - bone.y) * alpha
			return
		end

		-- Interpolate between the last frame and the current frame.
		local frameIndex = binarySearch(frames, time, 3)
		local prevFrameX = frames[frameIndex - 2]
		local prevFrameY = frames[frameIndex - 1]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 3 - 1, percent)

		bone.x = bone.x + (bone.data.x + prevFrameX + (frames[frameIndex + FRAME_X] - prevFrameX) * percent - bone.x) * alpha
		bone.y = bone.y + (bone.data.y + prevFrameY + (frames[frameIndex + FRAME_Y] - prevFrameY) * percent - bone.y) * alpha
	end

	return self
end

Animation.ScaleTimeline = {}
function Animation.ScaleTimeline.new ()
	local PREV_FRAME_TIME = -3
	local FRAME_X = 1
	local FRAME_Y = 2

	local self = Animation.TranslateTimeline.new()

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local bone = skeleton.bones[self.boneIndex]

		if time >= frames[#frames - 2] then -- Time is after last frame.
			bone.scaleX = bone.scaleX + (bone.data.scaleX * frames[#frames - 1] - bone.scaleX) * alpha
			bone.scaleY = bone.scaleY + (bone.data.scaleY * frames[#frames] - bone.scaleY) * alpha
			return
		end

		-- Interpolate between the last frame and the current frame.
		local frameIndex = binarySearch(frames, time, 3)
		local prevFrameX = frames[frameIndex - 2]
		local prevFrameY = frames[frameIndex - 1]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 3 - 1, percent)

		bone.scaleX = bone.scaleX + (bone.data.scaleX * (prevFrameX + (frames[frameIndex + FRAME_X] - prevFrameX) * percent) - bone.scaleX) * alpha
		bone.scaleY = bone.scaleY + (bone.data.scaleY * (prevFrameY + (frames[frameIndex + FRAME_Y] - prevFrameY) * percent) - bone.scaleY) * alpha
	end

	return self
end

Animation.ColorTimeline = {}
function Animation.ColorTimeline.new ()
	local PREV_FRAME_TIME = -5
	local FRAME_R = 1
	local FRAME_G = 2
	local FRAME_B = 3
	local FRAME_A = 4

	local self = Animation.CurveTimeline.new()
	self.frames = {}
	self.slotIndex = -1

	function self:getDuration ()
		return self.frames[#self.frames - 4]
	end

	function self:getFrameCount ()
		return (#self.frames + 1) / 5
	end

	function self:setFrame (frameIndex, time, r, g, b, a)
		frameIndex = frameIndex * 5
		self.frames[frameIndex] = time
		self.frames[frameIndex + 1] = r
		self.frames[frameIndex + 2] = g
		self.frames[frameIndex + 3] = b
		self.frames[frameIndex + 4] = a
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local r, g, b, a
		if time >= frames[#frames - 4] then -- Time is after last frame.
			r = frames[#frames - 3]
			g = frames[#frames - 2]
			b = frames[#frames - 1]
			a = frames[#frames]
		else
			-- Interpolate between the last frame and the current frame.
			local frameIndex = binarySearch(frames, time, 5)
			local prevFrameR = frames[frameIndex - 4]
			local prevFrameG = frames[frameIndex - 3]
			local prevFrameB = frames[frameIndex - 2]
			local prevFrameA = frames[frameIndex - 1]
			local frameTime = frames[frameIndex]
			local percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime)
			if percent < 0 then percent = 0 elseif percent > 255 then percent = 255 end
			percent = self:getCurvePercent(frameIndex / 5 - 1, percent)

			r = prevFrameR + (frames[frameIndex + FRAME_R] - prevFrameR) * percent
			g = prevFrameG + (frames[frameIndex + FRAME_G] - prevFrameG) * percent
			b = prevFrameB + (frames[frameIndex + FRAME_B] - prevFrameB) * percent
			a = prevFrameA + (frames[frameIndex + FRAME_A] - prevFrameA) * percent
		end
		local slot = skeleton.slots[self.slotIndex]
		if alpha < 1 then
			slot:setColor(slot.r + (r - slot.r) * alpha, slot.g + (g - slot.g) * alpha, slot.b + (b - slot.b) * alpha, slot.a + (a - slot.a) * alpha)
		else
			slot:setColor(r, g, b, a)
		end
	end

	return self
end

Animation.AttachmentTimeline = {}
function Animation.AttachmentTimeline.new ()
	local self = {
		frames = {}, -- time, ...
		attachmentNames = {},
		slotName = nil
	}

	function self:getDuration ()
		return self.frames[#self.frames]
	end

	function self:getFrameCount ()
		return #self.frames + 1
	end

	function self:setFrame (frameIndex, time, attachmentName)
		self.frames[frameIndex] = time
		self.attachmentNames[frameIndex] = attachmentName
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then
			if lastTime > time then self:apply(skeleton, lastTime, 999999, nil, 0) end
			return
		elseif lastTime > time then
			lastTime = -1
		end

		local frameIndex
		if time >= frames[#frames] then
			frameIndex = #frames
		else
			frameIndex = binarySearch1(frames, time) - 1
		end
		if frames[frameIndex] < lastTime then return end

		local attachmentName = self.attachmentNames[frameIndex]
		local slot = skeleton.slotsByName[self.slotName]
		if attachmentName then
			if not slot.attachment then
				slot:setAttachment(skeleton:getAttachment(self.slotName, attachmentName))
			elseif slot.attachment.name ~= attachmentName then
				slot:setAttachment(skeleton:getAttachment(self.slotName, attachmentName))
			end
		else
			slot:setAttachment(nil)
		end
	end

	return self
end

Animation.EventTimeline = {}
function Animation.EventTimeline.new ()
	local self = {
		frames = {},
		events = {}
	}

	function self:getDuration ()
		return self.frames[#self.frames]
	end

	function self:getFrameCount ()
		return #self.frames + 1
	end

	function self:setFrame (frameIndex, time, event)
		self.frames[frameIndex] = time
		self.events[frameIndex] = event
	end

	-- Fires events for frames > lastTime and <= time.
	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		if not firedEvents then return end

		local frames = self.frames
		local frameCount = #frames + 1

		if lastTime > time then -- Fire events after last time for looped animations.
			self:apply(skeleton, lastTime, 999999, firedEvents, alpha)
			lastTime = -1
		elseif lastTime >= frames[frameCount - 1] then -- Last time is after last frame.
			return
		end
		if time < frames[0] then return end -- Time is before first frame.

		local frameIndex
		if lastTime < frames[0] then
			frameIndex = 0
		else
			frameIndex = binarySearch1(frames, lastTime)
			local frame = frames[frameIndex]
			while frameIndex > 0 do -- Fire multiple events with the same frame.
				if frames[frameIndex - 1] ~= frame then break end
				frameIndex = frameIndex - 1
			end
		end
		local events = self.events
		while frameIndex < frameCount and time >= frames[frameIndex] do
			table.insert(firedEvents, events[frameIndex])
			frameIndex = frameIndex + 1
		end
	end

	return self
end

Animation.DrawOrderTimeline = {}
function Animation.DrawOrderTimeline.new ()
	local self = {
		frames = {},
		drawOrders = {}
	}

	function self:getDuration ()
		return self.frames[#self.frames]
	end

	function self:getFrameCount ()
		return #self.frames + 1
	end

	function self:setFrame (frameIndex, time, drawOrder)
		self.frames[frameIndex] = time
		self.drawOrders[frameIndex] = drawOrder
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local frameIndex
		if time >= frames[#frames] then -- Time is after last frame.
			frameIndex = #frames
		else
			frameIndex = binarySearch1(frames, time) - 1
		end

		local drawOrder = skeleton.drawOrder
		local slots = skeleton.slots
		local drawOrderToSetupIndex = self.drawOrders[frameIndex]
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

Animation.FfdTimeline = {}
function Animation.FfdTimeline.new ()
	local self = Animation.CurveTimeline.new()
	self.frames = {} -- time, ...
	self.frameVertices = {}
	self.slotIndex = -1

	function self:getDuration ()
		return self.frames[#self.frames]
	end

	function self:getFrameCount ()
		return #self.frames + 1
	end

	function self:setFrame (frameIndex, time, vertices)
		self.frames[frameIndex] = time
		self.frameVertices[frameIndex] = vertices
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local slot = skeleton.slots[self.slotIndex]
		if slot.attachment ~= self.attachment then return end

		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local frameVertices = self.frameVertices
		local vertexCount = #frameVertices[0]
		local vertices = slot.attachmentVertices
		if not vertices or #vertices < vertexCount then
			vertices = {}
			slot.attachmentVertices = vertices
		end
		if #vertices ~= vertexCount then
			alpha = 1 -- Don't mix from uninitialized slot vertices.
		end
		slot.attachmentVerticesCount = vertexCount
		if time >= frames[#frames] then -- Time is after last frame.
			local lastVertices = frameVertices[#frames]
			if alpha < 1 then
				for i = 1, vertexCount do
					local vertex = vertices[i]
					vertices[i] = vertex + (lastVertices[i] - vertex) * alpha
				end
			else
				for i = 1, vertexCount do
					vertices[i] = lastVertices[i]
				end
			end
			return
		end

		-- Interpolate between the previous frame and the current frame.
		local frameIndex = binarySearch1(frames, time)
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex - 1] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex - 1, percent)

		local prevVertices = frameVertices[frameIndex - 1]
		local nextVertices = frameVertices[frameIndex]

		if alpha < 1 then
			for i = 1, vertexCount do
				local prev = prevVertices[i]
				local vertex = vertices[i]
				vertices[i] = vertex + (prev + (nextVertices[i] - prev) * percent - vertex) * alpha
			end
		else
			for i = 1, vertexCount do
				local prev = prevVertices[i]
				vertices[i] = prev + (nextVertices[i] - prev) * percent
			end
		end
	end

	return self
end

Animation.IkConstraintTimeline = {}
function Animation.IkConstraintTimeline.new ()
	local PREV_FRAME_TIME = -3
	local PREV_FRAME_MIX = -2
	local PREV_FRAME_BEND_DIRECTION = -1
	local FRAME_MIX = 1

	local self = Animation.CurveTimeline.new()
	self.frames = {} -- time, mix, bendDirection, ...
	self.ikConstraintIndex = -1

	function self:getDuration ()
		return self.frames[#self.frames - 2]
	end

	function self:getFrameCount ()
		return (#self.frames + 1) / 3
	end

	function self:setFrame (frameIndex, time, mix, bendDirection)
		frameIndex = frameIndex * 3
		self.frames[frameIndex] = time
		self.frames[frameIndex + 1] = mix
		self.frames[frameIndex + 2] = bendDirection
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local ikConstraint = skeleton.ikConstraints[self.ikConstraintIndex]

		if time >= frames[#frames - 2] then -- Time is after last frame.
			ikConstraint.mix = ikConstraint.mix + (frames[#frames - 1] - ikConstraint.mix) * alpha
			ikConstraint.bendDirection = frames[#frames]
			return
		end

		-- Interpolate between the previous frame and the current frame.
		local frameIndex = binarySearch(frames, time, 3);
		local prevFrameMix = frames[frameIndex + PREV_FRAME_MIX]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + PREV_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 3 - 1, percent)

		local mix = prevFrameMix + (frames[frameIndex + FRAME_MIX] - prevFrameMix) * percent
		ikConstraint.mix = ikConstraint.mix + (mix - ikConstraint.mix) * alpha
		ikConstraint.bendDirection = frames[frameIndex + PREV_FRAME_BEND_DIRECTION]
	end

	return self
end

Animation.FlipXTimeline = {}
function Animation.FlipXTimeline.new ()
	local self = {
		frames = {}, -- time, flip, ...
		boneIndex = -1
	}

	function self:getDuration ()
		return self.frames[#self.frames - 1]
	end

	function self:getFrameCount ()
		return (#self.frames + 1) / 2
	end

	function self:setFrame (frameIndex, time, flip)
		frameIndex = frameIndex * 2
		self.frames[frameIndex] = time
		self.frames[frameIndex + 1] = flip
	end

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then
			if lastTime > time then self:apply(skeleton, lastTime, 999999, nil, 0) end
			return
		elseif lastTime > time then
			lastTime = -1
		end

		local frameIndex
		if time >= frames[#frames - 1] then
			frameIndex = #frames - 1
		else
			frameIndex = binarySearch(frames, time, 2) - 2
		end
		if frames[frameIndex] < lastTime then return end

		self:setFlip(skeleton.bones[self.boneIndex], frames[frameIndex + 1])
	end
	
	function self:setFlip (bone, flip)
		bone.flipX = flip
	end

	return self
end

Animation.FlipYTimeline = {}
function Animation.FlipYTimeline.new ()
	local self = Animation.FlipXTimeline.new()

	function self:setFlip (bone, flip)
		bone.flipY = flip
	end

	return self
end

return Animation
