// Copyright (c) 2012 Turbulenz Limited

/*global
Draw2D: false
Float32Array: false
*/

//
// Draw2DGroup. Wraps vertex buffer data with pairings of indices and textures
// representing subsets of buffer relating to a set of equal-texture quads.
//
// [ sprite1  sprite2  sprite3  sprite4  sprite5 ]
//  \---------------/  \------/ \--------------/
//       texture 1    texture 2     texture 3
//      12 indices    6 indices     12 indices
//
function Draw2DGroup() {}
Draw2DGroup.create = function draw2DGroupFn()
{
    var group = new Draw2DGroup();

    // pairs of index counts + associated texture for subset of group.
    group.indices = [];
    group.textures = [];
    group.numSets = 0;

    // vertex buffer for group.
    group.vertexBufferData = new Draw2D.prototype.floatArray(1024);
    group.numVertices = 0;

    return group;
};

function Draw2DSprite() {}
Draw2DSprite.prototype = {

    version : 1,

    //
    // Assumption is that user will not be performing these actions frequently.
    // To that end, we provide a function which performs the ssary side effects
    // on call, to prevent an overhead for lazy evaluation.
    //
    getTextureRectangle : function getTextureRectangleFn(dst)
    {
        if (dst === undefined)
        {
            dst = new Draw2D.prototype.floatArray(4);
        }
        var data = this.data;
        var texture = this._texture;
        if (texture)
        {
            dst[0] = data[12] * texture.width;
            dst[1] = data[13] * texture.height;
            dst[2] = data[14] * texture.width;
            dst[3] = data[15] * texture.height;
        }
        else
        {
            dst[0] = data[12];
            dst[1] = data[13];
            dst[2] = data[14];
            dst[3] = data[15];
        }
        return dst;
    },
    setTextureRectangle : function setTextureRectangleFn(uvRect)
    {
        var data = this.data;
        var texture = this._texture;
        if (texture)
        {
            var iwidth  = 1 / texture.width;
            var iheight = 1 / texture.height;
            data[12] = uvRect[0] * iwidth;
            data[13] = uvRect[1] * iheight;
            data[14] = uvRect[2] * iwidth;
            data[15] = uvRect[3] * iheight;
        }
        else
        {
            data[12] = uvRect[0];
            data[13] = uvRect[1];
            data[14] = uvRect[2];
            data[15] = uvRect[3];
        }
    },

    getColor : function getColorFn(dst)
    {
        if (dst === undefined)
        {
            dst = new Draw2D.prototype.floatArray(4);
        }
        var data = this.data;
        dst[0] = data[8];
        dst[1] = data[9];
        dst[2] = data[10];
        dst[3] = data[11];
        return dst;
    },
    setColor : function setColorFn(color)
    {
        var data = this.data;
        data[8]  = color[0];
        data[9]  = color[1];
        data[10] = color[2];
        data[11] = color[3];
    },

    getTexture : function getTextureFn()
    {
        return this._texture;
    },
    setTexture : function setTextureFn(texture)
    {
        if (this._texture !== texture)
        {
            var su = (this._texture ? this._texture.width  : 1.0) / (texture ? texture.width  : 1.0);
            var sv = (this._texture ? this._texture.height : 1.0) / (texture ? texture.height : 1.0);
            this._texture = texture || null;

            // re-normalise texture coordinates.
            var data = this.data;
            data[12] *= su;
            data[13] *= sv;
            data[14] *= su;
            data[15] *= sv;
        }
    },

    getWidth : function getWidthFn()
    {
        return this.data[17] * 2;
    },
    setWidth : function setWidthFn(width)
    {
        width *= 0.5;
        var data = this.data;
        if (data[17] !== width)
        {
            data[17] = width;
            this._invalidate();
        }
    },

    getHeight : function getHeightFn()
    {
        return this.data[18] * 2;
    },
    setHeight : function setHeightFn(height)
    {
        height *= 0.5;
        var data = this.data;
        if (data[18] !== height)
        {
            data[18] = height;
            this._invalidate();
        }
    },

    getScale : function getScaleFn(dst)
    {
        if (dst === undefined)
        {
            dst = new Draw2D.prototype.floatArray(2);
        }
        var data = this.data;
        dst[0] = data[19];
        dst[1] = data[20];
        return dst;
    },
    setScale : function setScaleFn(scale)
    {
        var scaleX = scale[0];
        var scaleY = scale[1];
        var data = this.data;
        if (data[19] !== scaleX || data[20] !== scaleY)
        {
            data[19] = scaleX;
            data[20] = scaleY;
            this._invalidate();
        }
    },

    getShear : function getShearFn(dst)
    {
        if (dst === undefined)
        {
            dst = new Draw2D.prototype.floatArray(2);
        }
        var data = this.data;
        dst[0] = data[21];
        dst[1] = data[22];
        return dst;
    },
    setShear : function setShearFn(shear)
    {
        var shearX = shear[0];
        var shearY = shear[1];
        var data = this.data;
        if (data[21] !== shearX || data[22] !== shearY)
        {
            data[21] = shearX;
            data[22] = shearY;
            this._invalidate();
        }
    },

    getOrigin : function getOriginFn(dst)
    {
        if (dst === undefined)
        {
            dst = new Draw2D.prototype.floatArray(2);
        }
        var data = this.data;
        dst[0] = data[23];
        dst[1] = data[24];
        return dst;
    },
    setOrigin : function setOriginFn(origin)
    {
        var originX = origin[0];
        var originY = origin[1];
        var data = this.data;
        if (data[23] !== originX || data[24] !== originY)
        {
            data[23] = originX;
            data[24] = originY;
            this._invalidate();
        }
    },

    // Method for internal use only.
    //
    // Recompute locally defined vectors.
    _invalidate : function invalidateFn()
    {
        var data = this.data;
        // [ T1 T2 ] = [ scaleX 0 ] [ 1 shearX ]
        // [ T3 T4 ]   [ 0 scaleY ] [ shearY 1 ]
        var T1 = data[19];
        var T2 = data[19] * data[21];
        var T3 = data[20] * data[22];
        var T4 = data[20];

        // Recompute locally defined position of true center of sprite.
        var x = data[17] - data[23];  // x = width/2 - originX
        var y = data[18] - data[24];  // y = height/2 - originY
        var cx = data[25] = (T1 * x + T2 * y); // (cx) = T (x)
        var cy = data[26] = (T3 * x + T4 * y); // (cy)     (y)

        // Recompute locally defined position of top-left vertex relative to center of sprite.
        x = -data[17]; // x = -width/2
        y = -data[18]; // y = -height/2
        var ux = data[27] = (T1 * x + T2 * y); // (ux) = T (x)
        var uy = data[28] = (T3 * x + T4 * y); // (uy)     (y)

        // Recompute locally defined position of top-right vertex relative to center of sprite.
        x = -x; // x = width / 2
        var vx = data[29] = (T1 * x + T2 * y); // (vx) = T (x)
        var vy = data[30] = (T3 * x + T4 * y); // (vy)     (y)

        // Rotate vectors to screen space so that in the case that rotation is not performed
        // These vectors are still valid.
        var rotation = data[16] = this.rotation;
        var cos = Math.cos(rotation);
        var sin = Math.sin(rotation);

        data[31] = ((cos * cx) - (sin * cy));
        data[32] = ((sin * cx) + (cos * cy));
        data[33] = ((cos * ux) - (sin * uy));
        data[34] = ((sin * ux) + (cos * uy));
        data[35] = ((cos * vx) - (sin * vy));
        data[36] = ((sin * vx) + (cos * vy));

        // Compute suitable epsilon to consider rotations equals.
        // We do this by finding the vertex furthest from defined center of rotation.
        // And using its distance to compute what rotation constitutes a 'visible' rotation.
        //
        // Positions of vertices relative to origin are given by:
        // v1 = c + u, v2 = c + v, v3 = c - v, v4 = c - u.
        // |v1|^2 = |c|^2 + |u|^2 + 2c.u
        // |v4|^2 = |c|^2 + |u|^2 - 2c.u
        // |v2|^2 = |c|^2 + |v|^2 + 2c.v
        // |v3|^2 = |c|^2 + |v|^2 - 2c.v
        //
        // Compute r1 = |u|^2 + abs(2c.u)
        // Compute r2 = |v|^2 + abs(2c.v)
        //
        // Finally max(|vi|^2) = |c|^2 + max(r1, r2)
        //
        var dot = 2 * ((cx * ux) + (cy * uy));
        if (dot < 0)
        {
            dot = -dot;
        }
        var r1 = (ux * ux) + (uy * uy) + dot;

        dot = 2 * ((cx * vx) + (cy * vy));
        if (dot < 0)
        {
            dot = -dot;
        }
        var r2 = (vx * vx) + (vy * vy) + dot;

        if (r2 > r1)
        {
            r1 = r2;
        }

        r1 += ((cx * cx) + (cy * cy));
        // r1 is the squared distance to furthest vertex.
        //
        // We permit a half pixel movement to be considered a 'true' movement.
        // Squared rotation required to impart this movement on furthest vertex is
        data[37] = (0.25 / r1); // squared epsilon
    },

    // Method for internal use only.
    //
    // Recompute draw2d coordinate space vertices and vectors.
    _update : function _updateFn(angleScaleFactor)
    {
        var data = this.data;
        var x, y, u, v;

        // Check if rotation has been modified
        x = this.rotation;
        y = x - data[16]; // y = rotation - previousRotation
        if ((y * y) > (data[37] * angleScaleFactor)) // if |y| > epsilon
        {
            data[16] = x; //previousRotation = rotation
            u = Math.cos(x);
            v = Math.sin(x);

            // rotate locally defined vectors.
            x = data[25];
            y = data[26];
            data[31] = (u * x - v * y); // (px) = [cos -sin] (cx)
            data[32] = (v * x + u * y); // (py) = [sin  cos] (cy)

            x = data[27];
            y = data[28];
            data[33] = (u * x - v * y); // (x1) = [cos -sin] (ux)
            data[34] = (v * x + u * y); // (y1) = [sin  cos] (uy)

            x = data[29];
            y = data[30];
            data[35] = (u * x - v * y); // (x2) = [cos -sin] (vx)
            data[36] = (v * x + u * y); // (y2) = [sin  cos] (vy)
        }

        // Compute center of this sprite in screen space.
        u = this.x + data[31]; // u = centerX = positionX + px
        v = this.y + data[32]; // v = centerY = positionY + py

        // Compute vertex positions in screen space.
        x = data[33];
        y = data[34];
        data[0] = u + x; // v1x = centerX + x1
        data[1] = v + y; // v1y = centerY + y1
        data[6] = u - x; // v4x = centerX - x1
        data[7] = v - y; // v4y = centerY - y1

        x = data[35];
        y = data[36];
        data[2] = u + x; // v2x = centerX + x2
        data[3] = v + y; // v2y = centerY + y2
        data[4] = u - x; // v3x = centerX - x2
        data[5] = v - y; // v3y = centerY - y2
    }
};

