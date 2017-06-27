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
local MixPose = Animation.MixPose
local MixDirection = Animation.MixDirection
local AnimationStateData = require "spine-lua.AnimationStateData"
local math_min = math.min
local math_max = math.max
local math_abs = math.abs
local math_signum = utils.signum
local math_floor = math.floor
local math_ceil = math.ceil
local math_mod = utils.mod

local function zlen(array)
	return #array + 1
end

local EMPTY_ANIMATION = Animation.new("<empty>", {}, 0)
local SUBSEQUENT = 0
local FIRST = 1
local DIP = 2
local DIP_MIX = 3;

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
	self.animationState.animationsChanged = true
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
  self.animationState.animationsChanged = true
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
			if as.onStart then as.onStart(entry) end
		elseif _type == EventType.interrupt then
			if entry.onInterrupt then entry.onInterrupt(entry) end
			if as.onInterrupt then as.onInterrupt(entry) end
		elseif _type == EventType._end then
			if entry.onEnd then entry.onEnd(entry) end
			if as.onEnd then as.onEnd(entry) end
			-- fall through in ref impl
			if entry.onDispose then entry.onDispose(entry) end
			if as.onDispose then as.onDispose(entry) end
		elseif _type == EventType._dispose then
			if entry.onDispose then entry.onDispose(entry) end
			if as.onDispose then as.onDispose(entry) end
		elseif _type == EventType.complete then
			if entry.onComplete then entry.onComplete(entry) end
			if as.onComplete then as.onComplete(entry) end
		elseif _type == EventType.event then
			local event = objects[i + 2]
			if entry.onEvent then entry.onEvent(entry, event) end
			if as.onEvent then as.onEvent(entry, event) end
			i = i + 1
		end
		i = i + 2
	end
	self:clear()

	self.drainDisabled = false;
end

function EventQueue:clear ()
	self.objects = {}
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
		alpha = 0, mixTime = 0, mixDuration = 0, interruptAlpha = 0, totalAlpha = 0,
		timelineData = {},
		timelineDipMix = {},
		timelinesRotation = {}
	}
	setmetatable(self, TrackEntry)
	return self
end

function TrackEntry:setTimelineData(to, mixingToArray, propertyIDs)
	if to then table_insert(mixingToArray, to) end
	local lastEntry = self
	if self.mixingFrom then lastEntry = self.mixingFrom:setTimelineData(self, mixingToArray, propertyIDs) end
	if to then mixingToArray[#mixingToArray] = nil end

	local mixingTo = mixingToArray
	local mixingToLast = #mixingToArray
	local timelines = self.animation.timelines
	local timelinesCount = #self.animation.timelines
	local timelineData = self.timelineData
	local timelineDipMix = self.timelineDipMix

	local i = 1
	while i <= timelinesCount do
		local id = "" .. timelines[i]:getPropertyId()
		if not (propertyIDs[id] == nil) then
			timelineData[i] = SUBSEQUENT
		elseif (to == nil or not to:hasTimeline(id)) then
			timelineData[i] = FIRST
		else
			local ii = mixingToLast
			while ii > 0 do
				local entry = mixingTo[ii]
				local skip = false
				if not entry:hasTimeline(id) then
					if entry.mixDuration > 0 then 
						timelineData[i] = DIP_MIX
						timelineDipMix[i] = entry 
						skip = true
						break
					end
					break;
				end
				ii = ii - 1
			end
			if not skip then 	timelineData[i] = DIP end
		end
		i = i + 1
	end
	return lastEntry
end

function TrackEntry:hasTimeline(id)
	local timelines = self.animation.timelines
	for i,timeline in ipairs(timelines) do
		if timeline:getPropertyId() == id then return true end
	end
	return false
end

function TrackEntry:getAnimationTime ()
	if self.loop then
		local duration = self.animationEnd - self.animationStart
		if duration == 0 then return self.animationStart end
		return (self.trackTime % duration) + self.animationStart
	end
	return math_min(self.trackTime + self.animationStart, self.animationEnd)
end

function TrackEntry:resetRotationDirections ()
  self.timelinesRotation = {}
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
		timeScale = 1,
		mixingTo = {}
	}
	self.queue = EventQueue.new(self)
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
				local _next = current.next
				if _next then
					-- When the next entry's delay is passed, change to the next entry, preserving leftover time.
					local nextTime = current.trackLast - _next.delay
					if nextTime >= 0 then
						_next.delay = 0
						_next.trackTime = nextTime + delta * _next.timeScale
						current.trackTime = current.trackTime + currentDelta
						self:setCurrent(i, _next, true)
						while _next.mixingFrom do
							_next.mixTime = _next.mixTime + currentDelta
							_next = _next.mixingFrom
						end
						skip = true
					end
				else
					-- Clear the track when there is no next entry, the track end time is reached, and there is no mixingFrom.
					if current.trackLast >= current.trackEnd and current.mixingFrom == nil then
						tracks[i] = nil
						queue:_end(current)
						self:disposeNext(current)
						skip = true
					end
				end

				if not skip then
					if current.mixingFrom and self:updateMixingFrom(current, delta) then
						-- End mixing from entries once all have completed.
						local from = current.mixingFrom
						current.mixingFrom = nil
						while from do
							queue:_end(from)
							from = from.mixingFrom
						end
					end
					
          current.trackTime = current.trackTime + currentDelta
        end
			end
		end
	end

	queue:drain()
