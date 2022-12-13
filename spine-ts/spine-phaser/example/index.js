
/// <reference path="../../spine-core/src/index.ts" />
/// <reference path="../../spine-canvas/src/index.ts" />
/// <reference path="../../spine-webgl/src/index.ts" />
/// <reference path="../src/index.ts" />

var config = {
    type: Phaser.AUTO,
    width: 800,
    height: 600,
    type: Phaser.WEBGL,
    scene: {
        preload: preload,
        create: create,
        update: update,
    },
    plugins: {
        scene: [
            { key: "spine.SpinePlugin", plugin: spine.SpinePlugin, mapping: "spine" }
        ]
    }
};

let game = new Phaser.Game(config);
let debug;

function preload () {
    this.load.spineJson("raptor-data", "assets/raptor-pro.json");
    this.load.spineAtlas("raptor-atlas", "assets/raptor-pma.atlas");
    this.load.spineBinary("spineboy-data", "assets/spineboy-pro.skel");
    this.load.spineAtlas("spineboy-atlas", "assets/spineboy-pma.atlas");
    this.load.image("nyan", "nyan.png");
    let canvas = document.querySelector("#game-canvas");
}

function create () {
    let plugin = this.spine;
    let x = 25;
    let y = 60;
    for (let j = 0; j < 10; j++, y+= 600 / 10) {
        for (let i = 0; i < 20; i++, x += 800 / 20) {
            let obj = Math.random() > 0.5
                ? this.add.spine(x, y, 'spineboy-data', "spineboy-atlas")
                : this.add.spine(x, y, 'raptor-data', "raptor-atlas");
            obj.animationState.setAnimation(0, "walk", true);
            obj.scale = 0.1;
        }
        x = 25;
    }
    debug = this.add.text(0, 600 - 40, "FPS: ");
}

function update () {
    debug.setText("draw calls: " + spine.PolygonBatcher.getAndResetGlobalDrawCalls() + "\ndelta: " + game.loop.delta);
}