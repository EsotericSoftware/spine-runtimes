import * as esbuild from "esbuild";

await esbuild.build({
  bundle: true,
  entryPoints: ["./src/index.ts"],
  outfile: "./dist/iife/spine-construct3-lib.js",
  format: "iife",
  globalName: "spine",
  footer: { js: "if(!globalThis.spine)globalThis.spine=spine;" },
});
