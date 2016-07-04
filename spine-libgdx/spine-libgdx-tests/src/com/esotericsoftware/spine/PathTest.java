
package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

public class PathTest extends ApplicationAdapter {
	
	
	@Override
	public void create () {
		TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("path/test.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		SkeletonData data = json.readSkeletonData(Gdx.files.internal("path/test.json"));
		Skeleton skeleton = new Skeleton(data);
		skeleton.x = 0;
		skeleton.y = 0;
		AnimationStateData animData = new AnimationStateData(data);
		AnimationState animState = new AnimationState(animData);
		Bone bone = skeleton.findBone("image");
		animState.setAnimation(0, "test", true);
		
		float d = 0;
		for (int i = 0; i < 20; i++) {
			skeleton.update(d);
			animState.update(d);
			animState.apply(skeleton);
			skeleton.updateWorldTransform();
			System.out.println(String.format("%f %f %f %f %f %f", bone.a, bone.b, bone.c, bone.d, bone.worldX, bone.worldY));
			d += 0.1f;
		}
	}

	public static void main (String[] args) {
		new LwjglApplication(new PathTest());
	}
}
