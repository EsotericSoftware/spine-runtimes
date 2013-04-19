
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
skeleton:setToBindPose()

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
