import * as fs from "fs"
import { fileURLToPath } from 'url';
import path from 'path';
import CanvasKitInit from "canvaskit-wasm/bin/canvaskit.js";
import UPNG from "@pdf-lib/upng"
import {loadTextureAtlas, SkeletonRenderer, Skeleton, SkeletonBinary, AnimationState, AnimationStateData, AtlasAttachmentLoader, Physics, loadSkeletonData, SkeletonDrawable} from "../dist/index.js"

// Get the current directory
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// This app loads the Spineboy skeleton and its atlas, then renders Spineboy's "portal" animation
// at 30 fps to individual frames, which are then encoded as an animated PNG (APNG), which is
// written to "output.png"
async function main() {
    // Initialize CanvasKit and create a surface and canvas.
    const ck = await CanvasKitInit();
    const surface = ck.MakeSurface(600, 400);
    if (!surface) throw new Error();
    const canvas = surface.getCanvas();

    // Load atlas
    const atlas = await loadTextureAtlas(ck, __dirname + "/assets/spineboy.atlas", async (path) => fs.readFileSync(path));

    // Load the skeleton data
    const skeletonData = await loadSkeletonData(__dirname + "/assets/spineboy-pro.skel", atlas, async (path) => fs.readFileSync(path));

    // Create a SkeletonDrawable
    const drawable = new SkeletonDrawable(skeletonData);

    // Scale and position the skeleton
    drawable.skeleton.x = 300;
    drawable.skeleton.y = 380;
    drawable.skeleton.scaleX = drawable.skeleton.scaleY = 0.5;

    // Set the "hoverboard" animation on track one
    drawable.animationState.setAnimation(0, "hoverboard", true);

    // Create a skeleton renderer to render the skeleton to the canvas with
    const renderer = new SkeletonRenderer(ck);

    // Render the full animation in 1/30 second steps (30fps) and save it to an APNG
    const animationDuration = skeletonData.findAnimation("hoverboard")?.duration ?? 0;
    const FRAME_TIME = 1 / 30; // 30 FPS
    let deltaTime = 0;
    const frames = [];
    const imageInfo = { width: 600, height: 400, colorType: ck.ColorType.RGBA_8888, alphaType: ck.AlphaType.Unpremul, colorSpace: ck.ColorSpace.SRGB };
    const pixelArray = ck.Malloc(Uint8Array, imageInfo.width * imageInfo.height * 4);
    for (let time = 0; time <= animationDuration; time += deltaTime) {
        // Clear the canvas
        canvas.clear(ck.WHITE);

        // Update the drawable, which will advance the animation(s)
        // apply them to the skeleton, and update the skeleton's pose.
        drawable.update(deltaTime);

        // Render the skeleton to the canvas
        renderer.render(canvas, drawable)

        // Read the pixels of the current frame and store it.
        canvas.readPixels(0, 0, imageInfo, pixelArray);
        frames.push(new Uint8Array(pixelArray.toTypedArray()).buffer.slice(0));

        // First frame has deltaTime 0, subsequent use FRAME_TIME
        deltaTime = FRAME_TIME;
    }

    const apng = UPNG.default.encode(frames, 600, 400, 0, frames.map(() => FRAME_TIME * 1000));
    fs.writeFileSync('output.png', Buffer.from(apng));
}

main();
