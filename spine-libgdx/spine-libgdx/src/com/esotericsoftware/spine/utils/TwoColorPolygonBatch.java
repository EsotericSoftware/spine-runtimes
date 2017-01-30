/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

package com.esotericsoftware.spine.utils;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.Mesh;
import com.badlogic.gdx.graphics.Mesh.VertexDataType;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.VertexAttribute;
import com.badlogic.gdx.graphics.VertexAttributes.Usage;
import com.badlogic.gdx.graphics.glutils.ShaderProgram;
import com.badlogic.gdx.math.Matrix4;

public class TwoColorPolygonBatch {
	private final Mesh mesh;
	private final float[] vertices;
	private final short[] triangles;
	private final Matrix4 transformMatrix = new Matrix4();
	private final Matrix4 projectionMatrix = new Matrix4();
	private final Matrix4 combinedMatrix = new Matrix4();
	private final ShaderProgram defaultShader;
	private ShaderProgram shader;
	private int vertexIndex, triangleIndex;
	private Texture lastTexture;
	private boolean drawing;
	private int blendSrcFunc = GL20.GL_SRC_ALPHA;
	private int blendDstFunc = GL20.GL_ONE_MINUS_SRC_ALPHA;

	public TwoColorPolygonBatch (int size) {
		this(size, size * 2);
	}

	public TwoColorPolygonBatch (int maxVertices, int maxTriangles) {
		// 32767 is max vertex index.
		if (maxVertices > 32767)
			throw new IllegalArgumentException("Can't have more than 32767 vertices per batch: " + maxTriangles);

		Mesh.VertexDataType vertexDataType = Mesh.VertexDataType.VertexArray;
		if (Gdx.gl30 != null) vertexDataType = VertexDataType.VertexBufferObjectWithVAO;
		mesh = new Mesh(vertexDataType, false, maxVertices, maxTriangles * 3, //
			new VertexAttribute(Usage.Position, 2, "a_position"), //
			new VertexAttribute(Usage.ColorPacked, 4, "a_light"), //
			new VertexAttribute(Usage.ColorPacked, 4, "a_dark"), //
			new VertexAttribute(Usage.TextureCoordinates, 2, "a_texCoord0"));

		vertices = new float[maxVertices * 6];
		triangles = new short[maxTriangles * 3];
		defaultShader = createDefaultShader();
		shader = defaultShader;
		projectionMatrix.setToOrtho2D(0, 0, Gdx.graphics.getWidth(), Gdx.graphics.getHeight());
	}

	public void begin () {
		if (drawing) throw new IllegalStateException("end must be called before begin.");
		Gdx.gl.glDepthMask(false);
		shader.begin();
		setupMatrices();
		drawing = true;
	}

	public void end () {
		if (!drawing) throw new IllegalStateException("begin must be called before end.");
		if (vertexIndex > 0) flush();
		shader.end();
		Gdx.gl.glDepthMask(true);
		Gdx.gl.glDisable(GL20.GL_BLEND);
		lastTexture = null;
		drawing = false;
	}

	public void draw (Texture texture, float[] polygonVertices, int verticesOffset, int verticesCount, short[] polygonTriangles,
		int trianglesOffset, int trianglesCount) {
		if (!drawing) throw new IllegalStateException("begin must be called before draw.");

		final short[] triangles = this.triangles;
		final float[] vertices = this.vertices;

		if (texture != lastTexture) {
			flush();
			lastTexture = texture;
		} else if (triangleIndex + trianglesCount > triangles.length || vertexIndex + verticesCount > vertices.length) //
			flush();

		int triangleIndex = this.triangleIndex;
		final int vertexIndex = this.vertexIndex;
		final int startVertex = vertexIndex / 6;

		for (int i = trianglesOffset, n = i + trianglesCount; i < n; i++)
			triangles[triangleIndex++] = (short)(polygonTriangles[i] + startVertex);
		this.triangleIndex = triangleIndex;

		System.arraycopy(polygonVertices, verticesOffset, vertices, vertexIndex, verticesCount);
		this.vertexIndex += verticesCount;
	}

