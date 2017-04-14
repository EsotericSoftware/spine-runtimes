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
-- PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS Owelp,F
-- USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
-- IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
-- ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
-- POSSIBILITY OF SUCH DAMAGE.
-------------------------------------------------------------------------------

-- FIXME
-- for performance increase zlen() was replace by (1 + #table) as this is faster as a function call (but rewrite would be best) Additionally the value has been localized for all occasions of (1 + #frames)

local utils 			= require "spine-lua.utils"
local AttachmentType 	= require "spine-lua.attachments.AttachmentType"

local utils_newNumberArrayZero = utils.newNumberArrayZero
local utils_clamp = utils.clamp

local AttachmentType_mesh 			= AttachmentType.mesh
local AttachmentType_linkedmesh 	= AttachmentType.linkedmesh
local AttachmentType_path 			= AttachmentType.path
local AttachmentType_boundingbox 	= AttachmentType.boundingbox 


local setmetatable 	= setmetatable
local type 			= type
local math_floor 	= math.floor
local math_abs 		= math.abs
local math_signum 	= utils.signum

local Animation = {}
Animation.__index = Animation

function Animation.new (name, timelines, duration)
	if not timelines then error("timelines cannot be nil", 2) end

	local self = {
		name = name,
		timelines = timelines,
		duration = duration
	}
	setmetatable(self, Animation)

	return self
end


function Animation:apply (skeleton, lastTime, time, loop, events, alpha, setupPose, mixingOut)
	if not skeleton then error("skeleton cannot be nil.", 2) end

	local duration = self.duration
	if loop and duration > 0 then
		time = time % duration
		if lastTime > 0 then lastTime = lastTime % duration end
	end

	local timelines = self.timelines
	for i=1, #timelines do
		timelines[i]:apply(skeleton, lastTime, time, events, alpha, setupPose, mixingOut)
	end
end


local function binarySearch (values, target, step)
	local low = 0
	local high = math_floor((1 + #values) / step - 2)
	if high == 0 then return step end
	local current = math_floor(high / 2)
	while true do
		if values[(current + 1) * step] <= target then
			low = current + 1
		else
			high = current
		end
		if low == high then return (low + 1) * step end
		current = math_floor((low + high) / 2)
	end
end
Animation.binarySearch = binarySearch

local function binarySearch1 (values, target)
	local low = 0
	local high = math_floor((1 + #values)	- 2)
	if high == 0 then return 1 end
	local current = math_floor(high / 2)
	while true do
		if values[current + 1] <= target then
			low = current + 1
		else
			high = current
		end
		if low == high then return low + 1 end
		current = math_floor((low + high) / 2)
	end
end

local function linearSearch (values, target, step)
	local i = 0
	local last = (1 + #values) - step
	while i <= last do
		if (values[i] > target) then return i end
		i = i + step
	end
	return -1
end


Animation.TimelineType = {
	rotate 					= 0, 
	translate 				= 1, 
	scale 					= 2, 
	shear 					= 3,
	attachment 				= 4, 
	color 					= 5, 
	deform 					= 6,
	event 					= 7, 
	drawOrder 				= 8,
	ikConstraint 			= 9, 
	transformConstraint 	= 10,
	pathConstraintPosition 	= 11, 
	pathConstraintSpacing 	= 12, 
	pathConstraintMix 		= 13
}
local TimelineType = Animation.TimelineType
local SHL_24 = 16777216



local CurveTimeline = {}
CurveTimeline.__index = CurveTimeline
Animation.CurveTimeline = CurveTimeline

local LINEAR 		= 0
local STEPPED 		= 1
local BEZIER 		= 2
local BEZIER_SIZE 	= 10 * 2 - 1

function CurveTimeline.new (frameCount)

	local self = {
		curves = utils_newNumberArrayZero((frameCount - 1) * BEZIER_SIZE) -- type, x, y, ...
	}
	setmetatable(self, CurveTimeline)

	return self
end
local CurveTimeline_new = CurveTimeline.new


function CurveTimeline:getFrameCount ()
	return math_floor((1 + #self.curves) / BEZIER_SIZE) + 1
end

function CurveTimeline:setStepped (frameIndex)
	self.curves[frameIndex * BEZIER_SIZE] = STEPPED
end

function CurveTimeline:getCurveType (frameIndex)
	local index 	= frameIndex * BEZIER_SIZE
	local curves 	= self.curves
	if index == (1 + #curves) then return LINEAR end
	local curveType = curves[index]
	if curveType == LINEAR then return LINEAR end
	if curveType == STEPPED then return STEPPED end
	return BEZIER
end

function CurveTimeline:setCurve (frameIndex, cx1, cy1, cx2, cy2)
	local tmpx 	= (-cx1 * 2 + cx2) * 0.03
	local tmpy 	= (-cy1 * 2 + cy2) * 0.03
	local dddfx = ((cx1 - cx2) * 3 + 1) * 0.006
	local dddfy = ((cy1 - cy2) * 3 + 1) * 0.006
	local ddfx 	= tmpx * 2 + dddfx
	local ddfy 	= tmpy * 2 + dddfy
	local dfx 	= cx1 * 0.3 + tmpx + dddfx * 0.16666667
	local dfy 	= cy1 * 0.3 + tmpy + dddfy * 0.16666667

	local i = frameIndex * BEZIER_SIZE
	local curves = self.curves
	curves[i] = BEZIER
	i = i + 1

	local x = dfx
	local y = dfy
	local n = i + BEZIER_SIZE - 1
	while i < n do
		curves[i] 		= x
		curves[i + 1] 	= y
		dfx 			= dfx + ddfx
		dfy 			= dfy + ddfy
		ddfx 			= ddfx + dddfx
		ddfy 			= ddfy + dddfy
		x 				= x + dfx
		y 				= y + dfy
		i 				= i + 2
	end
end

function CurveTimeline:getCurvePercent (frameIndex, percent)
	percent = utils_clamp(percent, 0, 1)
	local curves 	= self.curves
	local i 		= frameIndex * BEZIER_SIZE
	local curveType = curves[i]
	if curveType == LINEAR then return percent end
	if curveType == STEPPED then return 0 end
	i = i + 1	
	local x
	local n = i + BEZIER_SIZE - 1
	local start = i
	while i < n do
		x = curves[i]
		if x >= percent then
			local prevX, prevY
			if i == start then
				prevX = 0
				prevY = 0
			else
				prevX = curves[i - 2]
				prevY = curves[i - 1]
			end
			return prevY + (curves[i + 1] - prevY) * (percent - prevX) / (x - prevX)
		end
		i = i + 2
	end
	local y = curves[i - 1]
	return y + (1 - y) * (percent - x) / (1 - x) -- Last point is 1,1.
end


local rotateENTRIES 		= 2
local rotatePREV_TIME 		= -2
local rotatePREV_ROTATION 	= -1
local rotateROTATION 		= 1

local TimelineType_rotate = TimelineType.rotate

local RotateTimeline = {
	ENTRIES 		= rotateENTRIES,
	PREV_TIME 		= rotatePREV_TIME,
	PREV_ROTATION 	= rotatePREV_ROTATION,
	ROTATION 		= rotateROTATION,
}
setmetatable(RotateTimeline, CurveTimeline)
RotateTimeline.__index = RotateTimeline
Animation.RotateTimeline = RotateTimeline

function RotateTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.boneIndex 	= -1
	self.frames 	= utils_newNumberArrayZero(frameCount * 2)
	self.type 		= TimelineType_rotate

	setmetatable(self, RotateTimeline)

	return self
end

function RotateTimeline:getPropertyId ()
	return TimelineType_rotate * SHL_24 + self.boneIndex
end

function RotateTimeline:setFrame (frameIndex, time, degrees)
	local frames = self.frames

	frameIndex = frameIndex * 2
	frames[frameIndex] = time
	frames[frameIndex + rotateROTATION] = degrees
end

function RotateTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local bone = skeleton.bones[self.boneIndex]
	if time < frames[0] then
		if setupPose then
			bone.rotation = bone.data.rotation
		end
		return
	end

	local frameCount = #frames + 1
	local bone_data_rotation = bone.data.rotation

	if time >= frames[frameCount - rotateENTRIES] then -- Time is after last frame.
		if setupPose then
			bone.rotation = bone_data_rotation + frames[frameCount + rotatePREV_ROTATION] * alpha
		else
			local bone_rotation = bone.rotation
			local r = bone_data_rotation + frames[frameCount + rotatePREV_ROTATION] - bone_rotation
			r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360 -- Wrap within -180 and 180.
			bone.rotation = bone_rotation + r * alpha;
		end
		return
	end

	-- Interpolate between the last frame and the current frame.
	local frame 		= binarySearch(frames, time, rotateENTRIES)
	local prevRotation 	= frames[frame + rotatePREV_ROTATION]
	local frameTime 	= frames[frame]
	local percent 		= self:getCurvePercent((math_floor(frame / 2)) - 1, 1 - (time - frameTime) / (frames[frame + rotatePREV_TIME] - frameTime))

	local r = frames[frame + rotateROTATION] - prevRotation
	r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
	r = prevRotation + r * percent
	if setupPose then
		r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
		bone.rotation = bone_data_rotation + r * alpha
	else
		local bone_rotation = bone.rotation

		r = bone_data_rotation + r - bone_rotation
		r = r - (16384 - math_floor(16384.499999999996 - r / 360)) * 360
		bone.rotation = bone_rotation + r * alpha
	end
end


local translateENTRIES 	= 3
local translatePREV_TIME = -3
local translatePREV_X 	= -2
local translatePREV_Y 	= -1
local translateX 		= 1
local translateY 		= 2

local TimelineType_translate = TimelineType.translate

local TranslateTimeline = {}
setmetatable(TranslateTimeline, CurveTimeline)
TranslateTimeline.__index = TranslateTimeline
Animation.TranslateTimeline = TranslateTimeline
TranslateTimeline.ENTRIES = translateENTRIES

function TranslateTimeline.new (frameCount)
	local self 		= CurveTimeline_new(frameCount)
	self.frames 	= utils_newNumberArrayZero(frameCount * translateENTRIES)
	self.boneIndex 	= -1
	self.type 		= TimelineType_translate

	setmetatable(self, TranslateTimeline)

	return self
end
local TranslateTimeline_new = TranslateTimeline.new 


function TranslateTimeline:getPropertyId ()
	return TimelineType_translate * SHL_24 + self.boneIndex
end

function TranslateTimeline:setFrame (frameIndex, time, x, y)
	local frames = self.frames
	frameIndex = frameIndex * translateENTRIES
	frames[frameIndex] = time
	frames[frameIndex + translateX] = x
	frames[frameIndex + translateY] = y
end

function TranslateTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local bone = skeleton.bones[self.boneIndex]
	local bone_data = bone.data
	if time < frames[0] then 
		if (setupPose) then
			bone.x = bone_data.x
			bone.y = bone_data.y
		end
		return
	end

	local frameCount = #frames + 1
	local bone_data_x, bone_data_y = bone_data.x, bone_data.y

	local x = 0
	local y = 0
	if time >= frames[frameCount - translateENTRIES] then -- // Time is after last frame.
		x = frames[frameCount + translatePREV_X];
		y = frames[frameCount + translatePREV_Y];
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, translateENTRIES)
		x = frames[frame + translatePREV_X]
		y = frames[frame + translatePREV_Y]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / translateENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + translatePREV_TIME] - frameTime));

		x = x + (frames[frame + translateX] - x) * percent
		y = y + (frames[frame + translateY] - y) * percent
	end
	if setupPose then
		bone.x = bone_data_x + x * alpha
		bone.y = bone_data_y + y * alpha
	else
		local bone_x, bone_y = bone.x, bone.y
		bone.x = bone_x + (bone_data_x + x - bone_x) * alpha
		bone.y = bone_y + (bone_data_y + y - bone_y) * alpha
	end
end


local scaleENTRIES 	= translateENTRIES
local scalePREV_TIME = -3
local scalePREV_X 	= -2
local scalePREV_Y 	= -1
local scaleX 		= 1
local scaleY 		= 2

local TimelineType_scale = TimelineType.scale

local ScaleTimeline = {}
setmetatable(ScaleTimeline, TranslateTimeline)
ScaleTimeline.__index = ScaleTimeline
Animation.ScaleTimeline = ScaleTimeline
ScaleTimeline.ENTRIES = scaleENTRIES

function ScaleTimeline.new (frameCount)
	local self = TranslateTimeline_new(frameCount)
	self.type = TimelineType_scale

	setmetatable(self, ScaleTimeline)

	return self
end

function ScaleTimeline:getPropertyId ()
	return TimelineType.scale * SHL_24 + self.boneIndex
end

function ScaleTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local bone = skeleton.bones[self.boneIndex]
	local bone_data = bone.data
	if time < frames[0] then
		if setupPose then
			bone.scaleX = bone_data.scaleX
			bone.scaleY = bone_data.scaleY
		end
		return
	end

	local frameCount = #frames + 1
	local bone_data_scaleX, bone_data_scaleY = bone_data.scaleX, bone_data.scaleY

	local x = 0
	local y = 0
	if time >= frames[frameCount - scaleENTRIES] then -- Time is after last frame.
		x = frames[frameCount + scalePREV_X] * bone_data_scaleX
		y = frames[frameCount + scalePREV_Y] * bone_data_scaleY
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, scaleENTRIES)
		x = frames[frame + scalePREV_X]
		y = frames[frame + scalePREV_Y]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / scaleENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + scalePREV_TIME] - frameTime))

		x = (x + (frames[frame + scaleX] - x) * percent) * bone_data_scaleX
		y = (y + (frames[frame + scaleY] - y) * percent) * bone_data_scaleY
	end
	if alpha == 1 then
		bone.scaleX = x
		bone.scaleY = y
	else
		local bx = 0
		local by = 0
		if setupPose then
			bx = bone_data_scaleX
			by = bone_data_scaleY
		else
			bx = bone.scaleX
			by = bone.scaleY
		end
		-- Mixing out uses sign of setup or current pose, else use sign of key.
		if mixingOut then
			x = math_abs(x) * math_signum(bx)
			y = math_abs(y) * math_signum(by)
		else
			bx = math_abs(bx) * math_signum(x)
			by = math_abs(by) * math_signum(y)
		end
		bone.scaleX = bx + (x - bx) * alpha
		bone.scaleY = by + (y - by) * alpha
	end
end



local shearENTRIES 	= translateENTRIES
local shearPREV_TIME = -3
local shearPREV_X 	= -2
local shearPREV_Y 	= -1
local shearX 		= 1
local shearY 		= 2

local TimelineType_shear = TimelineType.shear

local ShearTimeline = {}
setmetatable(ShearTimeline, TranslateTimeline)
ShearTimeline.__index = ShearTimeline
Animation.ShearTimeline = ShearTimeline
ShearTimeline.ENTRIES = shearENTRIES

function ShearTimeline.new (frameCount)
	local self = TranslateTimeline_new(frameCount)
	self.type = TimelineType.shear

	setmetatable(self, ShearTimeline)

	return self
end

function ShearTimeline:getPropertyId ()
	return TimelineType_shear * SHL_24 + self.boneIndex
end

function ShearTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local bone = skeleton.bones[self.boneIndex]
	local bone_data = bone.data
	if time < frames[0] then
		if setupPose then
			bone.shearX = bone_data.shearX
			bone.shearY = bone_data.shearY
		end
		return
	end

	local frameCount = #frames + 1
	local bone_data_shearX, bone_data_shearY = bone_data.shearX, bone_data.shearY

	local x = 0
	local y = 0
	if time >= frames[frameCount - shearENTRIES] then -- // Time is after last frame.
		x = frames[frameCount + shearPREV_X]
		y = frames[frameCount + shearPREV_Y]
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, shearENTRIES)
		x = frames[frame + shearPREV_X]
		y = frames[frame + shearPREV_Y]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / shearENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + shearPREV_TIME] - frameTime))

		x = x + (frames[frame + shearX] - x) * percent
		y = y + (frames[frame + shearY] - y) * percent
	end

	if setupPose then
		bone.shearX = bone_data_shearX + x * alpha
		bone.shearY = bone_data_shearY + y * alpha
	else
		local bone_shearX, bone_shearY = bone.shearX, bone.shearY
		bone.shearX = bone_shearX + (bone_data_shearX + x - bone_shearX) * alpha
		bone.shearY = bone_shearY + (bone_data_shearY + y - bone_shearY) * alpha
	end
