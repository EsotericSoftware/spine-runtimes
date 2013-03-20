
local spine = require "spine.spine"

-- Using your own attachment loader is optional. It can customizes the path where images are 
-- loaded. To load from a texture atlas, use an image sheet. It also creates instances of
-- all attachments, which can be used for customization.
local attachmentLoader = spine.AttachmentLoader.new()
function attachmentLoader:createImage (attachment)
	return display.newImage("data/" .. attachment.name .. ".png")
end

local json = spine.SkeletonJson.new(attachmentLoader)
json.scale = 1
local skeletonData = json:readSkeletonDataFile("data/spineboy-skeleton.json")
local walkAnimation = json:readAnimationFile(skeletonData, "data/spineboy-walk.json")

-- Optional second parameter can be the group for the Skeleton to use. Eg, could be an image group.
local skeleton = spine.Skeleton.new(skeletonData)
skeleton.x = 150
skeleton.y = 325
skeleton.flipX = false
skeleton.flipY = false
skeleton.debug = true -- Omit or set to false to not draw debug lines on top of the images.
skeleton:setToBindPose()

local lastTime = 0
local animationTime = 0
Runtime:addEventListener("enterFrame", function (event)
	-- Compute time in seconds since last frame.
	local currentTime = event.time / 1000
	local delta = currentTime - lastTime
	lastTime = currentTime

	-- Accumulate time and pose skeleton using animation.
	animationTime = animationTime + delta
	walkAnimation:apply(skeleton, animationTime, true)
	skeleton:updateWorldTransform()
end)
