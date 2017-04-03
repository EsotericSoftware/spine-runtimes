
package com.esotericsoftware.spine;

import org.lwjgl.opengl.GL11;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input.Buttons;
import com.badlogic.gdx.Input.Keys;
import com.badlogic.gdx.backends.lwjgl.LwjglApplication;
import com.badlogic.gdx.backends.lwjgl.LwjglApplicationConfiguration;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.GL20;
import com.badlogic.gdx.graphics.OrthographicCamera;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.utils.Clipper;
import com.esotericsoftware.spine.utils.ConvexDecomposer;

public class SoftwareClippingTest extends ApplicationAdapter {
	OrthographicCamera sceneCamera;
	ShapeRenderer shapes;
	PolygonSpriteBatch polyBatcher;
	Texture image;

	float[] triangleOutline = {100, 100, 300, 100, 200, 300};
	float[] triangle = {100, 100, Color.WHITE.toFloatBits(), 0, 1, 300, 100, Color.WHITE.toFloatBits(), 1, 1, 200, 300,
		Color.WHITE.toFloatBits(), 0.5f, 0};
	short[] triangleIndices = {0, 1, 2};
	FloatArray clippingPolygon = new FloatArray();
	FloatArray clippedPolygon = new FloatArray();

	FloatArray clippedPolygonVertices = new FloatArray();
	ShortArray clippedPolygonIndices = new ShortArray();

	boolean isCreatingClippingArea = false;
	Vector3 tmp = new Vector3();
	Clipper clipper;
	ConvexDecomposer decomposer;

	@Override
	public void create () {
		sceneCamera = new OrthographicCamera();
		shapes = new ShapeRenderer();
		polyBatcher = new PolygonSpriteBatch();
		clipper = new Clipper();
		decomposer = new ConvexDecomposer();
		image = new Texture("skin/skin.png");
		
		float[] v = new float[] {430.90802f, 278.212f, 72.164f, 361.816f, 31.143997f, 128.804f, 191.896f, 61.0f, 291.312f,
			175.73201f, 143.956f, 207.408f, 161.4f, 145.628f, 227.456f, 160.61601f, 224.392f, 126.535995f, 188.264f, 113.144f,
			147.13199f, 108.87601f, 77.035995f, 158.212f, 86.15199f, 220.676f, 102.77199f, 240.716f, 174.74399f, 243.20801f,
			250.572f, 216.74802f, 324.772f, 200.33202f, 309.388f, 124.968f, 258.168f, 60.503998f, 199.696f, 42.872f, 116.951996f,
			6.7400017f, 11.332001f, 72.48f, -6.708008f, 143.136f, 1.0679932f, 239.92801f, 26.5f, 355.6f, -47.380005f, 377.52798f,
			-40.608f, 303.1f, -53.584015f, 77.316f, 5.4600067f, 8.728001f, 113.343994f, -56.04f, 192.42801f, -45.112f, 274.564f,
			-38.784f, 322.592f, -10.604f, 371.98f, 21.920002f, 405.16f, 60.896004f, 428.68f, 104.852005f, 406.996f, 188.976f,
			364.58398f, 220.14401f, 309.3f, 238.788f, 263.232f, 244.75201f, 219.468f, 271.58002f, 210.824f, 294.176f, 250.664f,
			295.2f, 295.972f, 276.02f, 357.46f, 269.172f, 420.008f, 242.37201f, 466.63602f, 207.648f, 437.516f, -10.579998f,
			378.05603f, -64.624f, 465.24f, -104.992f, 554.11206f, 95.43199f, 514.89197f, 259.02f};
		for (int i = 0, n = v.length; i < n; i++)
			v[i] = v[i] * 0.5f + 70;
		clippingPolygon.addAll(v);
		clip();
	}

	@Override
	public void resize (int width, int height) {
		sceneCamera.setToOrtho(false);
	}

	@Override
	public void render () {
		Gdx.gl.glClearColor(0.3f, 0.3f, 0.3f, 1);
		Gdx.gl.glClear(GL20.GL_COLOR_BUFFER_BIT);

		processInput();
		renderScene();
	}

	private void processInput () {
		tmp.set(Gdx.input.getX(), Gdx.input.getY(), 0);
		sceneCamera.unproject(tmp);

		if (Gdx.input.justTouched()) {
			if (!isCreatingClippingArea) {
				clippingPolygon.clear();
				isCreatingClippingArea = true;
			}

			clippingPolygon.add((int)tmp.x);
			clippingPolygon.add((int)tmp.y);

			if (Gdx.input.isButtonPressed(Buttons.RIGHT)) {
				isCreatingClippingArea = false;
				clip();
			}
		}

		if (Gdx.input.isKeyJustPressed(Keys.T)) {
			clip();
		}
	}

