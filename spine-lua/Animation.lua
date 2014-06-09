-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.1
-- 
-- Copyright (c) 2013, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to install, execute and perform the Spine Runtimes
-- Software (the "Software") solely for internal use. Without the written
-- permission of Esoteric Software (typically granted by licensing Spine), you
-- may not (a) modify, translate, adapt or otherwise create derivative works,
-- improvements of the Software or develop new applications using the Software
-- or (b) remove, delete, alter or obscure any trademarks or any copyright,
-- trademark, patent or other intellectual property or proprietary rights
-- notices on or in the Software, including any copy thereof. Redistributions
-- in binary or source form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
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

local function linearSearch (values, target, step)
	for i = 0, #values, step do
		if (values[i] > target) then return i end
	end
	return -1
end

Animation.CurveTimeline = {}
function Animation.CurveTimeline.new ()
	local LINEAR = 0
	local STEPPED = -1
	local BEZIER_SEGMENTS = 10

	local self = {
		curves = {}
	}

	function self:setLinear (frameIndex)
		self.curves[frameIndex * 6] = LINEAR
	end

	function self:setStepped (frameIndex)
		self.curves[frameIndex * 6] = STEPPED
	end

	function self:setCurve (frameIndex, cx1, cy1, cx2, cy2)
		local subdiv_step = 1 / BEZIER_SEGMENTS
		local subdiv_step2 = subdiv_step * subdiv_step
		local subdiv_step3 = subdiv_step2 * subdiv_step
		local pre1 = 3 * subdiv_step
		local pre2 = 3 * subdiv_step2
		local pre4 = 6 * subdiv_step2
		local pre5 = 6 * subdiv_step3
		local tmp1x = -cx1 * 2 + cx2
		local tmp1y = -cy1 * 2 + cy2
		local tmp2x = (cx1 - cx2) * 3 + 1
		local tmp2y = (cy1 - cy2) * 3 + 1
		local i = frameIndex * 6
		local curves = self.curves
		curves[i] = cx1 * pre1 + tmp1x * pre2 + tmp2x * subdiv_step3
		curves[i + 1] = cy1 * pre1 + tmp1y * pre2 + tmp2y * subdiv_step3
		curves[i + 2] = tmp1x * pre4 + tmp2x * pre5
		curves[i + 3] = tmp1y * pre4 + tmp2y * pre5
		curves[i + 4] = tmp2x * pre5
		curves[i + 5] = tmp2y * pre5
	end

	function self:getCurvePercent (frameIndex, percent)
		local curveIndex = frameIndex * 6
		local curves = self.curves
		local dfx = curves[curveIndex]
		if not dfx then return percent end -- linear
		if dfx == STEPPED then return 0 end
		local dfy = curves[curveIndex + 1]
		local ddfx = curves[curveIndex + 2]
		local ddfy = curves[curveIndex + 3]
		local dddfx = curves[curveIndex + 4]
		local dddfy = curves[curveIndex + 5]
		local x = dfx
		local y = dfy
		local i = BEZIER_SEGMENTS - 2
		while true do
			if x >= percent then
				local lastX = x - dfx
				local lastY = y - dfy
				return lastY + (y - lastY) * (percent - lastX) / (x - lastX)
			end
			if i == 0 then break end
			i = i - 1
			dfx = dfx + ddfx
			dfy = dfy + ddfy
			ddfx = ddfx + dddfx
			ddfy = ddfy + dddfy
			x = x + dfx
			y = y + dfy
		end
		return y + (1 - y) * (percent - x) / (1 - x) -- Last point is 1,1.
	end

	return self
end

Animation.RotateTimeline = {}
function Animation.RotateTimeline.new ()
	local LAST_FRAME_TIME = -2
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
		local lastFrameValue = frames[frameIndex - 1]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 2 - 1, percent)

		local amount = frames[frameIndex + FRAME_VALUE] - lastFrameValue
		while amount > 180 do
			amount = amount - 360
		end
		while amount < -180 do
			amount = amount + 360
		end
		amount = bone.data.rotation + (lastFrameValue + amount * percent) - bone.rotation
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
	local LAST_FRAME_TIME = -3
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
		local lastFrameX = frames[frameIndex - 2]
		local lastFrameY = frames[frameIndex - 1]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 3 - 1, percent)

		bone.x = bone.x + (bone.data.x + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.x) * alpha
		bone.y = bone.y + (bone.data.y + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.y) * alpha
	end

	return self
end

Animation.ScaleTimeline = {}
function Animation.ScaleTimeline.new ()
	local LAST_FRAME_TIME = -3
	local FRAME_X = 1
	local FRAME_Y = 2

	local self = Animation.TranslateTimeline.new()

	function self:apply (skeleton, lastTime, time, firedEvents, alpha)
		local frames = self.frames
		if time < frames[0] then return end -- Time is before first frame.

		local bone = skeleton.bones[self.boneIndex]

		if time >= frames[#frames - 2] then -- Time is after last frame.
			bone.scaleX = bone.scaleX + (bone.data.scaleX - 1 + frames[#frames - 1] - bone.scaleX) * alpha
			bone.scaleY = bone.scaleY + (bone.data.scaleY - 1 + frames[#frames] - bone.scaleY) * alpha
			return
		end

		-- Interpolate between the last frame and the current frame.
		local frameIndex = binarySearch(frames, time, 3)
		local lastFrameX = frames[frameIndex - 2]
		local lastFrameY = frames[frameIndex - 1]
		local frameTime = frames[frameIndex]
		local percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime)
		if percent < 0 then percent = 0 elseif percent > 1 then percent = 1 end
		percent = self:getCurvePercent(frameIndex / 3 - 1, percent)

		bone.scaleX = bone.scaleX + (bone.data.scaleX - 1 + lastFrameX + (frames[frameIndex + FRAME_X] - lastFrameX) * percent - bone.scaleX) * alpha
		bone.scaleY = bone.scaleY + (bone.data.scaleY - 1 + lastFrameY + (frames[frameIndex + FRAME_Y] - lastFrameY) * percent - bone.scaleY) * alpha
	end

	return self
end

Animation.ColorTimeline = {}
function Animation.ColorTimeline.new ()
	local LAST_FRAME_TIME = -5
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
			local lastFrameR = frames[frameIndex - 4]
			local lastFrameG = frames[frameIndex - 3]
			local lastFrameB = frames[frameIndex - 2]
			local lastFrameA = frames[frameIndex - 1]
			local frameTime = frames[frameIndex]
			local percent = 1 - (time - frameTime) / (frames[frameIndex + LAST_FRAME_TIME] - frameTime)
			if percent < 0 then percent = 0 elseif percent > 255 then percent = 255 end
			percent = self:getCurvePercent(frameIndex / 5 - 1, percent)

			r = lastFrameR + (frames[frameIndex + FRAME_R] - lastFrameR) * percent
			g = lastFrameG + (frames[frameIndex + FRAME_G] - lastFrameG) * percent
			b = lastFrameB + (frames[frameIndex + FRAME_B] - lastFrameB) * percent
			a = lastFrameA + (frames[frameIndex + FRAME_A] - lastFrameA) * percent
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
		frames = {},
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
		if time < frames[0] then return end -- Time is before first frame.

		local frameIndex
		if time >= frames[#frames] then -- Time is after last frame.
			frameIndex = #frames
		else
			frameIndex = binarySearch(frames, time, 1) - 1
		end

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
		local frameCount = #frames

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
			frameIndex = binarySearch(frames, lastTime, 1)
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
			frameIndex = binarySearch(frames, time, 1) - 1
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

return Animation
