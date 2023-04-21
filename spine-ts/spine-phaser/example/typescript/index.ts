import {Scene} from "phaser"
import {SpinePlugin} from "@esotericsoftware/spine-phaser"

class SpineDemo extends Scene {
    preload() {
        this.load.spineBinary("spineboy-data", "assets/spineboy-pro.skel");
        this.load.spineAtlas("spineboy-atlas", "assets/spineboy-pma.atlas");
    }

    create() {
        let spineboy = this.add.spine(400, 500, 'spineboy-data', "spineboy-atlas");
        this.make
        spineboy.scale = 0.5;
        spineboy.animationState.setAnimation(0, "walk", true);
    }
}

var config = {    
    width: 800,
    height: 600,
    type: Phaser.WEBGL,
    scene: [SpineDemo],
    plugins: {
        scene: [
            { key: "spine.SpinePlugin", plugin: SpinePlugin, mapping: "spine" }
        ]
    }
};

let game = new Phaser.Game(config);