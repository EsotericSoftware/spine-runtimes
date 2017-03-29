
package com.esotericsoftware.spine;

import org.lwjgl.opengl.GL11;

import com.badlogic.gdx.ApplicationAdapter;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.Input.Buttons;
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
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.utils.Array;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.utils.ConvexDecomposer;
import com.esotericsoftware.spine.utils.SutherlandHodgmanClipper;

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

			polygon.add(tmp.x);
			polygon.add(tmp.y);

			if (Gdx.input.isButtonPressed(Buttons.RIGHT)) {
				isCreatingPolygon = false;
				triangulate();
			}
		}
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
		shapes.setColor(Color.YELLOW);
		if (polygon.size > 2) {
			boolean clockwise = SutherlandHodgmanClipper.isClockwise(polygon);
			for (int i = 0; i < polygon.size; i += 2) {
				float x = polygon.get(i);
				float y = polygon.get(i + 1);
				float x2 = polygon.get((i + 2) % polygon.size);
				float y2 = polygon.get((i + 3) % polygon.size);

				float mx = x + (x2 - x) / 2;
				float my = y + (y2 - y) / 2;
				float nx = (y2 - y);
				float ny = -(x2 - x);
				if (clockwise) {
					nx = -nx;
					ny = -ny;
				}
				float l = 1 / (float)Math.sqrt(nx * nx + ny * ny);
				nx *= l * 20;
				ny *= l * 20;

				shapes.line(mx, my, mx + nx, my + ny);
			}
		}
		
		// decomposition		
		if (convexPolygons != null) {
			for (int i = 0, n = convexPolygons.size; i < n; i++) {
				if (colors.size <= i) {
					colors.add(new Color(MathUtils.random(), MathUtils.random(), MathUtils.random(), 1));
				}
				shapes.setColor(colors.get(i));
				shapes.polygon(convexPolygons.get(i).items, 0, convexPolygons.get(i).size);
//				if (i == 4) break;
			}
		}

		if (isCreatingPolygon) {
			polygon.setSize(polygon.size - 2);
		}	
		shapes.end();
		
		polyBatcher.begin();
		polyBatcher.enableBlending();
		for (int i = 0; i < polygon.size; i+=2) {
			float x = polygon.get(i);
			float y = polygon.get(i + 1);
			font.draw(polyBatcher, "" + (i >> 1), x, y);
		}
		polyBatcher.end();
	}

	private void triangulate () {
		SutherlandHodgmanClipper.makeClockwise(polygon);
		convexPolygons = decomposer.decompose(polygon);
	}

	public static void main (String[] args) {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		new LwjglApplication(new ConvexDecomposerTest(), config);
	}
}
