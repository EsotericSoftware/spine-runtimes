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

package spine.starling;

import spine.boundsprovider.BoundsProvider;
import spine.boundsprovider.SetupPoseBoundsProvider;
import starling.animation.IAnimatable;
import openfl.geom.Matrix;
import openfl.geom.Point;
import openfl.geom.Rectangle as OpenFlRectangle;
import spine.Bone;
import spine.MathUtils;
import spine.Skeleton;
import spine.SkeletonClipping;
import spine.SkeletonData;
import spine.Slot;
import spine.animation.AnimationState;
import spine.animation.AnimationStateData;
import spine.attachments.ClippingAttachment;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import starling.display.BlendMode;
import starling.display.DisplayObject;
import starling.rendering.IndexData;
import starling.rendering.Painter;
import starling.rendering.VertexData;
import starling.textures.Texture;
import starling.utils.Color;
import starling.utils.MatrixUtil;

/** A starling display object that draws a skeleton. */
class SkeletonSprite extends DisplayObject implements IAnimatable {
	static private var _tempPoint:Point = new Point();
	static private var _tempMatrix:Matrix = new Matrix();
	static private var _tempVertices:Array<Float> = new Array<Float>();
	static private var blendModes:Array<String> = [BlendMode.NORMAL, BlendMode.ADD, BlendMode.MULTIPLY, BlendMode.SCREEN];

	public var skeleton(default, null):Skeleton;
	public var state(default, null):AnimationState;

	public var boundsProvider:BoundsProvider;

	private var __bounds = new OpenFlRectangle();
	private var _boundsPoint = [.0, .0];

	private var _smoothing:String = "bilinear";

	public static var clipper(default, never):SkeletonClipping = new SkeletonClipping();
	private static var QUAD_INDICES:Array<Int> = [0, 1, 2, 2, 3, 0];

	private var tempLight:spine.Color = new spine.Color(0, 0, 0);
	private var tempDark:spine.Color = new spine.Color(0, 0, 0);

	public var beforeUpdateWorldTransforms:SkeletonSprite->Void = function(_) {};
	public var afterUpdateWorldTransforms:SkeletonSprite->Void = function(_) {};

	private var _physicsPositionInheritanceFactorX:Float = 1;
	private var _physicsPositionInheritanceFactorY:Float = 1;
	private var _physicsRotationInheritanceFactor:Float = 1;
	private var hasLastPhysicsTransform:Bool = false;
	private var lastPhysicsX:Float = 0;
	private var lastPhysicsY:Float = 0;
	private var lastPhysicsRotation:Float = 0;
	private final currentPhysicsPosition:Array<Float> = [0., 0.];
	private final lastPhysicsPosition:Array<Float> = [0., 0.];

	/** Scales how much horizontal translation of this Starling display object is inherited by skeleton physics constraints. */
	public var physicsPositionInheritanceFactorX(get, never):Float;

	private function get_physicsPositionInheritanceFactorX():Float {
		return _physicsPositionInheritanceFactorX;
	}

	/** Scales how much vertical translation of this Starling display object is inherited by skeleton physics constraints. */
	public var physicsPositionInheritanceFactorY(get, never):Float;

	private function get_physicsPositionInheritanceFactorY():Float {
		return _physicsPositionInheritanceFactorY;
	}

	/** Sets how much translation of this Starling display object is inherited by skeleton physics constraints.
	 * The default is (1, 1), which applies display object translation normally. Use (0, 0) to prevent display object translation from
	 * affecting physics constraints. */
	public function setPhysicsPositionInheritanceFactor(x:Float, y:Float):Void {
		var wasDisabled = _physicsPositionInheritanceFactorX == 0 && _physicsPositionInheritanceFactorY == 0;
		var isEnabled = x != 0 || y != 0;
		_physicsPositionInheritanceFactorX = x;
		_physicsPositionInheritanceFactorY = y;
		if (wasDisabled && isEnabled)
			resetPhysicsPosition();
	}

	/** Scales how much rotation of this Starling display object is inherited by skeleton physics constraints.
	 * The default is 1, which applies display object rotation normally. Use 0 to prevent display object rotation from affecting
	 * physics constraints. */
	public var physicsRotationInheritanceFactor(get, set):Float;

	private function get_physicsRotationInheritanceFactor():Float {
		return _physicsRotationInheritanceFactor;
	}

