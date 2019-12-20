/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated January 1, 2020. Replaces all prior versions.
 *
 * Copyright (c) 2013-2020, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

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
