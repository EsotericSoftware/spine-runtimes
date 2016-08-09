module spine.webgl {
    export class SkeletonRenderer {
        static QUAD_TRIANGLES = [0, 1, 2, 2, 3, 0];

        premultipliedAlpha = false;

        draw(batcher: PolygonBatcher, skeleton: Skeleton) {
            let premultipliedAlpha = this.premultipliedAlpha;
            var blendMode: BlendMode = null;

            var vertices: Array<number> = null;
            var triangles: Array<number>  = null;
            var drawOrder = skeleton.drawOrder;
            for (var i = 0, n = drawOrder.length; i < n; i++) {
                let slot = drawOrder[i];
                let attachment = slot.getAttachment();
                var texture: Texture = null;
                if (attachment instanceof RegionAttachment) {
                    let region = <RegionAttachment>attachment;
                    vertices = region.updateWorldVertices(slot, premultipliedAlpha);
                    triangles = SkeletonRenderer.QUAD_TRIANGLES;
                    texture = (<TextureAtlasRegion>region.region.renderObject).texture;

                } else if (attachment instanceof MeshAttachment) {
                    let mesh = <MeshAttachment>attachment;
                    vertices = mesh.updateWorldVertices(slot, premultipliedAlpha);
                    triangles = mesh.triangles;
                    texture = (<TextureAtlasRegion>mesh.region.renderObject).texture;
                }

                if (texture != null) {
                    let slotBlendMode = slot.data.blendMode;
                    if (slotBlendMode != blendMode) {
                        blendMode = slotBlendMode;
                        batcher.setBlendMode(getSourceGLBlendMode(blendMode, premultipliedAlpha), getDestGLBlendMode(blendMode));
                    }
                    batcher.draw(texture, vertices, triangles);
                }
            }
        }
    }
}

