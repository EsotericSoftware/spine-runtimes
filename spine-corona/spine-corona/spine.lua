-------------------------------------------------------------------------------
-- Spine Runtimes Software License v2.5
--
-- Copyright (c) 2013-2016, Esoteric Software
-- All rights reserved.
--
-- You are granted a perpetual, non-exclusive, non-sublicensable, and
-- non-transferable license to use, install, execute, and perform the Spine
-- Runtimes software and derivative works solely for personal or internal
-- use. Without the written permission of Esoteric Software (see Section 2 of
-- the Spine Software License Agreement), you may not (a) modify, translate,
-- adapt, or develop new applications using the Spine Runtimes or otherwise
-- create derivative works or improvements of the Spine Runtimes or (b) remove,
-- delete, alter, or obscure any trademarks or any copyright, trademark, patent,
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
--
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
-- USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
-- IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
-- ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
-- POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

spine = {}
local spine = spine

local spine_utils 					= require "spine-lua.utils"
spine.utils 						= spine_utils
local spine_SkeletonJson 			= require "spine-lua.SkeletonJson"
spine.SkeletonJson 					= spine_SkeletonJson
local spine_SkeletonData 			= require "spine-lua.SkeletonData"
spine.SkeletonData 					= spine_SkeletonData
local spine_BoneData 				= require "spine-lua.BoneData"
spine.BoneData 						= spine_BoneData
local spine_SlotData 				= require "spine-lua.SlotData"
spine.SlotData 						= spine_SlotData
local spine_IkConstraintData 		= require "spine-lua.IkConstraintData"
spine.IkConstraintData 				= spine_IkConstraintData
local spine_Skin 					= require "spine-lua.Skin"
spine.Skin 							= spine_Skin
local spine_Attachment 				= require "spine-lua.attachments.Attachment"
spine.Attachment 					= spine_Attachment
local spine_BoundingBoxAttachment 	= require "spine-lua.attachments.BoundingBoxAttachment"
spine.BoundingBoxAttachment 		= spine_BoundingBoxAttachment
local spine_RegionAttachment 		= require "spine-lua.attachments.RegionAttachment"
spine.RegionAttachment 				= spine_RegionAttachment
local spine_MeshAttachment 			= require "spine-lua.attachments.MeshAttachment"
spine.MeshAttachment 				= spine_MeshAttachment
local spine_VertexAttachment 		= require "spine-lua.attachments.VertexAttachment"
spine.VertexAttachment 				= spine_VertexAttachment
local spine_PathAttachment 			= require "spine-lua.attachments.PathAttachment"
spine.PathAttachment 				= spine_PathAttachment
local spine_Skeleton 				= require "spine-lua.Skeleton"
spine.Skeleton 						= spine_Skeleton
local spine_Bone 					= require "spine-lua.Bone"
spine.Bone 							= spine_Bone
local spine_Slot 					= require "spine-lua.Slot"
spine.Slot 							= spine_Slot
local spine_IkConstraint 			= require "spine-lua.IkConstraint"
spine.IkConstraint 					= spine_IkConstraint
local spine_AttachmentType 			= require "spine-lua.attachments.AttachmentType"
spine.AttachmentType 				= spine_AttachmentType
local spine_AttachmentLoader 		= require "spine-lua.AttachmentLoader"
spine.AttachmentLoader 				= spine_AttachmentLoader
local spine_Animation 				= require "spine-lua.Animation"
spine.Animation 					= spine_Animation
local spine_AnimationStateData 		= require "spine-lua.AnimationStateData"
spine.AnimationStateData 			= spine_AnimationStateData
local spine_AnimationState 			= require "spine-lua.AnimationState"
spine.AnimationState 				= spine_AnimationState
local spine_EventData 				= require "spine-lua.EventData"
spine.EventData 					= spine_EventData
local spine_Event 					= require "spine-lua.Event"
spine.Event 						= spine_Event
local spine_SkeletonBounds 			= require "spine-lua.SkeletonBounds"
spine.SkeletonBounds 				= spine_SkeletonBounds
local spine_BlendMode 				= require "spine-lua.BlendMode"
spine.BlendMode 					= spine_BlendMode
local spine_TextureAtlas 			= require "spine-lua.TextureAtlas"
spine.TextureAtlas 					= spine_TextureAtlas
local spine_TextureRegion 			= require "spine-lua.TextureRegion"
spine.TextureRegion 				= spine_TextureRegion
local spine_TextureAtlasRegion 		= require "spine-lua.TextureAtlasRegion"
spine.TextureAtlasRegion 			= spine_TextureAtlasRegion
local spine_AtlasAttachmentLoader 	= require "spine-lua.AtlasAttachmentLoader"
spine.AtlasAttachmentLoader 		= spine_AtlasAttachmentLoader
local spine_Color 					= require "spine-lua.Color"
spine.Color 						= spine_Color

local spine_AttachmentType_region 		= spine_AttachmentType.region
local spine_AttachmentType_mesh 		= spine_AttachmentType.mesh


