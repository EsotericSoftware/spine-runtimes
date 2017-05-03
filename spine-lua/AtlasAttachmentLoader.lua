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

local setmetatable = setmetatable

local AttachmentType = require "spine-lua.attachments.AttachmentType"
local RegionAttachment = require "spine-lua.attachments.RegionAttachment"
local BoundingBoxAttachment = require "spine-lua.attachments.BoundingBoxAttachment"
local MeshAttachment = require "spine-lua.attachments.MeshAttachment"
local PathAttachment = require "spine-lua.attachments.PathAttachment"
local PointAttachment = require "spine-lua.attachments.PointAttachment"
local ClippingAttachment = require "spine-lua.attachments.ClippingAttachment"
local TextureAtlas = require "spine-lua.TextureAtlas"

local AtlasAttachmentLoader = {}
AtlasAttachmentLoader.__index = AtlasAttachmentLoader

function AtlasAttachmentLoader.new (atlas)
	local self = {
		atlas = atlas
	}
	setmetatable(self, AtlasAttachmentLoader)
	return self
end

function AtlasAttachmentLoader:newRegionAttachment (skin, name, path)
	local region = self.atlas:findRegion(path)
	if not region then error("Region not found in atlas: " .. path .. " (region attachment: " .. name .. ")") end
	region.renderObject = region
	local attachment = RegionAttachment.new(name)
	attachment:setRegion(region)
	attachment.region = region
	return attachment
end

function AtlasAttachmentLoader:newMeshAttachment (skin, name, path)
	local region = self.atlas:findRegion(path)
	if not region then error("Region not found in atlas: " .. path .. " (mesh attachment: " .. name .. ")") end
	region.renderObject = region
	local attachment = MeshAttachment.new(name)
	attachment.region = region
	return attachment
end

function AtlasAttachmentLoader:newSkinningMeshAttachment (skin, name, path)
	return SkinningMeshAttachment.new(name)
end

function AtlasAttachmentLoader:newBoundingBoxAttachment (skin, name)
	return BoundingBoxAttachment.new(name)
end

function AtlasAttachmentLoader:newPathAttachment(skin, name)
	return PathAttachment.new(name)
end

function AtlasAttachmentLoader:newPointAttachment(skin, name)
	return PointAttachment.new(name)
end

function AtlasAttachmentLoader:newClippingAttachment(skin, name)
	return ClippingAttachment.new(name)
end

return AtlasAttachmentLoader
