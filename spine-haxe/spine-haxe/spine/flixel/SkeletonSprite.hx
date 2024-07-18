package spine.flixel;

import spine.animation.MixDirection;
import spine.animation.MixBlend;
import spine.animation.Animation;
import spine.TextureRegion;
import haxe.extern.EitherType;
import spine.attachments.Attachment;
import flixel.util.typeLimit.OneOfTwo;
import flixel.FlxCamera;
import flixel.math.FlxRect;
import flixel.FlxG;
import flixel.FlxObject;
import flixel.FlxSprite;
import flixel.FlxStrip;
import flixel.group.FlxSpriteGroup;
import flixel.graphics.FlxGraphic;
import flixel.util.FlxColor;
import openfl.Vector;
import openfl.display.BlendMode;
import spine.Bone;
import spine.Skeleton;
import spine.SkeletonData;
import spine.Slot;
import spine.animation.AnimationState;
import spine.animation.AnimationStateData;
import spine.atlas.TextureAtlasRegion;
import spine.attachments.MeshAttachment;
import spine.attachments.RegionAttachment;
import spine.attachments.ClippingAttachment;
import spine.flixel.SkeletonMesh;

class SkeletonSprite extends FlxObject
{
	public var skeleton(default, null):Skeleton;
	public var state(default, null):AnimationState;
	public var stateData(default, null):AnimationStateData;
	public var beforeUpdateWorldTransforms: SkeletonSprite -> Void = function(_) {};
	public var afterUpdateWorldTransforms: SkeletonSprite -> Void = function(_) {};
	public static var clipper(default, never):SkeletonClipping = new SkeletonClipping();

	public var offsetX = .0;
	public var offsetY = .0;
	public var alpha = 1.; // TODO: clamp
	public var color:FlxColor = 0xffffff;
	public var flipX(default, set):Bool = false;
	public var flipY(default, set):Bool = false;
	public var antialiasing:Bool = true;

	var _tempVertices:Array<Float> = new Array<Float>();
	var _quadTriangles:Array<Int>;
	var _meshes(default, null):Array<SkeletonMesh> = new Array<SkeletonMesh>();
	private static var QUAD_INDICES:Array<Int> = [0, 1, 2, 2, 3, 0];
	public function new(skeletonData:SkeletonData, animationStateData:AnimationStateData = null)
	{
		super(0, 0);
		Bone.yDown = true;
		skeleton = new Skeleton(skeletonData);
		skeleton.updateWorldTransform(Physics.update);
		state = new AnimationState(animationStateData != null ? animationStateData : new AnimationStateData(skeletonData));

		setBoundingBox();
	}

	public function setBoundingBox(?animation:Animation, ?clip:Bool = true) {
		var bounds = animation == null ? skeleton.getBounds() : getAnimationBounds(animation, clip);
		trace(bounds);
		if (bounds.width > 0 && bounds.height > 0) {
			width = bounds.width;
			height = bounds.height;
			offsetX = -bounds.x;
			offsetY = -bounds.y;
		}
	}

	public function getAnimationBounds(animation:Animation, clip:Bool = true): lime.math.Rectangle {
		var clipper = clip ? SkeletonSprite.clipper : null;
		skeleton.setToSetupPose();

		var steps = 100, time = 0.;
		var stepTime = animation.duration != 0 ? animation.duration / steps : 0;
		var minX = 100000000., maxX = -100000000., minY = 100000000., maxY = -100000000.;

		var bounds = new lime.math.Rectangle();
		for (i in 0...steps) {
			animation.apply(skeleton, time , time, false, [], 1, MixBlend.setup, MixDirection.mixIn);
			skeleton.updateWorldTransform(Physics.update);
			bounds = skeleton.getBounds(clipper);

			if (!Math.isNaN(bounds.x) && !Math.isNaN(bounds.y) && !Math.isNaN(bounds.width) && !Math.isNaN(bounds.height)) {
				minX = Math.min(bounds.x, minX);
				minY = Math.min(bounds.y, minY);
				maxX = Math.max(bounds.right, maxX);
				maxY = Math.max(bounds.bottom, maxY);
			} else
				trace("ERROR");

			time += stepTime;
		}
		bounds.x = minX;
		bounds.y = minY;
		bounds.width = maxX - minX;
		bounds.height = maxY - minY;
		return bounds;
	}

	override public function destroy():Void
	{
		skeleton = null;
		state = null;
		stateData = null;

		_tempVertices = null;
		_quadTriangles = null;

		if (_meshes != null) {
			for (mesh in _meshes) mesh.destroy();
			_meshes = null;
		}

		super.destroy();
	}

	override public function update(elapsed:Float):Void
	{
		super.update(elapsed);
		state.update(elapsed);
		state.apply(skeleton);
		this.beforeUpdateWorldTransforms(this);
		skeleton.update(elapsed);
		skeleton.updateWorldTransform(Physics.update);
		this.afterUpdateWorldTransforms(this);
	}

	override public function draw():Void
	{
		if (alpha == 0) return;

		renderMeshes();

		#if FLX_DEBUG
		if (FlxG.debugger.drawDebug) drawDebug();
		#end
	}