end

function AnimationState:updateMixingFrom (to, delta)
	local from = to.mixingFrom
	if from == nil then return true end

 	local finished = self:updateMixingFrom(from, delta)
	
	-- Require mixTime > 0 to ensure the mixing from entry was applied at least once.
	if (to.mixTime > 0 and (to.mixTime >= to.mixDuration or to.timeScale == 0)) then
		if (from.totalAlpha == 0) then
			to.mixingFrom = from.mixingFrom
			to.interruptAlpha = from.interruptAlpha
			self.queue:_end(from)
		end
		return finished
	end

	from.animationLast = from.nextAnimationLast
	from.trackLast = from.nextTrackLast
	from.trackTime = from.trackTime + delta * from.timeScale
	to.mixTime = to.mixTime + delta * to.timeScale
	return false;
end

function AnimationState:apply (skeleton)
	if skeleton == nil then error("skeleton cannot be null.", 2) end
	if self.animationsChanged then self:_animationsChanged() end

	local events = self.events
	local tracks = self.tracks
	local queue = self.queue
  local applied = false

	for i,current in pairs(tracks) do
		if not (current == nil or current.delay > 0) then
      applied = true
			local currrentPose = MixPose.currentLayered
			if i == 0 then currentPose = MixPose.current end
			
			-- Apply mixing from entries first.
			local mix = current.alpha
			if current.mixingFrom then 
				mix = mix * self:applyMixingFrom(current, skeleton, currentPose)
			elseif current.trackTime >= current.trackEnd and current.next == nil then
				mix = 0
			end

			-- Apply current entry.
			local animationLast = current.animationLast
			local animationTime = current:getAnimationTime()
			local timelines = current.animation.timelines
			if mix == 1 then
				for i,timeline in ipairs(timelines) do
					timeline:apply(skeleton, animationLast, animationTime, events, 1, MixPose.setup, MixDirection._in)
				end
			else
				local timelineData = current.timelineData
				local firstFrame = #current.timelinesRotation == 0
				local timelinesRotation = current.timelinesRotation

				for i,timeline in ipairs(timelines) do
					local pose = MixPose.currentPose
					if timelineData[i] >= FIRST then pose = MixPose.setup end
					
					if timeline.type == Animation.TimelineType.rotate then
						self:applyRotateTimeline(timeline, skeleton, animationTime, mix, pose, timelinesRotation, i * 2,
							firstFrame) -- FIXME passing ii * 2, indexing correct?
					else
						timeline:apply(skeleton, animationLast, animationTime, events, mix, pose, MixDirection._in)
					end
				end
			end
			self:queueEvents(current, animationTime)
			self.events = {};
			current.nextAnimationLast = animationTime
			current.nextTrackLast = current.trackTime
		end
	end

	queue:drain()
  return applied
end