end


local colorENTRIES = 5
local colorPREV_TIME = -5
local colorPREV_R = -4
local colorPREV_G = -3
local colorPREV_B = -2
local colorPREV_A = -1
local colorR = 1
local colorG = 2
local colorB = 3
local colorA = 4

local TimelineType_color = TimelineType.color

local ColorTimeline = {}
setmetatable(ColorTimeline, CurveTimeline)
ColorTimeline.__index = ColorTimeline
Animation.ColorTimeline = ColorTimeline
ColorTimeline.ENTRIES = colorENTRIES

function ColorTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount * colorENTRIES)
	self.slotIndex = -1
	self.type = TimelineType_color

	setmetatable(self, ColorTimeline)

	return self
end


function ColorTimeline:getPropertyId ()
	return TimelineType_color * SHL_24 + self.slotIndex
end

function ColorTimeline:setFrame (frameIndex, time, r, g, b, a)
	local frames = self.frames
	frameIndex = frameIndex * colorENTRIES
	frames[frameIndex] = time
	frames[frameIndex + colorR] = r
	frames[frameIndex + colorG] = g
	frames[frameIndex + colorB] = b
	frames[frameIndex + colorA] = a
end
	
function ColorTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames
	local slot = skeleton.slots[self.slotIndex]
	local color = slot.color
	if time < frames[0] then 
		if setupPose then
			color:setFrom(slot.data.color)
		end
		return
	end

	local frameCount = #frames + 1

	local r, g, b, a
	if time >= frames[frameCount - colorENTRIES] then -- Time is after last frame.
		local i = frameCount
		r = frames[i + colorPREV_R]
		g = frames[i + colorPREV_G]
		b = frames[i + colorPREV_B]
		a = frames[i + colorPREV_A]
	else
		-- Interpolate between the last frame and the current frame.
		local frame = binarySearch(frames, time, colorENTRIES)
		r = frames[frame + colorPREV_R]
		g = frames[frame + colorPREV_G]
		b = frames[frame + colorPREV_B]
		a = frames[frame + colorPREV_A]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / colorENTRIES) - 1,
				1 - (time - frameTime) / (frames[frame + colorPREV_TIME] - frameTime))

		r = r + (frames[frame + colorR] - r) * percent
		g = g + (frames[frame + colorG] - g) * percent
		b = b + (frames[frame + colorB] - b) * percent
		a = a + (frames[frame + colorA] - a) * percent
	end
	if alpha == 1 then
		color:set(r, g, b, a)
	else
		if setupPose then color:setFrom(slot.data.color) end
		color:add((r - color.r) * alpha, (g - color.g) * alpha, (b - color.b) * alpha, (a - color.a) * alpha)
	end
