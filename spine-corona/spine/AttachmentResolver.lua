
local AttachmentResolver = {
	failed = {}
}
function AttachmentResolver.new ()
	local self = {
		images = {}
	}

	function self:resolve (skeleton, attachment)
		local image = self:createImage(attachment)
		if image then
			image:setReferencePoint(display.CenterReferencePoint);
			image.width = attachment.width
			image.height = attachment.height
		else
			print("Error creating image: " .. attachment.name)
			image = AttachmentResolver.failed
		end
		skeleton.images[attachment] = image
		return image
	end

	function self:createImage (attachment)
		return display.newImage(attachment.name .. ".png")
	end

	return self
end
return AttachmentResolver
