import * as spine from "@esotericsoftware/spine-threejs";
import { Canvas, useFrame } from "@react-three/fiber";
import React, { useEffect, useMemo, useState } from "react";
import { createRoot } from "react-dom/client";
import * as THREE from "three";

type SpineAsset = {
  skeletonData: spine.SkeletonData;
};

const baseUrl = "/";
const skeletonFile = "raptor-pro.json";
const atlasFile = "raptor-pma.atlas";
const pma = true;
const animationName = "walk";

function useSpineAsset(): SpineAsset | null {
  const [asset, setAsset] = useState<SpineAsset | null>(null);

  useEffect(() => {
    let cancelled = false;
    const assetManager = new spine.AssetManager(baseUrl, undefined, pma);
    assetManager.loadText(skeletonFile);
    assetManager.loadTextureAtlas(atlasFile);

    assetManager.loadAll().then(() => {
      if (cancelled) return;

      const atlas = assetManager.require(atlasFile) as spine.TextureAtlas;
      const atlasLoader = new spine.AtlasAttachmentLoader(atlas);
      const skeletonJson = new spine.SkeletonJson(atlasLoader);
      skeletonJson.scale = 0.4;

      setAsset({
        skeletonData: skeletonJson.readSkeletonData(assetManager.require(skeletonFile)),
      });
    });

    return () => {
      cancelled = true;
    };
  }, []);

  return asset;
}

function SpineRaptor() {
  const asset = useSpineAsset();
  const skeletonMesh = useMemo(() => {
    if (!asset) return null;

    const mesh = new spine.SkeletonMesh({
      skeletonData: asset.skeletonData,
      materialFactory(parameters) {
        return new THREE.MeshBasicMaterial(parameters);
      },
    });

    mesh.state.setAnimation(0, animationName, true);
    mesh.position.set(-50, -150, 0);
    return mesh;
  }, [asset]);

  useEffect(() => {
    return () => {
      skeletonMesh?.dispose();
    };
  }, [skeletonMesh]);

  useFrame((_, delta) => {
    skeletonMesh?.update(delta);
  });

  return skeletonMesh ? <primitive object={skeletonMesh} /> : null;
}

function Scene() {
  return (
    <>
      <color attach="background" args={["#20232a"]} />
      <SpineRaptor />
      <gridHelper args={[600, 12, "#555", "#333"]} position={[0, 0, 0]} rotation={[Math.PI / 2, 0, 0]} />
    </>
  );
}

createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <Canvas flat camera={{ position: [0, 50, 450], fov: 75, near: 1, far: 3000 }}>
      <Scene />
    </Canvas>
  </React.StrictMode>,
);
