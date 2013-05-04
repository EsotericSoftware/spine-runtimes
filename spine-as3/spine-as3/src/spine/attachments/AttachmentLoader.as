package spine.attachments {
import spine.Skin;

public interface AttachmentLoader {
	/** @return May be null to not load an attachment. */
	function newAttachment (skin:Skin, type:AttachmentType, name:String) : Attachment;
}

}
