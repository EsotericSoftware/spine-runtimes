module spine {
	export class SlotData {
		index: number;
		name: string;
		boneData: BoneData;
		color = new Color(1, 1, 1, 1);
		attachmentName: string;
		blendMode: BlendMode;

		constructor (index: number, name: string, boneData: BoneData) {
			if (index < 0) throw new Error("index must be >= 0.");
			if (name == null) throw new Error("name cannot be null.");
			if (boneData == null) throw new Error("boneData cannot be null.");
			this.index = index;
			this.name = name;
			this.boneData = boneData;
		}
	}
}
