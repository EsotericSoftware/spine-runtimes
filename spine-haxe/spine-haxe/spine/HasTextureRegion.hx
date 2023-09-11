package spine;

interface HasTextureRegion {
	public var path:String;
	public var region:TextureRegion;
	public var color:Color;
	public var sequence:Sequence;
	public function updateRegion():Void;
}