	public void flush () {
		if (vertexIndex == 0) return;

		lastTexture.bind();
		Mesh mesh = this.mesh;
		mesh.setVertices(vertices, 0, vertexIndex);
		mesh.setIndices(triangles, 0, triangleIndex);
		Gdx.gl.glEnable(GL20.GL_BLEND);
		if (blendSrcFunc != -1) Gdx.gl.glBlendFunc(blendSrcFunc, blendDstFunc);
		mesh.render(shader, GL20.GL_TRIANGLES, 0, triangleIndex);

		vertexIndex = 0;
		triangleIndex = 0;
	}

	public void dispose () {
		mesh.dispose();
		shader.dispose();
	}

	public Matrix4 getProjectionMatrix () {
		return projectionMatrix;
	}

	public Matrix4 getTransformMatrix () {
		return transformMatrix;
	}

	public void setProjectionMatrix (Matrix4 projection) {
		if (drawing) flush();
		projectionMatrix.set(projection);
		if (drawing) setupMatrices();
	}

	public void setTransformMatrix (Matrix4 transform) {
		if (drawing) flush();
		transformMatrix.set(transform);
		if (drawing) setupMatrices();
	}

	private void setupMatrices () {
		combinedMatrix.set(projectionMatrix).mul(transformMatrix);
		shader.setUniformMatrix("u_projTrans", combinedMatrix);
		shader.setUniformi("u_texture", 0);
	}

	public void setShader (ShaderProgram newShader) {
		if (drawing) {
			flush();
			shader.end();
		}
		shader = newShader == null ? defaultShader : newShader;
		if (drawing) {
			shader.begin();
			setupMatrices();
		}
	}

	public void setBlendFunction (int srcFunc, int dstFunc) {
		if (blendSrcFunc == srcFunc && blendDstFunc == dstFunc) return;
		flush();
		blendSrcFunc = srcFunc;
		blendDstFunc = dstFunc;
	}

	private ShaderProgram createDefaultShader () {
		String vertexShader = "attribute vec4 a_position;\n" //
			+ "attribute vec4 a_light;\n" //
			+ "attribute vec3 a_dark;\n" //
			+ "attribute vec2 a_texCoord0;\n" //
			+ "uniform mat4 u_projTrans;\n" //
			+ "varying vec4 v_light;\n" //
			+ "varying vec3 v_dark;\n" //
			+ "varying vec2 v_texCoords;\n" //
			+ "\n" //
			+ "void main()\n" //
			+ "{\n" //
			+ "   v_light = a_light;\n" //
			+ "   v_light.a = v_light.a * (255.0/254.0);\n" //
			+ "   v_dark = a_dark;\n" //
			+ "   v_texCoords = a_texCoord0;\n" //
			+ "   gl_Position =  u_projTrans * a_position;\n" //
			+ "}\n";
		String fragmentShader = "#ifdef GL_ES\n" //
			+ "#define LOWP lowp\n" //
			+ "precision mediump float;\n" //
			+ "#else\n" //
			+ "#define LOWP \n" //
			+ "#endif\n" //
			+ "varying LOWP vec4 v_light;\n" //
			+ "varying LOWP vec3 v_dark;\n" //
			+ "varying vec2 v_texCoords;\n" //
			+ "uniform sampler2D u_texture;\n" //
			+ "void main()\n"//
			+ "{\n" //
			+ "  vec4 texColor = texture2D(u_texture, v_texCoords);\n" //
			+ "  gl_FragColor.a = texColor.a * v_light.a;\n" //
			+ "  gl_FragColor.rgb = (1.0 - texColor.rgb) * v_dark * gl_FragColor.a + texColor.rgb * v_light.rgb;\n" //
			+ "}";

		ShaderProgram shader = new ShaderProgram(vertexShader, fragmentShader);
		if (shader.isCompiled() == false) throw new IllegalArgumentException("Error compiling shader: " + shader.getLog());
		return shader;
	}
}
