
#pragma once

#include "ModuleManager.h"

class ISpineEditorPlugin : public IModuleInterface {

public:
	static inline ISpineEditorPlugin& Get() {
		return FModuleManager::LoadModuleChecked< ISpineEditorPlugin >( "SpineEditorPlugin" );
	}

	static inline bool IsAvailable() {
		return FModuleManager::Get().IsModuleLoaded( "SpineEditorPlugin" );
	}
};