function AnimationState:applyMixingFrom (to, skeleton, currentPose)
	local from = to.mixingFrom
	if from.mixingFrom then self:applyMixingFrom(from, skeleton, currentPose) end

	local mix = 0
	if to.mixDuration == 0 then -- Single frame mix to undo mixingFrom changes.
		mix = 1
	else
		mix = to.mixTime / to.mixDuration
		if mix > 1 then mix = 1 end
	end

	local events = nil
	if mix < from.eventThreshold then events = self.events end
	local attachments = mix < from.attachmentThreshold
	local drawOrder = mix < from.drawOrderThreshold
	local animationLast = from.animationLast
	local animationTime = from:getAnimationTime()
	local timelines = from.animation.timelines
	local timelineData = from.timelineData
	local timelineDipMix = from.timelineDipMix

	local firstFrame = #from.timelinesRotation == 0
	local timelinesRotation = from.timelinesRotation

	local pose = MixPose.setup
	local alphaDip = from.alpha * to.interruptAlpha
	local alphaMix = alphaDip * (1 - mix)
	local alpha = 0
	from.totalAlpha = 0;

	for i,timeline in ipairs(timelines) do
		local skipSubsequent = false;
		if timelineData[i] == SUBSEQUENT then
			if not attachments and timeline.type == Animation.TimelineType.attachment then skipSubsequent = true end
			if not drawOrder and timeline.type == Animation.TimelineType.drawOrder then skipSubsequent = true end
			pose = currentPose
			alpha = alphaMix
		elseif timelineData[i] == FIRST then
			pose = MixPose.setup
			alpha = alphaMix
		elseif timelineData[i] == DIP then
			pose = MixPose.setup
			alpha = alphaDip
		else
			pose = MixPose.setup
			alpha = alphaDip
			local dipMix = timelineDipMix[i]
			alpha = alpha * math_max(0, 1 - dipMix.mixtime / dipMix.mixDuration)
		end
		
		if not skipSubsequent then
			from.totalAlpha = from.totalAlpha + alpha
			if timeline.type == Animation.TimelineType.rotate then
				self:applyRotateTimeline(timeline, skeleton, animationTime, alpha, pose, timelinesRotation, i * 2, firstFrame)
			else
				timeline:apply(skeleton, animationLast, animationTime, events, alpha, pose, MixDirection.out)
			end
		end
	end

	if (to.mixDuration > 0) then 	self:queueEvents(from, animationTime) end
	self.events = {};
	from.nextAnimationLast = animationTime
	from.nextTrackLast = from.trackTime

	return mix
end

function AnimationState:applyRotateTimeline (timeline, skeleton, time, alpha, pose, timelinesRotation, i, firstFrame)
	if firstFrame then 
		timelinesRotation[i] = 0
		timelinesRotation[i+1] = 0
	end
	
  if alpha == 1 then
    timeline:apply(skeleton, 0, time, nil, 1, pose, MixDirection._in)
    return
  end

  local rotateTimeline = timeline
  local frames = rotateTimeline.frames
  local bone = skeleton.bones[rotateTimeline.boneIndex]
  if time < frames[0] then
		if pose == MixPose.setup then bone.rotation = bone.data.rotation end
		return
	end

  local r2 = 0
  if time >= frames[zlen(frames) - Animation.RotateTimeline.ENTRIES] then -- Time is after last frame.
    r2 = bone.data.rotation + frames[zlen(frames) + Animation.RotateTimeline.PREV_ROTATION]
  else
    -- Interpolate between the previous frame and the current frame.
    local frame = Animation.binarySearch(frames, time, Animation.RotateTimeline.ENTRIES)
    local prevRotation = frames[frame + Animation.RotateTimeline.PREV_ROTATION]
    local frameTime = frames[frame]
    local percent = rotateTimeline:getCurvePercent(math_floor(frame / 2) - 1,
      1 - (time - frameTime) / (frames[frame + Animation.RotateTimeline.PREV_TIME] - frameTime))

    r2 = frames[frame + Animation.RotateTimeline.ROTATION] - prevRotation
    r2 = r2 - (16384 - math_floor(16384.499999999996 - r2 / 360)) * 360
    r2 = prevRotation + r2 * percent + bone.data.rotation
    r2 = r2 - (16384 - math_floor(16384.499999999996 - r2 / 360)) * 360
  end

  -- Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
  local r1 = bone.rotation
  if pose == MixPose.setup then r1 = bone.data.rotation end
  local total = 0
  local diff = r2 - r1
  if diff == 0 then
    total = timelinesRotation[i]
  else
    diff = diff - (16384 - math_floor(16384.499999999996 - diff / 360)) * 360
    local lastTotal = 0
    local lastDiff = 0
    if firstFrame then
      lastTotal = 0
      lastDiff = diff
    else
      lastTotal = timelinesRotation[i] -- Angle and direction of mix, including loops.
      lastDiff = timelinesRotation[i + 1] -- Difference between bones.
    end
    local current = diff > 0
    local dir = lastTotal >= 0
    -- Detect cross at 0 (not 180).
    if math_signum(lastDiff) ~= math_signum(diff) and math_abs(lastDiff) <= 90 then
      -- A cross after a 360 rotation is a loop.
      if math_abs(lastTotal) > 180 then lastTotal = lastTotal + 360 * math_signum(lastTotal) end
      dir = current
    end
    total = diff + lastTotal - math_mod(lastTotal, 360) -- FIXME used to be %360, store loops as part of lastTotal.
    if dir ~= current then total = total + 360 * math_signum(lastTotal) end
    timelinesRotation[i] = total
  end
  timelinesRotation[i + 1] = diff
  r1 = r1 + total * alpha
  bone.rotation = r1 - (16384 - math_floor(16384.499999999996 - r1 / 360)) * 360
