class App {
    constructor() {
        this.skeleton = null;
        this.animationState = null;
        this.canvas = null;
        this.pma = true;
    }

    loadAssets(canvas) {
        this.canvas = canvas;

        // Load assets of Spineboy.
        canvas.assetManager.loadBinary("assets/spineboy-pro.skel");
        canvas.assetManager.loadTextureAtlas("assets/spineboy-pma.atlas");
    }

    initialize(canvas) {
        // Load the Spineboy skeleton
        this.loadSkeleton("assets/spineboy-pro.skel", "assets/spineboy-pma.atlas", "run");

        // Setup listener for animation selection box
        let animationSelectBox = document.body.querySelector("#animations");
        animationSelectBox.onchange = () => {
            this.animationState.setAnimation(0, animationSelectBox.value, true);
        }

        // Setup listener for the PMA checkbox
        let pmaCheckbox = document.body.querySelector("#pma");
        pmaCheckbox.onchange = () => {
            this.pma = pmaCheckbox.checked;
        }

        // Setup the drag and drop listener
        new FileDragAndDrop(canvas.htmlCanvas, (files) => this.onDrop(files))

        // Setup a camera controller for paning and zooming
        new spine.CameraController(canvas.htmlCanvas, canvas.renderer.camera);
    }

    onDrop(files) {
        let atlasFile;
        let skeletonFile;
        let pngs = [];
        let assetManager = this.canvas.assetManager;

        // We use data URIs to load the dropped files. Some file types
        // are binary, so we have to encode them to base64 for loading
        // through AssetManager.
        let bufferToBase64 = (buffer) => {
            var binary = '';
            var bytes = new Uint8Array(buffer);
            var len = bytes.byteLength;
            for (var i = 0; i < len; i++) {
                binary += String.fromCharCode(bytes[i]);
            }
            return window.btoa(binary);
        }

        for (var file of files) {
            if (file.name.endsWith(".atlas") || file.name.endsWith(".atlas.txt")) {
                atlasFile = file;
                assetManager.setRawDataURI(file.name, "data:text/plain;," + file.contentText);
            } else if (file.name.endsWith(".skel")) {
                skeletonFile = file;
                assetManager.setRawDataURI(file.name, "data:application/octet-stream;base64," + bufferToBase64(file.contentBinary));
                assetManager.loadBinary(file.name);
            } else if (file.name.endsWith(".json")) {
                skeletonFile = file;
                assetManager.setRawDataURI(file.name, "data:text/plain;," + file.contentText);
                assetManager.loadJson(file.name);
            } else if (file.name.endsWith(".png")) {
                pngs.push(file);
                assetManager.setRawDataURI(file.name, "data:image/png;base64," + bufferToBase64(file.contentBinary));
            }
        }

        if (!atlasFile) {
            alert("Please provide a .atlas or .atlas.txt atlas file.");
            return;
        }
        if (pngs.length == 0) {
            alert("Please provide the atlas page .png file(s).");
        }
        if (!skeletonFile) {
            alert("Please provide a .skel or .json skeleton file.");
            return;
        }

        assetManager.loadTextureAtlas(atlasFile.name);

        let waitForLoad = () => {
            if (this.canvas.assetManager.isLoadingComplete()) {
                this.loadSkeleton(skeletonFile.name, atlasFile.name);
            } else {
                requestAnimationFrame(waitForLoad);
            }
        }
        waitForLoad();
    }