Draw2DSprite.create = function draw2DSpriteCreateFn(params)
{
    if ((params.width === undefined || params.height === undefined) && !params.texture)
    {
        return null;
    }

    // data:
    // ---
    // First 16 values reserved for Draw2DSpriteData.
    //   includes colour and texture coordinates.
    //
    // 16    : old_rotation (for lazy evaluation)
    // 17,18 : width/2, height/2 (changed by user via function)
    // 19,20 : scaleX, scaleY    (changed by user via function)
    // 21,22 : shearX, shearY    (changed by user via function)
    // 23,24 : originX, originY  (changed by user via function)
    // 25,26 : cx, cy // locally defined position of true center of sprite relative to origin
    //    (dependant on scale/shear/center/dimension)
    // 27,28 : u1, v1 // locally defined position of top-left vertex relative to center of sprite.
    //    (dependant on scale/shear/dimension)
    // 29,30 : u2, v2 // locally defined position of top-right vertex relative to center of sprite.
    //    (dependant on scale/shear/dimension)
    // 31,32 : px, py // relative defined position of true center of sprite relative to origin
    //    (dependant on rotation and cx,cy)
    // 33,34 : x1, y1 // relative defined position of top-left vertex relative to center of sprite.
    //    (dependant on rotation and u1,v1)
    // 35,36 : x2, y2 // relative defined position of top-right vertex relative to center of sprite.
    //    (dependant on rotation and u2,v2)
    // 37 : Squared epsilon to consider rotations equal based on dimensions.
    var s = new Draw2DSprite();
    var data = s.data = new Draw2D.prototype.floatArray(38);

    // texture (not optional)
    var texture = s._texture = params.texture || null;

    // position (optional, default 0,0)
    s.x = (params.x || 0.0);
    s.y = (params.y || 0.0);

    // rotation (optional, default 0)
    s.rotation = data[16] = (params.rotation || 0.0);

    // colour (optional, default [1,1,1,1])
    var color = params.color;
    data[8]  = (color ? color[0] : 1.0);
    data[9]  = (color ? color[1] : 1.0);
    data[10] = (color ? color[2] : 1.0);
    data[11] = (color ? color[3] : 1.0);

    // uvRect (optional, default texture rectangle)
    var uvRect = params.textureRectangle;
    var iwidth  = (texture ? 1 / texture.width  : 1);
    var iheight = (texture ? 1 / texture.height : 1);
    data[12] = (uvRect ? (uvRect[0] * iwidth)  : 0.0);
    data[13] = (uvRect ? (uvRect[1] * iheight) : 0.0);
    data[14] = (uvRect ? (uvRect[2] * iwidth)  : 1.0);
    data[15] = (uvRect ? (uvRect[3] * iheight) : 1.0);

    // dimensions / 2 (default texture dimensions)
    data[17] = ((params.width  !== undefined) ? params.width  : texture.width)  * 0.5;
    data[18] = ((params.height !== undefined) ? params.height : texture.height) * 0.5;

    // scale (default [1,1])
    var scale = params.scale;
    data[19] = (scale ? scale[0] : 1.0);
    data[20] = (scale ? scale[1] : 1.0);

    // shear (default [0,0])
    var shear = params.shear;
    data[21] = (shear ? shear[0] : 0.0);
    data[22] = (shear ? shear[1] : 0.0);

    // origin (default dimensions / 2)
    var origin = params.origin;
    data[23] = (origin ? origin[0] : data[17]);
    data[24] = (origin ? origin[1] : data[18]);

    s._invalidate();
    return s;
};

//
// Used in rectangle draw routines to compute data to be pushed into vertex buffers.
//
function Draw2DSpriteData() {}
Draw2DSpriteData.setFromRotatedRectangle = function setFromRotatedRectangleFn(sprite, texture, rect, uvrect, color, rotation, origin)
{
    var x1 = rect[0];
    var y1 = rect[1];
    var x2 = rect[2];
    var y2 = rect[3];

    if (!rotation)
    {
        sprite[0] = x1;
        sprite[1] = y1;
        sprite[2] = x2;
        sprite[3] = y1;
        sprite[4] = x1;
        sprite[5] = y2;
        sprite[6] = x2;
        sprite[7] = y2;
    }
    else
    {
        var cx, cy;
        if (origin)
        {
            cx = x1 + origin[0];
            cy = y1 + origin[1];
        }
        else
        {
            cx = 0.5 * (x1 + x2);
            cy = 0.5 * (y1 + y2);
        }

        var dx = x1 - cx;
        var dy = y1 - cy;

        var cos = Math.cos(rotation);
        var sin = Math.sin(rotation);
        var w = (x2 - x1);
        var h = (y2 - y1);

        sprite[0] = x1 = cx + (cos * dx - sin * dy);
        sprite[1] = y1 = cy + (sin * dx + cos * dy);
        sprite[2] = x1 + (cos * w);
        sprite[3] = y1 + (sin * w);
        sprite[4] = x1 - (sin * h);
        sprite[5] = y1 + (cos * h);
        sprite[6] = x1 + (cos * w - sin * h);
        sprite[7] = y1 + (sin * w + cos * h);
    }

    if (color)
    {
        sprite[8]  = color[0];
        sprite[9]  = color[1];
        sprite[10] = color[2];
        sprite[11] = color[3];
    }
    else
    {
        sprite[8] = sprite[9] = sprite[10] = sprite[11] = 1.0;
    }

    if (uvrect && texture)
    {
        var iwidth  = 1 / texture.width;
        var iheight = 1 / texture.height;
        sprite[12] = uvrect[0] * iwidth;
        sprite[13] = uvrect[1] * iheight;
        sprite[14] = uvrect[2] * iwidth;
        sprite[15] = uvrect[3] * iheight;
    }
    else
    {
        sprite[12] = sprite[13] = 0;
        sprite[14] = sprite[15] = 1;
    }
};

Draw2DSpriteData.create = function draw2DSpriteFn()
{
    // x1 y1 x2 y2 x3 y3 x4 y4 - vertices [0,8)
    // cr cg cb ca u1 v1 u2 v2 - normalized color + texture [8,16)
    return new Draw2D.prototype.floatArray(16);
};

function Draw2D() {}

