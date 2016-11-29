// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#include "SpinePluginPrivatePCH.h"
#include "spine/spine.h"


class FSpinePlugin : public SpinePlugin {
	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
};

IMPLEMENT_MODULE( FSpinePlugin, SpinePlugin )



void FSpinePlugin::StartupModule() {
	// This code will execute after your module is loaded into memory (but after global variables are initialized, of course.)
    printf("This is a test");
}


void FSpinePlugin::ShutdownModule() {
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
}

extern "C" {
    void _spAtlasPage_createTexture (spAtlasPage* self, const char* path) {
        
    }
    void _spAtlasPage_disposeTexture (spAtlasPage* self) {
        
    }
    
    char* _spUtil_readFile (const char* path, int* length) {
        return 0;
    }
}



