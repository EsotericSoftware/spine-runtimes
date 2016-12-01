#include "SpinePluginPrivatePCH.h"


class FSpinePlugin : public SpinePlugin {
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
};

IMPLEMENT_MODULE( FSpinePlugin, SpinePlugin )

void FSpinePlugin::StartupModule() { }


void FSpinePlugin::ShutdownModule() { }

// These are not used in the Spine UE4 plugin, see SpineAtlasAsset on how atlas page textures
// are loaded, See SpineSkeletonRendererComponent on how these textures are used for rendering.
extern "C" {
    void _spAtlasPage_createTexture (spAtlasPage* self, const char* path) { }
    void _spAtlasPage_disposeTexture (spAtlasPage* self) { }
    char* _spUtil_readFile (const char* path, int* length) { return 0; }
}



