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
local table_insert = table.insert
local utils = require "spine-lua.utils"
local Animation = require "spine-lua.Animation"
local AnimationStateData = require "spine-lua.AnimationStateData"
local math_min = math.min
local math_abs = math.abs
local math_signum = utils.signum

local EventType = {
	start = 0,
	interrupt = 1,
	_end = 2,
	dispose = 3,
	complete = 4,
	event = 5
}

local EventQueue = {}
EventQueue.__index = EventQueue

function EventQueue.new (animationState)
	local self = {
		objects = {},
		animationState = animationState,
		drainDisabled = false
	}
	setmetatable(self, EventQueue)
	return self
end

function EventQueue:start (entry)
	local objects = self.objects
	table_insert(objects, EventType.start)
	table_insert(objects, entry)
	animationState.animationsChanged = true
end

function EventQueue:interrupt (entry)
	local objects = self.objects
	table_insert(objects, EventType.interrupt)
	table_insert(objects, entry)
end

function EventQueue:_end (entry)
	local objects = self.objects
	table_insert(objects, EventType._end)
	table_insert(objects, entry)
	animationState.animationsChanged = true
end

function EventQueue:dispose (entry)
	local objects = self.objects
	table_insert(objects, EventType.dispose)
	table_insert(objects, entry)
end

function EventQueue:complete (entry)
	local objects = self.objects
	table_insert(objects, EventType.complete)
	table_insert(objects, entry)
end

function EventQueue:event (entry, event)
	local objects = self.objects
	table_insert(objects, EventType.event)
	table_insert(objects, entry)
	table_insert(objects, event)
end

function EventQueue:drain ()
	if self.drainDisabled then return end -- Not reentrant.
	self.drainDisabled = true

	local objects = self.objects
	local as = self.animationState
	local i = 1
	local n = #objects
	while i <= n do
		local _type = objects[i]
		local entry = objects[i + 1]
		if _type == EventType.start then
			if entry.onStart then entry.onStart(entry) end
			if as.onStart then entry.onStart(entry) end
		elseif _type == EventType.interrupt then
			if entry.onInterrupt then entry.onInterrupt(entry) end
			if as.onInterrupt then entry.onInterrupt(entry) end
		elseif _type == EventType._end then
			if entry.onEnd then entry.onEnd(entry) end
			if as.onEnd then entry.onEnd(entry) end
			-- fall through in ref impl
			if entry.onDispose then entry.onDispose(entry) end
			if as.onDispose then entry.onDispose(entry) end
		elseif _type == EventType._dispose then
			if entry.onDispose then entry.onDispose(entry) end
			if as.onDispose then entry.onDispose(entry) end
		elseif _type == EventType.complete then
			if entry.onComplete then entry.onComplete(entry) end
			if as.onComplete then entry.onComplete(entry) end
		elseif _type == EventType.event then
			local event = objects[i + 2]
			if entry.onEvent then entry.onEvent(entry, event) end
			if as.onEvent then entry.onEvent(entry, event) end
			i = i + 1
		end
		i = i + 2
	end
	self:clear()

	self.drainDisabled = false;
end

function EventQueue:clear ()
	self.objects[1] = nil -- dirty trick so we don't re-alloc, relies on using # in drain
end


local TrackEntry = {}
TrackEntry.__index = TrackEntry

function TrackEntry.new ()
	local self = {
		animation = nil,
		next = nil, mixingFrom = nil,
		onStart = nil, onInterrupt = nil, onEnd = nil, onDispose = nil, onComplete = nil, onEvent = nil,
		trackIndex = 0,
		loop = false,
		eventThreshold = 0, attachmentThreshold = 0, drawOrderThreshold = 0,
		animationStart = 0, animationEnd = 0, animationLast = 0, nextAnimationLast = 0,
		delay = 0, trackTime = 0, trackLast = 0, nextTrackLast = 0, trackEnd = 0, timeScale = 0,
		alpha = 0, mixTime = 0, mixDuration = 0, mixAlpha = 0,
		timelinesFirst = {},
		timelinesRotation = {}
	}
	setmetatable(self, TrackEntry)
	return self
end

function TrackEntry:getAnimationTime ()
	if loop then
		local duration = animationEnd - animationStart
		if duration == 0 then return animationStart end
		return (trackTime % duration) + animationStart
	end
	return math_min(trackTime + animationStart, animationEnd)
end

local AnimationState = {}
AnimationState.__index = AnimationState

function AnimationState.new (data)
	if not data then error("data cannot be nil", 2) end

	local self = {
		data = data,
		tracks = {},
		events = {},
		onStart = nil, onInterrupt = nil, onEnd = nil, onDispose = nil, onComplete = nil, onEvent = nil,
		queue = nil,
		propertyIDs = {},
		animationsChanged = false,
		timeScale = 1
	}
	queue = EventQueue.new(self)
	setmetatable(self, AnimationState)
	return self
end

AnimationState.TrackEntry = TrackEntry