end


local TimelineType_attachment = TimelineType.attachment

local Animation_AttachmentTimeline = {}
Animation.AttachmentTimeline = Animation_AttachmentTimeline

local AttachmentTimelineClass = {}
AttachmentTimelineClass.__index = AttachmentTimelineClass

function Animation_AttachmentTimeline.new (frameCount)
	local self = {
		frames = utils_newNumberArrayZero(frameCount), -- time, ...
		attachmentNames = {},
		slotIndex = -1,
		type = TimelineType_attachment
	}

	setmetatable(self, AttachmentTimelineClass)

	return self
end


function AttachmentTimelineClass:getFrameCount ()
	return (1 + #self.frames)
end

function AttachmentTimelineClass:setFrame (frameIndex, time, attachmentName)
	self.frames[frameIndex] = time
	self.attachmentNames[frameIndex] = attachmentName
end
	
function AttachmentTimelineClass:getPropertyId ()
	return TimelineType_attachment * SHL_24 + self.slotIndex
end

function AttachmentTimelineClass:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local slot = skeleton.slots[self.slotIndex]
	local attachmentName
	if mixingOut and setupPose then
		attachmentName = slot.data.attachmentName
		if not attachmentName then
			slot:setAttachment(nil)
		else
			slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
		end
		return
	end
		
	local frames = self.frames
	if time < frames[0] then 
		if setupPose then
			attachmentName = slot.data.attachmentName
			if not attachmentName then
				slot:setAttachment(nil)
			else
				slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
			end
		end
		return
	end

	local frameCount = #frames + 1

	local frameIndex = 0
	if time >= frames[frameCount - 1] then
		frameIndex = frameCount - 1
	else
		frameIndex = binarySearch(frames, time, 1) - 1
	end

	attachmentName = self.attachmentNames[frameIndex]
	if not attachmentName then
		slot:setAttachment(nil)
	else
		slot:setAttachment(skeleton:getAttachmentByIndex(self.slotIndex, attachmentName))
	end
end


local TimelineType_deform = TimelineType.deform

local DeformTimeline = {}
setmetatable(DeformTimeline, CurveTimeline)
DeformTimeline.__index = DeformTimeline
Animation.DeformTimeline = DeformTimeline

function DeformTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount)
	self.frameVertices = utils_newNumberArrayZero(frameCount)
	self.slotIndex = -1
	self.attachment = nil
	self.type = TimelineType_deform

	setmetatable(self, DeformTimeline)

	return self
