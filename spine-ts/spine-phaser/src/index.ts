export * from "./require-shim"
import { SpinePlugin } from "./SpinePlugin";
{
    let w = window as any;
    w["spine.SpinePlugin"] = SpinePlugin;
}
export * from "./SpinePlugin"
export * from "./SpineFile"
export * from "@esotericsoftware/spine-core";
export * from "@esotericsoftware/spine-canvas";
