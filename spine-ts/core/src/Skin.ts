module spine {
    export class Skin {
        name: string;
        attachments = new Array<Map<Attachment>>();

        constructor (name: string) {
            if (name == null) throw new Error("name cannot be null.");
            this.name = name;
        }

        addAttachment (slotIndex: number, name: string, attachment: Attachment) {
            if (attachment == null) throw new Error("attachment cannot be null.");
            let attachments = this.attachments;
            if (slotIndex >= attachments.length) attachments.length = slotIndex + 1;
            if (!attachments[slotIndex]) attachments[slotIndex] = { };
            attachments[slotIndex][name] = attachment;
        }

        /** @return May be null. */
        getAttachment (slotIndex: number, name: string): Attachment {
            if (slotIndex >= this.attachments.length) return null;
            let dictionary = this.attachments[slotIndex];
            return dictionary ? dictionary[name] : null;
        }

        /** Attach each attachment in this skin if the corresponding attachment in the old skin is currently attached. */
        attachAll (skeleton: Skeleton, oldSkin: Skin) {
            var slotIndex = 0;
            for (var i = 0; i < skeleton.slots.length; i++) {
                let slot = skeleton.slots[i];
                let slotAttachment = slot.getAttachment();
                if (slotAttachment && slotIndex < oldSkin.attachments.length) {
                    let dictionary = oldSkin.attachments[slotIndex];
                    for (var key in dictionary) {
                        var skinAttachment:Attachment = dictionary[key];
                        if (slotAttachment == skinAttachment) {
                            let attachment = this.getAttachment(slotIndex, name);
                            if (attachment != null) slot.setAttachment(attachment);
                            break;
                        }
                    }
                }
                slotIndex++;
            }
        }
    }
}