end

function DeformTimeline:getPropertyId ()
	return TimelineType_deform * SHL_24 + self.slotIndex
end

function DeformTimeline:setFrame (frameIndex, time, vertices)
	self.frames[frameIndex] = time
	self.frameVertices[frameIndex] = vertices
end

function DeformTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local slot = skeleton.slots[self.slotIndex]
	local slotAttachment = slot.attachment
	if not slotAttachment then return end
	local slotAttachment_type = slotAttachment.type
	if not (slotAttachment_type == AttachmentType_mesh or slotAttachment_type == AttachmentType_linkedmesh or slotAttachment_type == AttachmentType_path or slotAttachment_type == AttachmentType_boundingbox) then return end
	if not slotAttachment:applyDeform(self.attachment) then return end

	local frames = self.frames
	local verticesArray = slot.attachmentVertices
	if time < frames[0] then
		if setupPose then
			verticesArray = {}
			slot.attachmentVertices = verticesArray
		end
		return
	end

	local frameCount = #frames + 1
	local frameVertices = self.frameVertices
	local vertexCount = #(frameVertices[0])

	if (#verticesArray ~= vertexCount) then alpha = 1 end -- Don't mix from uninitialized slot vertices.
	local vertices = utils.setArraySize(verticesArray, vertexCount)

	if time >= frames[frameCount - 1] then -- Time is after last frame.
		local lastVertices = frameVertices[frameCount - 1]
		if alpha == 1 then
			-- Vertex positions or deform offsets, no alpha.
			local i = 1
			while i <= vertexCount do
				vertices[i] = lastVertices[i]
				i = i + 1
			end
		elseif setupPose then
			local vertexAttachment = slotAttachment
			if vertexAttachment.bones == nil then
				-- Unweighted vertex positions, with alpha.
				local setupVertices = vertexAttachment.vertices
				local i = 1
				while i <= vertexCount do
					local setup = setupVertices[i]
					vertices[i] = setup + (lastVertices[i] - setup) * alpha
					i = i + 1
				end
			else
				-- Weighted deform offsets, with alpha.
				local i = 1
				while i <= vertexCount do
					vertices[i] = lastVertices[i] * alpha
					i = i + 1
				end
			end
		else
			-- Vertex positions or deform offsets, with alpha.
			local i = 1
			while i <= vertexCount do
				vertices[i] = vertices[i] + (lastVertices[i] - vertices[i]) * alpha
				i = i + 1
			end
		end
		return
	end

	-- Interpolate between the previous frame and the current frame.
	local frame 		= binarySearch(frames, time, 1)
	local prevVertices 	= frameVertices[frame - 1]
	local nextVertices 	= frameVertices[frame]
	local frameTime 	= frames[frame]
	local percent 		= self:getCurvePercent(frame - 1, 1 - (time - frameTime) / (frames[frame - 1] - frameTime))

	if alpha == 1 then
		-- Vertex positions or deform offsets, no alpha.
		local i = 1
		while i <= vertexCount do
			local prev = prevVertices[i]
			vertices[i] = prev + (nextVertices[i] - prev) * percent
			i = i + 1
		end
	elseif setupPose then
		local vertexAttachment = slotAttachment
		if vertexAttachment.bones == nil then
			-- Unweighted vertex positions, with alpha.
			local setupVertices = vertexAttachment.vertices
			local i = 1
			while i <= vertexCount do
				local prev = prevVertices[i]
				local setup = setupVertices[i]
				vertices[i] = setup + (prev + (nextVertices[i] - prev) * percent - setup) * alpha
				i = i + 1
			end
		else
			-- Weighted deform offsets, with alpha.
			local i = 1
			while i <= vertexCount do
				local prev = prevVertices[i]
				vertices[i] = (prev + (nextVertices[i] - prev) * percent) * alpha
				i = i + 1
			end
		end
	else
		-- Vertex positions or deform offsets, with alpha.
		local i = 1
		while i <= vertexCount do
			local prev = prevVertices[i]
			vertices[i] = vertices[i] + (prev + (nextVertices[i] - prev) * percent - vertices[i]) * alpha
			i = i + 1
		end
	end
end


local TimelineType_event = TimelineType.event

local EventTimeline = {}
EventTimeline.__index = EventTimeline
Animation.EventTimeline = EventTimeline

function EventTimeline.new (frameCount)
	local self = {
		frames = utils_newNumberArrayZero(frameCount),
		events = {},
		type = TimelineType_event
	}

	setmetatable(self, EventTimeline)

	return self
end


function EventTimeline:getPropertyId ()
	return TimelineType_event * SHL_24
end

function EventTimeline:getFrameCount ()
	return (1 + #self.frames)
end

function EventTimeline:setFrame (frameIndex, event)
	self.frames[frameIndex] = event.time
	self.events[frameIndex] = event
end

-- Fires events for frames > lastTime and <= time.
function EventTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	if not firedEvents then return end

	local frames = self.frames
	local frameCount = (1 + #frames)

	if lastTime > time then -- Fire events after last time for looped animations.
		self:apply(skeleton, lastTime, 999999, firedEvents, alpha, setupPose, mixingOut)
		lastTime = -1
	elseif lastTime >= frames[frameCount - 1] then -- Last time is after last frame.
		return
	end
	if time < frames[0] then return end -- Time is before first frame.

	local frame
	if lastTime < frames[0] then
		frame = 0
	else
		frame = binarySearch1(frames, lastTime)
		local frame = frames[frame]
		while frame > 0 do -- Fire multiple events with the same frame.
			if frames[frame - 1] ~= frame then break end
			frame = frame - 1
		end
	end
	local events = self.events
	while frame < frameCount and time >= frames[frame] do
		firedEvents[#firedEvents + 1] = events[frame]
		frame = frame + 1
	end
end


local TimelineType_drawOrder = TimelineType.drawOrder

local DrawOrderTimeline = {}
DrawOrderTimeline.__index = DrawOrderTimeline
Animation.DrawOrderTimeline = DrawOrderTimeline

function DrawOrderTimeline.new (frameCount)
	local self = {
		frames = utils_newNumberArrayZero(frameCount),
		drawOrders = {},
		type = TimelineType_drawOrder
	}

	setmetatable(self, DrawOrderTimeline)

	return self
end

function DrawOrderTimeline:getPropertyId ()
	return TimelineType_drawOrder * SHL_24
end

function DrawOrderTimeline:getFrameCount ()
	return (1 + #self.frames)
end

function DrawOrderTimeline:setFrame (frameIndex, time, drawOrder)
	self.frames[frameIndex] = time
	self.drawOrders[frameIndex] = drawOrder
end

function DrawOrderTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local drawOrder = skeleton.drawOrder
	local slots = skeleton.slots
	if mixingOut and setupPose then
		for i=1, #slots do
			drawOrder[i] = slots[i]
		end
		return
	end
	local frames = self.frames
	if time < frames[0] then 
		if setupPose then
			for i=1, #slots do
				drawOrder[i] = slots[i]
			end
		end
		return
	end

	local frameCount = #frames + 1

	local frame
	if time >= frames[frameCount - 1] then -- Time is after last frame.
		frame = frameCount - 1
	else
		frame = binarySearch1(frames, time) - 1
	end

	local drawOrderToSetupIndex = self.drawOrders[frame]
	if not drawOrderToSetupIndex then
		for i=1, #slots do
			drawOrder[i] = slots[i]
		end
	else
		for i=1, #drawOrderToSetupIndex do
			drawOrder[i] = skeleton.slots[drawOrderToSetupIndex[i]]
		end
	end
end


local ikENTRIES = 3
local ikPREV_TIME = -3
local ikPREV_MIX = -2
local ikPREV_BEND_DIRECTION = -1
local ikMIX = 1
local ikBEND_DIRECTION = 2

local TimelineType_ikConstraint = TimelineType.ikConstraint

local IkConstraintTimeline = {}
setmetatable(IkConstraintTimeline, CurveTimeline)
IkConstraintTimeline.__index = IkConstraintTimeline
Animation.IkConstraintTimeline = IkConstraintTimeline
IkConstraintTimeline.ENTRIES = ikENTRIES

function IkConstraintTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount * ikENTRIES) -- time, mix, bendDirection, ...
	self.ikConstraintIndex = -1
	self.type = TimelineType_ikConstraint

	setmetatable(self, IkConstraintTimeline)

	return self
end

function IkConstraintTimeline:getPropertyId ()
	return TimelineType_ikConstraint * SHL_24 + self.ikConstraintIndex
end

function IkConstraintTimeline:setFrame (frameIndex, time, mix, bendDirection)
	frameIndex = frameIndex * ikENTRIES
	self.frames[frameIndex] = time
	self.frames[frameIndex + ikMIX] = mix
	self.frames[frameIndex + ikBEND_DIRECTION] = bendDirection
end

function IkConstraintTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local constraint = skeleton.ikConstraints[self.ikConstraintIndex]
	local constraint_data = constraint.data
	if time < frames[0] then
		if setupPose then
			constraint.mix = constraint_data.mix
			constraint.bendDirection = constraint_data.bendDirection
		end
		return
	end

	local frameCount = #frames + 1
	local constraint_data_bendDirection = constraint_data_bendDirection
	local constraint_data_mix = constraint_data.mix

	if time >= frames[frameCount - ikENTRIES] then -- Time is after last frame.
		if setupPose then
			constraint.mix = constraint_data_mix + (frames[frameCount + ikPREV_MIX] - constraint_data_mix) * alpha
			if mixingOut then 
				constraint.bendDirection = constraint_data_bendDirection
			else
				constraint.bendDirection = math_floor(frames[frameCount + ikPREV_BEND_DIRECTION]);
			end
		else
			constraint.mix = constraint.mix + (frames[frames.length + ikPREV_MIX] - constraint.mix) * alpha;
			if not mixingOut then constraint.bendDirection = math_floor(frames[frameCount + ikPREV_BEND_DIRECTION]) end
		end
		return
	end

	-- Interpolate between the previous frame and the current frame.
	local frame = binarySearch(frames, time, ikENTRIES)
	local mix = frames[frame + ikPREV_MIX]
	local frameTime = frames[frame]
	local percent = self:getCurvePercent(math_floor(frame / ikENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + ikPREV_TIME] - frameTime))

	if setupPose then
		constraint.mix = constraint_data_mix + (mix + (frames[frame + ikMIX] - mix) * percent - constraint_data_mix) * alpha
		if mixingOut then
			constraint.bendDirection = constraint_data_bendDirection
		else
			constraint.bendDirection = math_floor(frames[frame + ikPREV_BEND_DIRECTION])
		end
	else
		constraint.mix = constraint.mix + (mix + (frames[frame + ikMIX] - mix) * percent - constraint.mix) * alpha;
		if not mixingOut then constraint.bendDirection = math_floor(frames[frame + ikPREV_BEND_DIRECTION]) end
	end
end


local tConstraintENTRIES = 5
local tConstraintPREV_TIME = -5
local tConstraintPREV_ROTATE = -4
local tConstraintPREV_TRANSLATE = -3
local tConstraintPREV_SCALE = -2
local tConstraintPREV_SHEAR = -1
local tConstraintROTATE = 1
local tConstraintTRANSLATE = 2
local tConstraintSCALE = 3
local tConstraintSHEAR = 4

local TimelineType_transformConstraint = TimelineType.transformConstraint

local TransformConstraintTimeline = {}
setmetatable(TransformConstraintTimeline, CurveTimeline)
TransformConstraintTimeline.__index = TransformConstraintTimeline
Animation.TransformConstraintTimeline = TransformConstraintTimeline
TransformConstraintTimeline.ENTRIES = tConstraintENTRIES

function TransformConstraintTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount * tConstraintENTRIES)
	self.transformConstraintIndex = -1
	self.type = TimelineType_transformConstraint

	setmetatable(self, TransformConstraintTimeline)

	return self
end

function TransformConstraintTimeline:getPropertyId ()
	return TimelineType_transformConstraint * SHL_24 + self.transformConstraintIndex
end

function TransformConstraintTimeline:setFrame (frameIndex, time, rotateMix, translateMix, scaleMix, shearMix)
	local frames = self.frames
	frameIndex = frameIndex * tConstraintENTRIES
	frames[frameIndex] = time
	frames[frameIndex + tConstraintROTATE] = rotateMix
	frames[frameIndex + tConstraintTRANSLATE] = translateMix
	frames[frameIndex + tConstraintSCALE] = scaleMix
	frames[frameIndex + tConstraintSHEAR] = shearMix
end

function TransformConstraintTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local constraint = skeleton.transformConstraints[self.transformConstraintIndex]
	if time < frames[0] then
		if setupPose then
			local data = constraint.data
			constraint.rotateMix = data.rotateMix
			constraint.translateMix = data.translateMix
			constraint.scaleMix = data.scaleMix
			constraint.shearMix = data.shearMix
		end
		return
	end

	local frameCount = #frames + 1

	local rotate = 0
	local translate = 0
	local scale = 0
	local shear = 0
	if time >= frames[frameCount - tConstraintENTRIES] then -- Time is after last frame.
		local i = frameCount
		rotate = frames[i + tConstraintPREV_ROTATE]
		translate = frames[i + tConstraintPREV_TRANSLATE]
		scale = frames[i + tConstraintPREV_SCALE]
		shear = frames[i + tConstraintPREV_SHEAR]
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, tConstraintENTRIES)
		rotate = frames[frame + tConstraintPREV_ROTATE]
		translate = frames[frame + tConstraintPREV_TRANSLATE]
		scale = frames[frame + tConstraintPREV_SCALE]
		shear = frames[frame + tConstraintPREV_SHEAR]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / tConstraintENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + tConstraintPREV_TIME] - frameTime));

		rotate = rotate + (frames[frame + tConstraintROTATE] - rotate) * percent
		translate = translate + (frames[frame + tConstraintTRANSLATE] - translate) * percent
		scale = scale + (frames[frame + tConstraintSCALE] - scale) * percent
		shear = shear + (frames[frame + tConstraintSHEAR] - shear) * percent
	end
	if setupPose then
		local data = constraint.data
		local data_rotateMix = data.rotateMix
		local data_translateMix = data.translateMix
		local data_scaleMix = data.scaleMix
		local data_shearMix = data.shearMix
		constraint.rotateMix = data_rotateMix + (rotate - data_rotateMix) * alpha
		constraint.translateMix = data_translateMix + (translate - data_translateMix) * alpha
		constraint.scaleMix = data_scaleMix + (scale - data_scaleMix) * alpha
		constraint.shearMix = data_shearMix + (shear - data_shearMix) * alpha
	else
		local constraint_rotateMix = constraint.rotateMix
		local constraint_translateMix = constraint.translateMix
		local constraint_scaleMix = constraint.scaleMix
		local constraint_shearMix = constraint.shearMix
		constraint.rotateMix = constraint_rotateMix + (rotate - constraint_rotateMix) * alpha
		constraint.translateMix = constraint_translateMix + (translate - constraint_translateMix) * alpha
		constraint.scaleMix = constraint_scaleMix + (scale - constraint_scaleMix) * alpha
		constraint.shearMix = constraint_shearMix + (shear - constraint_shearMix) * alpha
	end
