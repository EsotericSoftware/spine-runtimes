
/// <reference path="../../spine-core/src/index.ts" />
/// <reference path="../../spine-canvas/src/index.ts" />
/// <reference path="../../spine-webgl/src/index.ts" />
/// <reference path="../src/index.ts" />

var config = {
    type: Phaser.AUTO,
    width: 800,
    height: 600,
    type: Phaser.CANVAS,
    scene: {
        preload: preload,
        create: create,
    },
    plugins: {
        scene: [
            { key: "spine.SpinePlugin", plugin: spine.SpinePlugin, mapping: "spine" }
        ]
    }
};

var game = new Phaser.Game(config);

function preload () {
    this.load.spineJson("raptor-data", "assets/raptor-pro.json");
    this.load.spineAtlas("raptor-atlas", "assets/raptor-pma.atlas");
    this.load.spineBinary("spineboy-data", "assets/spineboy-pro.skel");
    this.load.spineAtlas("spineboy-atlas", "assets/spineboy-pma.atlas");
}

function create () {
    let plugin = this.spine;
    var raptor = this.add.spine(400, 600, 'raptor-data', "raptor-atlas");
    var spineboy = this.add.spine(400, 600, 'spineboy-data', "spineboy-atlas");
    this.add.text(10, 10, "Spine", { font: '16px Courier', fill: '#00ff00' });
}