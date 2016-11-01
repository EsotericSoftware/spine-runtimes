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

local function zlen(array)
	return #array + 1
end

local EMPTY_ANIMATION = Animation.new("<empty>", {}, 0)

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

function AnimationState:applyRotateTimeline (timeline, skeleton, time, alpha, setupPose, timelinesRotation, i, firstFrame)
  if alpha == 1 then
    timeline:apply(skeleton, 0, time, nil, 1, setupPose, false)
    return
  end

  local rotateTimeline = timeline
  local frames = rotateTimeline.frames
  if time < frames[0] then return end -- Time is before first frame.

  local bone = skeleton.bones[rotateTimeline.boneIndex]

  local r2 = 0
  if time >= frames[zlen(frames) - Animation.RotateTimeline.ENTRIES] then -- Time is after last frame.
    r2 = bone.data.rotation + frames[zlen(frames) + PREV_ROTATION]
  else
    -- Interpolate between the previous frame and the current frame.
    local frame = Animation.binarySearch(frames, time, ENTRIES)
    local prevRotation = frames[frame + PREV_ROTATION]
    local frameTime = frames[frame]
    local percent = rotateTimeline:getCurvePercent(math_floor(frame / 2) - 1,
      1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

    r2 = frames[frame + ROTATION] - prevRotation
    r2 = r2 - (16384 - math_floor(16384.499999999996 - r2 / 360)) * 360
    r2 = prevRotation + r2 * percent + bone.data.rotation
    r2 = r2 - (16384 - math_floor(16384.499999999996 - r2 / 360)) * 360
  end

  -- Mix between rotations using the direction of the shortest route on the first frame while detecting crosses.
  local r1 = bone.rotation
  if setupPose then r1 = bone.data.rotation end
  local total = 0
  local diff = r2 - r1
  if diff == 0 then
    if firstFrame then
      timelinesRotation[i] = 0
      total = 0
    else
      total = timelinesRotation[i]
    end
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
    total = diff + lastTotal - math_ceil(lastTotal / 360 - 0.5) * 360 -- FIXME used to be %360, store loops as part of lastTotal.
    if dir ~= current then total = total + 360 * Math.signum(lastTotal) end
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
      queue.event(entry, event)
    end
    i = i + 1
  end
  events = {}
end

function AnimationState:clearTracks ()
  local queue = self.queue
  local tracks = self.tracks
  queue.drainDisabled = true;
  for i,track in pairs(tracks) do
    self:clearTrack(i)
  end
  tracks = {}
  queue.drainDisabled = false;
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

function AnimationState:setCurrent (index, current)
  local from = self:expandToIndex(index)
  local tracks = self.tracks
  local queue = self.queue
  tracks[index] = current

  if from then
    queue:interrupt(from)
    current.mixingFrom = from
    current.mixTime = 0

    -- If not completely mixed in, set mixAlpha so mixing out happens from current mix to zero.
    if from.mixingFrom then current.mixAlpha = current.mixAlpha * math_min(from.mixTime / from.mixDuration, 1) end
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
  local current = self:expandToIndex(trackIndex)
  local queue = self.queue
  if current then
    if current.nextTrackLast == -1 then
      -- Don't mix from an entry that was never applied.
      tracks[trackIndex] = nil
      queue:interrupt(current)
      queue:_end(current)
      self:disposeNext(current)
      current = nil
    else
      self:disposeNext(current)
    end
  end
  local entry = self:trackEntry(trackIndex, animation, loop, current)
  self:setCurrent(trackIndex, entry)
  queue:drain()
  return entry
end

function AnimationState:addAnimationByName (trackIndex, animationName, loop, delay)
		local animation = data.skeletonData:findAnimation(animationName)
		if not animation then error("Animation not found: " + animationName) end
		return self:addAnimation(trackIndex, animation, loop, delay)
end

function AnimationState:addAnimation (trackIndex, animationName, loop, delay)
  if not nimation then error("animation cannot be null.") end

  local last = self:expandToIndex(trackIndex)
  if last then
    while last.next do
      last = last.next
    end
  end

  local entry = self:trackEntry(trackIndex, animation, loop, last)
  local queue = self.queue
  
  if not last then
    self:setCurrent(trackIndex, entry)
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
  queue.drainDisabled = true
  for i,current in pairs(self.tracks) do
    if current then self:setEmptyAnimation(current.trackIndex, mixDuration) end
  end
  queue.drainDisabled = false
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
  if loop then
    entry.trackEnd = 999999999
  else
    entry.trackEnd = entry.animationEnd
  end
  entry.timeScale = 1

  entry.alpha = 1
  entry.mixAlpha = 1
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
    _next = next.next
  end
  entry.next = nil 
end

function AnimationState:animationsChanged ()
  self.animationsChanged = false;

  self.propertyIDs = {}
  local propertyIDs = self.propertyIDs;

  -- need to get the highest index cause Lua is funny
  local highest = -1
  for i,entry in pairs(tracks) do
    if i > highest then highest = i end
  end

  -- Set timelinesFirst for all entries, from lowest track to highest.
  local i = 1
  local n = highest
  local tracks = self.tracks
  while i <= n do
    local entry = tracks[i]
    if entry then
      self:setTimelinesFirst(entry);
      i = i + 1
      break;
    end
    i = i + 1
  end
  while i <= n do
    local entry = tracks[i]
    if entry then self:checkTimelinesFirst(entry) end
    i = i + 1
  end
end

function AnimationState:setTimelinesFirst (entry)
  if entry.mixingFrom then
    self:setTimelinesFirst(entry.mixingFrom)
    self:checkTimelinesUsage(entry, entry.timelinesFirst)
    return
  end
  local propertyIDs = self.propertyIDs
  local n = #entry.animation.timelines
  local timelines = entry.animation.timelines
  entry.timelinesFirst = {}
  local usage = entry.timelinesFirst;
  local i = 1
  while i <= n do
    local id = "" .. timelines[i]:getPropertyId()
    propertyIDs[id] = id
    usage[i] = true;
    i = i + 1
  end
end

function AnimationState:checkTimlinesFirst (entry)
  if entry.mixingFrom then self:checkTimelinesFirst(entry.mixingFrom) end
  self:checkTimelinesUsage(entry, entry.timelinesFirst)
end

function AnimationState:checkTimelinesUsage (entry, usageArray)
  local propertyIDs = self.propertyIDs
  local n = #entry.animation.timelines
  local timelines = entry.animation.timelines
  local usage = usageArray
  while i <= n do
    local id = "" .. timelines[i]:getPropertyId()
    local contained = propertyIDs[id] == id
    propertyIDs[id] = id
    usage[i] = not contained
    i = i + 1
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