end



local pConstraintENTRIES = 2
local pConstraintPREV_TIME = -2
local pConstraintPREV_VALUE = -1
local pConstraintVALUE = 1

local TimelineType_pathConstraintPosition = TimelineType.pathConstraintPosition

local PathConstraintPositionTimeline = {}
setmetatable(PathConstraintPositionTimeline, CurveTimeline)
PathConstraintPositionTimeline.__index = PathConstraintPositionTimeline
Animation.PathConstraintPositionTimeline = PathConstraintPositionTimeline
PathConstraintPositionTimeline.ENTRIES = pConstraintENTRIES

function PathConstraintPositionTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount * pConstraintENTRIES)
	self.pathConstraintIndex = -1
	self.type = TimelineType_pathConstraintPosition

	setmetatable(self, PathConstraintPositionTimeline)

	return self
end

function PathConstraintPositionTimeline:getPropertyId ()
	return TimelineType_pathConstraintPosition * SHL_24 + self.pathConstraintIndex
end

function PathConstraintPositionTimeline:setFrame (frameIndex, time, value)
	local frames = self.frames
	frameIndex = frameIndex * pConstraintENTRIES
	frames[frameIndex] = time
	frames[frameIndex + pConstraintVALUE] = value
end

function PathConstraintPositionTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
	if (time < frames[0]) then
		if setupPose then
			constraint.position = constraint.data.position	
		end
		return
	end

	local frameCount = #frames + 1
	local constraint_data_position = constraint.data.position

	local position = 0
	if time >= frames[frameCount - pConstraintENTRIES] then -- Time is after last frame.
		position = frames[frameCount + pConstraintPREV_VALUE]
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, pConstraintENTRIES)
		position = frames[frame + pConstraintPREV_VALUE]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / pConstraintENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + pConstraintPREV_TIME] - frameTime))

		position = position + (frames[frame + pConstraintVALUE] - position) * percent
	end
	if setupPose then
		constraint.position = constraint_data_position + (position - constraint_data_position) * alpha
	else
		local constraint_position = constraint.position
		constraint.position = constraint_position + (position - constraint_position) * alpha
	end
