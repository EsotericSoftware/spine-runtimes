export * from "./require-shim"
export * from "./SpinePlugin"
export * from "./SpineGameObject"
export * from "./mixins"
export * from "@esotericsoftware/spine-core";
export * from "@esotericsoftware/spine-webgl";
import { SpinePlugin } from "./SpinePlugin";
(window as any).spine = { SpinePlugin: SpinePlugin };
