
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
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.utils.FloatArray;
import com.esotericsoftware.spine.utils.SutherlandHodgmanClipper;

public class SoftwareClippingTest extends ApplicationAdapter {
	OrthographicCamera sceneCamera;
	ShapeRenderer shapes;

	float[] triangle = {100, 100, 300, 100, 200, 300};
	FloatArray clippingPolygon = new FloatArray();
	FloatArray clippedPolygon = new FloatArray();

	boolean isCreatingClippingArea = false;
	Vector3 tmp = new Vector3();
	SutherlandHodgmanClipper clipper;

	@Override
	public void create () {
		sceneCamera = new OrthographicCamera();
		shapes = new ShapeRenderer();
		clipper = new SutherlandHodgmanClipper();
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
			
			clippingPolygon.add(tmp.x);
			clippingPolygon.add(tmp.y);
			
			if (Gdx.input.isButtonPressed(Buttons.RIGHT)) {
				isCreatingClippingArea = false;					
				clip();			
			}
		}
	}

	private void renderScene () {
		sceneCamera.update();
		shapes.setProjectionMatrix(sceneCamera.combined);
		shapes.begin(ShapeType.Line);

		// triangle
		shapes.setColor(Color.GREEN);
		shapes.polygon(triangle);

		// clipped polygons
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

		// edge normals
		shapes.setColor(Color.YELLOW);		
		if (clippingPolygon.size > 2) {
			boolean clockwise = SutherlandHodgmanClipper.clockwise(clippingPolygon);
			for (int i = 0; i < clippingPolygon.size; i += 2) {
				float x = clippingPolygon.get(i);
				float y = clippingPolygon.get(i + 1);
				float x2 = clippingPolygon.get((i + 2) % clippingPolygon.size);
				float y2 = clippingPolygon.get((i + 3) % clippingPolygon.size);

				float mx = x + (x2 - x) / 2;
				float my = y + (y2 - y) / 2;
				float nx = (y2 - y);
				float ny = -(x2 - x);
				if (!clockwise) {
					nx = -nx;
					ny = -ny;
				}
				float l = 1 / (float)Math.sqrt(nx * nx + ny * ny);
				nx *= l * 20;
				ny *= l * 20;

				shapes.line(mx, my, mx + nx, my + ny);
			}
		}
		
		if (isCreatingClippingArea) {
			clippingPolygon.setSize(clippingPolygon.size - 2);
		}

		// clipped polygon
		shapes.setColor(Color.PINK);
		if (clippedPolygon.size > 0) {
			shapes.polygon(clippedPolygon.items, 0, clippedPolygon.size);
		}

		shapes.end();
	}

	private void clip () {
		FloatArray input = new FloatArray();
		input.addAll(triangle, 0, triangle.length);
		clippedPolygon.clear();
		System.out.println("Clipped: " + (clipper.clip(input, clippingPolygon, clippedPolygon) != null));
	}

	public static void main (String[] args) {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		new LwjglApplication(new SoftwareClippingTest(), config);
	}
}
