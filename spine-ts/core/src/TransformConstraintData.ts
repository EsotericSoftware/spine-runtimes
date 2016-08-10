module spine {
	export class TransformConstraintData {
		name: string;
	    bones = new Array<BoneData>();
	    target: BoneData;
	    rotateMix = 0; translateMix = 0; scaleMix = 0; shearMix = 0;
	    offsetRotation = 0; offsetX = 0; offsetY = 0; offsetScaleX = 0; offsetScaleY = 0; offsetShearY = 0;

		constructor (name: string) {
			if (name == null) throw new Error("name cannot be null.");
			this.name = name;
		}
	}
}
