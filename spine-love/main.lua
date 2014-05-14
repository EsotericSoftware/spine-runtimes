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

local spine = require "spine-love.spine"

local json = spine.SkeletonJson.new()
json.scale = 1
local skeletonData = json:readSkeletonDataFile("data/spineboy.json")

local skeleton = spine.Skeleton.new(skeletonData)
function skeleton:createImage (attachment)
	-- Customize where images are loaded.
	return love.graphics.newImage("data/" .. attachment.name .. ".png")
end
skeleton.x = love.graphics.getWidth() / 2
skeleton.y = love.graphics.getHeight() / 2 + 150
skeleton.flipX = false
skeleton.flipY = false
skeleton.debugBones = true -- Omit or set to false to not draw debug lines on top of the images.
skeleton.debugSlots = true
skeleton:setToSetupPose()

-- AnimationStateData defines crossfade durations between animations.
local stateData = spine.AnimationStateData.new(skeletonData)
stateData:setMix("walk", "jump", 0.2)
stateData:setMix("jump", "walk", 0.4)

-- AnimationState has a queue of animations and can apply them with crossfading.
local state = spine.AnimationState.new(stateData)
state:setAnimationByName(0, "drawOrder")
state:addAnimationByName(0, "jump", false, 0)
state:addAnimationByName(0, "walk", true, 0)

state.onStart = function (trackIndex)
	print(trackIndex.." start: "..state:getCurrent(trackIndex).animation.name)
end
state.onEnd = function (trackIndex)
	print(trackIndex.." end: "..state:getCurrent(trackIndex).animation.name)
end
state.onComplete = function (trackIndex, loopCount)
	print(trackIndex.." complete: "..state:getCurrent(trackIndex).animation.name..", "..loopCount)
end
state.onEvent = function (trackIndex, event)
	print(trackIndex.." event: "..state:getCurrent(trackIndex).animation.name..", "..event.data.name..", "..event.intValue..", "..event.floatValue..", '"..(event.stringValue or "").."'")
end

function love.update (delta)
	-- Update the state with the delta time, apply it, and update the world transforms.
	state:update(delta)
	state:apply(skeleton)
	skeleton:updateWorldTransform()
end

function love.draw ()
	love.graphics.setColor(255, 255, 255)
	skeleton:draw()
end