	function renderMeshes():Void {
		var clipper:SkeletonClipping = SkeletonSprite.clipper;
		var drawOrder:Array<Slot> = skeleton.drawOrder;
		var attachmentColor:spine.Color;
		var mesh:SkeletonMesh = null;
		var numVertices:Int;
		var numFloats:Int;
		var triangles:Array<Int> = null;
		var uvs:Array<Float>;
		var twoColorTint:Bool = false;
		var vertexSize:Int = twoColorTint ? 12 : 8;
		for (slot in drawOrder) {
			var clippedVertexSize:Int = clipper.isClipping() ? 2 : vertexSize;
			if (!slot.bone.active) {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			var worldVertices:Array<Float> = _tempVertices;
			if (Std.isOfType(slot.attachment, RegionAttachment)) {
				var region:RegionAttachment = cast(slot.attachment, RegionAttachment);
				numVertices = 4;
				numFloats = clippedVertexSize << 2;
				if (numFloats > worldVertices.length) {
					worldVertices.resize(numFloats);
				}
				region.computeWorldVertices(slot, worldVertices, 0, clippedVertexSize);

				mesh = getFlixelMeshFromRendererAttachment(region);
				mesh.graphic = region.region.texture;
				triangles = QUAD_INDICES;
				uvs = region.uvs;
				attachmentColor = region.color;
			} else if (Std.isOfType(slot.attachment, MeshAttachment)) {
				var meshAttachment:MeshAttachment = cast(slot.attachment, MeshAttachment);
				numVertices = meshAttachment.worldVerticesLength >> 1;
				numFloats = numVertices * clippedVertexSize; // 8 for now because I'm excluding clipping
				if (numFloats > worldVertices.length) {
					worldVertices.resize(numFloats);
				}
				meshAttachment.computeWorldVertices(slot, 0, meshAttachment.worldVerticesLength, worldVertices, 0, clippedVertexSize);

				mesh = getFlixelMeshFromRendererAttachment(meshAttachment);
				mesh.graphic = meshAttachment.region.texture;
				triangles = meshAttachment.triangles;
				uvs = meshAttachment.uvs;
				attachmentColor = meshAttachment.color;
			} else if (Std.isOfType(slot.attachment, ClippingAttachment)) {
				var clip:ClippingAttachment = cast(slot.attachment, ClippingAttachment);
				clipper.clipStart(slot, clip);
				continue;
			} else {
				clipper.clipEndWithSlot(slot);
				continue;
			}

			if (mesh != null) {

				// cannot use directly mesh.color.setRGBFloat otherwise the setter won't be called and transfor color not set
				var _tmpColor:Int;
				_tmpColor = FlxColor.fromRGBFloat(
					skeleton.color.r * slot.color.r * attachmentColor.r * color.redFloat,
					skeleton.color.g * slot.color.g * attachmentColor.g * color.greenFloat,
					skeleton.color.b * slot.color.b * attachmentColor.b * color.blueFloat,
					1
				);
				// mesh color and alpha are bugged
				// mesh.color = _tmpColor;
				// mesh.alpha = skeleton.color.a * slot.color.a * attachmentColor.a * alpha;

				if (clipper.isClipping()) {
					clipper.clipTriangles(worldVertices, triangles, triangles.length, uvs);
					mesh.vertices = Vector.ofArray(clipper.clippedVertices);
					mesh.indices = Vector.ofArray(clipper.clippedTriangles);
					mesh.uvtData = Vector.ofArray(clipper.clippedUvs);
				} else {
					var v = 0;
					var n = numFloats;
					var i = 0;
					mesh.vertices.length = numVertices;
					while (v < n) {
						mesh.vertices[i] = worldVertices[v];
						mesh.vertices[i + 1] = worldVertices[v + 1];
						v += 8;
						i += 2;
					}
					mesh.indices = Vector.ofArray(triangles);
					mesh.uvtData = Vector.ofArray(uvs);
				}

				mesh.antialiasing = antialiasing;
				mesh.blend = SpineTexture.toFlixelBlending(slot.data.blendMode);
				mesh.x = x + offsetX;
				mesh.y = y + offsetY;
				// mesh._cameras = _cameras;
				mesh.draw();
			}

			clipper.clipEndWithSlot(slot);
		}
		clipper.clipEnd();
	}

	private function getFlixelMeshFromRendererAttachment(region: RenderedAttachment) {
		if (region.rendererObject == null) {
			var skeletonMesh = new SkeletonMesh();
			region.rendererObject = skeletonMesh;
			skeletonMesh.exists = false;
			_meshes.push(skeletonMesh);
		}
		return region.rendererObject;
	}

	function set_flipX(value:Bool):Bool
	{
		if (value != skeleton.flipX) skeleton.scaleX = -skeleton.scaleX;
		skeleton.flipX = value;
		return flipX = value;
	}

	function set_flipY(value:Bool):Bool
	{
		if (value != skeleton.flipY) skeleton.scaleY = -skeleton.scaleY;
		skeleton.flipY = value;
		return flipY = value;
	}
}

typedef RenderedAttachment = {
	var rendererObject:Dynamic;
	var region:TextureRegion;
}