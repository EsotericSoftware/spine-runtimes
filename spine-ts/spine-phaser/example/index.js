
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
        create: create
    }
};

var game = new Phaser.Game(config);

function preload () {
    this.load.scenePlugin("spine.SpinePlugin", "../dist/iife/spine-phaser.js", "spinePlugin", "spinePlugin");
}

function create () {
    let plugin = this.spinePlugin;
    let numbers = plugin.getNumbers(10);
    this.add.text(10, 10, numbers, { font: '16px Courier', fill: '#00ff00' });
}