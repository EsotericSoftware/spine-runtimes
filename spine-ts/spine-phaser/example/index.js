
/// <reference path="../../spine-core/src/index.ts" />
/// <reference path="../../spine-canvas/src/index.ts" />
/// <reference path="../../spine-webgl/src/index.ts" />
/// <reference path="../src/index.ts" />

var config = {
    type: Phaser.AUTO,
    width: 800,
    height: 600,
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
    this.load.spine("raptor", "assets/raptor-pro.json", "assets/raptor.atlas", true);
}

function create () {
    let plugin = this.spine;
    let numbers = plugin.getNumbers(10);
    this.add.text(10, 10, numbers, { font: '16px Courier', fill: '#00ff00' });
}