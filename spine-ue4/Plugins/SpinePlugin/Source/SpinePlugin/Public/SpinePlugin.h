#pragma once

#include "ModuleManager.h"

class SPINEPLUGIN_API SpinePlugin : public IModuleInterface {

public:
	
	static inline SpinePlugin& Get() {
		return FModuleManager::LoadModuleChecked< SpinePlugin >( "SpinePlugin" );
	}
	
	static inline bool IsAvailable() {
		return FModuleManager::Get().IsModuleLoaded( "SpinePlugin" );
	}
};

