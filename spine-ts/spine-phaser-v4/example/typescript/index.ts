import * as Phaser from "phaser"
import * as spine from "@esotericsoftware/spine-phaser-v4"

class SpineDemo extends Phaser.Scene {
    preload() {
        this.load.spineBinary("spineboy-data", "assets/spineboy-pro.skel");
        this.load.spineAtlas("spineboy-atlas", "assets/spineboy-pma.atlas");
    }

    create() {
        const spineboy = this.add.spine(400, 500, 'spineboy-data', "spineboy-atlas");
        spineboy.scale = 0.5;
        spineboy.animationState.setAnimation(0, "walk", true);
    }
}

const config = {
    width: 800,
    height: 600,
    type: Phaser.WEBGL,
    scene: [SpineDemo],
    plugins: {
        scene: [
            { key: "spine.SpinePlugin", plugin: spine.SpinePlugin, mapping: "spine" }
        ]
    }
};

new Phaser.Game(config);