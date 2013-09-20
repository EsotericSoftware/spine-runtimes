------------------------------------------------------------------------------
 -- Spine Runtime Software License - Version 1.0
 -- 
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms in whole or in part, with
 -- or without modification, are permitted provided that the following conditions
 -- are met:
 -- 
 -- 1. A Spine Single User License or Spine Professional License must be
 --    purchased from Esoteric Software and the license must remain valid:
 --    http://esotericsoftware.com/
 -- 2. Redistributions of source code must retain this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer.
 -- 3. Redistributions in binary form must reproduce this license, which is the
 --    above copyright notice, this declaration of conditions and the following
 --    disclaimer, in the documentation and/or other materials provided with the
 --    distribution.
 -- 
 -- THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 -- ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 -- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 -- DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 -- ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 -- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 -- LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 -- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 -- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 -- SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ------------------------------------------------------------------------------

local AnimationState = {}

function AnimationState.new (data)
	if not data then error("data cannot be nil", 2) end

	local self = {
		data = data,
		animation = nil,
		previous = nil,
		currentTime = 0,
		previousTime = 0,
		currentLoop = false,
		previousLoop = false,
		mixTime = 0,
		mixDuration = 0,
		queue = {}
	}

	local function setAnimationInternal (animation, loop)
		self.previous = nil
		if (animation and self.animation) then
			self.mixDuration = data:getMix(self.animation.name, animation.name)
			if (self.mixDuration > 0) then
				self.mixTime = 0
				self.previous = self.animation
				self.previousTime = self.currentTime
				self.previousLoop = self.currentLoop
			end
		end
		self.animation = animation
		self.currentLoop = loop
		self.currentTime = 0
	end

	function self:update (delta)
		self.currentTime = self.currentTime + delta
		self.previousTime = self.previousTime + delta
		self.mixTime = self.mixTime + delta
		
		if (#self.queue > 0) then
			local entry = self.queue[1]
			if (self.currentTime >= entry.delay) then 
				setAnimationInternal(entry.animation, entry.loop)
				table.remove(self.queue, 1)
			end
		end
	end
	
	function self:apply(skeleton)
		if (not self.animation) then return end
		if (self.previous) then
			self.previous:apply(skeleton, self.previousTime, self.previousLoop)
			local alpha = self.mixTime / self.mixDuration
			if (alpha >= 1) then
				alpha = 1
				self.previous = nil
			end
			self.animation:mix(skeleton, self.currentTime, self.currentLoop, alpha)
		else
			self.animation:apply(skeleton, self.currentTime, self.currentLoop)
		end
	end
	
	-- Queues an animation to be played after a delay. The delay starts when the last queued animation (if any) begins.
	-- The delay may be <= 0 to use duration of the previous animation minus any mix duration plus the negative delay.
	function self:addAnimationWithDelay (animationName, loop, delay)
		if (delay <= 0) then
			-- Find the animation that is queued before this one.
			local last
			if (#self.queue == 0) then
				last = self.animation
			else
				last = self.queue[#self.queue].animation
			end
			if (last) then
				delay = last.duration - data:getMix(last.name, animationName) + delay
			else
				delay = 0
			end
		end
		local animation = nil
		if animationName then animation = data.skeletonData:findAnimation(animationName) end
		table.insert(self.queue, {animation = animation, loop = loop, delay = delay})
	end
	
	-- Queues an animation to be played after the last queued animation (if any).
	function self:addAnimation (animationName, loop)
		self:addAnimationWithDelay(animationName, loop, 0)
	end

	-- Clears the animation queue and sets the current animation.
	function self:setAnimation (animationName, loop)
		self.queue = {}
		local animation = nil
		if animationName then animation = data.skeletonData:findAnimation(animationName) end
		setAnimationInternal(animation, loop) 
	end

	function self:isComplete ()
		return (not self.animation) or self.currentTime >= self.animation.duration
	end

	return self
end
return AnimationState
