


import { _decorator, Component, sp, Label, Node, Button } from 'cc';
const { ccclass, property } = _decorator;

@ccclass('SpineAttach')
export default class extends Component {

    @property({ type: sp.Skeleton })
    skeleton: sp.Skeleton = null!;

    @property({ type: Node })
    attachNode: Node = null!;

    @property({ type: Label })
    modeLabel: Label = null!;

    @property({ type: Button })
    attachBtn: Button = null!;

    backSockets: sp.SpineSocket[] = null!;

    onLoad() {
        var socket = new sp.SpineSocket("root/hip/tail1/tail2/tail3/tail4/tail5/tail6/tail7/tail8/tail9/tail10", this.attachNode);
        this.skeleton!.sockets.push(socket);
        this.skeleton!.sockets = this.skeleton!.sockets;
    }

    changeAttach() {
        if (!this.backSockets) {
            this.backSockets = this.skeleton!.sockets;
            this.skeleton!.sockets = [];
        }
        else {
            this.skeleton!.sockets = this.backSockets;
            this.backSockets = null!;
        }
    }

    changeMode() {
        let isCached = this.skeleton!.isAnimationCached();
        if (isCached) {
            this.skeleton.setAnimationCacheMode(sp.Skeleton.AnimationCacheMode.REALTIME);
            this.modeLabel.string = "realtime";
            this.attachBtn.interactable = true;
        } else {
            this.skeleton.setAnimationCacheMode(sp.Skeleton.AnimationCacheMode.SHARED_CACHE);
            this.modeLabel.string = "cache";
            this.attachBtn.interactable = false;
        }
    }
}