	private function set_physicsRotationInheritanceFactor(value:Float):Float {
		var wasDisabled = _physicsRotationInheritanceFactor == 0;
		_physicsRotationInheritanceFactor = value;
		if (wasDisabled && value != 0)
			resetPhysicsRotation();
		return _physicsRotationInheritanceFactor;
	}

	/** Creates an uninitialized SkeletonSprite. The skeleton and animation state must be set before use. */
	public function new(skeletonData:SkeletonData, animationStateData:AnimationStateData = null, ?boundsProvider:BoundsProvider) {
		super();
		Bone.yDown = true;
		skeleton = new Skeleton(skeletonData);
		skeleton.updateWorldTransform(Physics.update);
		state = new AnimationState(animationStateData != null ? animationStateData : new AnimationStateData(skeletonData));
		this.boundsProvider = boundsProvider ?? new SetupPoseBoundsProvider();
		this.calculateBounds();
	}

	override public function render(painter:Painter):Void {
		var clipper:SkeletonClipping = SkeletonSprite.clipper;
		painter.state.alpha *= skeleton.color.a;
		var originalBlendMode:String = painter.state.blendMode;
		var r:Float = skeleton.color.r * 255;
		var g:Float = skeleton.color.g * 255;
		var b:Float = skeleton.color.b * 255;
		var drawOrder:Array<Slot> = skeleton.drawOrder.appliedPose;
		var attachmentColor:spine.Color;
		var rgb:Int;
		var a:Float;
		var dark:Int;
		var mesh:SkeletonMesh = null;
		var verticesLength:Int;
		var verticesCount:Int;
		var indicesLength:Int;
		var indexData:IndexData;
		var indices:Array<Int> = null;
		var vertexData:VertexData;
		var uvs:Array<Float>;

		for (slot in drawOrder) {
			if (!slot.bone.active) {
				clipper.clipEnd(slot);
				continue;
			}

			var worldVertices:Array<Float> = _tempVertices;
			var pose = slot.appliedPose;
			var attachment = pose.attachment;
			if (Std.isOfType(attachment, RegionAttachment)) {
				var region:RegionAttachment = cast(attachment, RegionAttachment);
				verticesLength = 8;
				verticesCount = verticesLength >> 1;
				if (worldVertices.length < verticesLength)
					worldVertices.resize(verticesLength);
				var sequence = region.sequence;
				var sequenceIndex = sequence.resolveIndex(pose);
				region.computeWorldVertices(slot, sequence.offsets[sequenceIndex], worldVertices, 0, 2);

				mesh = null;
				var regionTexture = sequence.regions[sequenceIndex].texture;
				if (Std.isOfType(region.rendererObject, SkeletonMesh)) {
					mesh = cast(region.rendererObject, SkeletonMesh);
					mesh.texture = regionTexture;
					indices = QUAD_INDICES;
				} else {
					mesh = region.rendererObject = new SkeletonMesh(cast(regionTexture, Texture));

					indexData = mesh.getIndexData();
					indices = QUAD_INDICES;
					for (i in 0...indices.length) {
						indexData.setIndex(i, indices[i]);
					}
					indexData.numIndices = indices.length;
					indexData.trim();
				}

				indexData = mesh.getIndexData();
				attachmentColor = region.color;
				uvs = sequence.getUVs(sequenceIndex);
			} else if (Std.isOfType(attachment, MeshAttachment)) {
				var meshAttachment:MeshAttachment = cast(attachment, MeshAttachment);
				verticesLength = meshAttachment.worldVerticesLength;
				verticesCount = verticesLength >> 1;
				if (worldVertices.length < verticesLength)
					worldVertices.resize(verticesLength);
				meshAttachment.computeWorldVertices(skeleton, slot, 0, meshAttachment.worldVerticesLength, worldVertices, 0, 2);

				var sequence = meshAttachment.sequence;
				var sequenceIndex = sequence.resolveIndex(pose);
				mesh = null;
				var regionTexture = sequence.regions[sequenceIndex].texture;
				if (Std.isOfType(meshAttachment.rendererObject, SkeletonMesh)) {
					mesh = cast(meshAttachment.rendererObject, SkeletonMesh);
					mesh.texture = regionTexture;
					indices = meshAttachment.triangles;
				} else {
					mesh = meshAttachment.rendererObject = new SkeletonMesh(cast(regionTexture, Texture));

					indexData = mesh.getIndexData();
					indices = meshAttachment.triangles;
					indicesLength = indices.length;
					for (i in 0...indicesLength) {
						indexData.setIndex(i, indices[i]);
					}
					indexData.numIndices = indicesLength;
					indexData.trim();
				}

				indexData = mesh.getIndexData();
				attachmentColor = meshAttachment.color;
				uvs = sequence.getUVs(sequenceIndex);
			} else if (Std.isOfType(attachment, ClippingAttachment)) {
				var clip:ClippingAttachment = cast(attachment, ClippingAttachment);
				clipper.clipEnd(slot);
				clipper.clipStart(skeleton, slot, clip);
				continue;
			} else {
				clipper.clipEnd(slot);
				continue;
			}

			a = pose.color.a * attachmentColor.a;
			if (a == 0) {
				clipper.clipEnd(slot);
				continue;
			}
			rgb = Color.rgb(Std.int(r * pose.color.r * attachmentColor.r), Std.int(g * pose.color.g * attachmentColor.g),
				Std.int(b * pose.color.b * attachmentColor.b));
			if (pose.darkColor == null) {
				dark = Color.rgb(0, 0, 0);
			} else {
				dark = Color.rgb(Std.int(pose.darkColor.r * 255), Std.int(pose.darkColor.g * 255), Std.int(pose.darkColor.b * 255));
			}

			if (clipper.isClipping() && clipper.clipTriangles(worldVertices, indices, indices.length, uvs)) {
				// Need to create a new mesh here, see https://github.com/EsotericSoftware/spine-runtimes/issues/1125
				mesh = new SkeletonMesh(mesh.texture);
				indexData = mesh.getIndexData();

				verticesCount = clipper.clippedVertices.length >> 1;
				worldVertices = clipper.clippedVertices;
				uvs = clipper.clippedUvs;

				indices = clipper.clippedTriangles;
				indicesLength = indices.length;
				indexData.numIndices = indicesLength;
				indexData.trim();
				for (i in 0...indicesLength) {
					indexData.setIndex(i, indices[i]);
				}
			}

			vertexData = mesh.getVertexData();
			vertexData.numVertices = verticesCount;
			vertexData.colorize("color", rgb, a);
			var ii:Int = 0;
			for (i in 0...verticesCount) {
				mesh.setVertexPosition(i, worldVertices[ii], worldVertices[ii + 1]);
				mesh.setTexCoords(i, uvs[ii], uvs[ii + 1]);
				ii += 2;
			}

			if (indexData.numIndices > 0 && vertexData.numVertices > 0) {
				painter.state.blendMode = blendModes[slot.data.blendMode.ordinal];
				painter.batchMesh(mesh);
			}

			clipper.clipEnd(slot);
		}
		painter.state.blendMode = originalBlendMode;
		clipper.clipEnd();
	}

