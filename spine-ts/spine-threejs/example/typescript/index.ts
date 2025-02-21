import * as THREE from "three";
import * as spine from "@esotericsoftware/spine-threejs";
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';

let scene: THREE.Scene;
let camera: THREE.PerspectiveCamera;
let renderer: THREE.WebGLRenderer;
let geometry, material, mesh, skeletonMesh: spine.SkeletonMesh;
let atlas;
let atlasLoader;
let assetManager: spine.AssetManager;
let canvas: HTMLCanvasElement;
let controls: OrbitControls;
let lastFrameTime = Date.now() / 1000;

let baseUrl = "assets/";
let skeletonFile = "raptor-pro.json";
let atlasFile = skeletonFile
    .replace("-pro", "")
    .replace("-ess", "")
    .replace(".json", ".atlas");
let animation = "walk";

function init() {
    // create the THREE.JS camera, scene and renderer (WebGL)
    let width = window.innerWidth,
    height = window.innerHeight;
    camera = new THREE.PerspectiveCamera(75, width / height, 1, 3000);
    camera.position.y = 100;
    camera.position.z = 400;
    scene = new THREE.Scene();
    renderer = new THREE.WebGLRenderer();
    renderer.setSize(width, height);
    document.body.appendChild(renderer.domElement);
    canvas = renderer.domElement;
    controls = new OrbitControls(camera, renderer.domElement);

    // LIGHTS - Ambient
    const ambientLight = new THREE.AmbientLight(0xffffff, 7.0)
    scene.add(ambientLight)

    // LIGHTS - spotLight
    const spotLight = new THREE.SpotLight(0xffffff, 5, 1200, Math.PI / 4, 0, 0)
    spotLight.position.set(0, 1000, 0)
    spotLight.castShadow = true
    spotLight.shadow.mapSize.set(8192, 8192)
    spotLight.shadow.bias = -0.00001;
    scene.add(spotLight)

    // load the assets required to display the Raptor model
    assetManager = new spine.AssetManager(baseUrl);
    assetManager.loadText(skeletonFile);
    assetManager.loadTextureAtlas(atlasFile);

    requestAnimationFrame(load);
}

function load() {
    if (assetManager.isLoadingComplete()) {
    // Add a box to the scene to which we attach the skeleton mesh
    geometry = new THREE.BoxGeometry(200, 200, 200);
    material = new THREE.MeshBasicMaterial({
        color: 0xff0000,
        wireframe: true,
    });
    mesh = new THREE.Mesh(geometry, material);
    scene.add(mesh);

    // Load the texture atlas using name.atlas and name.png from the AssetManager.
    // The function passed to TextureAtlas is used to resolve relative paths.
    atlas = assetManager.require(atlasFile);

    // Create a AtlasAttachmentLoader that resolves region, mesh, boundingbox and path attachments
    atlasLoader = new spine.AtlasAttachmentLoader(atlas);

    // Create a SkeletonJson instance for parsing the .json file.
    let skeletonJson = new spine.SkeletonJson(atlasLoader);

    // Set the scale to apply during parsing, parse the file, and create a new skeleton.
    skeletonJson.scale = 0.4;
    let skeletonData = skeletonJson.readSkeletonData(
        assetManager.require(skeletonFile)
    );

    // Create a SkeletonMesh from the data and attach it to the scene
    skeletonMesh = new spine.SkeletonMesh({
        skeletonData,
        materialFactory(parameters) {
            return new THREE.MeshStandardMaterial({ ...parameters, metalness: .5 });
        },
    });
    skeletonMesh.state.setAnimation(0, animation, true);
    mesh.add(skeletonMesh);

    requestAnimationFrame(render);
    } else requestAnimationFrame(load);
}

let lastTime = Date.now();
function render() {
    // calculate delta time for animation purposes
    let now = Date.now() / 1000;
    let delta = now - lastFrameTime;
    lastFrameTime = now;

    // resize canvas to use full page, adjust camera/renderer
    resize();

    // Update orbital controls
    controls.update();

    // update the animation
    skeletonMesh.update(delta);

    // render the scene
    renderer.render(scene, camera);

    requestAnimationFrame(render);
}

function resize() {
    let w = window.innerWidth;
    let h = window.innerHeight;
    if (canvas.width != w || canvas.height != h) {
    canvas.width = w;
    canvas.height = h;
    }

    camera.aspect = w / h;
    camera.updateProjectionMatrix();

    renderer.setSize(w, h);
}

init();