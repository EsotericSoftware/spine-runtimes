
package com.esotericsoftware.spine.vertexeffects;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.math.Interpolation;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector2;
import com.esotericsoftware.spine.Skeleton;
import com.esotericsoftware.spine.SkeletonRenderer.VertexEffect;
import com.esotericsoftware.spine.utils.SpineUtils;

public class SwirlEffect implements VertexEffect {
	private float worldX, worldY, radius, angle;
	private Interpolation interpolation = Interpolation.pow2Out;
	private float centerX, centerY;

	public SwirlEffect (float radius) {
		this.radius = radius;
	}

	public void begin (Skeleton skeleton) {
		worldX = skeleton.getX() + centerX;
		worldY = skeleton.getY() + centerY;
	}

	public void transform (Vector2 position, Vector2 uv, Color light, Color dark) {
		float x = position.x - worldX;
		float y = position.y - worldY;
		float dist = (float)Math.sqrt(x * x + y * y);
		if (dist < radius) {
			float theta = interpolation.apply(0, angle, (radius - dist) / radius);
			float cos = SpineUtils.cos(theta), sin = SpineUtils.sin(theta);
			position.x = cos * x - sin * y + worldX;
			position.y = sin * x + cos * y + worldY;
		}
	}

	public void end () {
	}

	public void setRadius (float radius) {
		this.radius = radius;
	}

	public void setCenter (float centerX, float centerY) {
		this.centerX = centerX;
		this.centerY = centerY;
	}

	public void setCenterX (float centerX) {
		this.centerX = centerX;
	}

	public void setCenterY (float centerY) {
		this.centerY = centerY;
	}

	public void setAngle (float degrees) {
		this.angle = degrees * MathUtils.degRad;
	}

	public Interpolation getInterpolation () {
		return interpolation;
	}

	public void setInterpolation (Interpolation interpolation) {
		this.interpolation = interpolation;
	}
}