end


local pcSpacingENTRIES = 2
local pcSpacingPREV_TIME = -2
local pcSpacingPREV_VALUE = -1
local pcSpacingVALUE = 1

local TimelineType_pathConstraintSpacing = TimelineType.pathConstraintSpacing

local PathConstraintSpacingTimeline = {}
setmetatable(PathConstraintSpacingTimeline, CurveTimeline)
PathConstraintSpacingTimeline.__index = PathConstraintSpacingTimeline
Animation.PathConstraintSpacingTimeline = PathConstraintSpacingTimeline
PathConstraintSpacingTimeline.ENTRIES = pcSpacingENTRIES

function PathConstraintSpacingTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount * pcSpacingENTRIES)
	self.pathConstraintIndex = -1
	self.type = TimelineType_pathConstraintSpacing

	setmetatable(self, PathConstraintSpacingTimeline)

	return self
end

function PathConstraintSpacingTimeline:getPropertyId ()
	return TimelineType.pathConstraintSpacing * SHL_24 + self.pathConstraintIndex
end

function PathConstraintSpacingTimeline:setFrame (frameIndex, time, value)
	frameIndex = frameIndex * pcSpacingENTRIES
	self.frames[frameIndex] = time
	self.frames[frameIndex + pcSpacingVALUE] = value
