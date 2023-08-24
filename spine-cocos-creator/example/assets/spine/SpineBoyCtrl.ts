import { _decorator, CCClass, Component, sp } from "cc";
const { ccclass, property } = _decorator;

@ccclass('SpineBoyCtrl')
export default class SpineBoyCtrl extends Component{

    mixTime:number= 0.2;

    private spine?: sp.Skeleton;
    private _hasStop = true;
    onLoad () {
        var spine = this.spine = this.getComponent('sp.Skeleton') as sp.Skeleton;
        this._setMix('walk', 'run');
        this._setMix('run', 'jump');
        this._setMix('walk', 'jump');

        spine.setStartListener(trackEntry => {
            var animationName = trackEntry.animation ? trackEntry.animation.name : "";
            console.log("[track %s][animation %s] start.", trackEntry.trackIndex, animationName);
        });
        spine.setInterruptListener(trackEntry => {
            var animationName = trackEntry.animation ? trackEntry.animation.name : "";
            console.log("[track %s][animation %s] interrupt.", trackEntry.trackIndex, animationName);
        });
        spine.setEndListener(trackEntry => {
            var animationName = trackEntry.animation ? trackEntry.animation.name : "";
            console.log("[track %s][animation %s] end.", trackEntry.trackIndex, animationName);
        });
        spine.setDisposeListener(trackEntry => {
            var animationName = trackEntry.animation ? trackEntry.animation.name : "";
            console.log("[track %s][animation %s] will be disposed.", trackEntry.trackIndex, animationName);
        });
        spine.setCompleteListener((trackEntry) => {
            var animationName = trackEntry.animation ? trackEntry.animation.name : "";
            if (animationName === 'shoot') {
                this.spine!.clearTrack(1);
            }
            var loopCount = Math.floor(trackEntry.trackTime / trackEntry.animationEnd);
            console.log("[track %s][animation %s] complete: %s", trackEntry.trackIndex, animationName, loopCount);
        });
        spine.setEventListener(((trackEntry:any, event:any) => {
            var animationName = trackEntry.animation ? trackEntry.animation.name : "";
            console.log("[track %s][animation %s] event: %s, %s, %s, %s", trackEntry.trackIndex, animationName, event.data.name, event.intValue, event.floatValue, event.stringValue);
        }) as any);

        this._hasStop = false;
    }

    // OPTIONS

    toggleDebugSlots () {
        this.spine!.debugSlots = !this.spine?.debugSlots;
    }

    toggleDebugBones () {
        this.spine!.debugBones = !this.spine?.debugBones;
    }

    toggleDebugMesh () {
        this.spine!.debugMesh = !this.spine?.debugMesh;
    }

    toggleUseTint () {
        this.spine!.useTint = !this.spine?.useTint;
    }

    toggleTimeScale () {
        if (this.spine!.timeScale === 1.0) {
            this.spine!.timeScale = 0.3;
        }
        else {
            this.spine!.timeScale = 1.0;
        }
    }

    // ANIMATIONS

    stop () {
        this.spine?.clearTrack(0);
        this._hasStop = true;
    }

    walk () {
        if (this._hasStop) {
            this.spine?.setToSetupPose();
        }
        this.spine?.setAnimation(0, 'walk', true);
        this._hasStop = false;
    }

    run () {
        if (this._hasStop) {
            this.spine?.setToSetupPose();
        }
        this.spine?.setAnimation(0, 'run', true);
        this._hasStop = false;
    }

    jump () {
        if (this._hasStop) {
            this.spine?.setToSetupPose();
        }
        this.spine?.setAnimation(0, 'jump', true);
        this._hasStop = false;
    }

    shoot () {
        this.spine?.setAnimation(1, 'shoot', false);
    }

    idle () {
        this.spine?.setToSetupPose();
        this.spine?.setAnimation(0, 'idle', true);
    }

    portal () {
        this.spine?.setToSetupPose();
        this.spine?.setAnimation(0, 'portal', false);
    }

    //

    _setMix (anim1: string, anim2: string) {
        this.spine?.setMix(anim1, anim2, this.mixTime);
        this.spine?.setMix(anim2, anim1, this.mixTime);
    }
}
