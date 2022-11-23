#include "spine/spine.h"
#include "spine_flutter.h"

using namespace spine;

int main(int argc, char** argv) {
    int atlasLength = 0;
    void* atlasData = SpineExtension::getInstance()->_readFile("/Users/badlogic/workspaces/spine-runtimes/spine-flutter/example/assets/spineboy.atlas", &atlasLength);
    uint8_t* cstringAtlas = SpineExtension::calloc<uint8_t>(atlasLength + 1, __FILE__, __LINE__);
    memcpy(cstringAtlas, atlasData, atlasLength);
    int dataLength = 0;
    uint8_t* data = (uint8_t*)SpineExtension::getInstance()->_readFile("/Users/badlogic/workspaces/spine-runtimes/spine-flutter/example/assets/spineboy-pro.skel", &dataLength);

    spine_atlas atlas = spine_atlas_load((const utf8*)cstringAtlas);
    spine_skeleton_data_result result = spine_skeleton_data_load_binary(atlas, data, dataLength);
    spine_skeleton_drawable drawable = spine_skeleton_drawable_create(spine_skeleton_data_result_get_data(result));
    spine_render_command cmd = spine_skeleton_drawable_render(drawable);
    while (cmd) {
        uint16_t *indices = spine_render_command_get_indices(cmd);
        cmd = spine_render_command_get_next(cmd);
    }
}