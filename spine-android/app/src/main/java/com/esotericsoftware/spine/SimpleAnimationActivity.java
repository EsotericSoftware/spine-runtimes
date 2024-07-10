package com.esotericsoftware.spine;

import android.os.Bundle;
import android.view.MenuItem;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import com.esotericsoftware.spine.android.SpineController;
import com.esotericsoftware.spine.android.SpineView;

public class SimpleAnimationActivity extends AppCompatActivity {
    /** @noinspection FieldCanBeLocal*/
    private SpineView spineView;
    /** @noinspection FieldCanBeLocal*/
    private SpineController spineController;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_simple_animation);

        // Set up the toolbar
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        if (getSupportActionBar() != null) {
            getSupportActionBar().setTitle("Simple Animation");
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);
            getSupportActionBar().setDisplayShowHomeEnabled(true);
        }

        spineView = findViewById(R.id.spineView);
        spineController = new SpineController( controller ->
            controller.getAnimationState().setAnimation(0, "walk", true)
        );

        spineView.setController(spineController);
        spineView.loadFromAsset("spineboy.atlas","spineboy-pro.json");
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        if (item.getItemId() == android.R.id.home) {
            finish();
            return true;
        }
        return super.onOptionsItemSelected(item);
    }
}
