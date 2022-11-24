#include "spine/spine.h"
#include "spine_flutter.h"

using namespace spine;

String blendMode(spine_blend_mode mode) {
    switch(mode) {
        case SPINE_BLEND_MODE_NORMAL:
            return "normal";
        case SPINE_BLEND_MODE_ADDITIVE:
            return "additiev";
        case SPINE_BLEND_MODE_MULTIPLY:
            return "multiply";
        case SPINE_BLEND_MODE_SCREEN:
            return "screen";
    }
}

int main(int argc, char** argv) {
    int atlasLength = 0;
    void* atlasData = SpineExtension::getInstance()->_readFile("/Users/badlogic/workspaces/spine-runtimes/spine-flutter/example/assets/dragon.atlas", &atlasLength);
    uint8_t* cstringAtlas = SpineExtension::calloc<uint8_t>(atlasLength + 1, __FILE__, __LINE__);
    memcpy(cstringAtlas, atlasData, atlasLength);
    int dataLength = 0;
    uint8_t* data = (uint8_t*)SpineExtension::getInstance()->_readFile("/Users/badlogic/workspaces/spine-runtimes/spine-flutter/example/assets/dragon-ess.skel", &dataLength);

    spine_atlas atlas = spine_atlas_load((const utf8*)cstringAtlas);
    Vector<String> atlasPages;
    for (int i = 0, n = spine_atlas_get_num_image_paths(atlas); i < n; i++) {
        atlasPages.add(spine_atlas_get_image_path(atlas, i));
    }

    spine_skeleton_data_result result = spine_skeleton_data_load_binary(atlas, data, dataLength);
    spine_skeleton_drawable drawable = spine_skeleton_drawable_create(spine_skeleton_data_result_get_data(result));
    spine_skeleton skeleton = spine_skeleton_drawable_get_skeleton(drawable);
    spine_skeleton_update_world_transform(skeleton);
    spine_render_command cmd = spine_skeleton_drawable_render(drawable);

    int batchId = 0;
    while(cmd) {
        int numVertices = spine_render_command_get_num_vertices(cmd);
        int numIndices = spine_render_command_get_num_indices(cmd);
        float *positions = spine_render_command_get_positions(cmd);
        float *uvs = spine_render_command_get_uvs(cmd);
        int32_t *colors = spine_render_command_get_colors(cmd);
        uint16_t *indices = spine_render_command_get_indices(cmd);
        String str;
        int atlasPage = spine_render_command_get_atlas_page(cmd);
        str.append(atlasPages[atlasPage]);
        str.append("\n");
        str.append(blendMode(spine_render_command_get_blend_mode(cmd)));
        str.append("\n");
        str.append(numVertices);
        str.append("\n");
        str.append(numIndices);
        str.append("\n");
        for (int i = 0; i < numVertices * 2; i++) {
            str.append(positions[i]);
            str.append("\n");
        }
        for (int i = 0; i < numVertices * 2; i++) {
            str.append(uvs[i]);
            str.append("\n");
        }
        for (int i = 0; i < numVertices; i++) {
            str.append(colors[i]);
            str.append("\n");
        }
        for (int i = 0; i < numIndices; i++) {
            str.append(indices[i]);
            str.append("\n");
        }
        String outputFile = "";
        outputFile.append("/Users/badlogic/Desktop/dragon-");
        outputFile.append(batchId);
        outputFile.append(".mesh");
        FILE *file = fopen(outputFile.buffer(), "w");
        fwrite(str.buffer(), str.length(), 1, file);
        fclose(file);
        batchId++;
        cmd = spine_render_command_get_next(cmd);
    }
}