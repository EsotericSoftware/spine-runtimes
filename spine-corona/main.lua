require("mobdebug").start()

-- require "examples.spineboy.spineboy"
-- require "examples.spineboy-atlas.spineboy"
-- require "examples.spineboy.spineboy-mesh"
-- require "examples.goblins.goblins"
-- require "examples.dragon.dragon"
-- require "examples.hero.hero"

local spine = require "spine-corona.spine-corona"

local QUAD_TRIANGLES = { 1, 2, 3, 3, 4, 1 }
spine.Skeleton.new_super = spine.Skeleton.new
spine.Skeleton.updateWorldTransform_super = spine.Skeleton.updateWorldTransform
spine.Skeleton.new = function(skeletonData, group)
  self = spine.Skeleton.new_super(skeletonData)
  self.parentGroup = group or display.newGroup()
  self.drawingGroup = display.newGroup()
  self.parentGroup:insert(self.drawingGroup)
  self.premultipliedAlpha = false
  
  return self
end

function spine.Skeleton:updateWorldTransform()
  spine.Skeleton.updateWorldTransform_super(self)
  local premultipliedAlpha = self.premultipliedAlpha
  
  -- Remove old drawing group, we will start anew
  self.drawingGroup:removeSelf()
  local drawingGroup = display.newGroup()
  self.drawingGroup = drawingGroup
  self.drawingGroup.parent = self.parentGroup
  
  local drawOrder = self.drawOrder
  local currentGroup = nil
  local groupVertices = {}
  local groupIndices = {}
  local groupUvs = {}
  local texture = nil
  local lastTexture = nil
  for i,slot in ipairs(drawOrder) do
    local attachment = slot.attachment
    local vertices = nil
    local indices = nil
    if attachment then
      if attachment.type == spine.AttachmentType.region then
        vertices = attachment:updateWorldVertices(slot, premultipliedAlpha)
        indices = QUAD_TRIANGLES
        texture = attachment.region.renderObject.texture
      elseif attachment.type == spine.AttachmentType.mesh then
        vertices = attachment:updateWorldVertices(slot, premultipliedAlpha)
        indices = attachment.triangles
        texture = attachment.region.renderObject.texture
      end
      
      if texture then
        if texture ~= lastTexture and lastTexture then -- FIXME need to take color and blend mode into account
          self:flush(groupVertices, groupUvs, groupIndices, texture, drawingGroup)
          lastTexture = texture
          groupVertices = {}
          groupUvs = {}
          groupIndices = {}
        end
      
        self:batch(vertices, indices, groupVertices, groupUvs, groupIndices)
      end
    end
  end
  
  if #groupVertices > 0 then
    self:flush(groupVertices, groupUvs, groupIndices, texture, drawingGroup)
  end
end

function spine.Skeleton:flush(groupVertices, groupUvs, groupIndices, texture, drawingGroup)
  mesh = display.newMesh(drawingGroup, 0, 0, {
      mode = "indexed",
      vertices = groupVertices,
      uvs = groupUvs,
      indices = groupIndices
  })
  mesh.fill = texture
  mesh:translate(mesh.path:getVertexOffset())
end

function spine.Skeleton:batch(vertices, indices, groupVertices, groupUvs, groupIndices)
  local numIndices = #indices
  local i = 1
  local indexStart = #groupIndices + 1
  local offset = #groupVertices / 2
  local indexEnd = indexStart + numIndices

  while indexStart < indexEnd do
    groupIndices[indexStart] = indices[i] + offset
    indexStart = indexStart + 1
    i = i + 1
  end
  
  i = 1
  local numVertices = #vertices
  local vertexStart = #groupVertices + 1
  local vertexEnd = vertexStart + numVertices / 4
  while vertexStart < vertexEnd do
    groupVertices[vertexStart] = vertices[i]
    groupVertices[vertexStart+1] = vertices[i+1]
    groupUvs[vertexStart] = vertices[i+2]
    groupUvs[vertexStart+1] = vertices[i+3]
    vertexStart = vertexStart + 2
    i = i + 8
    -- FIXME color
  end
end

local loader = function (path) 
  local paint = { type = "image", filename = "data/" .. path }
  return paint
end
local atlas = spine.TextureAtlas.new(spine.utils.readFile("data/spineboy.atlas"), loader)
local json = spine.SkeletonJson.new(spine.TextureAtlasAttachmentLoader.new(atlas))
json.scale = 0.3
local skeletonData = json:readSkeletonDataFile("data/spineboy.json")

local skeletonGroup = display.newGroup()
skeletonGroup.name = "muhhh"
skeletonGroup.y = 100
local skeleton = spine.Skeleton.new(skeletonData, skeletonGroup)
skeleton.flipY = true
local animationStateData = spine.AnimationStateData.new(skeletonData)
local animation = spine.AnimationState.new(animationStateData)
animation:setAnimationByName(0, "walk", true)

local img = display.newImage("data/spineboy.png")
img.width = 100
img.height = 100

display.setDefault("background", 0.2, 0.2, 0.2, 1)

local lastTime = 0
Runtime:addEventListener("enterFrame", function (event)        
	local currentTime = event.time / 1000
	local delta = currentTime - lastTime
	lastTime = currentTime
  
  animation:update(delta)
  animation:apply(skeleton)
  skeleton:updateWorldTransform()
end)
    
Runtime:addEventListener("touch", function(event)
    skeletonGroup.x = event.x
    skeletonGroup.y = event.y
    img.x = event.x
    img.y = event.y
end)