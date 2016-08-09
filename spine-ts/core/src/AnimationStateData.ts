module spine {
    export class AnimationStateData {
        skeletonData: SkeletonData;
        animationToMixTime: Map<number> = { };        
        defaultMix = 0;

        constructor (skeletonData: SkeletonData) {
            if (skeletonData == null) throw new Error("skeletonData cannot be null.");
            this.skeletonData = skeletonData;
        }

        setMix (fromName: string, toName: string, duration: number) {
            let from = this.skeletonData.findAnimation(fromName);
            if (from == null) throw new Error("Animation not found: " + fromName);
            let to = this.skeletonData.findAnimation(toName);
            if (to == null) throw new Error("Animation not found: " + toName);
            this.setMixWith(from, to, duration);
        }

        setMixWith (from: Animation, to: Animation, duration: number) {
            if (from == null) throw new Error("from cannot be null.");
            if (to == null) throw new Error("to cannot be null.");
            let key = from.name + to.name;     
            this.animationToMixTime[key] = duration;
        }

        getMix (from: Animation, to: Animation) {
            let key = from.name + to.name;            
            let value = this.animationToMixTime[key];
            return value === undefined ? this.defaultMix : value;
        }
    }
}