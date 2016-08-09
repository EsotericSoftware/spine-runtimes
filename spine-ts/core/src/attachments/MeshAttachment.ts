module spine {
    export class MeshAttachment extends VertexAttachment {
        region: TextureRegion;
        path: string;
        regionUVs: Array<number>; worldVertices: Array<number>;
        triangles: Array<number>;
        color = new Color(1, 1, 1, 1);
        hullLength: number;
        private _parentMesh: MeshAttachment;
        inheritDeform = false;
        tempColor = new Color(0, 0, 0, 0);

        constructor (name: string) {
            super(name);
        }

        updateUVs () {
            let regionUVs = this.regionUVs;
            let verticesLength = regionUVs.length;
            let worldVerticesLength = (verticesLength >> 1) * 5;
            if (this.worldVertices == null || this.worldVertices.length != worldVerticesLength) {
                this.worldVertices = Utils.newArray<number>(worldVerticesLength, 0);                
            }

            var u = 0, v = 0, width = 0, height = 0;
            if (this.region == null) {
                u = v = 0;
                width = height = 1;
            } else {
                u = this.region.u;
                v = this.region.v;
                width = this.region.u2 - u;
                height = this.region.v2 - v;
            }
            if (this.region.rotate) {
                for (var i = 0, w = 6; i < verticesLength; i += 2, w += 8) {
                    this.worldVertices[w] = u + regionUVs[i + 1] * width;
                    this.worldVertices[w + 1] = v + height - regionUVs[i] * height;
                }
            } else {
                for (var i = 0, w = 6; i < verticesLength; i += 2, w += 8) {
                    this.worldVertices[w] = u + regionUVs[i] * width;
                    this.worldVertices[w + 1] = v + regionUVs[i + 1] * height;
                }
            }
        }

        /** @return The updated world vertices. */
        updateWorldVertices (slot: Slot, premultipliedAlpha: boolean) {
            let skeleton = slot.bone.skeleton;
            let skeletonColor = skeleton.color, slotColor = slot.color, meshColor = this.color;
            let alpha = skeletonColor.a * slotColor.a * meshColor.a;
            let multiplier = premultipliedAlpha ? alpha : 1;
            let color = this.tempColor;
            color.set(skeletonColor.r * slotColor.r * meshColor.r * multiplier,
                          skeletonColor.g * slotColor.g * meshColor.g * multiplier,
                          skeletonColor.b * slotColor.b * meshColor.b * multiplier,
                          alpha);                    

            let x = skeleton.x, y = skeleton.y;
            let deformArray = slot.attachmentVertices;
            var vertices = this.vertices, worldVertices = this.worldVertices;
            let bones = this.bones;
            if (bones == null) {
                let verticesLength = vertices.length;
                if (deformArray.length > 0) vertices = deformArray;
                let bone = slot.bone;
                x += bone.worldX;
                y += bone.worldY;
                let a = bone.a, b = bone.b, c = bone.c, d = bone.d;
                for (var v = 0, w = 0; v < verticesLength; v += 2, w += 8) {
                    let vx = vertices[v], vy = vertices[v + 1];
                    worldVertices[w] = vx * a + vy * b + x;
                    worldVertices[w + 1] = vx * c + vy * d + y;
                    worldVertices[w + 2] = color.r;
                    worldVertices[w + 3] = color.g;
                    worldVertices[w + 4] = color.b;
                    worldVertices[w + 5] = color.a;
                }
                return worldVertices;
            }
            let skeletonBones = skeleton.bones;
            if (deformArray.length == 0) {
                for (var w = 0, v = 0, b = 0, n = bones.length; v < n; w += 8) {
                    let wx = x, wy = y;
                    let nn = bones[v++] + v;
                    for (; v < nn; v++, b += 3) {
                        let bone = skeletonBones[bones[v]];
                        let vx = vertices[b], vy = vertices[b + 1], weight = vertices[b + 2];
                        wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
                        wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
                    }
                    worldVertices[w] = wx;
                    worldVertices[w + 1] = wy;
                    worldVertices[w + 2] = color.r;
                    worldVertices[w + 3] = color.g;
                    worldVertices[w + 4] = color.b;
                    worldVertices[w + 5] = color.a;
                }
            } else {
                let deform = deformArray;
                for (var w = 0, v = 0, b = 0, f = 0, n = bones.length; v < n; w += 8) {
                    let wx = x, wy = y;
                    let nn = bones[v++] + v;
                    for (; v < nn; v++, b += 3, f += 2) {
                        let bone = skeletonBones[bones[v]];
                        let vx = vertices[b] + deform[f], vy = vertices[b + 1] + deform[f + 1], weight = vertices[b + 2];
                        wx += (vx * bone.a + vy * bone.b + bone.worldX) * weight;
                        wy += (vx * bone.c + vy * bone.d + bone.worldY) * weight;
                    }
                    worldVertices[w] = wx;
                    worldVertices[w + 1] = wy;
                    worldVertices[w + 2] = color.r;
                    worldVertices[w + 3] = color.g;
                    worldVertices[w + 4] = color.b;
                    worldVertices[w + 5] = color.a;
                }
            }
            return worldVertices;
        }

        applyDeform (sourceAttachment: VertexAttachment): boolean {
            return this == sourceAttachment || (this.inheritDeform && this._parentMesh == sourceAttachment);
        }       

        getParentMesh() {
            return this._parentMesh;
        } 

        /** @param parentMesh May be null. */
        setParentMesh (parentMesh: MeshAttachment) {
            this._parentMesh = parentMesh;
            if (parentMesh != null) {
                this.bones = parentMesh.bones;
                this.vertices = parentMesh.vertices;
                this.regionUVs = parentMesh.regionUVs;
                this.triangles = parentMesh.triangles;
                this.hullLength = parentMesh.hullLength;                
            }
        }        
    }

}