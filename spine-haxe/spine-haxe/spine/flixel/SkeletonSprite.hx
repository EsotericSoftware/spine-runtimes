package spine.flixel;

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

	public function setBoundingBox() {
		var bounds = skeleton.getBounds();
		if (bounds.width > 0 && bounds.height > 0) {
			width = bounds.width;
			height = bounds.height;
			offsetX = bounds.width / 2;
			offsetY = bounds.height;
		}
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
				// trace('${slot.data.name}');
				// trace(skeleton.color.r * slot.color.r * attachmentColor.r * color.redFloat);
				// trace(skeleton.color.g * slot.color.g * attachmentColor.g * color.greenFloat);
				// trace(skeleton.color.b * slot.color.b * attachmentColor.b * color.blueFloat);
				// trace('${mesh.color}\n');
				var _tmpColor:Int;
				// _tmpColor = FlxColor.fromRGBFloat(1,1,1,1);


				_tmpColor = FlxColor.fromRGBFloat(
					skeleton.color.r * slot.color.r * attachmentColor.r * color.redFloat,
					skeleton.color.g * slot.color.g * attachmentColor.g * color.greenFloat,
					skeleton.color.b * slot.color.b * attachmentColor.b * color.blueFloat,
					1
				);


				// // if (slot.data.name == "hair-patch") {
				// if (slot.data.name == "square2") {
				// 	_tmpColor = FlxColor.fromRGBFloat(
				// 		skeleton.color.r * slot.color.r * attachmentColor.r * color.redFloat,
				// 		skeleton.color.g * slot.color.g * attachmentColor.g * color.greenFloat,
				// 		skeleton.color.b * slot.color.b * attachmentColor.b * color.blueFloat,
				// 		1
				// 	);
				// 	// continue;
				// 	// trace('${mesh.color.red} | ${mesh.color.green} | ${mesh.color.blue} | ${mesh.color.alpha}');
				// } else {
				// 	// trace(slot.data.name);
				// 	_tmpColor = FlxColor.fromRGBFloat(1,1,1,1);
				// }
				// trace('${slot.data.name}\t${mesh.color}');

				mesh.color = _tmpColor;
				mesh.alpha = skeleton.color.a * slot.color.a * attachmentColor.a * alpha;

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