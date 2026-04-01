/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

package spine.flixel;

import flixel.util.FlxDirectionFlags;
import flixel.math.FlxRect;
import flixel.FlxCamera;
import flixel.FlxBasic;
import flixel.util.FlxAxes;
import flixel.group.FlxGroup.FlxTypedGroup;
import spine.boundsprovider.SetupPoseBoundsProvider;
import spine.boundsprovider.BoundsProvider;
import openfl.geom.Point;
import flixel.math.FlxPoint;
import flixel.math.FlxMatrix;
import spine.TextureRegion;
import flixel.FlxG;
import flixel.FlxObject;
import flixel.util.FlxColor;
import openfl.Vector;
import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.animation.AnimationState;
import spine.animation.AnimationStateData;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.ClippingAttachment;
import spine.flixel.SkeletonMesh;

/** A FlxObject that draws a skeleton. The animation state and skeleton must be updated each frame. */
class SkeletonSprite extends FlxTypedGroup<FlxObject> {
	public var skeleton(default, null):Skeleton;
	public var state(default, null):AnimationState;
	public var stateData(default, null):AnimationStateData;
	public var beforeUpdateWorldTransforms:SkeletonSprite->Void = function(_) {};
	public var afterUpdateWorldTransforms:SkeletonSprite->Void = function(_) {};

	public static var clipper(default, never):SkeletonClipping = new SkeletonClipping();

	public var offsetX = .0;
	public var offsetY = .0;
	public var alpha = 1.; // TODO: clamp
	public var color:FlxColor = 0xffffff;
	public var flipX(default, set):Bool = false;
	public var flipY(default, set):Bool = false;
	public var antialiasing:Bool = true;

	public var boundsProvider:BoundsProvider;

	public var angle(default, set) = 0.;
	public var x(default, set) = 0.;
	public var y(default, set) = 0.;
	public var width(get, set):Float;
	public var height(get, set):Float;

	/** The bounds of the gameobject. */
	public var bounds(get, never):Rectangle;

	@:isVar
	public var scale(never, set):FlxPoint;
	@:isVar
	public var scaleX(get, set):Float = 1;
	@:isVar
	public var scaleY(get, set):Float = 1;

	var _tempVertices:Array<Float> = new Array<Float>();
	var _quadTriangles:Array<Int>;
	var _meshes(default, null):Array<SkeletonMesh> = new Array<SkeletonMesh>();

	private var _tempMatrix = new FlxMatrix();
	private var _tempPoint = new Point();
	private var _tempPointFlip = [.0, .0];
	private var __bounds = new openfl.geom.Rectangle();
	private var __objectBounds = new FlxObject();

	private static var QUAD_INDICES:Array<Int> = [0, 1, 2, 2, 3, 0];

	/** Creates an uninitialized SkeletonSprite. The renderer, skeleton, and animation state must be set before use. */
	public function new(skeletonData:SkeletonData, animationStateData:AnimationStateData = null, ?boundsProvider:BoundsProvider) {
		super(1);
		Bone.yDown = true;
		skeleton = new Skeleton(skeletonData);
		skeleton.updateWorldTransform(Physics.update);
		state = new AnimationState(animationStateData != null ? animationStateData : new AnimationStateData(skeletonData));
		// setBoundingBox();
		this.boundsProvider = boundsProvider ?? new SetupPoseBoundsProvider();
		this.calculateBounds();
		add(__objectBounds);
	}

	// TODO: this changes the scale
	// public function setSize(width:Float, height:Float):Void {
	// 	this.width = width;
	// 	this.height = height;
	// }
	// ============================================================
	// DEBUG METHODS (if FLX_DEBUG)
	// ============================================================
	#if FLX_DEBUG
	public function drawDebug():Void {
		__objectBounds.drawDebug();
	}

	public function drawDebugOnCamera(camera:FlxCamera):Void {
		__objectBounds.drawDebugOnCamera(camera);
	}
	#end

	// ============================================================
	// SKELETON SPRITE METHODS
	// ============================================================
	public function calculateBounds() {
		this.boundsProvider.calculateBounds(this, __bounds);
		__objectBounds.setPosition(x + __bounds.x, y + __bounds.y);
		__objectBounds.setSize(__bounds.width, __bounds.height);
	}

