 -------------------------------------------------------------------------------
 -- Copyright (c) 2013, Esoteric Software
 -- All rights reserved.
 -- 
 -- Redistribution and use in source and binary forms, with or without
 -- modification, are permitted provided that the following conditions are met:
 -- 
 -- 1. Redistributions of source code must retain the above copyright notice, this
 --    list of conditions and the following disclaimer.
 -- 2. Redistributions in binary form must reproduce the above copyright notice,
 --    this list of conditions and the following disclaimer in the documentation
 --    and/or other materials provided with the distribution.
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

local spine = require "spine-love.spine"

local json = spine.SkeletonJson.new()
json.scale = 1
local skeletonData = json:readSkeletonDataFile("data/spineboy.json")
local walkAnimation = skeletonData:findAnimation("walk")

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
skeleton.debugSlots = false
skeleton:setToSetupPose()

local animationTime = 0
function love.update (delta)
	animationTime = animationTime + delta
	walkAnimation:apply(skeleton, animationTime, true)
	skeleton:updateWorldTransform()
end

function love.draw ()
	love.graphics.setColor(255, 255, 255)
	skeleton:draw()
end
