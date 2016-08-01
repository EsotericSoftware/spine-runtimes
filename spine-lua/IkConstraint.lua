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
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
-- OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
-- WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
-- OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
-- ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

local IkConstraint = {}
function IkConstraint.new (data, skeleton)
	if not data then error("data cannot be nil", 2) end
	if not skeleton then error("skeleton cannot be nil", 2) end

	local self = {
		data = data,
		skeleton = skeleton,
		bones = {},
		target = nil,
		bendDirection = data.bendDirection,
		mix = data.mix
	}

	function self:apply ()
		local target = self.target
		local bones = self.bones
		local boneCount = #bones
		if boneCount == 1 then
			IkConstraint.apply1(bones[1], target.worldX, target.worldY, self.mix)
		elseif boneCount == 2 then
			IkConstraint.apply2(bones[1], bones[2], target.worldX, target.worldY, self.bendDirection, self.mix)
		end
	end

	for i,boneData in ipairs(data.bones) do
		table.insert(self.bones, skeleton:findBone(boneData.name))
	end
	self.target = skeleton:findBone(data.target.name)

	return self
end

local radDeg = 180 / math.pi
local degRad = math.pi / 180

function IkConstraint.apply1 (bone, targetX, targetY, alpha)
	local parentRotation
	if not bone.data.inheritRotation or not bone.parent then
		parentRotation = 0
	else
		parentRotation = bone.parent.worldRotation
	end
	local rotation = bone.rotation
	local rotationIK = math.atan2(targetY - bone.worldY, targetX - bone.worldX) * radDeg
	if bone.worldFlipX ~= bone.worldFlipY then
		rotationIK = -rotationIK
	end
	rotationIK = rotationIK - parentRotation
	bone.rotationIK = rotation + (rotationIK - rotation) * alpha
end

local temp = {}

function IkConstraint.apply2 (parent, child, targetX, targetY, bendDirection, alpha)
	local childRotation = child.rotation
	local parentRotation = parent.rotation
	if not alpha then
		child.rotationIK = childRotation
		parent.rotationIK = parentRotation
		return
	end
	local positionX, positionY
	local tempPosition = temp
	local parentParent = parent.parent
	if parentParent then
		tempPosition[1] = targetX
		tempPosition[2] = targetY
		parentParent:worldToLocal(tempPosition)
		targetX = (tempPosition[1] - parent.x) * parentParent.worldScaleX
		targetY = (tempPosition[2] - parent.y) * parentParent.worldScaleY
	else
		targetX = targetX - parent.x
		targetY = targetY - parent.y
	end
	if child.parent == parent then
		positionX = child.x
		positionY = child.y
	else
		tempPosition[1] = child.x
		tempPosition[2] = child.y
		child.parent:localToWorld(tempPosition)
		parent:worldToLocal(tempPosition)
		positionX = tempPosition[1]
		positionY = tempPosition[2]
	end
	local childX = positionX * parent.worldScaleX
	local childY = positionY * parent.worldScaleY
	local offset = math.atan2(childY, childX)
	local len1 = math.sqrt(childX * childX + childY * childY)
	local len2 = child.data.length * child.worldScaleX
	-- Based on code by Ryan Juckett with permission: Copyright (c) 2008-2009 Ryan Juckett, http://www.ryanjuckett.com/
	local cosDenom = 2 * len1 * len2
	if cosDenom < 0.0001 then
		child.rotationIK = childRotation + (math.atan2(targetY, targetX) * radDeg - parentRotation - childRotation) * alpha
		return
	end
	local cos = (targetX * targetX + targetY * targetY - len1 * len1 - len2 * len2) / cosDenom
	if cos < -1 then
		cos = -1
	elseif cos > 1 then
		cos = 1
	end
	local childAngle = math.acos(cos) * bendDirection
	local adjacent = len1 + len2 * cos
	local opposite = len2 * math.sin(childAngle)
	local parentAngle = math.atan2(targetY * adjacent - targetX * opposite, targetX * adjacent + targetY * opposite)
	local rotation = (parentAngle - offset) * radDeg - parentRotation
	if rotation > 180 then
		rotation = rotation - 360
	elseif rotation < -180 then
		rotation = rotation + 360
	end
	parent.rotationIK = parentRotation + rotation * alpha
	rotation = (childAngle + offset) * radDeg - childRotation
	if rotation > 180 then
		rotation = rotation - 360
	elseif rotation < -180 then
		rotation = rotation + 360
	end
	child.rotationIK = childRotation + (rotation + parent.worldRotation - child.parent.worldRotation) * alpha
end

return IkConstraint