end

function AnimationState:queueEvents (entry, animationTime)
  local animationStart = entry.animationStart
  local animationEnd = entry.animationEnd
  local duration = animationEnd - animationStart
  local trackLastWrapped = entry.trackLast % duration

  -- Queue events before complete.
  local events = self.events
  local queue = self.queue
  local i = 1
  local n = #events
  while i <= n do
    local event = events[i]
    if event.time < trackLastWrapped then break end
    if not (event.time > animationEnd) then -- Discard events outside animation start/end.
      queue:event(entry, event)
    end
    i = i + 1
  end

  -- Queue complete if completed a loop iteration or the animation.
  local queueComplete = false
  if entry.loop then 
    queueComplete = (trackLastWrapped > entry.trackTime % duration)
  else
    queueComplete = (animationTime >= animationEnd and entry.animationLast < animationEnd)
  end
  if queueComplete then
    queue:complete(entry)
  end

  -- Queue events after complete.
  while i <= n do
    local event = events[i]
    if not (event.time < animationStart) then --// Discard events outside animation start/end.
      queue:event(entry, event)
    end
    i = i + 1
  end
end

function AnimationState:clearTracks ()
  local queue = self.queue
  local tracks = self.tracks
	local oldDrainDisabled = queue.drainDisabled
  queue.drainDisabled = true;
  for i,track in pairs(tracks) do
    self:clearTrack(i)
  end
  tracks = {}
  queue.drainDisabled = oldDrainDisabled
  queue:drain();
end

function AnimationState:clearTrack (trackIndex)
  local tracks = self.tracks
  local queue = self.queue
  local current = tracks[trackIndex]
  if current == nil then return end

  queue:_end(current)

  self:disposeNext(current)

  local entry = current;
  while (true) do
    local from = entry.mixingFrom
    if from == nil then break end
    queue:_end(from)
    entry.mixingFrom = nil
    entry = from
  end

  tracks[current.trackIndex] = nil

  queue:drain()
end

function AnimationState:setCurrent (index, current, interrupt)
  local from = self:expandToIndex(index)
  local tracks = self.tracks
  local queue = self.queue
  tracks[index] = current

  if from then
    if interrupt then queue:interrupt(from) end
    current.mixingFrom = from
    current.mixTime = 0
		
		if from.mixingFrom and from.mixDuration > 0 then
			current.interruptAlpha = current.interruptAlpha * math_min(1, from.mixTime / from.mixDuration)
		end
		
		from.timelinesRotation = {};
  end

  queue:start(current)
end

function AnimationState:setAnimationByName (trackIndex, animationName, loop)
		local animation = self.data.skeletonData:findAnimation(animationName)
		if not animation then error("Animation not found: " .. animationName, 2) end
		return self:setAnimation(trackIndex, animation, loop)
end

function AnimationState:setAnimation (trackIndex, animation, loop)
  if not animation then error("animation cannot be null.") end
  local interrupt = true;
  local current = self:expandToIndex(trackIndex)
  local queue = self.queue
  local tracks = self.tracks
  if current then
    if current.nextTrackLast == -1 then
      -- Don't mix from an entry that was never applied.
      tracks[trackIndex] = current.mixingFrom
      queue:interrupt(current)
      queue:_end(current)
      self:disposeNext(current)
      current = current.mixingFrom
      interrupt = false;
    else
      self:disposeNext(current)
    end
  end
  local entry = self:trackEntry(trackIndex, animation, loop, current)
  self:setCurrent(trackIndex, entry, interrupt)
  queue:drain()
  return entry
