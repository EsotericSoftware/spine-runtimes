
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
import com.badlogic.gdx.graphics.g2d.PolygonSpriteBatch;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer;
import com.badlogic.gdx.graphics.glutils.ShapeRenderer.ShapeType;
import com.badlogic.gdx.math.Vector3;
import com.badlogic.gdx.utils.FloatArray;
import com.badlogic.gdx.utils.ShortArray;
import com.esotericsoftware.spine.utils.SutherlandHodgmanClipper;

public class SoftwareClippingTest extends ApplicationAdapter {
	OrthographicCamera sceneCamera;
	ShapeRenderer shapes;
	PolygonSpriteBatch polyBatcher;
	Texture image;

	float[] triangleOutline = { 100, 100, 300, 100, 200, 300 };
	float[] triangle = { 
		100, 100, Color.WHITE.toFloatBits(), 0, 1, 
		300, 100, Color.WHITE.toFloatBits(), 1, 1, 
		200, 300, Color.WHITE.toFloatBits(), 0.5f, 0
	};
	short[] triangleIndices = { 0, 1, 2 };	
	FloatArray clippingPolygon = new FloatArray();
	FloatArray clippedPolygon = new FloatArray();
	
	FloatArray clippedPolygonVertices = new FloatArray();
	ShortArray clippedPolygonIndices = new ShortArray();

	boolean isCreatingClippingArea = false;
	Vector3 tmp = new Vector3();
	SutherlandHodgmanClipper clipper;	

	@Override
	public void create () {
		sceneCamera = new OrthographicCamera();
		shapes = new ShapeRenderer();
		polyBatcher = new PolygonSpriteBatch();
		clipper = new SutherlandHodgmanClipper();
		image = new Texture("skin/skin.png");
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
		polyBatcher.setProjectionMatrix(sceneCamera.combined);
		
		polyBatcher.begin();
		polyBatcher.disableBlending();
		
		if (clippedPolygon.size == 0) {
			polyBatcher.draw(image, triangle, 0, 15, triangleIndices, 0, 3);
		} else {
			polyBatcher.draw(image, clippedPolygonVertices.items, 0, clippedPolygonVertices.size, clippedPolygonIndices.items, 0, clippedPolygonIndices.size);
		}
		polyBatcher.end();
		
		shapes.begin(ShapeType.Line);

		// triangle
		shapes.setColor(Color.GREEN);
		shapes.polygon(triangleOutline);

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
		float x1 = triangle[0];
		float y1 = triangle[1];
		float x2 = triangle[5];
		float y2 = triangle[6];
		float x3 = triangle[10];
		float y3 = triangle[11];
		
		// must duplicate first vertex at end of polygon
		// so we can avoid module/branch in clipping code
		SutherlandHodgmanClipper.makeClockwise(clippingPolygon);
		clippingPolygon.add(clippingPolygon.get(0));
		clippingPolygon.add(clippingPolygon.get(1));
		
		boolean clipped = clipper.clip(x1, y1, x2, y2, x3, y3, clippingPolygon, clippedPolygon);
		System.out.println("Clipped: " + clipped);
		if (clipped) {
			clippedPolygonVertices.clear();
			clippedPolygonIndices.clear();
			
			float d0 = y2 - y3;
			float d1 = x3 - x2;
			float d2 = x1 - x3;
			float d3 = y1 - y3;
			float d4 = y3 - y1;
			
			float denom = 1 / (d0 * d2 + d1 * d3);
			
			// triangulate by creating a triangle fan, duplicate vertices
			float color = Color.WHITE.toFloatBits();
			for (int i = 0; i < clippedPolygon.size; i+=2) {				
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
				clippedPolygonIndices.add(0);
				clippedPolygonIndices.add(i);
				clippedPolygonIndices.add(i + 1);
			}
		} else {
			clippedPolygon.clear();
		}
		
		clippingPolygon.setSize(clippingPolygon.size - 2);
	}

	public static void main (String[] args) {
		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
		new LwjglApplication(new SoftwareClippingTest(), config);
	}
}
