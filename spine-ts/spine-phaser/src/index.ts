export * from "./require-shim"
export * from "./SpinePlugin"
export * from "./mixins"
export * from "@esotericsoftware/spine-core";
export * from "@esotericsoftware/spine-canvas";
import { SpinePlugin } from "./SpinePlugin";
(window as any).spine = { SpinePlugin: SpinePlugin };
