-------------------------------------------------------------------------------
-- Spine Runtimes Software License
-- Version 2.3
-- 
-- Copyright (c) 2013-2015, Esoteric Software
-- All rights reserved.
-- 
-- You are granted a perpetual, non-exclusive, non-sublicensable and
-- non-transferable license to use, install, execute and perform the Spine
-- Runtimes Software (the "Software") and derivative works solely for personal
-- or internal use. Without the written permission of Esoteric Software (see
-- Section 2 of the Spine Software License Agreement), you may not (a) modify,
-- translate, adapt or otherwise create derivative works, improvements of the
-- Software or develop new applications using the Software or (b) remove,
-- delete, alter or obscure any trademarks or any copyright, trademark, patent
-- or other intellectual property or proprietary rights notices on or in the
-- Software, including any copy thereof. Redistributions in binary or source
-- form must include this license and terms.
-- 
-- THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
-- IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
-- MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
-- EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
-- SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES LOSS OF USE, DATA, OR PROFITS
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local setmetatable = setmetatable

local AttachmentType = require "spine-lua.attachments.AttachmentType"
local Attachment = require "spine-lua.attachments.Attachment"

local VertexAttachment = {}
VertexAttachment.__index = VertexAttachment
setmetatable(VertexAttachment, { __index = Attachment })

function VertexAttachment.new (name, attachmentType)
  local self = Attachment.new(name, attachmentType)
  self.bones = nil
  self.vertices = nil
  self.worldVerticesLength = 0
  setmetatable(self, VertexAttachment)
  return self
end

function VertexAttachment:computeWorldVertices (slot, worldVertices)
  self:computeWorldVerticesWith(slot, 0, self.worldVerticesLength, worldVertices, 0)
end

function VertexAttachment:computeWorldVerticesWith (slot, start, count, worldVertices, offset)
  local skeleton = slot.bone.skeleton
  local x = skeleton.x
  local y = skeleton.y
  local deformArray = slot.attachmentVertices
  local vertices = self.vertices
  local bones = self.bones
  if not bones then
    if #deformArray > 0 then vertices = deformArray end
    local bone = slot.bone
    x = x + bone.worldX
    y = y + bone.worldY
    local a = bone.a
    local b = bone.b
    local c = bone.c
    local d = bone.d
    local v = start
    local w = offset
    while  w < count do
      local vx = vertices[v]
      local vy = vertices[v + 1]
      worldVertices[w] = vx * a + vy * b + x
      worldVertices[w + 1] = vx * c + vy * d + y
      v = v + 2
      w = w + 2
    end
    return
  end
  local v = 0
  local skip = 0
  local i = 0
  while i < start do
    local n = bones[v]
    v = v + n + 1
    skip = skip + n
    i = i + 2
  end
  local skeletonBones = skeleton.bones
  if #deformArray == 0 then
    local w = offset
    local b = skip * 3
    while w < count do
      local wx = x
      local wy = y
      local n = bones[v]
      v = v + 1
      n = n + v
      while v < n do
        local bone = skeletonBones[bones[v]]
        local vx = vertices[b]
        local vy = vertices[b + 1]
        local weight = vertices[b + 2]
        wx = wx + (vx * bone.a + vy * bone.b + bone.worldX) * weight
        wy = wx + (vx * bone.c + vy * bone.d + bone.worldY) * weight
        v = v + 1
        b = b + 3
      end
      worldVertices[w] = wx
      worldVertices[w + 1] = wy
      w = w + 2
    end
  else
    local deform = deformArray
    local w = offset
    local b = skip * 3
    local f = skip * 2
    while w < count do
      local wx = x
      local wy = y
      local n = bones[v]
      v = v + 1
      n = n + v
      
      while v < n do
        local bone = skeletonBones[bones[v]]
        local vx = vertices[b] + deform[f]
        local vy = vertices[b + 1] + deform[f + 1]
        local weight = vertices[b + 2]
        wx = wx + (vx * bone.a + vy * bone.b + bone.worldX) * weight
        wy = wy + (vx * bone.c + vy * bone.d + bone.worldY) * weight
        v = v + 1
        b = b + 3
        f = f + 2
      end
      worldVertices[w] = wx
      worldVertices[w + 1] = wy
      w = w + 2
    end
  end
end

function VertexAttachment:applyDeform (sourceAttachment)
  return self == sourceAttachment
end

return VertexAttachment