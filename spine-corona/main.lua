require("mobdebug").start()

local spine = require "spine-corona.spine"

local skeletons = {}
local activeSkeleton = 1
local lastTime = 0

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
	skeleton.flipY = true -- Corona's coordinate system has its y-axis point downwards
	skeleton.group.x = x
	skeleton.group.y = y

	-- Set the skin if we got one
	if skin then skeleton:setSkin(skin) end

	-- create an animation state object to apply animations to the skeleton
	local animationStateData = spine.AnimationStateData.new(skeletonData)
	animationStateData.defaultMix = 0.2
	local animationState = spine.AnimationState.new(animationStateData)

	-- set the skeleton invisible
	skeleton.group.isVisible = false

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
		print(entry.trackIndex.." event: "..entry.animation.name..", "..event.data.name..", "..event.intValue..", "..event.floatValue..", '"..(event.stringValue or "").."'")
	end
	
  if atlasFile == "spineboy.atlas" then
    animationStateData:setMix("walk", "jump", 0.4)		
		animationStateData:setMix("jump", "run", 0.4);
		animationState:setAnimationByName(0, "walk", true)
		local jumpEntry = animationState:addAnimationByName(0, "jump", false, 3)
		animationState:addAnimationByName(0, "run", true, 0)
  else
    animationState:setAnimationByName(0, animation, true)
  end

	-- return the skeleton an animation state
	return { skeleton = skeleton, state = animationState }
end

table.insert(skeletons, loadSkeleton("spineboy.atlas", "spineboy.json", 240, 300, 0.4, "walk"))
table.insert(skeletons, loadSkeleton("raptor.atlas", "raptor.json", 200, 300, 0.25, "walk"))
table.insert(skeletons, loadSkeleton("goblins.atlas", "goblins-mesh.json", 240, 300, 0.8, "walk", "goblin"))
table.insert(skeletons, loadSkeleton("stretchyman.atlas", "stretchyman.json", 40, 300, 0.5, "sneak"))
table.insert(skeletons, loadSkeleton("tank.atlas", "tank.json", 400, 300, 0.2, "drive"))
table.insert(skeletons, loadSkeleton("vine.atlas", "vine.json", 240, 300, 0.3, "animation"))

display.setDefault("background", 0.2, 0.2, 0.2, 1)

Runtime:addEventListener("enterFrame", function (event)
	local currentTime = event.time / 1000
	local delta = currentTime - lastTime
	lastTime = currentTime

	skeleton = skeletons[activeSkeleton].skeleton
	skeleton.group.isVisible = true
	state = skeletons[activeSkeleton].state

	state:update(delta)
	state:apply(skeleton)
	skeleton:updateWorldTransform()

	-- uncomment if you want to know how many batches a skeleton renders to
	-- print(skeleton.batches)
end)

Runtime:addEventListener("key", function(event)
	if activeSkeleton == 2 and event.phase == "down" then
		state = skeletons[activeSkeleton].state
		state:setAnimationByName(0, "Jump", false)
		state:addAnimationByName(0, "walk", true, 0)
	end
	return false
end)

Runtime:addEventListener("tap", function(event)
	skeletons[activeSkeleton].skeleton.group.isVisible = false
	activeSkeleton = activeSkeleton + 1
	if activeSkeleton > #skeletons then activeSkeleton = 1 end
	skeletons[activeSkeleton].skeleton.group.isVisible = true
end)