	function renderMeshes():Void {
		var clipper:SkeletonClipping = SkeletonSprite.clipper;
		var drawOrder:Array<Slot> = skeleton.drawOrder.appliedPose;
		var attachmentColor:spine.Color;
		var mesh:SkeletonMesh = null;
		var numVertices:Int;
		var numFloats:Int;
		var triangles:Array<Int> = null;
		var uvs:Array<Float>;
		var twoColorTint:Bool = false;
		var vertexSize:Int = twoColorTint ? 12 : 8;
		_tempMatrix = getTransformMatrix();
		for (slot in drawOrder) {
			// no two tint color support and tint is passed as parameter to mesh, so vertex size is 2
			// var clippedVertexSize = clipper.isClipping() ? 2 : vertexSize;
			var clippedVertexSize = 2;
			if (!slot.bone.active) {
				clipper.clipEnd(slot);
				continue;
			}

			var worldVertices:Array<Float> = _tempVertices;
			var pose = slot.appliedPose;
			var attachment = pose.attachment;
			if (Std.isOfType(attachment, RegionAttachment)) {
				var region:RegionAttachment = cast(attachment, RegionAttachment);
				numVertices = 4;
				numFloats = clippedVertexSize << 2;
				if (numFloats > worldVertices.length)
					worldVertices.resize(numFloats);
				var sequence = region.sequence;
				var sequenceIndex = sequence.resolveIndex(pose);
				region.computeWorldVertices(slot, sequence.offsets[sequenceIndex], worldVertices, 0, clippedVertexSize);

				mesh = getFlixelMeshFromRendererAttachment(region);
				mesh.graphic = sequence.regions[sequenceIndex].texture;
				triangles = QUAD_INDICES;
				uvs = sequence.getUVs(sequenceIndex);
				attachmentColor = region.color;
			} else if (Std.isOfType(attachment, MeshAttachment)) {
				var meshAttachment:MeshAttachment = cast(attachment, MeshAttachment);
				numVertices = meshAttachment.worldVerticesLength >> 1;
				numFloats = numVertices * clippedVertexSize; // 8 for now because I'm excluding clipping
				if (numFloats > worldVertices.length) {
					worldVertices.resize(numFloats);
				}
				meshAttachment.computeWorldVertices(skeleton, slot, 0, meshAttachment.worldVerticesLength, worldVertices, 0, clippedVertexSize);

				var sequence = meshAttachment.sequence;
				var sequenceIndex = sequence.resolveIndex(pose);
				mesh = getFlixelMeshFromRendererAttachment(meshAttachment);
				mesh.graphic = sequence.regions[sequenceIndex].texture;
				triangles = meshAttachment.triangles;
				uvs = sequence.getUVs(sequenceIndex);
				attachmentColor = meshAttachment.color;
			} else if (Std.isOfType(attachment, ClippingAttachment)) {
				var clip:ClippingAttachment = cast(attachment, ClippingAttachment);
				clipper.clipEnd(slot);
				clipper.clipStart(skeleton, slot, clip);
				continue;
			} else {
				clipper.clipEnd(slot);
				continue;
			}

			if (mesh != null) {
				// cannot use directly mesh.color.setRGBFloat otherwise the setter won't be called and transfor color not set
				mesh.color = FlxColor.fromRGBFloat(skeleton.color.r * pose.color.r * attachmentColor.r * color.redFloat,
					skeleton.color.g * pose.color.g * attachmentColor.g * color.greenFloat,
					skeleton.color.b * pose.color.b * attachmentColor.b * color.blueFloat, 1);
				mesh.alpha = skeleton.color.a * pose.color.a * attachmentColor.a * alpha;

				if (clipper.isClipping() && clipper.clipTriangles(worldVertices, triangles, triangles.length, uvs)) {
					mesh.indices = Vector.ofArray(clipper.clippedTriangles);
					mesh.uvtData = Vector.ofArray(clipper.clippedUvs);
					if (angle == 0) {
						mesh.vertices = Vector.ofArray(clipper.clippedVertices);
						mesh.x = x + offsetX;
						mesh.y = y + offsetY;
					} else {
						var i = 0;
						mesh.vertices.length = clipper.clippedVertices.length;
						while (i < mesh.vertices.length) {
							_tempPoint.setTo(clipper.clippedVertices[i], clipper.clippedVertices[i + 1]);
							_tempPoint = _tempMatrix.transformPoint(_tempPoint);
							mesh.vertices[i] = _tempPoint.x;
							mesh.vertices[i + 1] = _tempPoint.y;
							i += 2;
						}
					}
				} else {
					var n = numFloats;
					var i = 0;
					mesh.vertices.length = numVertices;
					while (i < n) {
						if (angle == 0) {
							mesh.vertices[i] = worldVertices[i];
							mesh.vertices[i + 1] = worldVertices[i + 1];
						} else {
							_tempPoint.setTo(worldVertices[i], worldVertices[i + 1]);
							_tempPoint = _tempMatrix.transformPoint(_tempPoint);
							mesh.vertices[i] = _tempPoint.x;
							mesh.vertices[i + 1] = _tempPoint.y;
						}
						i += 2;
					}
					if (angle == 0) {
						mesh.x = x + offsetX;
						mesh.y = y + offsetY;
					}
					mesh.indices = Vector.ofArray(triangles);
					mesh.uvtData = Vector.ofArray(uvs);
				}

				mesh.antialiasing = antialiasing;
				mesh.blend = SpineTexture.toFlixelBlending(slot.data.blendMode);
				// x/y position works for mesh, but angle does not work.
				// if the transformation matrix is moved into the FlxStrip draw and used there
				// we can just put vertices without doing any transformation
				// mesh.x = x + offsetX;
				// mesh.y = y + offsetY;
				// mesh.angle = angle;
				mesh.draw();
			}

			clipper.clipEnd(slot);
		}
		clipper.clipEnd();
	}