end

function PathConstraintSpacingTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
	if (time < frames[0]) then
		if setupPose then
			constraint.spacing = constraint.data.spacing
		end
		return
	end

	local frameCount = #frames + 1
	local constraint_data_spacing = constraint.data.spacing

	local spacing = 0
	if time >= frames[frameCount - pcSpacingENTRIES] then -- Time is after last frame.
		spacing = frames[frameCount + pcSpacingPREV_VALUE]
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, pcSpacingENTRIES)
		spacing = frames[frame + pcSpacingPREV_VALUE]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / pcSpacingENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + pcSpacingPREV_TIME] - frameTime))

		spacing = spacing + (frames[frame + pcSpacingVALUE] - spacing) * percent
	end

	if setupPose then
		constraint.spacing = constraint_data_spacing + (spacing - constraint_data_spacing) * alpha
	else
		local constraint_spacing = constraint.spacing 
		constraint.spacing = constraint_spacing + (spacing - constraint_spacing) * alpha
	end
end


local ENTRIES = 3
local PREV_TIME = -3
local PREV_ROTATE = -2
local PREV_TRANSLATE = -1
local ROTATE = 1
local TRANSLATE = 2

local TimelineType_pathConstraintMix = TimelineType.pathConstraintMix

local PathConstraintMixTimeline = {}
setmetatable(PathConstraintMixTimeline, CurveTimeline)
PathConstraintMixTimeline.__index = PathConstraintMixTimeline
Animation.PathConstraintMixTimeline = PathConstraintMixTimeline
Animation.PathConstraintMixTimeline.ENTRIES = ENTRIES