	private void renderScene () {
		sceneCamera.update();
		shapes.setProjectionMatrix(sceneCamera.combined);
		polyBatcher.setProjectionMatrix(sceneCamera.combined);

		polyBatcher.begin();
		polyBatcher.disableBlending();

		// clipped polygon
		if (clippedPolygonVertices.size == 0) {
			polyBatcher.draw(image, triangle, 0, 15, triangleIndices, 0, 3);
		} else {
			polyBatcher.draw(image, clippedPolygonVertices.items, 0, clippedPolygonVertices.size, clippedPolygonIndices.items, 0,
				clippedPolygonIndices.size);
		}
		polyBatcher.end();

		shapes.begin(ShapeType.Line);

		// triangle
		shapes.setColor(Color.GREEN);
		shapes.polygon(triangleOutline);

		// clipping area
		shapes.setColor(Color.RED);
		if (isCreatingClippingArea) {
			tmp.set(Gdx.input.getX(), Gdx.input.getY(), 0);
			sceneCamera.unproject(tmp);
			clippingPolygon.add(tmp.x);
			clippingPolygon.add(tmp.y);
		}

		switch (clippingPolygon.size) {
		case 0:
			break;
		case 2:
			shapes.end();
			shapes.begin(ShapeType.Point);
			GL11.glPointSize(4);
			shapes.point(clippingPolygon.get(0), clippingPolygon.get(1), 0);
			shapes.end();
			shapes.begin(ShapeType.Line);
			break;
		case 4:
			shapes.line(clippingPolygon.get(0), clippingPolygon.get(1), clippingPolygon.get(2), clippingPolygon.get(3));
			break;
		default:
			shapes.polygon(clippingPolygon.items, 0, clippingPolygon.size);
		}

// // edge normals
// shapes.setColor(Color.YELLOW);
// if (clippingPolygon.size > 2) {
// boolean clockwise = Clipper.isClockwise(clippingPolygon);
// for (int i = 0; i < clippingPolygon.size; i += 2) {
// float x = clippingPolygon.get(i);
// float y = clippingPolygon.get(i + 1);
// float x2 = clippingPolygon.get((i + 2) % clippingPolygon.size);
// float y2 = clippingPolygon.get((i + 3) % clippingPolygon.size);
//
// float mx = x + (x2 - x) / 2;
// float my = y + (y2 - y) / 2;
// float nx = (y2 - y);
// float ny = -(x2 - x);
// if (!clockwise) {
// nx = -nx;
// ny = -ny;
// }
// float l = 1 / (float)Math.sqrt(nx * nx + ny * ny);
// nx *= l * 20;
// ny *= l * 20;
//
// shapes.line(mx, my, mx + nx, my + ny);
// }
// }

		if (isCreatingClippingArea) {
			clippingPolygon.setSize(clippingPolygon.size - 2);
		}

// // clipped polygon
// shapes.setColor(Color.PINK);
// if (clippedPolygon.size > 0) {
// shapes.polygon(clippedPolygon.items, 0, clippedPolygon.size);
// }

		shapes.end();
	}

	private void clip () {
		float x1 = triangle[0];
		float y1 = triangle[1];
		float x2 = triangle[5];
		float y2 = triangle[6];
		float x3 = triangle[10];
		float y3 = triangle[11];

		Clipper.makeClockwise(clippingPolygon);
		Array<FloatArray> clippingPolygons = decomposer.decompose(clippingPolygon);
		clippedPolygonVertices.clear();
		clippedPolygonIndices.clear();

		for (FloatArray poly : clippingPolygons) {
			Clipper.makeClockwise(poly);
			poly.add(poly.get(0));
			poly.add(poly.get(1));

			boolean clipped = clipper.clip(x1, y1, x2, y2, x3, y3, poly, clippedPolygon);
			System.out.println("Clipped: " + clipped);
			if (clipped) {
				float d0 = y2 - y3;
				float d1 = x3 - x2;
				float d2 = x1 - x3;
				float d3 = y1 - y3;
				float d4 = y3 - y1;

				float denom = 1 / (d0 * d2 + d1 * d3);

				// triangulate by creating a triangle fan, duplicate vertices
				int o = clippedPolygonVertices.size / 5;
				float color = Color.WHITE.toFloatBits();
				for (int i = 0; i < clippedPolygon.size; i += 2) {
					float x = clippedPolygon.get(i);
					float y = clippedPolygon.get(i + 1);

					float a = (d0 * (x - x3) + d1 * (y - y3)) * denom;
					float b = (d4 * (x - x3) + d2 * (y - y3)) * denom;
					float c = 1.0f - a - b;

					float u = triangle[3] * a + triangle[8] * b + triangle[13] * c;
					float v = triangle[4] * a + triangle[9] * b + triangle[14] * c;
					clippedPolygonVertices.add(x);
					clippedPolygonVertices.add(y);
					clippedPolygonVertices.add(color);
					clippedPolygonVertices.add(u);
					clippedPolygonVertices.add(v);
				}

				for (int i = 1; i < (clippedPolygon.size >> 1) - 1; i++) {
					clippedPolygonIndices.add(o);
					clippedPolygonIndices.add(o + i);
					clippedPolygonIndices.add(o + i + 1);
				}
			} else {
				clippedPolygon.clear();
			}

			poly.setSize(poly.size - 2);
		}
	}

	public static void main (String[] args) {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		new LwjglApplication(new SoftwareClippingTest(), config);
	}
}