	private function getTransformMatrix():FlxMatrix {
		_tempMatrix.identity();
		// scale is connected to the skeleton scale - no need to rescale
		// _tempMatrix.scale(1, 1);
		_tempMatrix.rotate(angle * Math.PI / 180);
		_tempMatrix.translate(x + offsetX, y + offsetY);
		return _tempMatrix;
	}

	public function skeletonToHaxeWorldCoordinates(point:Array<Float>):Void {
		var transform = getTransformMatrix();
		var a = transform.a,
			b = transform.b,
			c = transform.c,
			d = transform.d,
			tx = transform.tx,
			ty = transform.ty;
		var x = point[0];
		var y = point[1];
		point[0] = x * a + y * c + tx;
		point[1] = x * b + y * d + ty;
	}

	public function haxeWorldCoordinatesToSkeleton(point:Array<Float>):Void {
		var transform = getTransformMatrix().invert();
		var a = transform.a,
			b = transform.b,
			c = transform.c,
			d = transform.d,
			tx = transform.tx,
			ty = transform.ty;
		var x = point[0];
		var y = point[1];
		point[0] = x * a + y * c + tx;
		point[1] = x * b + y * d + ty;
	}

	public function haxeWorldCoordinatesToBone(point:Array<Float>, bone:Bone):Void {
		this.haxeWorldCoordinatesToSkeleton(point);
		var parentBone = bone.parent;
		if (parentBone != null) {
			parentBone.appliedPose.worldToLocal(point);
		} else {
			bone.appliedPose.worldToLocal(point);
		}
	}

	private function getFlixelMeshFromRendererAttachment(region:RenderedAttachment) {
		if (region.rendererObject == null) {
			var skeletonMesh = new SkeletonMesh();
			region.rendererObject = skeletonMesh;
			skeletonMesh.exists = false;
			_meshes.push(skeletonMesh);
		}
		return region.rendererObject;
	}

	function set_flipX(value:Bool):Bool {
		if (value != flipX) {
			skeleton.scaleX = -skeleton.scaleX;
			this.calculateBounds();
		}
		return flipX = value;
	}

	function set_flipY(value:Bool):Bool {
		if (value != flipY) {
			skeleton.scaleY = -skeleton.scaleY * Bone.yDir;
			this.calculateBounds();
		}
		return flipY = value;
	}

	function set_scale(value:FlxPoint):FlxPoint {
		scaleX = value.x;
		scaleY = value.y;
		return value;
	}

	function get_scaleX():Float {
		return skeleton.scaleX;
	}

	function set_scaleX(value:Float):Float {
		skeleton.scaleX = value;
		this.calculateBounds();
		return value;
	}

	function get_scaleY():Float {
		return skeleton.scaleY * Bone.yDir;
	}

	function set_scaleY(value:Float):Float {
		skeleton.scaleY = value;
		this.calculateBounds();
		return value;
	}

	function set_angle(value:Float):Float {
		__objectBounds.angle = value;
		return angle = value;
	}

	function set_x(value:Float):Float {
		__objectBounds.x = __bounds.x + value;
		return x = value;
	}

	function set_y(value:Float):Float {
		__objectBounds.y = __bounds.y + value;
		return y = value;
	}

	function get_height():Float {
		return __bounds.height;
	}

