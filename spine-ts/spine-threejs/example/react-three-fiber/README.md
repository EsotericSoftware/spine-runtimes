# spine-threejs + React Three Fiber example

This example verifies that `@esotericsoftware/spine-threejs` can be used from React Three Fiber by loading Spine assets in a React hook, adding a `spine.SkeletonMesh` to the scene with R3F's `<primitive />`, and advancing it from `useFrame`.

```sh
npm install
npm run dev
```

Open the Vite URL and you should see the animated raptor. The Vite dev server serves the repository's top-level `assets/` directory as this example's public directory, so no asset copies are needed.

For a production app, copy the Spine `.json`/`.skel`, `.atlas`, and texture files into your app's public assets directory and update `baseUrl` in `src/main.tsx`.
