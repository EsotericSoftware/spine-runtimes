import { _decorator, Component, Node, sp, Button, Label, Color } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('GoblinTest')
export class GoblinTest extends Component {
    @property({ type: Label })
    runMode: Label = null!;

    start() {

    }

    onChangeSkin() {
        const skinBoy = 'goblin';
        const skinGirl = 'goblingirl';
        const skelComp = this.node.getComponent(sp.Skeleton);
        const curName = skelComp!._skeleton!.skin.name;
        if (curName === skinBoy) {
            skelComp!.setSkin(skinGirl);
        } else {
            skelComp!.setSkin(skinBoy);
        }
    }

    onChangeMode() {
        const skelComp = this.node.getComponent(sp.Skeleton);
        const skinName = skelComp!._skeleton!.skin.name;
        if (skelComp!.isAnimationCached()) {
            skelComp!.setAnimationCacheMode(sp.AnimationCacheMode.REALTIME);
            this.runMode.string = 'realtime';
        } else {
            skelComp!.setAnimationCacheMode(sp.AnimationCacheMode.PRIVATE_CACHE);
            this.runMode.string = 'cache';
        }

    }
    private colorCount = 0;
    onChangeColor() {
        const skelComp = this.node.getComponent(sp.Skeleton);
        if (this.colorCount === 0) {
            skelComp!.color = new Color(255, 255, 0, 255);
        } else if (this.colorCount === 1) {
            skelComp!.color = new Color(0, 0, 255, 150);
        } else {
            skelComp!.color = new Color(179, 245, 170, 255);
        }
        this.colorCount++;
        this.colorCount = this.colorCount % 3;
    }

    update(deltaTime: number) {
        
    }
}


