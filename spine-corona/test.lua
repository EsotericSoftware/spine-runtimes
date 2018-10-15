require("mobdebug").start()

local spine = require "spine-corona.spine"

function loadSkeleton(atlasFile, jsonFile, x, y, scale, animation, skin)
	-- to load an atlas, we need to define a function that returns
	-- a Corona paint object. This allows you to resolve images
	-- however you see fit
	local imageLoader = function (path)
		local paint = { type = "image", filename = "data/" .. path }
		return paint
	end

	-- load the atlas
	local atlas = spine.TextureAtlas.new(spine.utils.readFile("data/" .. atlasFile), imageLoader)

	-- load the JSON and create a Skeleton from it
	local json = spine.SkeletonJson.new(spine.AtlasAttachmentLoader.new(atlas))
	json.scale = scale
	local skeletonData = json:readSkeletonDataFile("data/" .. jsonFile)
	local skeleton = spine.Skeleton.new(skeletonData)
	skeleton.scaleY = -1 -- Corona's coordinate system has its y-axis point downwards
	skeleton.group.x = x
	skeleton.group.y = y

	-- Set the skin if we got one
	if skin then skeleton:setSkin(skin) end

	-- create an animation state object to apply animations to the skeleton
	local animationStateData = spine.AnimationStateData.new(skeletonData)
	animationStateData.defaultMix = 0.5
	local animationState = spine.AnimationState.new(animationStateData)

	-- set a name on the group of the skeleton so we can find it during debugging
	skeleton.group.name = jsonFile

	-- set some event callbacks
	animationState.onStart = function (entry)
		print(entry.trackIndex.." start: "..entry.animation.name)
	end
	animationState.onInterrupt = function (entry)
		print(entry.trackIndex.." interrupt: "..entry.animation.name)
	end
	animationState.onEnd = function (entry)
		print(entry.trackIndex.." end: "..entry.animation.name)
	end
	animationState.onComplete = function (entry)
		print(entry.trackIndex.." complete: "..entry.animation.name)
	end
	animationState.onDispose = function (entry)
		print(entry.trackIndex.." dispose: "..entry.animation.name)
	end
	animationState.onEvent = function (entry, event)
		print(entry.trackIndex.." event: "..entry.animation.name..", "..event.data.name..", "..event.intValue..", "..event.floatValue..", '"..(event.stringValue or "").."'" .. ", " .. event.volume .. ", " .. event.balance)
	end

	-- return the skeleton an animation state
	return { skeleton = skeleton, state = animationState }
end

local lastTime = 0
local result = loadSkeleton("spineboy.atlas", "spineboy-pro.json", 240, 300, 0.4, "walk")
local skeleton = result.skeleton;
local state = result.state;

state:setAnimationByName(0, "idle", true)
state:setAnimationByName(5, "shoot", true, 0)

display.setDefault("background", 0.2, 0.2, 0.2, 1)

Runtime:addEventListener("enterFrame", function (event)
	local currentTime = event.time / 1000
	local delta = currentTime - lastTime
	lastTime = currentTime

	state:update(delta)
	state:apply(skeleton)
	skeleton:updateWorldTransform()
end)