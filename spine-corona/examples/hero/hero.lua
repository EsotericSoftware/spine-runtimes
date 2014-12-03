
-- This skeleton uses IK for the feet.

local spine = require "spine-corona.spine"

local json = spine.SkeletonJson.new()
local skeletonData = json:readSkeletonDataFile("examples/hero/hero.json")

local skeleton = spine.Skeleton.new(skeletonData)
function skeleton:createImage (attachment)
	return display.newImage("examples/hero/images/" .. attachment.name .. ".png")
end
skeleton.group.x = 95
skeleton.group.y = 385
skeleton.flipX = false
skeleton.flipY = false
skeleton.debug = true -- Omit or set to false to not draw debug lines on top of the images.
skeleton:setToSetupPose()

-- AnimationStateData defines crossfade durations between animations.
local stateData = spine.AnimationStateData.new(skeletonData)
stateData.defaultMix = 0.2;
stateData:setMix("Walk", "Attack", 0)
stateData:setMix("Attack", "Run", 0)
stateData:setMix("Run", "Attack", 0)
-- AnimationState has a queue of animations and can apply them with crossfading.
local state = spine.AnimationState.new(stateData)
state:setAnimationByName(0, "Idle", true)

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

Runtime:addEventListener("touch", function (event)
	if event.phase ~= "ended" and event.phase ~= "cancelled" then return end
	local name = state:getCurrent(0).animation.name
	if name == "Idle" then
		state:setAnimationByName(0, "Crouch", true)
	elseif name == "Crouch" then
		state:setAnimationByName(0, "Walk", true)
	else
		state:setAnimationByName(0, "Attack", false)
		state:addAnimationByName(0, "Run", true, 0)
	end
end)