	override public function hitTest(localPoint:Point):DisplayObject {
		if (!visible || !touchable)
			return null;
		else if (__bounds.containsPoint(localPoint))
			return this;
		else
			return null;
	}

	public function calculateBounds() {
		this.boundsProvider.calculateBounds(this, __bounds);
	}

	override public function getBounds(targetSpace:DisplayObject, out:OpenFlRectangle = null):OpenFlRectangle {
		if (out == null)
			out = new OpenFlRectangle();

		if (targetSpace == this) {
			out.setTo(0, 0, __bounds.width, __bounds.height);
		} else if (targetSpace == parent) {
			_boundsPoint[0] = __bounds.x;
			_boundsPoint[1] = __bounds.y;
			skeletonToHaxeWorldCoordinates(_boundsPoint);
			out.setTo(_boundsPoint[0], _boundsPoint[1], __bounds.width, __bounds.height);
		} else {
			getTransformationMatrix(targetSpace, _tempMatrix);
			out.setTo(__bounds.x, __bounds.y, __bounds.width, __bounds.height);
			MatrixUtil.transformCoords(_tempMatrix, out.x, out.y, _tempPoint);
			out.setTo(_tempPoint.x, _tempPoint.y, out.width, out.height);
		}

		return out;
	}

	public var smoothing(get, set):String;

	private function get_smoothing():String {
		return _smoothing;
	}

	private function set_smoothing(smoothing:String):String {
		_smoothing = smoothing;
		return _smoothing;
	}

	override function get_scaleX():Float {
		return skeleton.scaleX;
	}

	override function set_scaleX(value:Float):Float {
		skeleton.scaleX = value;
		calculateBounds();
		return skeleton.scaleX;
	}

