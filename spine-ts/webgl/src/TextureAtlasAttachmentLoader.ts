module spine.webgl {
	export class TextureAtlasAttachmentLoader implements AttachmentLoader {
		atlas: TextureAtlas;

		constructor (atlas: TextureAtlas) {
			this.atlas = atlas;
		}

		/** @return May be null to not load an attachment. */
		newRegionAttachment (skin: Skin, name: string, path: string): RegionAttachment {
			let region = this.atlas.findRegion(path);
			region.renderObject = region;
		    if (region == null) throw new Error("Region not found in atlas: " + path + " (region attachment: " + name + ")");
		    let attachment = new RegionAttachment(name);
			attachment.setRegion(region);
		    attachment.region = region;
		    return attachment;
		}

		/** @return May be null to not load an attachment. */
		newMeshAttachment (skin: Skin, name: string, path: string) : MeshAttachment {
			let region = this.atlas.findRegion(path);
			region.renderObject = region;
		    if (region == null) throw new Error("Region not found in atlas: " + path + " (mesh attachment: " + name + ")");
		    let attachment = new MeshAttachment(name);            
		    attachment.region = region;
		    return attachment;
		} 

		/** @return May be null to not load an attachment. */
		newBoundingBoxAttachment (skin: Skin, name: string) : BoundingBoxAttachment {
			return new BoundingBoxAttachment(name);
		} 
		
		/** @return May be null to not load an attachment */
		newPathAttachment(skin: Skin, name: string): PathAttachment {
			return new PathAttachment(name);
		}  
	} 
}