function AnimationState:update (delta)
	delta = delta * self.timeScale
	local tracks = self.tracks
	local queue = self.queue
	for i,current in pairs(tracks) do
		if current then
			current.animationLast = current.nextAnimationLast
			current.trackLast = current.nextTrackLast

			local currentDelta = delta * current.timeScale

			local skip = false
			if current.delay > 0 then
				current.delay = current.delay - currentDelta
				if current.delay <= 0 then
					skip = true
					currentDelta = -current.delay
					current.delay = 0
				end
			end
			
			if not skip then
				local next = current.next
				if next then
					-- When the next entry's delay is passed, change to the next entry, preserving leftover time.
					local nextTime = current.trackLast - next.delay
					if nextTime >= 0 then
						next.delay = 0
						next.trackTime = nextTime + delta * next.timeScale
						current.trackTime = current.trackTime + currentDelta
						self:setCurrent(i, next)
						while next.mixingFrom do
							next.mixTime = next.mixTime + currentDelta
							next = next.mixingFrom
						end
						skip = true
					end
					if not skip then
						self:updateMixingFrom(current, delta, true);
					end
				else
					self:updateMixingFrom(current, delta, true)
					-- Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
					if current.trackLast >= current.trackEnd and current.mixingFrom == nil then
						tracks[i] = nil
						queue:_end(current)
						self:disposeNext(current)
						skip = true
					end
				end

				if not skip then current.trackTime = current.trackTime + currentDelta end
			end
		end
	end

	queue:drain()
end

function AnimationState:updateMixingFrom (entry, delta, canEnd)
	local from = entry.mixingFrom
	if from == nil then return end

	local queue = self.queue
	if canEnd and entry.mixTime >= entry.mixDuration and entry.mixTime > 0 then
		queue:_end(from)
		local newFrom = from.mixingFrom
		entry.mixingFrom = newFrom
		if newFrom == nil then return end
		entry.mixTime = from.mixTime;
		entry.mixDuration = from.mixDuration;
		from = newFrom;
	end

	from.animationLast = from.nextAnimationLast
	from.trackLast = from.nextTrackLast
	local mixingFromDelta = delta * from.timeScale
	from.trackTime = from.trackTime + mixingFromDelta;
	entry.mixTime = entry.mixtime + mixingFromDelta;

	self:updateMixingFrom(from, delta, canEnd and from.alpha == 1)
end

function AnimationState:apply (skeleton)
	if skeleton == nil then error("skeleton cannot be null.", 2) end
	if self.animationsChanged then self:_animationsChanged() end

	local events = self.events
	local tracks = self.tracks
	local queue = self.queue

	for i,current in pairs(tracks) do
		if not (current == nil or current.delay > 0) then
			-- Apply mixing from entries first.
			local mix = current.alpha
			if current.mixingFrom then mix = mix * applyMixingFrom(current, skeleton) end

			-- Apply current entry.
			local animationLast = current.animationLast
			local animationTime = current:getAnimationTime()
			local timelines = current.animation.timelines
			if mix == 1 then
				for i,timeline in ipairs(timelines) do
					timeline:apply(skeleton, animationLast, animationTime, events, 1, true, false)
				end
			else
				local firstFrame = #current.timelinesRotation == 0
				local timelinesRotation = current.timelinesRotation;
				local timelinesFirst = current.timelinesFirst
				for i,timeline in ipairs(timelines) do
					if timeline.type == Animation.TimelineType.rotate then
						self:applyRotateTimeline(timeline, skeleton, animationTime, mix, timelinesFirst[ii], timelinesRotation, ii * 2,
							firstFrame) -- FIXME passing ii * 2, indexing correct?
					else
						timeline:apply(skeleton, animationLast, animationTime, events, mix, timelinesFirst[ii], false)
					end
				end
			end
			self:queueEvents(current, animationTime)
			current.nextAnimationLast = animationTime
			current.nextTrackLast = current.trackTime
		end
	end

	queue:drain()
end

function AnimationState:applyMixingFrom (entry, skeleton)
	local from = entry.mixingFrom
	if from.mixingFrom then self:applyMixingFrom(from, skeleton) end

	local mix = 0
	if entry.mixDuration == 0 then -- Single frame mix to undo mixingFrom changes.
		mix = 1
	else
		mix = entry.mixTime / entry.mixDuration
		if mix > 1 then mix = 1 end
	end

	local events = nil
	if mix < from.eventThreshold then events = self.events end
	local attachments = mix < from.attachmentThreshold
	local drawOrder = mix < from.drawOrderThreshold
	local animationLast = from.animationLast
	local animationTime = from:getAnimationTime()
	local timelines = from.animation.timelines
	local timelinesFirst = from.timelinesFirst;
	local alpha = from.alpha * entry.mixAlpha * (1 - mix)

	local firstFrame = #from.timelinesRotation.size == 0
	local timelinesRotation = from.timelinesRotation

	local skip = false
	for i,timeline in ipairs(timelines) do
		local setupPose = timelinesFirst[i]
		if timeline.type == Animation.TimelineType.rotate then
			self:applyRotateTimeline(timeline, skeleton, animationTime, alpha, setupPose, timelinesRotation, i * 2, firstFrame) -- FIXME passing i * 2, correct indexing?
		else
			if not setupPose then
				if not attachments and timeline.type == Animation.TimelineType.attackment then skip = true end
				if not drawOrder and timeline.type == Animation.TimelineType.drawOrder then skip = true end
			end
			if not skip then timeline:apply(skeleton, animationLast, animationTime, events, alpha, setupPose, true) end
		end
	end

	self:queueEvents(from, animationTime)
	from.nextAnimationLast = animationTime
	from.nextTrackLast = from.trackTime

	return mix
end

-- CONTINUE WITH applyRotateTimeline here

return AnimationState