	function get_width():Float {
		return __bounds.width;
	}

	function set_width(value:Float):Float {
		var scale = value / __bounds.width;
		scaleX *= scale;
		return __bounds.width;
	}

	function set_height(value:Float):Float {
		var scale = value / __bounds.height;
		scaleY *= scale;
		return __bounds.height;
	}

	function get_bounds():Rectangle {
		var bounds = new Rectangle();
		bounds.x = __objectBounds.x;
		bounds.y = __objectBounds.y;
		bounds.width = __objectBounds.width;
		bounds.height = __objectBounds.height;
		return bounds;
	}

	// ============================================================
	// OVERRIDE METHODS FROM FlxBasic
	// ============================================================

	override public function update(elapsed:Float):Void {
		super.update(elapsed);
		state.update(elapsed);
		state.apply(skeleton);
		this.beforeUpdateWorldTransforms(this);
		skeleton.update(elapsed);
		skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);
	}

	override public function draw():Void {
		if (alpha == 0)
			return;

		renderMeshes();

		#if FLX_DEBUG
		if (FlxG.debugger.drawDebug)
			__objectBounds.drawDebug();
		#end
	}

	override public function destroy():Void {
		state.clearListeners();
		state = null;
		skeleton = null;

		_tempVertices = null;
		_quadTriangles = null;
		_tempMatrix = null;
		_tempPoint = null;

		if (_meshes != null) {
			for (mesh in _meshes)
				mesh.destroy();
			_meshes = null;
		}

		super.destroy();
	}

	// ============================================================
	// OVERLAP/COLLISION METHODS
	// ============================================================

	public function overlaps(objectOrGroup:FlxBasic, inScreenSpace:Bool = false, ?camera:FlxCamera):Bool {
		return __objectBounds.overlaps(objectOrGroup, inScreenSpace, camera);
	}

	public function overlapsAt(x:Float, y:Float, objectOrGroup:FlxBasic, inScreenSpace = false, ?camera:FlxCamera):Bool {
		return __objectBounds.overlapsAt(x, y, objectOrGroup, inScreenSpace, camera);
	}

	public function overlapsPoint(point:FlxPoint, inScreenSpace = false, ?camera:FlxCamera):Bool {
		return __objectBounds.overlapsPoint(point, inScreenSpace, camera);
	}

	// ============================================================
	// BOUNDS/POSITION METHODS
	// ============================================================

	public function inWorldBounds():Bool {
		return __objectBounds.inWorldBounds();
	}

	public function getScreenPosition(?result:FlxPoint, ?camera:FlxCamera):FlxPoint {
		return __objectBounds.getScreenPosition(result, camera);
	}

	public function getPosition(?result:FlxPoint):FlxPoint {
		return __objectBounds.getPosition(result);
	}

	public function getMidpoint(?point:FlxPoint):FlxPoint {
		return __objectBounds.getMidpoint(point);
	}

	public function getHitbox(?rect:FlxRect):FlxRect {
		return __objectBounds.getHitbox(rect);
	}

	public function getRotatedBounds(?newRect:FlxRect):FlxRect {
		return __objectBounds.getRotatedBounds(newRect);
	}

	// ============================================================
	// STATE METHODS
	// ============================================================

	public function reset(x:Float, y:Float):Void {
		__objectBounds.reset(x, y);
	}

	public function isOnScreen(?camera:FlxCamera):Bool {
		return __objectBounds.isOnScreen(camera);
	}

	public function isPixelPerfectRender(?camera:FlxCamera):Bool {
		return __objectBounds.isPixelPerfectRender(camera);
	}

	public function isTouching(direction:FlxDirectionFlags):Bool {
		return __objectBounds.isTouching(direction);
	}

	public function justTouched(direction:FlxDirectionFlags):Bool {
		return __objectBounds.justTouched(direction);
	}

	// ============================================================
	// UTILITY METHODS
	// ============================================================

	public inline function screenCenter(axes:FlxAxes = XY):SkeletonSprite {
		if (axes.x)
			x = (FlxG.width - __bounds.width) / 2 - __bounds.x;

		if (axes.y)
			y = (FlxG.height - __bounds.height) / 2 - __bounds.y;

		return this;
	}

	public function setPosition(x = 0.0, y = 0.0):Void {
		this.x = x;
		this.y = y;
	}

	public function setSize(width:Float, height:Float):Void {
		this.width = width;
		this.height = height;
	}
}

typedef RenderedAttachment = {
	var rendererObject:Dynamic;
}