Draw2D.prototype = {

    version : 7,

    forceUpdate : false,
    clearBackBuffer : false,

    // supported sort modes.
    sort : {
        deferred  : 'deferred',
        immediate : 'immediate',
        texture   : 'texture'
    },

    // supported scale modes.
    scale : {
        scale : 'scale',
        none  : 'none'
    },

    drawStates: {
        uninit: 0,
        ready : 1,
        draw  : 2
    },

    defaultClearColor: [0, 0, 0, 1],

    clear: function clearFn(clearColor)
    {
        if (this.state !== this.drawStates.ready)
        {
            return false;
        }

        var gd = this.graphicsDevice;
        if (this.currentRenderTarget)
        {
            if (!gd.beginRenderTarget(this.currentRenderTarget.renderTarget))
            {
                return false;
            }

            gd.clear(clearColor || this.defaultClearColor);
            gd.endRenderTarget();
        }
        else
        {
            gd.clear(clearColor || this.defaultClearColor);
        }

        return true;
    },

    clearBatch: function clearFn()
    {
        for (var name in this.texLists)
        {
            if (this.texLists.hasOwnProperty(name))
            {
                delete this.texLists[name];
            }
        }
        this.currentTextureGroup = undefined;
        this.numGroups = 0;
    },

    bufferSprite : function bufferSpriteFn(buffer, sprite, index)
    {
        sprite._update(0);
        /*jshint bitwise: false*/
        index <<= 4;
        /*jshint bitwise: true*/

        var data = sprite.data;
        buffer[index]      = data[0];
        buffer[index + 1]  = data[1];
        buffer[index + 2]  = data[2];
        buffer[index + 3]  = data[3];
        buffer[index + 4]  = data[4];
        buffer[index + 5]  = data[5];
        buffer[index + 6]  = data[6];
        buffer[index + 7]  = data[7];
        buffer[index + 8]  = data[8];
        buffer[index + 9]  = data[9];
        buffer[index + 10] = data[10];
        buffer[index + 11] = data[11];
        buffer[index + 12] = data[12];
        buffer[index + 13] = data[13];
        buffer[index + 14] = data[14];
        buffer[index + 15] = data[15];
    },

    update: function updateFn()
    {
        var graphicsDevice = this.graphicsDevice;
        var width = this.width;
        var height = this.height;

        var graphicsDeviceWidth = graphicsDevice.width;
        var graphicsDeviceHeight = graphicsDevice.height;

        if (width !== graphicsDeviceWidth || height !== graphicsDeviceHeight || this.forceUpdate)
        {
            var viewWidth, viewHeight, viewX, viewY;
            var viewportRectangle = this.viewportRectangle;

            if (viewportRectangle)
            {
                viewX = viewportRectangle[0];
                viewY = viewportRectangle[1];
                viewWidth  = viewportRectangle[2] - viewX;
                viewHeight = viewportRectangle[3] - viewY;
            }
            else
            {
                viewX = 0;
                viewY = 0;
                viewWidth = graphicsDeviceWidth;
                viewHeight = graphicsDeviceHeight;
            }

            if ((viewWidth === graphicsDeviceWidth) && (viewHeight === graphicsDeviceHeight))
            {
                this.clearBackBuffer = false;
            }
            else
            {
                this.clearBackBuffer = true;
            }

            var target = this.currentRenderTarget;

            if (this.scaleMode === 'scale')
            {
                var viewAspectRatio = viewWidth / viewHeight;
                var graphicsDeviceAspectRatio = graphicsDeviceWidth / graphicsDeviceHeight;
                var calcViewWidth, calcViewHeight, diffWidth, diffHeight, halfDiffWidth, halfDiffHeight;

                if (graphicsDeviceAspectRatio > viewAspectRatio)
                {
                    calcViewWidth = Math.ceil((graphicsDeviceHeight / viewHeight) * viewWidth);
                    diffWidth = graphicsDeviceWidth - calcViewWidth;
                    halfDiffWidth = Math.floor(diffWidth * 0.5);

                    this.scissorX = halfDiffWidth;
                    this.scissorY = 0;
                    this.scissorWidth = calcViewWidth;
                    this.scissorHeight = graphicsDeviceHeight;

                    this.viewScaleX = viewWidth / calcViewWidth;
                    this.viewScaleY = viewHeight / graphicsDeviceHeight;

                    if (!target)
                    {
                        this.clipOffsetX = (halfDiffWidth / graphicsDeviceWidth * 2.0) - 1.0;
                        this.clipOffsetY = 1;
                        this.clipScaleX = (calcViewWidth / graphicsDeviceWidth * 2.0) / viewWidth;
                        this.clipScaleY = -2.0 / viewHeight;
                    }
                }
                else
                {
                    calcViewHeight = Math.ceil((graphicsDeviceWidth / viewWidth) * viewHeight);
                    diffHeight = graphicsDeviceHeight - calcViewHeight;
                    halfDiffHeight = Math.floor(diffHeight * 0.5);

                    this.scissorX = 0;
                    this.scissorY = halfDiffHeight;
                    this.scissorWidth = graphicsDeviceWidth;
                    this.scissorHeight = calcViewHeight;

                    this.viewScaleX = viewWidth / graphicsDeviceWidth;
                    this.viewScaleY = viewHeight / calcViewHeight;

                    if (!target)
                    {
                        this.clipOffsetX = -1.0;
                        this.clipOffsetY = 1 - ((halfDiffHeight / graphicsDeviceHeight) * 2.0);
                        this.clipScaleX = 2.0 / viewWidth;
                        this.clipScaleY = ((calcViewHeight / graphicsDeviceHeight) * -2.0) / viewHeight;
                    }
                }
            }
            else
            {
                this.viewScaleX = 1;
                this.viewScaleY = 1;

                if (!target)
                {
                    this.clipOffsetX = -1.0;
                    this.clipOffsetY = 1.0;
                    this.clipScaleX = 2.0 / graphicsDeviceWidth;
                    this.clipScaleY = -2.0 / graphicsDeviceHeight;
                }

                this.scissorX = 0;
                this.scissorY = (graphicsDeviceHeight - viewHeight);
                this.scissorWidth = viewWidth;
                this.scissorHeight = viewHeight;
            }

            this.spriteAngleFactor = Math.min(this.viewScaleX, this.viewScaleY);
            this.spriteAngleFactor *= this.spriteAngleFactor;

            this.width = graphicsDeviceWidth;
            this.height = graphicsDeviceHeight;

            var i = 0;
            var renderTargets = this.renderTargetStructs;
            var limit = renderTargets.length;
            for (i = 0; i < limit; i += 1)
            {
                this.validateTarget(renderTargets[i], this.scissorWidth, this.scissorHeight);
            }

            if (target)
            {
                this.clipOffsetX = -1.0;
                this.clipOffsetY = -1.0;
                this.clipScaleX = 2.0 * target.actualWidth / target.texture.width / viewWidth;
                this.clipScaleY = 2.0 * target.actualHeight / target.texture.height / viewHeight;
            }

            // Deal with viewports that are not started at (0,0)
            this.clipOffsetX -= viewX * this.clipScaleX;
            this.clipOffsetY -= viewY * this.clipScaleY;

            var clipSpace = this.techniqueParameters.clipSpace;
            clipSpace[0] = this.clipScaleX;
            clipSpace[1] = this.clipScaleY;
            clipSpace[2] = this.clipOffsetX;
            clipSpace[3] = this.clipOffsetY;

            this.updateRenderTargetVbo(this.scissorX, this.scissorY, this.scissorWidth, this.scissorHeight);
            this.forceUpdate = false;
        }
    },

    getViewport: function getViewportFn(dst)
    {
        if (!dst)
        {
            dst = new Draw2D.prototype.floatArray(4);
        }
        var viewport = this.viewportRectangle;
        if (viewport)
        {
            dst[0] = viewport[0];
            dst[1] = viewport[1];
            dst[2] = viewport[2];
            dst[3] = viewport[3];
        }
        else
        {
            dst[0] = dst[1] = 0;
            dst[2] = this.graphicsDevice.width;
            dst[3] = this.graphicsDevice.height;
        }
        return dst;
    },
    getScreenSpaceViewport: function screenSpaceViewportFn(dst)
    {
        if (!dst)
        {
            dst = new Draw2D.prototype.floatArray(4);
        }
        // ensure mapping is correct.
        this.update();

        dst[0] = this.scissorX;
        dst[1] = this.height - (this.scissorY + this.scissorHeight);
        dst[2] = dst[0] + this.scissorWidth;
        dst[3] = dst[1] + this.scissorHeight;
        return dst;
    },

    viewportMap: function viewportMapFn(screenX, screenY, dst)
    {
        if (!dst)
        {
            dst = new Draw2D.prototype.floatArray(2);
        }
        // ensure mapping is correct.
        this.update();

        // webgl coordinates have flipped y.
        var scissorY = (this.height - this.scissorHeight - this.scissorY);

        dst[0] = (screenX - this.scissorX) * this.viewScaleX;
        dst[1] = (screenY - scissorY) * this.viewScaleY;

        var viewport = this.viewportRectangle;
        if (viewport)
        {
            dst[0] += viewport[0];
            dst[1] += viewport[1];
        }

        return dst;
    },
    viewportUnmap: function screenMapFn(x, y, dst)
    {
        if (!dst)
        {
            dst = new Draw2D.prototype.floatArray(2);
        }
        // ensure mapping is correct.
        this.update();

        var viewport = this.viewportRectangle;
        if (viewport)
        {
            x -= viewport[0];
            y -= viewport[1];
        }

        // webgl coordinates have flipped y.
        var scissorY = (this.height - this.scissorHeight - this.scissorY);

        dst[0] = (x / this.viewScaleX) + this.scissorX;
        dst[1] = (y / this.viewScaleY) + scissorY;
        return dst;
    },

    viewportClamp: function viewportClampFn(point)
    {
        if (point)
        {
            var x = point[0];
            var y = point[1];

            var minX, minY, maxX, maxY;
            var viewport = this.viewportRectangle;
            if (viewport)
            {
                minX = viewport[0];
                minY = viewport[1];
                maxX = viewport[2];
                maxY = viewport[3];
            }
            else
            {
                minX = 0;
                minY = 0;
                maxX = this.graphicsDevice.width;
                maxY = this.graphicsDevice.height;
            }

            if (x < minX)
            {
                x = minX;
            }
            else if (x > maxX)
            {
                x = maxX;
            }

            if (y < minY)
            {
                y = minY;
            }
            else if (y > maxY)
            {
                y = maxY;
            }

            point[0] = x;
            point[1] = y;
        }

        return point;
    },

    configure: function configureFn(params)
    {
        if (this.state !== this.drawStates.ready)
        {
            return false;
        }

        var viewportRectangle = ("viewportRectangle" in params) ? params.viewportRectangle : this.viewportRectangle;

        var scaleMode = params.scaleMode;
        if (scaleMode !== undefined)
        {
            // check scaleMode is supported.
            if (!(scaleMode in this.scale))
            {
                return false;
            }
            if (scaleMode === 'scale' && !viewportRectangle)
            {
                return false;
            }
            this.scaleMode = scaleMode;
        }

        this.viewportRectangle = viewportRectangle;

        this.forceUpdate = true;
        this.update();

        return true;
    },

    destroy: function destroyFn()
    {
        this.texLists = null;
        this.state = this.drawStates.uninit;

        delete this.graphicsDevice;

        if (this.vertexBuffer)
        {
            this.vertexBuffer.destroy();
        }
        if (this.indexBuffer)
        {
            this.indexBuffer.destroy();
        }

        this.copyVertexBuffer.destroy();

        var renderTargets = this.renderTargetStructs;
        while (renderTargets.length > 0)
        {
            var target = renderTargets.pop();
            target.texture.destroy();
            target.renderTarget.destroy();
            delete target.texture;
            delete target.renderTarget;
        }
    },

    begin: function beginFn(blendMode, sortMode)
    {
        // Check sort mode is well defined (or undefined signifying default)
        if (sortMode && !(sortMode in this.sort))
        {
            return false;
        }

        // Check blend mode is well defined (or undefined signifying default)
        if (blendMode && !(blendMode in this.blend))
        {
            return false;
        }

        //if there are render states left in the stack
        //and begin has been called without an end
        //draw previous data with current render state
        var firstTime = !this.sortMode;
        if (this.dispatch())
        {
            this.clearBatch();
        }

        if (firstTime)
        {
            if (this.state !== this.drawStates.ready)
            {
                return false;
            }

            // Check the buffers are correct before we render
            this.update();

            if (!this.currentRenderTarget)
            {
                this.graphicsDevice.setScissor(this.scissorX, this.scissorY, this.scissorWidth, this.scissorHeight);
            }
        }

        this.state = this.drawStates.draw;

        sortMode  = (sortMode)  ? sortMode  : (firstTime ? 'deferred' : this.sortMode);
        blendMode = (blendMode) ? blendMode : (firstTime ? 'opaque'   : this.blendMode);


        if (!firstTime)
        {
            this.sortModeStack.push(this.sortMode);
            this.blendModeStack.push(this.blendMode);
        }
        this.sortMode = sortMode;
        this.blendMode = blendMode;

        this.prepareSortMode(sortMode);
        this.graphicsDevice.setTechnique(this.blendModeTechniques[blendMode]);

        return true;
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    // append sprite data to group buffer.
    _bufferSprite : function bufferSpriteFn(group, sprite)
    {
        var vertexData = group.vertexBufferData;
        var vertexBuffer = this.vertexBuffer;

        var index = group.numVertices * vertexBuffer.stride;
        var total = index + (4 * vertexBuffer.stride);
        if (total >= vertexData.length)
        {
            // allocate new vertex buffer data array.
            var size = this.bufferSizeAlgorithm(total, this.cpuStride);
            var newData = new Draw2D.prototype.floatArray(size);

            // copy data from existing buffer.
            var i;
            for (i = 0; i < index; i += 1)
            {
                newData[i] = vertexData[i];
            }

            group.vertexBufferData = vertexData = newData;
        }

        var c1 = sprite[8];
        var c2 = sprite[9];
        var c3 = sprite[10];
        var c4 = sprite[11];
        var u1 = sprite[12];
        var v1 = sprite[13];
        var u2 = sprite[14];
        var v2 = sprite[15];

        vertexData[index]      = sprite[0];
        vertexData[index + 1]  = sprite[1];
        vertexData[index + 2]  = c1;
        vertexData[index + 3]  = c2;
        vertexData[index + 4]  = c3;
        vertexData[index + 5]  = c4;
        vertexData[index + 6]  = u1;
        vertexData[index + 7]  = v1;

        vertexData[index + 8]  = sprite[2];
        vertexData[index + 9]  = sprite[3];
        vertexData[index + 10] = c1;
        vertexData[index + 11] = c2;
        vertexData[index + 12] = c3;
        vertexData[index + 13] = c4;
        vertexData[index + 14] = u2;
        vertexData[index + 15] = v1;

        vertexData[index + 16] = sprite[4];
        vertexData[index + 17] = sprite[5];
        vertexData[index + 18] = c1;
        vertexData[index + 19] = c2;
        vertexData[index + 20] = c3;
        vertexData[index + 21] = c4;
        vertexData[index + 22] = u1;
        vertexData[index + 23] = v2;

        vertexData[index + 24] = sprite[6];
        vertexData[index + 25] = sprite[7];
        vertexData[index + 26] = c1;
        vertexData[index + 27] = c2;
        vertexData[index + 28] = c3;
        vertexData[index + 29] = c4;
        vertexData[index + 30] = u2;
        vertexData[index + 31] = v2;

        group.numVertices += 4;

        // increment number of indices in present subset.
        group.indices[group.numSets - 1] += 6;
    },

    bufferMultiSprite : function bufferMultiSprite(group, buffer, count, offset)
    {
        var vertexData = group.vertexBufferData;
        var vertexBuffer = this.vertexBuffer;

        var numSprites = (count === undefined) ? Math.floor(buffer.length / 16) : count;
        count = numSprites * 16;

        offset = (offset !== undefined ? offset : 0) * 16;

        var i;
        var index = (group.numVertices * vertexBuffer.stride);
        var total = index + (numSprites * 4 * vertexBuffer.stride);
        if (total >= vertexData.length)
        {
            // allocate new vertex buffer data array.
            var size = this.bufferSizeAlgorithm(total, this.cpuStride);
            var newData = new Draw2D.prototype.floatArray(size);

            // copy data from existing buffer.
            for (i = 0; i < index; i += 1)
            {
                newData[i] = vertexData[i];
            }

            group.vertexBufferData = vertexData = newData;
        }

        var limit = offset + count;
        for (i = offset; i < limit; i += 16)
        {
            var c1 = buffer[i + 8];
            var c2 = buffer[i + 9];
            var c3 = buffer[i + 10];
            var c4 = buffer[i + 11];
            var u1 = buffer[i + 12];
            var v1 = buffer[i + 13];
            var u2 = buffer[i + 14];
            var v2 = buffer[i + 15];

            vertexData[index]      = buffer[i];
            vertexData[index + 1]  = buffer[i + 1];
            vertexData[index + 2]  = c1;
            vertexData[index + 3]  = c2;
            vertexData[index + 4]  = c3;
            vertexData[index + 5]  = c4;
            vertexData[index + 6]  = u1;
            vertexData[index + 7]  = v1;

            vertexData[index + 8]  = buffer[i + 2];
            vertexData[index + 9]  = buffer[i + 3];
            vertexData[index + 10] = c1;
            vertexData[index + 11] = c2;
            vertexData[index + 12] = c3;
            vertexData[index + 13] = c4;
            vertexData[index + 14] = u2;
            vertexData[index + 15] = v1;

            vertexData[index + 16] = buffer[i + 4];
            vertexData[index + 17] = buffer[i + 5];
            vertexData[index + 18] = c1;
            vertexData[index + 19] = c2;
            vertexData[index + 20] = c3;
            vertexData[index + 21] = c4;
            vertexData[index + 22] = u1;
            vertexData[index + 23] = v2;

            vertexData[index + 24] = buffer[i + 6];
            vertexData[index + 25] = buffer[i + 7];
            vertexData[index + 26] = c1;
            vertexData[index + 27] = c2;
            vertexData[index + 28] = c3;
            vertexData[index + 29] = c4;
            vertexData[index + 30] = u2;
            vertexData[index + 31] = v2;

            index += 32;
        }

        group.numVertices += (numSprites * 4);
        // increment number of indices in present subset.
        group.indices[group.numSets - 1] += (numSprites * 6);
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    indexData : function indexDataFn(count)
    {
        var indexData = new Draw2D.prototype.uint16Array(count);
        var i;
        var vertexIndex = 0;
        for (i = 0; i < count; i += 6)
        {
            indexData[i]     = vertexIndex;
            indexData[i + 1] = vertexIndex + 1;
            indexData[i + 2] = vertexIndex + 2;
            indexData[i + 3] = vertexIndex + 1;
            indexData[i + 4] = vertexIndex + 2;
            indexData[i + 5] = vertexIndex + 3;
            vertexIndex += 4;
        }
        return indexData;
    },

    // upload group buffer to graphics device vertexBuffer.
    uploadBuffer : function uploadBufferFn(group, count, offset)
    {
        var vertexBuffer = this.vertexBuffer;
        var vertexBufferParameters = this.vertexBufferParameters;
        var graphicsDevice = this.graphicsDevice;
        var vertexData = group.vertexBufferData;

        var performanceData = this.performanceData;

        // Resize buffers.
        if (count > vertexBufferParameters.numVertices)
        {
            var newSize = this.bufferSizeAlgorithm(count, this.gpuStride);
            if (newSize > this.maxVertices)
            {
                newSize = this.maxVertices;
            }

            vertexBufferParameters.numVertices = newSize;
            this.vertexBuffer.destroy();
            this.vertexBuffer = vertexBuffer = graphicsDevice.createVertexBuffer(vertexBufferParameters);

            // 32 bytes per vertex.
            // 2 bytes per index, 1.5 indices per vertex.
            performanceData.gpuMemoryUsage = newSize * 35; // 32 + (1.5 * 2)

            newSize *= 1.5;

            // Set indices.
            var indexBufferParameters = this.indexBufferParameters;
            indexBufferParameters.data = this.indexData(newSize);
            indexBufferParameters.numIndices = newSize;
            this.indexBuffer.destroy();
            this.indexBuffer = graphicsDevice.createIndexBuffer(indexBufferParameters);
            graphicsDevice.setIndexBuffer(this.indexBuffer);
        }

        performanceData.dataTransfers += 1;

        // Upload data.
        if (offset === 0)
        {
            vertexBuffer.setData(vertexData, 0, count);
        }
        else
        {
            var stride = vertexBuffer.stride;
            vertexBuffer.setData(vertexData.subarray(offset * stride, (offset + count) * stride), 0, count);
        }
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    drawRawImmediate : function drawRawImmediateFn(texture, multiSprite, count, offset)
    {
        var group = this.drawGroups[0];
        group.textures[0] = texture || this.defaultTexture;
        group.indices[0] = 0;
        group.numSets = 1;
        this.numGroups = 1;

        this.bufferMultiSprite(group, multiSprite, count, offset);

        // Draw render group immediately.
        this.dispatch();
    },

    drawSpriteImmediate : function drawSpriteImmediateFn(sprite)
    {
        var group = this.drawGroups[0];
        group.textures[0] = sprite._texture || this.defaultTexture;
        group.indices[0] = 0;
        group.numSets = 1;
        this.numGroups = 1;

        sprite._update(this.spriteAngleFactor);
        this._bufferSprite(group, sprite.data);

        // Draw render group immediately.
        this.dispatch();
    },

    drawImmediate : function drawImmediateFn(params)
    {
        var texture = params.texture || this.defaultTexture;
        var destRect = params.destinationRectangle;
        var srcRect = params.sourceRectangle;
        var color = params.color;
        var rotation = params.rotation;

        var group = this.drawGroups[0];
        group.textures[0] = texture;
        group.indices[0] = 0;
        group.numSets = 1;
        this.numGroups = 1;

        var drawSprite = this.drawSprite;
        Draw2DSpriteData.setFromRotatedRectangle(drawSprite, texture, destRect, srcRect, color, rotation, params.origin);
        this._bufferSprite(group, drawSprite);

        // Draw render group immediately.
        this.dispatch();
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    drawRawDeferred : function drawRawDeferredFn(texture, multiSprite, count, offset)
    {
        texture = texture || this.defaultTexture;
        var group = this.drawGroups[0];
        this.numGroups = 1;
        // If present group draw list uses a different texture
        // We must start a new draw list.
        var numSets = group.numSets;
        if (numSets === 0 || group.textures[numSets - 1] !== texture)
        {
            group.textures[numSets] = texture;
            group.indices[numSets] = 0;
            group.numSets += 1;
        }

        this.bufferMultiSprite(group, multiSprite, count, offset);
    },

    drawSpriteDeferred : function drawSpriteDeferredFn(sprite)
    {
        var texture = sprite._texture || this.defaultTexture;

        var group = this.drawGroups[0];
        this.numGroups = 1;
        // If present group draw list uses a different texture
        // We must start a new draw list.
        var numSets = group.numSets;
        if (numSets === 0 || group.textures[numSets - 1] !== texture)
        {
            group.textures[numSets] = texture;
            group.indices[numSets] = 0;
            group.numSets += 1;
        }

        sprite._update(this.spriteAngleFactor);
        this._bufferSprite(group, sprite.data);
    },

    drawDeferred : function drawDeferredFn(params)
    {
        var texture = params.texture || this.defaultTexture;

        var group = this.drawGroups[0];
        this.numGroups = 1;
        // If present group draw list uses a different texture
        // We must start a new draw list.
        var numSets = group.numSets;
        if (numSets === 0 || group.textures[numSets - 1] !== texture)
        {
            group.textures[numSets] = texture;
            group.indices[numSets] = 0;
            group.numSets += 1;
        }

        var destRect = params.destinationRectangle;
        var srcRect = params.sourceRectangle;
        var color = params.color;
        var rotation = params.rotation;

        var drawSprite = this.drawSprite;
        Draw2DSpriteData.setFromRotatedRectangle(drawSprite, texture, destRect, srcRect, color, rotation, params.origin);

        this._bufferSprite(group, drawSprite);
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    drawRawTextured : function drawRawTexturedFn(texture, multiSprite, count, offset)
    {
        texture = texture || this.defaultTexture;
        var group;
        // If last call to drawTextured used the same texture, then we need not look up render group.
        if (this.currentTextureGroup !== undefined && this.currentTextureGroup.textures[0] === texture)
        {
            group = this.currentTextureGroup;
        }
        else
        {
            // Look up render group in texLists.
            var name = texture.name;
            var texLists = this.texLists;
            group = texLists[name];
            if (!group)
            {
                // Create new render group.
                group = this.drawGroups[this.numGroups];
                if (!group)
                {
                    group = Draw2DGroup.create();
                }
                this.drawGroups[this.numGroups] = texLists[name] = group;
                group.textures[0] = texture;
                group.indices[0] = 0;
                group.numSets = 1;
                this.numGroups += 1;
            }
            this.currentTextureGroup = group;
        }

        this.bufferMultiSprite(group, multiSprite, count, offset);
    },

    drawSpriteTextured : function drawSpriteTexturedFn(sprite)
    {
        var texture = sprite._texture || this.defaultTexture;

        var group;
        // If last call to drawTextured used the same texture, then we need not look up render group.
        if (this.currentTextureGroup !== undefined && this.currentTextureGroup.textures[0] === texture)
        {
            group = this.currentTextureGroup;
        }
        else
        {
            // Look up render group in texLists.
            var name = texture.name;
            var texLists = this.texLists;
            group = texLists[name];
            if (!group)
            {
                // Create new render group.
                group = this.drawGroups[this.numGroups];
                if (!group)
                {
                    group = Draw2DGroup.create();
                }
                this.drawGroups[this.numGroups] = texLists[name] = group;
                group.textures[0] = texture;
                group.indices[0] = 0;
                group.numSets = 1;
                this.numGroups += 1;
            }
            this.currentTextureGroup = group;
        }

        sprite._update(this.spriteAngleFactor);
        this._bufferSprite(group, sprite.data);
    },

    drawTextured : function drawTexturedFn(params)
    {
        var texture = params.texture || this.defaultTexture;

        var group;
        // If last call to drawTextured used the same texture, then we need not look up render group.
        if (this.currentTextureGroup !== undefined && this.currentTextureGroup.textures[0] === texture)
        {
            group = this.currentTextureGroup;
        }
        else
        {
            // Look up render group in texLists.
            var name = texture.name;
            var texLists = this.texLists;
            group = texLists[name];
            if (!group)
            {
                // Create new render group.
                group = this.drawGroups[this.numGroups];
                if (!group)
                {
                    group = Draw2DGroup.create();
                }
                this.drawGroups[this.numGroups] = texLists[name] = group;
                group.textures[0] = texture;
                group.indices[0] = 0;
                group.numSets = 1;
                this.numGroups += 1;
            }
            this.currentTextureGroup = group;
        }

        var destRect = params.destinationRectangle;
        var srcRect = params.sourceRectangle;
        var color = params.color;
        var rotation = params.rotation;

        var drawSprite = this.drawSprite;
        Draw2DSpriteData.setFromRotatedRectangle(drawSprite, texture, destRect, srcRect, color, rotation, params.origin);

        this._bufferSprite(group, drawSprite);
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    prepareSortMode : function refreshSortModeFn(sortMode)
    {
        if (sortMode === 'deferred')
        {
            this.draw = this.drawDeferred;
            this.drawSprite = this.drawSpriteDeferred;
            this.drawRaw = this.drawRawDeferred;
        }
        else if (sortMode === 'immediate')
        {
            this.draw = this.drawImmediate;
            this.drawSprite = this.drawSpriteImmediate;
            this.drawRaw = this.drawRawImmediate;
        }
        else
        {
            this.draw = this.drawTextured;
            this.drawSprite = this.drawSpriteTextured;
            this.drawRaw = this.drawRawTextured;
        }
    },

    ///////////////////////////////////////////////////////////////////////////////////////////////

    end: function endFn()
    {
        if (this.state !== this.drawStates.draw)
        {
            return false;
        }

        //dispatch objects to the graphics card
        if (this.dispatch())
        {
            this.clearBatch();
        }

        if (this.blendModeStack.length !== 0)
        {
            this.blendMode = this.blendModeStack.pop();
            this.sortMode = this.sortModeStack.pop();
            this.prepareSortMode(this.sortMode);
            this.graphicsDevice.setTechnique(this.blendModeTechniques[this.blendMode]);
        }
        else
        {
            this.blendMode = undefined;
            this.sortMode = undefined;
            this.state = this.drawStates.ready;
        }

        return true;
    },

    dispatch: function dispatchFn()
    {
        // Nothing to dispatch.
        var numGroups = this.numGroups;
        if (numGroups === 0)
        {
            return false;
        }

        var graphicsDevice = this.graphicsDevice;
        var techniqueParameters = this.techniqueParameters;
        graphicsDevice.setIndexBuffer(this.indexBuffer);

        var drawGroups = this.drawGroups;
        var renderTargetUsed = false;
        if (this.currentRenderTarget)
        {
            renderTargetUsed = graphicsDevice.beginRenderTarget(this.currentRenderTarget.renderTarget);
        }

        var performanceData = this.performanceData;

        var i;
        for (i = 0; i < numGroups; i += 1)
        {
            var group = drawGroups[i];

            var textures = group.textures;
            var indices = group.indices;
            var setIndex = 0;

            var vindex = 0;
            var vlimit = group.numVertices;
            while (vindex < vlimit)
            {
                // number of vertices remaining.
                var vcount = vlimit - vindex;
                if (vcount > this.maxVertices)
                {
                    vcount = this.maxVertices;
                }

                // Upload group vertex sub-buffer to graphics device.
                this.uploadBuffer(group, vcount, vindex);
                graphicsDevice.setStream(this.vertexBuffer, this.semantics);

                // sprite uses 4 vertices, and 6 indices
                // so for 'vcount' number of vertices, we have vcount * 1.5 indices
                var ilimit = vcount * 1.5;
                var iindex = 0;
                while (iindex < ilimit) {
                    techniqueParameters.texture = textures[setIndex];

                    // number of indices remaining to render.
                    var icount = ilimit - iindex;
                    if (icount >= indices[setIndex])
                    {
                        // finish rendering sub list.
                        icount = indices[setIndex];
                        setIndex += 1;
                    }
                    else
                    {
                        // sub list still has remaining indices to render.
                        indices[setIndex] -= icount;
                    }

                    var batchSize = icount / 6;
                    if (performanceData.batchCount === 0)
                    {
                        performanceData.minBatchSize = batchSize;
                        performanceData.maxBatchSize = batchSize;
                        performanceData.avgBatchSize = batchSize;
                        performanceData.batchCount = 1;
                    }
                    else
                    {
                        if (batchSize < performanceData.minBatchSize)
                        {
                            performanceData.minBatchSize = batchSize;
                        }
                        if (batchSize > performanceData.maxBatchSize)
                        {
                            performanceData.maxBatchSize = batchSize;
                        }
                        performanceData.avgBatchSize *= performanceData.batchCount;
                        performanceData.avgBatchSize += batchSize;
                        performanceData.batchCount += 1;
                        performanceData.avgBatchSize /= performanceData.batchCount;
                    }

                    graphicsDevice.setTechniqueParameters(techniqueParameters);
                    graphicsDevice.drawIndexed(graphicsDevice.PRIMITIVE_TRIANGLES, icount, iindex);

                    iindex += icount;
                }

                vindex += vcount;
            }

            group.numSets = 0;
            group.numVertices = 0;
        }

        if (this.currentRenderTarget && renderTargetUsed)
        {
            graphicsDevice.endRenderTarget();
        }

        return true;
    },

    bufferSizeAlgorithm : function bufferSizeAlgorithmFn(target, stride)
    {
        // scale factor of 2 is asymtopically optimal in terms of number of resizes
        // performed and copies performed, but we want to try and conserve memory
        // and so choose a less optimal 1.25 so that buffer will never be too much
        // larger than necessary.
        var factor = 1.25;

        // We size buffer to the next power of the factor which is >= target
        var logf = Math.ceil(Math.log(target) / Math.log(factor));
        var size = Math.floor(Math.pow(factor, logf));

        // Additionally ensure that we always take a multiple of of the stride
        // to avoid wasted bytes that could never be used.
        return (stride * Math.ceil(size / stride));
    },

    updateRenderTargetVbo : function updateRenderTargetVboFn(viewX, viewY, viewWidth, viewHeight)
    {
        var graphicsDevice = this.graphicsDevice;
        var halfGraphicsDeviceWidth = 0.5 * graphicsDevice.width;
        var halfGraphicsDeviceHeight = 0.5 * graphicsDevice.height;

        //
        // Update the VBO for the presentRenderTarget
        //
        var vertexBuffer = this.copyVertexBuffer;

        var left = (viewX - halfGraphicsDeviceWidth) / halfGraphicsDeviceWidth;
        var right = (viewX + viewWidth - halfGraphicsDeviceWidth) / halfGraphicsDeviceWidth;
        var topv = (viewY - halfGraphicsDeviceHeight) / halfGraphicsDeviceHeight;
        var bottom = (viewY + viewHeight - halfGraphicsDeviceHeight) / halfGraphicsDeviceHeight;

        var vertexData = this.vertexBufferData;
        vertexData[0] = left;
        vertexData[1] = bottom;
        vertexData[2] = 0.0;
        vertexData[3] = 1.0;

        vertexData[4] = left;
        vertexData[5] = topv;
        vertexData[6] = 0.0;
        vertexData[7] = 0.0;

        vertexData[8] = right;
        vertexData[9] = bottom;
        vertexData[10] = 1.0;
        vertexData[11] = 1.0;

        vertexData[12] = right;
        vertexData[13] = topv;
        vertexData[14] = 1.0;
        vertexData[15] = 0.0;

        vertexBuffer.setData(vertexData, 0, 4);
    },

    // always overallocate.
    /*jshint bitwise: false*/
    makePow2 : function makePow2Fn(dim)
    {
        var index = Math.log(dim) / Math.log(2);
        return (1 << Math.ceil(index));
    },
    /*jshint bitwise: true*/

    createRenderTarget : function createRenderTargetFn(params)
    {
        var gd = this.graphicsDevice;
        var renderTargets = this.renderTargetStructs;
        var index = renderTargets.length;

        var name = (params && params.name) ? params.name : ("RenderTarget#" + index);
        var backBuffer = (params && params.backBuffer !== undefined) ? params.backBuffer : true;
        var matchScreen = (params.width === undefined || params.height === undefined);

        var texParams = this.renderTargetTextureParameters;
        texParams.name = name;

        var width  = (matchScreen) ? gd.width  : params.width;
        var height = (matchScreen) ? gd.height : params.height;
        texParams.width  = this.makePow2(width);
        texParams.height = this.makePow2(height);

        var texture = gd.createTexture(texParams);
        var targetParams = this.renderTargetParams;
        targetParams.colorTexture0 = texture;
        var renderTarget = gd.createRenderTarget(targetParams);

        renderTargets.push({
            managed : matchScreen,
            renderTarget : renderTarget,
            texture : texture,
            backBuffer : backBuffer,
            actualWidth  : (backBuffer ? width  : texture.width),
            actualHeight : (backBuffer ? height : texture.height)
        });

        return index;
    },

    validateTarget : function validateTargetFn(target, viewWidth, viewHeight)
    {
        if (target.managed)
        {
            var tex = target.texture;
            if (target.backBuffer)
            {
                target.actualWidth = viewWidth;
                target.actualHeight = viewHeight;
            }
            viewWidth = this.makePow2(viewWidth);
            viewHeight = this.makePow2(viewHeight);
            if (!target.backBuffer)
            {
                target.actualWidth = viewWidth;
                target.actualHeight = viewHeight;
            }
            if (tex.width !== viewWidth || tex.height !== viewHeight)
            {
                var texParams = this.renderTargetTextureParameters;
                var targetParams = this.renderTargetParams;

                texParams.name = tex.name;
                texParams.width  = viewWidth;
                texParams.height = viewHeight;

                tex.destroy();
                target.renderTarget.destroy();

                var graphicsDevice = this.graphicsDevice;
                target.texture = graphicsDevice.createTexture(texParams);
                targetParams.colorTexture0 = target.texture;
                target.renderTarget = graphicsDevice.createRenderTarget(targetParams);
            }
        }
    },

    setBackBuffer : function setBackBufferFn()
    {
        if (this.state !== this.drawStates.ready)
        {
            return false;
        }

        this.currentRenderTarget = null;
        this.forceUpdate = true;

        return true;
    },

    getRenderTargetTexture : function getRenderTargetTextureFn(renderTargetIndex)
    {
        var renderTargets = this.renderTargetStructs;
        if (renderTargetIndex < 0 || renderTargetIndex >= renderTargets.length)
        {
            return null;
        }

        return renderTargets[renderTargetIndex].texture;
    },

    getRenderTarget : function getRenderTargetFn(renderTargetIndex)
    {
        var renderTargets = this.renderTargetStructs;
        if (renderTargetIndex < 0 || renderTargetIndex >= renderTargets.length)
        {
            return null;
        }

        return renderTargets[renderTargetIndex].renderTarget;
    },

    setRenderTarget : function setRenderTargetFn(renderTargetIndex)
    {
        var renderTargets = this.renderTargetStructs;
        if (renderTargetIndex < 0 || renderTargetIndex >= renderTargets.length)
        {
            return false;
        }

        if (this.state !== this.drawStates.ready)
        {
            return false;
        }

        this.currentRenderTarget = renderTargets[renderTargetIndex];
        this.forceUpdate = true;

        return true;
    },

    copyRenderTarget: function copyRenderTargetFn(renderTargetIndex)
    {
        if (this.state !== this.drawStates.ready)
        {
            return false;
        }

        var renderTargets = this.renderTargetStructs;
        if (renderTargetIndex < 0 || renderTargetIndex >= renderTargets.length)
        {
            return false;
        }

        // Check the buffers are correct before we render.
        this.update();

        if (!this.currentRenderTarget)
        {
            this.graphicsDevice.setScissor(this.scissorX, this.scissorY, this.scissorWidth, this.scissorHeight);
        }

        var graphicsDevice = this.graphicsDevice;
        var target = renderTargets[renderTargetIndex];
        var tex = target.texture;

        var technique = this.copyTechnique;
        var params = this.copyTechniqueParameters;
        var copyUVScale = params.copyUVScale;
        copyUVScale[0] = target.actualWidth / tex.width;
        copyUVScale[1] = target.actualHeight / tex.height;
        params.copyFlip = (!this.currentRenderTarget ? -1.0 : 1.0);
        params.inputTexture0 = tex;

        var renderTargetUsed = false;
        var currentTarget = this.currentRenderTarget;
        var vbo = this.copyVertexBuffer;
        if (currentTarget)
        {
            renderTargetUsed = graphicsDevice.beginRenderTarget(currentTarget.renderTarget);
        }

        graphicsDevice.setTechnique(technique);
        graphicsDevice.setTechniqueParameters(params);

        graphicsDevice.setStream(vbo, this.quadSemantics);
        graphicsDevice.draw(this.quadPrimitive, 4, 0);

        if (currentTarget && renderTargetUsed)
        {
            graphicsDevice.endRenderTarget();
        }

        return true;
    },

    resetPerformanceData : function resetPerformanceDataFn()
    {
        var data = this.performanceData;
        data.minBatchSize = data.maxBatchSize = data.avgBatchSize = undefined;
        data.batchCount = 0;
        data.dataTransfers = 0;
    }
};

// Constructor function
//
// params : {
//    graphicsDevice : gd,
//    blendModes : { // optional
//       name : Technique,
//       **repeated**
//    }
// }
Draw2D.create = function draw2DCreateFn(params)
{
    var o = new Draw2D();
    var gd = o.graphicsDevice = params.graphicsDevice;

    // Current sort and blend mode.
    o.sortMode  = undefined;
    o.blendMode = undefined;
    // Disjoint stack of modes for nested begins.
    o.sortModeStack  = [];
    o.blendModeStack = [];

    // Set of render groups to be dispatched.
    o.drawGroups = [Draw2DGroup.create()];
    o.numGroups = 0;

    // Set of render groups for texture sort mode.
    // dictionary on texture name.
    o.texLists = [];
    // Cached reference to last retrieved group to accelerate
    // texture sort mode draw calls.
    o.texGroup = undefined;

    // Sprite data instance used for rectangle draw calls.
    o.drawSprite = Draw2DSpriteData.create();

    // Solid fill texture for draw calls that do not specify a texture.
    o.defaultTexture = gd.createTexture({
        name : "DefaultDraw2DTexture",
        width : 1,
        height : 1,
        depth : 1,
        format : "L8",
        cubemap : false,
        mipmaps : true,
        renderable : false,
        dynamic : false,
        data : [0xff]
    });

    // Draw call methods.
    // These are set based on current sort mode.
    o.draw = undefined;
    o.drawSprite = undefined;
    o.drawRaw = undefined;

    // Load embedded default shader and techniques
    /*jshint white: false*/
    var shader = gd.createShader(
{
 "version": 1,
 "name": "draw2D.cgfx",
 "samplers":
 {
  "texture":
  {
   "MinFilter": 9985,
   "MagFilter": 9729,
   "WrapS": 33071,
   "WrapT": 33071
  },
  "inputTexture0":
  {
   "MinFilter": 9728,
   "MagFilter": 9729,
   "WrapS": 33071,
   "WrapT": 33071
  }
 },
 "parameters":
 {
  "clipSpace":
  {
   "type": "float",
   "columns": 4
  },
  "copyUVScale":
  {
   "type": "float",
   "columns": 2
  },
  "copyFlip":
  {
   "type": "float"
  },
  "texture":
  {
   "type": "sampler2D"
  },
  "inputTexture0":
  {
   "type": "sampler2D"
  }
 },
 "techniques":
 {
  "opaque":
  [
   {
    "parameters": ["clipSpace","texture"],
    "semantics": ["POSITION","COLOR","TEXCOORD0"],
    "states":
    {
     "DepthTestEnable": false,
     "DepthMask": false,
     "CullFaceEnable": false,
     "BlendEnable": false
    },
    "programs": ["vp_draw2D","fp_draw2D"]
   }
  ],
  "alpha":
  [
   {
    "parameters": ["clipSpace","texture"],
    "semantics": ["POSITION","COLOR","TEXCOORD0"],
    "states":
    {
     "DepthTestEnable": false,
     "DepthMask": false,
     "CullFaceEnable": false,
     "BlendEnable": true,
     "BlendFunc": [770,771]
    },
    "programs": ["vp_draw2D","fp_draw2D"]
   }
  ],
  "additive":
  [
   {
    "parameters": ["clipSpace","texture"],
    "semantics": ["POSITION","COLOR","TEXCOORD0"],
    "states":
    {
     "DepthTestEnable": false,
     "DepthMask": false,
     "CullFaceEnable": false,
     "BlendEnable": true,
     "BlendFunc": [770,1]
    },
    "programs": ["vp_draw2D","fp_draw2D"]
   }
  ],
  "copy":
  [
   {
    "parameters": ["copyUVScale","copyFlip","inputTexture0"],
    "semantics": ["POSITION","TEXCOORD0"],
    "states":
    {
     "DepthTestEnable": false,
     "DepthMask": false,
     "CullFaceEnable": false,
     "BlendEnable": false
    },
    "programs": ["vp_copy","fp_copy"]
   }
  ]
 },
 "programs":
 {
  "fp_copy":
  {
   "type": "fragment",
   "code": "#ifdef GL_ES\n#define TZ_LOWP lowp\nprecision mediump float;\nprecision mediump int;\n#else\n#define TZ_LOWP\n#endif\nvarying vec4 tz_TexCoord[8];\nvec4 _ret_0;uniform sampler2D inputTexture0;void main()\n{_ret_0=texture2D(inputTexture0,tz_TexCoord[0].xy);gl_FragColor=_ret_0;}"
  },
  "vp_copy":
  {
   "type": "vertex",
   "code": "#ifdef GL_ES\n#define TZ_LOWP lowp\nprecision mediump float;\nprecision mediump int;\n#else\n#define TZ_LOWP\n#endif\nvarying vec4 tz_TexCoord[8];attribute vec4 ATTR8;attribute vec4 ATTR0;\nvec4 _OutPosition1;vec2 _OutUV1;uniform vec2 copyUVScale;uniform float copyFlip;void main()\n{_OutPosition1.x=ATTR0.x;_OutPosition1.y=ATTR0.y*copyFlip;_OutPosition1.zw=ATTR0.zw;_OutUV1=ATTR8.xy*copyUVScale;tz_TexCoord[0].xy=_OutUV1;gl_Position=_OutPosition1;}"
  },
  "fp_draw2D":
  {
   "type": "fragment",
   "code": "#ifdef GL_ES\n#define TZ_LOWP lowp\nprecision mediump float;\nprecision mediump int;\n#else\n#define TZ_LOWP\n#endif\nvarying TZ_LOWP vec4 tz_Color;varying vec4 tz_TexCoord[8];\nvec4 _ret_0;vec4 _TMP0;uniform sampler2D texture;void main()\n{_TMP0=texture2D(texture,tz_TexCoord[0].xy);_ret_0=tz_Color*_TMP0;gl_FragColor=_ret_0;}"
  },
  "vp_draw2D":
  {
   "type": "vertex",
   "code": "#ifdef GL_ES\n#define TZ_LOWP lowp\nprecision mediump float;\nprecision mediump int;\n#else\n#define TZ_LOWP\n#endif\nvarying TZ_LOWP vec4 tz_Color;varying vec4 tz_TexCoord[8];attribute vec4 ATTR8;attribute vec4 ATTR3;attribute vec4 ATTR0;\nvec4 _OUTPosition1;vec4 _OUTColor1;vec2 _OUTTexCoord01;uniform vec4 clipSpace;void main()\n{vec2 _position;_position=ATTR0.xy*clipSpace.xy+clipSpace.zw;_OUTPosition1.x=_position.x;_OUTPosition1.y=_position.y;_OUTPosition1.z=0.0;_OUTPosition1.w=1.0;_OUTColor1=ATTR3;_OUTTexCoord01=ATTR8.xy;tz_TexCoord[0].xy=ATTR8.xy;tz_Color=ATTR3;gl_Position=_OUTPosition1;}"
  }
 }
}
    );
    /*jshint white: true*/

    // supported blend modes.
    o.blend = {
        additive : 'additive',
        alpha    : 'alpha',
        opaque   : 'opaque'
    },

    // Mapping from blend mode name to Technique object.
    o.blendModeTechniques = {};
    o.blendModeTechniques.additive = shader.getTechnique("additive");
    o.blendModeTechniques.alpha    = shader.getTechnique("alpha");
    o.blendModeTechniques.opaque   = shader.getTechnique("opaque");

    // Append techniques and supported blend modes with user supplied techniques.
    if (params.blendModes)
    {
        for (var name in params.blendModes)
        {
            if (params.blendModes.hasOwnProperty(name))
            {
                o.blend[name] = name;
                o.blendModeTechniques[name] = params.blendModes[name];
            }
        }
    }

    // Blending techniques.
    o.techniqueParameters = gd.createTechniqueParameters({
        clipSpace: new Draw2D.prototype.floatArray(4),
        texture: null
    });

    // Current render target
    o.currentRenderTarget = null;
    o.renderTargetStructs = [];

    o.state = o.drawStates.ready;

    o.scaleMode = 'none';
    o.blendMode = 'opaque';

    // View port, back buffer and managed render target values.
    o.width = 0;
    o.height = 0;

    o.scissorX = 0;
    o.scissorY = 0;
    o.scissorWidth = o.graphicsDevice.width;
    o.scissorHeight = o.graphicsDevice.height;

    o.clipOffsetX = -1.0;
    o.clipOffsetY = 1;
    o.clipScaleX = 2.0 / o.graphicsDevice.width;
    o.clipScaleY = -2.0 / o.graphicsDevice.height;

    o.viewScaleX = 1;
    o.viewScaleY = 1;

    // GPU Memory.
    // -----------

    var initial = (params.initialGpuMemory ? params.initialGpuMemory : 0);
    if (initial < 140)
    {
        // 140 = minimum that can be used to draw a single sprite.
        initial = 140;
    }
    if (initial > 2293760)
    {
        // 2293760 = maximum that can ever be used in 16bit indices.
        initial = 2293760;
    }

    o.performanceData = {
        gpuMemoryUsage : initial,
        minBatchSize : 0,
        maxBatchSize : 0,
        avgBatchSize : 0,
        batchCount : 0,
        dataTransfers : 0
    };

    o.maxGpuMemory = (params.maxGpuMemory ? params.maxGpuMemory : 2293760);
    if (o.maxGpuMemory < initial)
    {
        o.maxGpuMemory = initial;
    }

    var initialVertices = Math.floor(initial / 140) * 4;
    o.maxVertices = Math.floor(o.maxGpuMemory / 140) * 4;
    if (o.maxVertices > 65536)
    {
        o.maxVertices = 65536;
    }

    // number of bytes used per-sprite on cpu vertex buffers.
    o.cpuStride = 64;

    // vertex buffer is in terms of number of vertices.
    // so we have a stride of 4 rather than 128.
    o.gpuStride = 4;

    // Index and vertex buffer setup.
    o.vertexBufferParameters = {
        numVertices: initialVertices,
        attributes: [gd.VERTEXFORMAT_FLOAT2, gd.VERTEXFORMAT_FLOAT4, gd.VERTEXFORMAT_FLOAT2],
        'transient': true
    };
    o.vertexBuffer = gd.createVertexBuffer(o.vertexBufferParameters);

    o.semantics = gd.createSemantics([gd.SEMANTIC_POSITION, gd.SEMANTIC_COLOR, gd.SEMANTIC_TEXCOORD0]);
    o.indexBufferParameters = {
        numIndices: (initialVertices * 1.5),
        format: gd.INDEXFORMAT_USHORT,
        dynamic: false,
        data : o.indexData((initialVertices * 1.5))
    };
    o.indexBuffer = gd.createIndexBuffer(o.indexBufferParameters);

    // Render Target API
    // -----------------

    // Objects and values used in render target management.
    o.renderTargetIndex = 0;
    o.renderTargetCount = 0;

    o.renderTargetTextureParameters = {
        name   : '',
        width  : 0,
        height : 0,
        depth  : 1,
        format     : "R8G8B8A8",
        cubemap    : false,
        mipmaps    : true,
        renderable : true,
        dynamic    : true
    };

    o.renderTargetParams = {
        colorTexture0 : null
    };

    // Render Target copying.
    // ----------------------

    // Copy technique for copyRenderTarget
    o.copyTechnique = shader.getTechnique("copy");
    o.copyTechniqueParameters = gd.createTechniqueParameters({
        inputTexture0 : null,
        copyFlip : 1,
        copyUVScale : new Draw2D.prototype.floatArray([1, 1])
    });

    // Objects used in copyRenderTarget method.
    o.quadSemantics = gd.createSemantics([gd.SEMANTIC_POSITION, gd.SEMANTIC_TEXCOORD0]);
    o.quadPrimitive = gd.PRIMITIVE_TRIANGLE_STRIP;

    o.copyVertexBufferParams = {
        numVertices: 4,
        attributes: [gd.VERTEXFORMAT_FLOAT2, gd.VERTEXFORMAT_FLOAT2],
        'transient': true
    };
    o.copyVertexBuffer = gd.createVertexBuffer(o.copyVertexBufferParams);

    // updateRenderTargetVBO
    // ---------------------

    /*jshint white: false*/
    o.vertexBufferData = new Draw2D.prototype.floatArray([-1.0, -1.0, 0.0, 0.0,
                                                           1.0, -1.0, 1.0, 0.0,
                                                          -1.0,  1.0, 0.0, 1.0,
                                                           1.0,  1.0, 1.0, 1.0]);
    /*jshint white: true*/

    return o;
};

// Detect correct typed arrays
(function () {
    Draw2D.prototype.uint16Array = function (arg) {
        if (arguments.length === 0)
        {
            return [];
        }

        var i, ret;
        if (typeof arg === "number")
        {
            ret = new Array(arg);
        }
        else
        {
            ret = [];
            for (i = 0; i < arg.length; i += 1)
            {
                ret[i] = arg[i];
            }
        }
        return ret;
    };

    var testArray;
    var textDescriptor;

    if (typeof Uint16Array !== "undefined")
    {
        testArray = new Uint16Array(4);
        textDescriptor = Object.prototype.toString.call(testArray);
        if (textDescriptor === '[object Uint16Array]')
        {
            Draw2D.prototype.uint16Array = Uint16Array;
        }
    }

    Draw2D.prototype.floatArray = function (arg) {
        if (arguments.length === 0)
        {
            return [];
        }

        var i, ret;
        if (typeof arg === "number")
        {
            ret = new Array(arg);
        }
        else
        {
            ret = [];
            for (i = 0; i < arg.length; i += 1)
            {
                ret[i] = arg[i];
            }
        }
        return ret;
    };

    if (typeof Float32Array !== "undefined")
    {
        testArray = new Float32Array(4);
        textDescriptor = Object.prototype.toString.call(testArray);
        if (textDescriptor === '[object Float32Array]')
        {
            Draw2D.prototype.floatArray = Float32Array;
            Draw2D.prototype.defaultClearColor = new Float32Array(Draw2D.prototype.defaultClearColor);
        }
    }
}());