local json = require "json"

--localizing functions
local system_ResourceDirectory 	= system.ResourceDirectory
local system_pathForFile 		= system.pathForFile
local io_open 					= io.open
local io_close 					= io.close
local json_decode 				= json.decode
local display_newGroup 			= display.newGroup
local display_newMesh			= display.newMesh


local coronaBlendModes = {
	[0] = "normal",
	[1] = "add",
	[2] = "multiply",
	[3] = "screen",
}


spine_utils.readFile = function (fileName, base)
	if not base then base = system_ResourceDirectory end
	local path = system_pathForFile(fileName, base)
	local file = io_open(path, "r")
	if not file then return nil end
	local contents = file:read("*a")
	io_close(file)
	return contents
end

spine_utils.readJSON = function (text)
	return json_decode(text)
end

local QUAD_TRIANGLES = { 1, 2, 3, 3, 4, 1 }
local spine_Skeleton_new_super = spine_Skeleton.new
spine_Skeleton.new_super = spine_Skeleton_new_super
spine_Skeleton.updateWorldTransform_super = spine_Skeleton.updateWorldTransform

spine_Skeleton.new = function(skeletonData, group)
	self = spine_Skeleton_new_super(skeletonData)
	self.group = group or display_newGroup()
	self.drawingGroup = nil
	self.premultipliedAlpha = false
	self.batches = 0
	return self
end


local function colorEquals(color1, color2)
	if not color1 and not color2 then return true end
	if not color1 and color2 then return false end
	if color1 and not color2 then return false end
	return color1[1] == color2[1] and color1[2] == color2[2] and color1[3] == color2[3] and color1[4] == color2[4]
end


function spine_Skeleton:updateWorldTransform()
	spine_Skeleton.updateWorldTransform_super(self)
	local premultipliedAlpha = self.premultipliedAlpha

	self.batches = 0

	-- Remove old drawing group, we will start anew
	if self.drawingGroup then self.drawingGroup:removeSelf() end
	local drawingGroup = display_newGroup()
	self.drawingGroup = drawingGroup
	self.group:insert(drawingGroup)

	local drawOrder 	= self.drawOrder
	local currentGroup 	= nil
	local groupVertices = {}
	local groupIndices 	= {}
	local groupUvs 		= {}
	local color 		= nil
	local lastColor 	= nil
	local texture 		= nil
	local lastTexture 	= nil
	local blendMode 	= nil
	local lastBlendMode = nil

	for i=1, #drawOrder do
		local slot = drawOrder[i]
		local attachment 	= slot.attachment
		local vertices 		= nil
		local indices 		= nil

		if attachment then
			local attachment_type = attachment.type
			if attachment_type == spine_AttachmentType_region then
				vertices 	= attachment:updateWorldVertices(slot, premultipliedAlpha)
				indices 	= QUAD_TRIANGLES
				texture 	= attachment.region.renderObject.texture
				color 		= { vertices[5], vertices[6], vertices[7], vertices[8]}
				blendMode 	= coronaBlendModes[slot.data.blendMode]

			elseif attachment_type == spine_AttachmentType_mesh then
				vertices 	= attachment:updateWorldVertices(slot, premultipliedAlpha)
				indices 	= attachment.triangles
				texture 	= attachment.region.renderObject.texture
				color 		= { vertices[5], vertices[6], vertices[7], vertices[8] }
				blendMode 	= coronaBlendModes[slot.data.blendMode]
			end

			if texture and vertices and indices then
				if not lastTexture then lastTexture = texture end
				if not lastColor then lastColor = color end
				if not lastBlendMode then lastBlendMode = blendMode end

				if (texture ~= lastTexture or not colorEquals(color, lastColor) or blendMode ~= lastBlendMode) then
					self:flush(groupVertices, groupUvs, groupIndices, lastTexture, lastColor, lastBlendMode, drawingGroup)
					lastTexture = texture
					lastColor = color
					lastBlendMode = blendMode
					groupVertices = {}
					groupUvs = {}
					groupIndices = {}
				end

				self:batch(vertices, indices, groupVertices, groupUvs, groupIndices)
			end
		end
	end

	if #groupVertices > 0 then
		self:flush(groupVertices, groupUvs, groupIndices, texture, color, blendMode, drawingGroup)
	end
end

function spine_Skeleton:flush(groupVertices, groupUvs, groupIndices, texture, color, blendMode, drawingGroup)
	mesh = display_newMesh(drawingGroup, 0, 0, {
			mode 		= "indexed",
			vertices 	= groupVertices,
			uvs 		= groupUvs,
			indices 	= groupIndices
	})
	mesh.fill = texture
	mesh:setFillColor(color[1], color[2], color[3])
	mesh.alpha = color[4]
	mesh.blendMode = blendMode
	mesh:translate(mesh.path:getVertexOffset())
	self.batches = self.batches + 1
end

function spine_Skeleton:batch(vertices, indices, groupVertices, groupUvs, groupIndices)
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
	end
end

return spine
