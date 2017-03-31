
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
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.math.Intersector;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector2;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.utils.ConvexDecomposer;
import com.esotericsoftware.spine.utils.Clipper;

public class ConvexDecomposerTest extends ApplicationAdapter {
	OrthographicCamera sceneCamera;
	ShapeRenderer shapes;
	PolygonSpriteBatch polyBatcher;
	Texture image;
	ConvexDecomposer decomposer = new ConvexDecomposer();
	FloatArray polygon = new FloatArray();
	Array<FloatArray> convexPolygons = new Array<FloatArray>();
	boolean isCreatingPolygon = false;
	Vector3 tmp = new Vector3();
	Array<Color> colors = new Array<Color>();
	BitmapFont font;

	@Override
	public void create () {
		sceneCamera = new OrthographicCamera();
		shapes = new ShapeRenderer();
		polyBatcher = new PolygonSpriteBatch();
		image = new Texture("skin/skin.png");
		font = new BitmapFont();

		float[] v = new float[] { 94.0f, 84.0f, 45.0f, 165.0f, 218.0f, 292.0f, 476.0f, 227.0f, 480.0f, 125.0f, 325.0f, 191.0f, 333.0f, 77.0f, 302.0f, 30.0f, 175.0f, 140.0f };

// float[] v = {87, 288, 217, 371, 456, 361, 539, 175, 304, 194, 392, 290, 193, 214, 123, 15, 14, 137};
// float[] v = { 336, 153, 207, 184, 364, 333, 529, 326, 584, 130, 438, 224 };
		polygon.addAll(v);
		triangulate();
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
			if (!isCreatingPolygon) {
				polygon.clear();
				convexPolygons = null;
				isCreatingPolygon = true;
			}

			polygon.add((int)tmp.x);
			polygon.add((int)tmp.y);

			if (Gdx.input.isButtonPressed(Buttons.RIGHT)) {
				isCreatingPolygon = false;
				System.out.print("float[] v = { ");
				for (int i = 0; i < polygon.size; i++) {
					System.out.print(polygon.get(i));
					if (i != polygon.size - 1) System.out.print(", ");
				}
				System.out.println("};");
				triangulate();
			}
		}

		if (Gdx.input.isKeyJustPressed(Keys.R)) {
			long start = System.nanoTime();
			generateRandomPolygon();
			System.out.println("Took: " + (System.nanoTime() - start) / 1000000000.0f + " secs");
			System.out.print("float[] v = { ");
			for (int i = 0; i < polygon.size; i++) {
				System.out.print(polygon.get(i));
				if (i != polygon.size - 1) System.out.print(", ");
			}
			System.out.println("};");
			triangulate();
		}

		if (Gdx.input.isKeyJustPressed(Keys.T)) {
			triangulate();
		}
	}

	private void generateRandomPolygon () {
		polygon.clear();
		convexPolygons.clear();

		int numVertices = MathUtils.random(3, 30);
		for (int i = 0; i < numVertices; i++) {
			float x = (float)(50 + Math.random() * (Gdx.graphics.getWidth() - 50));
			float y = (float)(50 + Math.random() * (Gdx.graphics.getHeight() - 50));

			polygon.add(x);
			polygon.add(y);
			System.out.println(polygon.toString(","));
			if (selfIntersects(polygon)) {
				polygon.size -= 2;
				i--;
			}
		}
	}

	private boolean selfIntersects (FloatArray polygon) {
		Vector2 tmp = new Vector2();
		if (polygon.size == 6) return false;
		for (int i = 0, n = polygon.size; i <= n; i += 2) {
			float x1 = polygon.get(i % n);
			float y1 = polygon.get((i + 1) % n);
			float x2 = polygon.get((i + 2) % n);
			float y2 = polygon.get((i + 3) % n);

			for (int j = 0; j <= n; j += 2) {
				float x3 = polygon.get(j % n);
				float y3 = polygon.get((j + 1) % n);
				float x4 = polygon.get((j + 2) % n);
				float y4 = polygon.get((j + 3) % n);
				if (x1 == x3 && y1 == y3) continue;
				if (x1 == x4 && y1 == y4) continue;
				if (x2 == x3 && y2 == y3) continue;
				if (x2 == x4 && y2 == y4) continue;
				if (Intersector.intersectSegments(x1, y1, x2, y2, x3, y3, x4, y4, tmp)) return true;
			}
		}
		return false;
	}

	private void renderScene () {
		sceneCamera.update();
		shapes.setProjectionMatrix(sceneCamera.combined);
		polyBatcher.setProjectionMatrix(sceneCamera.combined);

		polyBatcher.begin();
		polyBatcher.disableBlending();

		polyBatcher.end();

		// polygon
		shapes.setColor(Color.RED);
		shapes.begin(ShapeType.Line);
		if (isCreatingPolygon) {
			tmp.set(Gdx.input.getX(), Gdx.input.getY(), 0);
			sceneCamera.unproject(tmp);
			polygon.add(tmp.x);
			polygon.add(tmp.y);
		}

		// polygon while drawing
		switch (polygon.size) {
		case 0:
			break;
		case 2:
			shapes.end();
			shapes.begin(ShapeType.Point);
			GL11.glPointSize(4);
			shapes.point(polygon.get(0), polygon.get(1), 0);
			shapes.end();
			shapes.begin(ShapeType.Line);
			break;
		case 4:
			shapes.line(polygon.get(0), polygon.get(1), polygon.get(2), polygon.get(3));
			break;
		default:
			shapes.polygon(polygon.items, 0, polygon.size);
		}

		// edge normals
// shapes.setColor(Color.YELLOW);
// if (polygon.size > 2) {
// boolean clockwise = Clipper.isClockwise(polygon);
// for (int i = 0; i < polygon.size; i += 2) {
// float x = polygon.get(i);
// float y = polygon.get(i + 1);
// float x2 = polygon.get((i + 2) % polygon.size);
// float y2 = polygon.get((i + 3) % polygon.size);
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

		// decomposition
		if (convexPolygons != null) {
			for (int i = 0, n = convexPolygons.size; i < n; i++) {
				if (colors.size <= i) {
					colors.add(new Color(MathUtils.random(), MathUtils.random(), MathUtils.random(), 1));
				}
				shapes.setColor(colors.get(i));
				shapes.polygon(convexPolygons.get(i).items, 0, convexPolygons.get(i).size);
			}
		}

		if (isCreatingPolygon) {
			polygon.setSize(polygon.size - 2);
		}
		shapes.end();

		polyBatcher.begin();
		polyBatcher.enableBlending();
		for (int i = 0; i < polygon.size; i += 2) {
			float x = polygon.get(i);
			float y = polygon.get(i + 1);
			font.draw(polyBatcher, "" + (i >> 1), x, y); // + ", " + x + ", " + y, x, y);
		}
		font.draw(polyBatcher, Gdx.input.getX() + ", " + (Gdx.graphics.getHeight() - Gdx.input.getY()), 0, 20);
		polyBatcher.end();
	}

	private void triangulate () {
		Clipper.makeClockwise(polygon);
		convexPolygons = decomposer.decompose(polygon);
	}

	public static void main (String[] args) {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		new LwjglApplication(new ConvexDecomposerTest(), config);
	}
}