function PathConstraintMixTimeline.new (frameCount)
	local self = CurveTimeline_new(frameCount)
	self.frames = utils_newNumberArrayZero(frameCount * ENTRIES)
	self.pathConstraintIndex = -1
	self.type = TimelineType_pathConstraintMix

	setmetatable(self, PathConstraintMixTimeline)

	return self
end

function PathConstraintMixTimeline:getPropertyId ()
	return TimelineType_pathConstraintMix * SHL_24 + self.pathConstraintIndex
end

function PathConstraintMixTimeline:setFrame (frameIndex, time, rotateMix, translateMix)
	local frames = self.frames
	frameIndex = frameIndex * ENTRIES
	frames[frameIndex] = time
	frames[frameIndex + ROTATE] = rotateMix
	frames[frameIndex + TRANSLATE] = translateMix
end

function PathConstraintMixTimeline:apply (skeleton, lastTime, time, firedEvents, alpha, setupPose, mixingOut)
	local frames = self.frames

	local constraint = skeleton.pathConstraints[self.pathConstraintIndex]
	local constraint_data = constraint.data
	if (time < frames[0]) then
		if setupPose then
			constraint.rotateMix = constraint_data.rotateMix
			constraint.translateMix = constraint_data.translateMix
		end
		return
	end

	local frameCount = #frames + 1
	local constraint_data_rotateMix = constraint_data.rotateMix
	local constraint_data_translateMix = constraint_data.translateMix

	local rotate = 0
	local translate = 0
	if time >= frames[frameCount - ENTRIES] then -- Time is after last frame.
		rotate = frames[frameCount + PREV_ROTATE]
		translate = frames[frameCount + PREV_TRANSLATE]
	else
		-- Interpolate between the previous frame and the current frame.
		local frame = binarySearch(frames, time, ENTRIES)
		rotate = frames[frame + PREV_ROTATE]
		translate = frames[frame + PREV_TRANSLATE]
		local frameTime = frames[frame]
		local percent = self:getCurvePercent(math_floor(frame / ENTRIES) - 1,
			1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime))

		rotate = rotate + (frames[frame + ROTATE] - rotate) * percent
		translate = translate + (frames[frame + TRANSLATE] - translate) * percent
	end

	if setupPose then
		constraint.rotateMix = constraint_data_rotateMix + (rotate - constraint_data_rotateMix) * alpha
		constraint.translateMix = constraint_data_translateMix + (translate - constraint_data_translateMix) * alpha
	else
		local constraint_rotateMix = constraint.rotateMix
		local constraint_translateMix = constraint.translateMix
		constraint.rotateMix = constraint_rotateMix + (rotate - constraint_rotateMix) * alpha
		constraint.translateMix = constraint_translateMix + (translate - constraint_translateMix) * alpha
	end
end


return Animation