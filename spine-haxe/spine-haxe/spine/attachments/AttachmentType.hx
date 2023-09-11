package spine.attachments;

import openfl.Vector;

class AttachmentType {
	public static var region(default, never):AttachmentType = new AttachmentType(0, "region");
	public static var boundingbox(default, never):AttachmentType = new AttachmentType(1, "boundingbox");
	public static var mesh(default, never):AttachmentType = new AttachmentType(2, "mesh");
	public static var linkedmesh(default, never):AttachmentType = new AttachmentType(3, "linkedmesh");
	public static var path(default, never):AttachmentType = new AttachmentType(4, "path");
	public static var point(default, never):AttachmentType = new AttachmentType(5, "point");
	public static var clipping(default, never):AttachmentType = new AttachmentType(6, "clipping");

	public static var values(default, never):Vector<AttachmentType> = Vector.ofArray([region, boundingbox, mesh, linkedmesh, path, point, clipping]);

	public var ordinal(default, null):Int;
	public var name(default, null):String;

	public function new(ordinal:Int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}

	public static function fromName(name:String):AttachmentType {
		for (value in values) {
			if (value.name == name)
				return value;
		}
		return null;
	}
}