    loadSkeleton(skeletonFile, atlasFile, animationName) {
        // Load the skeleton and setup the animation state
        let assetManager = this.canvas.assetManager;
        var atlas = assetManager.require(atlasFile);
        var atlasLoader = new spine.AtlasAttachmentLoader(atlas);
        var skeletonData;
        var skeletonBinaryOrJson = skeletonFile.endsWith(".skel") ?
            new spine.SkeletonBinary(atlasLoader) :
            new spine.SkeletonJson(atlasLoader);
        skeletonBinaryOrJson.scale = 1;
        skeletonData = skeletonBinaryOrJson.readSkeletonData(assetManager.require(skeletonFile));
        this.skeleton = new spine.Skeleton(skeletonData);
        var animationStateData = new spine.AnimationStateData(skeletonData);
        this.animationState = new spine.AnimationState(animationStateData);

        // Fill the animation selection box.
        let animationSelectBox = document.body.querySelector("#animations");
        animationSelectBox.innerHTML = "";
        for (var animation of this.skeleton.data.animations) {
            if (!animationName) animationName = animation.name;
            let option = document.createElement("option");
            option.value = option.innerText = animation.name;
            option.selected = animation.name == animationName;
            animationSelectBox.appendChild(option);
        }
        this.animationState.setAnimation(0, animationName, true);

        // Center the skeleton in the viewport
        this.centerSkeleton();
    }

    centerSkeleton() {
        // Calculate the bounds of the skeleton
        this.animationState.update(0);
        this.animationState.apply(this.skeleton);
        this.skeleton.updateWorldTransform();
        let offset = new spine.Vector2(), size = new spine.Vector2();
        this.skeleton.getBounds(offset, size);

        // Make sure the canvas is sized properly and position and zoom
        // the camera so the skeleton is centered in the viewport.
        let renderer = this.canvas.renderer;
        renderer.resize(spine.ResizeMode.Expand);
        let camera = this.canvas.renderer.camera;
        camera.position.x = offset.x + size.x / 2;
        camera.position.y = offset.y + size.y / 2;
        camera.zoom = size.x > size.y ? size.x / this.canvas.htmlCanvas.width * 3 : size.y / this.canvas.htmlCanvas.height * 3;
        camera.update();
    }

    update(canvas, delta) {
        this.animationState.update(delta);
        this.animationState.apply(this.skeleton);
        this.skeleton.updateWorldTransform();
    }

    render(canvas) {
        let renderer = canvas.renderer;
        renderer.resize(spine.ResizeMode.Expand);

        canvas.clear(0.2, 0.2, 0.2, 1);

        renderer.begin();
        renderer.line(-10000, 0, 10000, 0, spine.Color.RED);
        renderer.line(0, -10000, 0, 10000, spine.Color.GREEN);
        renderer.drawSkeleton(this.skeleton, this.pma);
        renderer.end();
    }
}

new spine.SpineCanvas(document.getElementById("canvas"), {
    app: new App(),
    webglConfig: {
        alpha: false
    }
});

class FileDragAndDrop {
    constructor(element, callback) {
        this.callback = callback;
        element.ondrop = (ev) => this.onDrop(ev);
        element.ondragover = (ev) => ev.preventDefault();
    }

    async onDrop(event) {
        event.preventDefault();
        event.stopPropagation();

        const items = Object.keys(event.dataTransfer.items);
        let files = [];
        await Promise.all(items.map(async (key) => {
            var file = event.dataTransfer.items[key].getAsFile();
            if (file.kind == "string") return;
            let contentBinary = await file.arrayBuffer();
            let contentText = await file.text();
            files.push({ name: file.name, contentBinary: contentBinary, contentText: contentText });
        }));
        this.callback(files);
    }
}

// Shim for older browsers for File/Blob.arrayBuffer() and .text()
(function () {
    function arrayBuffer() {
        return new Promise(function () {
            let fr = new FileReader();
            fr.onload = () => {
                resolve(fr.result);
            };
            fr.readAsArrayBuffer();
        })
    }

    function text() {
        return new Promise(function () {
            let fr = new FileReader();
            fr.onload = () => {
                resolve(fr.result);
            };
            fr.readAsText(this);
        })
    }

    if ('File' in self) {
        File.prototype.arrayBuffer = File.prototype.arrayBuffer || arrayBuffer;
        File.prototype.text = File.prototype.text || text;
    }
    Blob.prototype.arrayBuffer = Blob.prototype.arrayBuffer || arrayBuffer;
    Blob.prototype.text = Blob.prototype.text || text;
})();