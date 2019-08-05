-------------------------------------------------------------------------------
-- Spine Runtimes License Agreement
-- Last updated May 1, 2019. Replaces all prior versions.
--
-- Copyright (c) 2013-2019, Esoteric Software LLC
--
-- Integration of the Spine Runtimes into software or otherwise creating
-- derivative works of the Spine Runtimes is permitted under the terms and
-- conditions of Section 2 of the Spine Editor License Agreement:
-- http://esotericsoftware.com/spine-editor-license
--
-- Otherwise, it is permitted to integrate the Spine Runtimes into software
-- or otherwise create derivative works of the Spine Runtimes (collectively,
-- "Products"), provided that each user of the Products must obtain their own
-- Spine Editor license and redistribution of the Products in any form must
-- include this license and copyright notice.
--
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY EXPRESS
-- OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
-- OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
-- NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
-- INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
-- BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS
-- INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY
-- THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
-- NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
-- EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local spine = require "spine-love.spine"

local skeletons = {}
local activeSkeleton = 1
local swirl = spine.SwirlEffect.new(400)
local swirlTime = 0

function loadSkeleton (jsonFile, atlasFile, animation, skin, scale, x, y)
	local loader = function (path) return love.graphics.newImage("data/" .. path) end
	local atlas = spine.TextureAtlas.new(spine.utils.readFile("data/" .. atlasFile .. ".atlas"), loader)

	local json = spine.SkeletonJson.new(spine.AtlasAttachmentLoader.new(atlas))
	json.scale = scale
	local skeletonData = json:readSkeletonDataFile("data/" .. jsonFile .. ".json")
	local skeleton = spine.Skeleton.new(skeletonData)
	skeleton.x = x
	skeleton.y = y
	skeleton.scaleY = -1
	if skin then
		skeleton:setSkin(skin)
	end
	skeleton:setToSetupPose()
	
	local stateData = spine.AnimationStateData.new(skeletonData)
	local state = spine.AnimationState.new(stateData)
	state:setAnimationByName(0, animation, true)
	if (jsonFile == "spineboy-ess") then
		stateData:setMix("walk", "jump", 0.5)
		stateData:setMix("jump", "run", 0.5)
		state:addAnimationByName(0, "jump", false, 3)
		state:addAnimationByName(0, "run", true, 0)
	end
	
	if (jsonFile == "raptor-pro") then
		swirl.centerY = -200
		skeleton.vertexEffect = swirl
		-- skeleton.vertexEffect = spine.JitterEffect.new(10, 10)
	end
  
  if jsonFile == "mix-and-match-pro" then
    -- Create a new skin, by mixing and matching other skins
    -- that fit together. Items making up the girl are individual
    -- skins. Using the skin API, a new skin is created which is
    -- a combination of all these individual item skins.
    local skin = spine.Skin.new("mix-and-match")
    skin:addSkin(skeletonData:findSkin("skin-base"))
    skin:addSkin(skeletonData:findSkin("nose/short"))
    skin:addSkin(skeletonData:findSkin("eyelids/girly"))
    skin:addSkin(skeletonData:findSkin("eyes/violet"))
    skin:addSkin(skeletonData:findSkin("hair/brown"))
    skin:addSkin(skeletonData:findSkin("clothes/hoodie-orange"))
    skin:addSkin(skeletonData:findSkin("legs/pants-jeans"))
    skin:addSkin(skeletonData:findSkin("accessories/bag"))
    skin:addSkin(skeletonData:findSkin("accessories/hat-red-yellow"))
    skeleton:setSkinByReference(skin)
  end
	
	-- set some event callbacks
	state.onStart = function (entry)
		print(entry.trackIndex.." start: "..entry.animation.name)
	end
	state.onInterrupt = function (entry)
		print(entry.trackIndex.." interrupt: "..entry.animation.name)
	end
	state.onEnd = function (entry)
		print(entry.trackIndex.." end: "..entry.animation.name)
	end
	state.onComplete = function (entry)
		print(entry.trackIndex.." complete: "..entry.animation.name)
	end
	state.onDispose = function (entry)
		print(entry.trackIndex.." dispose: "..entry.animation.name)
	end
	state.onEvent = function (entry, event)
		print(entry.trackIndex.." event: "..entry.animation.name..", "..event.data.name..", "..event.intValue..", "..event.floatValue..", '"..(event.stringValue or "").."'" .. ", " .. event.volume .. ", " .. event.balance)
	end
	
	state:update(0.5)
	state:apply(skeleton)
	
	return { state = state, skeleton = skeleton }
end

function love.load(arg)
	if arg[#arg] == "-debug" then require("mobdebug").start() end
	skeletonRenderer = spine.SkeletonRenderer.new(true)
  table.insert(skeletons, loadSkeleton("mix-and-match-pro", "mix-and-match", "dance", nil, 0.5, 400, 500))
	table.insert(skeletons, loadSkeleton("spineboy-pro", "spineboy", "walk", nil, 0.5, 400, 500))
	table.insert(skeletons, loadSkeleton("stretchyman-pro", "stretchyman", "sneak", nil, 0.5, 200, 500))
	table.insert(skeletons, loadSkeleton("coin-pro", "coin", "animation", nil, 0.5, 400, 300))
	table.insert(skeletons, loadSkeleton("raptor-pro", "raptor", "walk", nil, 0.3, 400, 500))
	table.insert(skeletons, loadSkeleton("goblins-pro", "goblins", "walk", "goblin", 1, 400, 500))
	table.insert(skeletons, loadSkeleton("tank-pro", "tank", "drive", nil, 0.2, 600, 500))
	table.insert(skeletons, loadSkeleton("vine-pro", "vine", "grow", nil, 0.3, 400, 500))
end

function love.update (delta)
	-- Update the state with the delta time, apply it, and update the world transforms.
	local state = skeletons[activeSkeleton].state
	local skeleton = skeletons[activeSkeleton].skeleton
	state:update(delta)
	state:apply(skeleton)
	skeleton:updateWorldTransform()
	
	if (skeleton.vertexEffect) then
		skeletonRenderer.vertexEffect = skeleton.vertexEffect
		if (skeleton.vertexEffect == swirl) then
			swirlTime = swirlTime + delta
			local percent = swirlTime % 2
			if (percent > 1) then percent = 1 - (percent - 1) end
			swirl.angle = spine.Interpolation.apply(spine.Interpolation.pow2, -60, 60, percent)
		end
	else
		skeletonRenderer.vertexEffect = nil
	end
end

function love.draw ()
	love.graphics.setBackgroundColor(0, 0, 0, 255)
	love.graphics.setColor(255, 255, 255)
	local skeleton = skeletons[activeSkeleton].skeleton
	
	skeletonRenderer:draw(skeleton)
end

function love.mousepressed (x, y, button, istouch)
	activeSkeleton = activeSkeleton + 1
	if activeSkeleton > #skeletons then activeSkeleton = 1 end
end
