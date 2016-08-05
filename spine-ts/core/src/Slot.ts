module spine {
    export class Slot {
        data: SlotData;
        bone: Bone;
        color: Color;
        private attachment: Attachment;
        private attachmentTime: number;
        attachmentVertices = new Array<number>();

        constructor (data: SlotData, bone: Bone) {
            if (data == null) throw new Error("data cannot be null.");
            if (bone == null) throw new Error("bone cannot be null.");
            this.data = data;
            this.bone = bone;
            this.color = new Color();
            this.setToSetupPose();
        }
        
        /** @return May be null. */
        getAttachment (): Attachment {
            return this.attachment;
        }

        /** Sets the attachment and if it changed, resets {@link #getAttachmentTime()} and clears {@link #getAttachmentVertices()}.
         * @param attachment May be null. */
        setAttachment (attachment: Attachment) {
            if (this.attachment == attachment) return;
            this.attachment = attachment;
            this.attachmentTime = this.bone.skeleton.time;
            this.attachmentVertices.length = 0;
        }

        setAttachmentTime (time: number) {
            this.attachmentTime = this.bone.skeleton.time - time;
        }

        /** Returns the time since the attachment was set. */
        getAttachmentTime (): number {
            return this.bone.skeleton.time - this.attachmentTime;
        }

        setToSetupPose () {
            this.color.setFromColor(this.data.color);
            if (this.data.attachmentName == null)
                this.attachment = null;
            else {
                this.attachment = null;
                this.setAttachment(this.bone.skeleton.getAttachment(this.data.index, this.data.attachmentName));
            }
        }        
    }
}