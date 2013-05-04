package spine.attachments {

public class AttachmentType {
	public static const region:AttachmentType = new AttachmentType(0, "region");
	public static const regionSequence:AttachmentType = new AttachmentType(1, "regionSequence");

	public var ordinal:int;
	public var name:String;

	public function AttachmentType (ordinal:int, name:String) {
		this.ordinal = ordinal;
		this.name = name;
	}
}

}
