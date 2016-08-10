module spine {
	export interface AttachmentLoader {
		/** @return May be null to not load an attachment. */
		newRegionAttachment (skin: Skin, name: string, path: string): RegionAttachment;

		/** @return May be null to not load an attachment. */
		newMeshAttachment (skin: Skin, name: string, path: string) : MeshAttachment;

		/** @return May be null to not load an attachment. */
		newBoundingBoxAttachment (skin: Skin, name: string) : BoundingBoxAttachment;
		
		/** @return May be null to not load an attachment */
		newPathAttachment(skin: Skin, name: string): PathAttachment;
	}
}
