
-- This example shows how to use skins.

local spine = require "spine-corona.spine"

local json = spine.SkeletonJson.new()
json.scale = 1
local skeletonData = json:readSkeletonDataFile("examples/goblins/goblins.json")

local skeleton = spine.Skeleton.new(skeletonData)
function skeleton:createImage (attachment)
	return display.newImage("examples/goblins/images/" .. attachment.name .. ".png")
end
skeleton.group.x = 150
skeleton.group.y = 325
skeleton.flipX = false
skeleton.flipY = false
skeleton.debug = true -- Omit or set to false to not draw debug lines on top of the images.
skeleton:setSkin("goblingirl")
skeleton:setToSetupPose() -- Required after changing skin to attach attachments from skin.

-- AnimationState has a queue of animations and can apply them with crossfading.
local stateData = spine.AnimationStateData.new(skeletonData)
local state = spine.AnimationState.new(stateData)
state:setAnimationByName(0, "walk", true, 0)

local lastTime = 0
local animationTime = 0
Runtime:addEventListener("enterFrame", function (event)
	-- Compute time in seconds since last frame.
	local currentTime = event.time / 1000
	local delta = currentTime - lastTime
	lastTime = currentTime

	-- Update the state with the delta time, apply it, and update the world transforms.
	state:update(delta)
	state:apply(skeleton)
	skeleton:updateWorldTransform()
end)

