
package com.esotericsoftware.spine;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;

public class PathTest extends ApplicationAdapter {
	
	
	@Override
	public void create () {
		TextureAtlas atlas = new TextureAtlas(Gdx.files.internal("path/tank.atlas"));
		SkeletonJson json = new SkeletonJson(atlas);
		SkeletonData data = json.readSkeletonData(Gdx.files.internal("path/tank.json"));
		Skeleton skeleton = new Skeleton(data);
		skeleton.x = 0;
		skeleton.y = 0;
		AnimationStateData animData = new AnimationStateData(data);
		AnimationState animState = new AnimationState(animData);		
		animState.setAnimation(0, "drive", true);
		
		float d = 3;
		for (int i = 0; i < 1; i++) {
			skeleton.update(d);
			animState.update(d);
			animState.apply(skeleton);
			skeleton.updateWorldTransform();
			for (Bone bone: skeleton.getBones()) {
				System.out.println(String.format("%s %f %f %f %f %f %f", bone.data.name, bone.a, bone.b, bone.c, bone.d, bone.worldX, bone.worldY));
			}
			System.out.println("===========================================");
			for (int ii = 0; ii < skeleton.slots.size; ii++) {
				Slot slot = skeleton.drawOrder.get(ii);
				System.out.println(slot.data.name);
			}
			System.out.println("===========================================");
			d += 0.1f;
		}
	}

	public static void main (String[] args) {
		new LwjglApplication(new PathTest());
	}
}
