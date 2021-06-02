-------------------------------------------------------------------------------
-- Spine Runtimes License Agreement
-- Last updated January 1, 2020. Replaces all prior versions.
--
-- Copyright (c) 2013-2020, Esoteric Software LLC
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
-- THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
-- EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
-- WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
-- DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
-- DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
-- (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
-- BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
-- ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
-- (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
-- THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local AttachmentType = require "spine-lua.attachments.AttachmentType"
local RegionAttachment = require "spine-lua.attachments.RegionAttachment"
local BoundingBoxAttachment = require "spine-lua.attachments.BoundingBoxAttachment"
local MeshAttachment = require "spine-lua.attachments.MeshAttachment"
local PathAttachment = require "spine-lua.attachments.PathAttachment"
local PointAttachment = require "spine-lua.attachments.PointAttachment"
local ClippingAttachment = require "spine-lua.attachments.ClippingAttachment"

local AttachmentLoader = {}
function AttachmentLoader.new ()
	local self = {}

	function self:newRegionAttachment (skin, name, path)
		return RegionAttachment.new(name)
	end

	function self:newMeshAttachment (skin, name, path)
		return MeshAttachment.new(name)
	end

	function self:newBoundingBoxAttachment (skin, name)
		return BoundingBoxAttachment.new(name)
	end

	function self:newPathAttachment(skin, name)
		return PathAttachment.new(name)
	end

	function self:newPointAttachment(skin, name)
		return PointAttachment.new(name)
	end

	function self:newClippingAttachment(skin, name)
		return ClippingAttachment.new(name)
	end

	return self
end
return AttachmentLoader