	override function get_scaleY():Float {
		return skeleton.scaleY * Bone.yDir;
	}

	override function set_scaleY(value:Float):Float {
		skeleton.scaleY = value;
		calculateBounds();
		return value;
	}

	override function get_scale():Float {
		return skeleton.scaleX;
	}

	override function set_scale(value:Float):Float {
		scaleY = value;
		return scaleX = value;
	}

	public function advanceTime(time:Float):Void {
		state.update(time);
		state.apply(skeleton);
		applyTransformMovementToPhysics();
		this.beforeUpdateWorldTransforms(this);
		skeleton.update(time);
		skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);
		this.setRequiresRedraw();
	}

	/** Resets the position used for calculating inherited physics translation. */
	public function resetPhysicsPosition():Void {
		var transform = this.transformationMatrix;
		lastPhysicsX = transform.tx;
		lastPhysicsY = transform.ty;
		if (!hasLastPhysicsTransform)
			lastPhysicsRotation = getPhysicsRotation();
		hasLastPhysicsTransform = true;
	}

	/** Resets the rotation used for calculating inherited physics rotation. */
	public function resetPhysicsRotation():Void {
		var transform = this.transformationMatrix;
		lastPhysicsRotation = getPhysicsRotation();
		if (!hasLastPhysicsTransform) {
			lastPhysicsX = transform.tx;
			lastPhysicsY = transform.ty;
		}
		hasLastPhysicsTransform = true;
	}

	/** Resets the transform used for calculating inherited physics translation and rotation. */
	public function resetPhysicsTransform():Void {
		resetPhysicsPosition();
		resetPhysicsRotation();
	}

	private function applyTransformMovementToPhysics():Void {
		var transform = this.transformationMatrix;
		var currentX = transform.tx;
		var currentY = transform.ty;
		var currentRotation = getPhysicsRotation();
		if (hasLastPhysicsTransform) {
			applyPositionMovementToPhysics(currentX, currentY);
			applyRotationMovementToPhysics(currentRotation);
		}
		setLastPhysicsTransform(currentX, currentY, currentRotation);
	}

	private function applyPositionMovementToPhysics(currentX:Float, currentY:Float):Void {
		if (_physicsPositionInheritanceFactorX == 0 && _physicsPositionInheritanceFactorY == 0)
			return;

		currentPhysicsPosition[0] = currentX;
		currentPhysicsPosition[1] = currentY;
		haxeWorldCoordinatesToSkeleton(currentPhysicsPosition);

		lastPhysicsPosition[0] = lastPhysicsX;
		lastPhysicsPosition[1] = lastPhysicsY;
		haxeWorldCoordinatesToSkeleton(lastPhysicsPosition);

		skeleton.physicsTranslate((currentPhysicsPosition[0] - lastPhysicsPosition[0]) * _physicsPositionInheritanceFactorX,
			(currentPhysicsPosition[1] - lastPhysicsPosition[1]) * _physicsPositionInheritanceFactorY);
	}

	private function applyRotationMovementToPhysics(currentRotation:Float):Void {
		var rotationFactor = _physicsRotationInheritanceFactor;
		if (rotationFactor == 0)
			return;
		skeleton.physicsRotate(0, 0, getRotationDelta(currentRotation, lastPhysicsRotation) * rotationFactor);
	}

	private function setLastPhysicsTransform(x:Float, y:Float, rotation:Float):Void {
		lastPhysicsX = x;
		lastPhysicsY = y;
		lastPhysicsRotation = rotation;
		hasLastPhysicsTransform = true;
	}

	private function getPhysicsRotation():Float {
		var transform = this.transformationMatrix;
		return Math.atan2(transform.b, transform.a) * MathUtils.radDeg;
	}

	private function getRotationDelta(current:Float, previous:Float):Float {
		var delta = current - previous;
		delta = (delta + 180) % 360 - 180;
		return delta < -180 ? delta + 360 : delta;
	}

	public function skeletonToHaxeWorldCoordinates(point:Array<Float>):Void {
		var transform = this.transformationMatrix;
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
		var transform = this.transformationMatrix.clone().invert();
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

	override public function dispose():Void {
		if (state != null) {
			state.clearListeners();
			state = null;
		}
		if (skeleton != null)
			skeleton = null;
		dispatchEventWith(starling.events.Event.REMOVE_FROM_JUGGLER);
		removeFromParent();
		// this will remove also all starling event listeners
		super.dispose();
	}
}
