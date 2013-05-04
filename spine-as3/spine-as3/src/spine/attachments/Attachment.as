package spine.attachments {

public class Attachment {
	internal var _name:String;

	public function Attachment (name:String) {
		if (name == null)
			throw new ArgumentError("name cannot be null.");
		_name = name;
	}

	public function get name () : String {
		return _name;
	}

	public function toString () : String {
		return name;
	}
}

}
