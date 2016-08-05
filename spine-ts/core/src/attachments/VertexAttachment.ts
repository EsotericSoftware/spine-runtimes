module spine {
    export class VertexAttachment extends Attachment {
        bones: Array<number>;
        vertices: Array<number>;
        worldVerticesLength = 0;

        constructor (name: string) {
            super(name);
        }

        computeWorldVertices (slot: Slot, worldVertices: Array<number>) {
            this.computeWorldVerticesWith(slot, 0, this.worldVerticesLength, worldVertices, 0);
        }

        /** Transforms local vertices to world coordinates.
         * @param start The index of the first local vertex value to transform. Each vertex has 2 values, x and y.
         * @param count The number of world vertex values to output. Must be <= {@link #getWorldVerticesLength()} - start.
         * @param worldVertices The output world vertices. Must have a length >= offset + count.
         * @param offset The worldVertices index to begin writing values. */
        computeWorldVerticesWith (slot: Slot, start: number, count: number, worldVertices: Array<number>, offset: number) {
            count += offset;
            let skeleton = slot.bone.skeleton;
            let x = skeleton.x, y = skeleton.y;
            let deformArray = slot.attachmentVertices;
            var vertices = this.vertices;
            let bones = this.bones;
            if (bones == null) {
                if (deformArray.length > 0) vertices = deformArray;
                let bone = slot.bone;
                x += bone.worldX;
                y += bone.worldY;
                let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
                for (var v = start, w = offset; w < count; v += 2, w += 2) {
                    let vx = vertices[v], vy = vertices[v + 1];
                    worldVertices[w] = vx * a + vy * b + x;
                    worldVertices[w + 1] = vx * c + vy * d + y;
                }
                return;
            }
            var v = 0, skip = 0;
            for (var i = 0; i < start; i += 2) {
                let n = bones[v];
                v += n + 1;
                skip += n;
            }
            let skeletonBones = skeleton.bones;
            if (deformArray.length == 0) {
                for (var w = offset, b = skip * 3; w < count; w += 2) {
                    let wx = x, wy = y;
                    let n = bones[v++];
                    n += v;
                    for (; v < n; v++, b += 3) {
                        let bone = skeletonBones[bones[v]];
                        let vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
                        wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
                        wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
                    }
                    worldVertices[w] = wx;
                    worldVertices[w + 1] = wy;
                }
            } else {
                let deform = deformArray;
                for (var w = offset, b = skip * 3, f = skip << 1; w < count; w += 2) {
                    let wx = x, wy = y;
                    let n = bones[v++];
                    n += v;
                    for (; v < n; v++, b += 3, f += 2) {
                        let bone = skeletonBones[bones[v]];
                        let vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
                        wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
                        wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
                    }
                    worldVertices[w] = wx;
                    worldVertices[w + 1] = wy;
                }
            }
        }

        /** Returns true if a deform originally applied to the specified attachment should be applied to this attachment. */
        applyDeform (sourceAttachment: VertexAttachment) {
            return this == sourceAttachment;
        }        
    }

}