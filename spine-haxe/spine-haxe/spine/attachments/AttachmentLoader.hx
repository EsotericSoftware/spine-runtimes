package spine.attachments;

import spine.Skin;

interface AttachmentLoader {
	/** @return May be null to not load an attachment. */
	function newRegionAttachment(skin:Skin, name:String, path:String, sequence:Sequence):RegionAttachment;

	/** @return May be null to not load an attachment. */
	function newMeshAttachment(skin:Skin, name:String, path:String, sequence:Sequence):MeshAttachment;

	/** @return May be null to not load an attachment. */
	function newBoundingBoxAttachment(skin:Skin, name:String):BoundingBoxAttachment;

	/** @return May be null to not load an attachment */
	function newPathAttachment(skin:Skin, name:String):PathAttachment;

	/** @return May be null to not load an attachment */
	function newPointAttachment(skin:Skin, name:String):PointAttachment;

	/** @return May be null to not load an attachment */
	function newClippingAttachment(skin:Skin, name:String):ClippingAttachment;
}