end

function AnimationState:addAnimationByName (trackIndex, animationName, loop, delay)
		local animation = self.data.skeletonData:findAnimation(animationName)
		if not animation then error("Animation not found: " + animationName) end
		return self:addAnimation(trackIndex, animation, loop, delay)
end

function AnimationState:addAnimation (trackIndex, animation, loop, delay)
  if not animation then error("animation cannot be null.") end

  local last = self:expandToIndex(trackIndex)
  if last then
    while last.next do
      last = last.next
    end
  end

  local entry = self:trackEntry(trackIndex, animation, loop, last)
  local queue = self.queue
  local data = self.data
  
  if not last then
    self:setCurrent(trackIndex, entry, true)
    queue:drain()
  else
    last.next = entry
    if delay <= 0 then
      local duration = last.animationEnd - last.animationStart
      if duration ~= 0 then
        delay = delay + duration * (1 + math_floor(last.trackTime / duration)) - data:getMix(last.animation, animation)
      else
        delay = 0
      end
    end
  end

  entry.delay = delay
  return entry
end

function AnimationState:setEmptyAnimation (trackIndex, mixDuration)
  local entry = self:setAnimation(trackIndex, EMPTY_ANIMATION, false)
  entry.mixDuration = mixDuration
  entry.trackEnd = mixDuration
  return entry
end

function AnimationState:addEmptyAnimation (trackIndex, mixDuration, delay)
  if delay <= 0 then delay = delay - mixDuration end
  local entry = self:addAnimation(trackIndex, EMPTY_ANIMATION, false, delay)
  entry.mixDuration = mixDuration
  entry.trackEnd = mixDuration
  return entry
end

function AnimationState:setEmptyAnimations (mixDuration)
  local queue = self.queue
	local oldDrainDisabled = queue.drainDisabled
  queue.drainDisabled = true
  for i,current in pairs(self.tracks) do
    if current then self:setEmptyAnimation(current.trackIndex, mixDuration) end
  end
  queue.drainDisabled = oldDrainDisabled
  queue:drain()
end

function AnimationState:expandToIndex (index)
  return self.tracks[index]
end

function AnimationState:trackEntry (trackIndex, animation, loop, last)
  local data = self.data
  local entry = TrackEntry.new()
  entry.trackIndex = trackIndex
  entry.animation = animation
  entry.loop = loop

  entry.eventThreshold = 0
  entry.attachmentThreshold = 0
  entry.drawOrderThreshold = 0

  entry.animationStart = 0
  entry.animationEnd = animation.duration
  entry.animationLast = -1
  entry.nextAnimationLast = -1

  entry.delay = 0
  entry.trackTime = 0
  entry.trackLast = -1
  entry.nextTrackLast = -1
  entry.trackEnd = 999999999  
  entry.timeScale = 1

  entry.alpha = 1
  entry.interruptAlpha = 1
  entry.mixTime = 0
  if not last then
    entry.mixDuration = 0
  else
    entry.mixDuration = data:getMix(last.animation, animation)
  end
  return entry
end

function AnimationState:disposeNext (entry)
  local _next = entry.next
  local queue = self.queue
  while _next do
    queue:dispose(_next)
    _next = _next.next
  end
  entry.next = nil 
end

function AnimationState:_animationsChanged ()
	self.animationsChanged = false

	self.propertyIDs = {}
	local propertyIDs = self.propertyIDs
	local mixingTo = self.mixingTo
	  
	for i, entry in pairs(self.tracks) do
		if entry then
			entry:setTimelineData(nil, mixingTo, propertyIDs)			
		end
	end
end

function AnimationState:getCurrent (trackIndex)
  return self.tracks[trackIndex]
end

function AnimationState:clearListeners ()
  self.onStart = nil
  self.onInterrupt = nil
  self.onEnd = nil
  self.onComplete = nil
  self.onDispose = nil
  self.onEvent = nil
end

function AnimationState:clearListenerNotificatin ()
  self.queue:clear()
end

return AnimationState
