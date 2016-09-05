
-- This example shows simple usage of displaying a skeleton with queued animations.

local spine = require "spine-corona.spine"

local json = spine.SkeletonJson.new()
json.scale = 0.6
local skeletonData = json:readSkeletonDataFile("examples/spineboy/spineboy-mesh.json")

local skeleton = spine.Skeleton.new(skeletonData)
function skeleton:createImage (attachment)
	-- Customize where images are loaded.
	return display.newImage("examples/spineboy/images/" .. attachment.name .. ".png")
end
function skeleton:createMesh (attachment, meshParameters)
	-- Customize where mesh fill is loaded from.
	local mesh = display.newMesh(meshParameters)
	mesh.fill = {type="image", filename="examples/spineboy/images/" .. attachment.name .. ".png"}
	return mesh
end

skeleton.group.x = display.contentWidth * 0.5
skeleton.group.y = display.contentHeight * 0.9
skeleton.flipX = false
skeleton.flipY = false
skeleton.debug = true -- Omit or set to false to not draw debug lines on top of the images.
skeleton.debugAabb = true
skeleton:setToSetupPose()


local stateData = spine.AnimationStateData.new(skeletonData)

-- AnimationState has a queue of animations and can apply them with crossfading.
local state = spine.AnimationState.new(stateData)
state:addAnimationByName(0, "run", true, 0)

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

local lastTime = 0
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

