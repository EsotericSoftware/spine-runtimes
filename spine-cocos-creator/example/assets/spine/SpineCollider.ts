
import { _decorator, Component, Node, PhysicsSystem2D, Contact2DType, Collider2D, Color, Sprite, ParticleSystem2D, EPhysics2DDrawFlags } from 'cc';
const { ccclass } = _decorator;

@ccclass('SpineCollider')
export class SpineCollider extends Component {

    touchingCountMap: Map<Node, number> = new Map;

    private debugDrawFlags:number = 0;
    start () {
        // Your initialization goes here.
        PhysicsSystem2D.instance.on(Contact2DType.BEGIN_CONTACT, this.onBeginContact, this);
        PhysicsSystem2D.instance.on(Contact2DType.END_CONTACT, this.onEndContact, this);
        this.debugDrawFlags = PhysicsSystem2D.instance.debugDrawFlags;
    }

    onEnable () {
        PhysicsSystem2D.instance.debugDrawFlags = this.debugDrawFlags | EPhysics2DDrawFlags.Shape;
    }
    onDisable () {
        PhysicsSystem2D.instance.debugDrawFlags = this.debugDrawFlags;
    }

    addContact (c: Collider2D) {
        let count = this.touchingCountMap.get(c.node) || 0;
        this.touchingCountMap.set(c.node, ++count);

        let sprite = c.getComponent(Sprite);
        if (sprite) {
            sprite.color = Color.RED;
        }
    }

    removeContact (c: Collider2D) {
        let count = this.touchingCountMap.get(c.node) || 0;
        --count;
        if (count <= 0) {
            this.touchingCountMap.delete(c.node);

            let sprite = c.getComponent(Sprite);
            if (sprite) {
                sprite.color = Color.WHITE;
            }
        }
        else {
            this.touchingCountMap.set(c.node, count);
        }
    }

    onBeginContact (a: Collider2D, b: Collider2D) {
        this.addContact(a);
        this.addContact(b);
    }

    onEndContact (a: Collider2D, b: Collider2D) {
        this.removeContact(a);
        this.removeContact(b);
    }
}
