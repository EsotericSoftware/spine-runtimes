// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#include "SpineEditorPluginPrivatePCH.h"
#include "spine/spine.h"


class FSpineEditorPlugin: public ISpineEditorPlugin {
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
};

IMPLEMENT_MODULE(FSpineEditorPlugin, ISpineEditorPlugin)



void FSpineEditorPlugin::StartupModule () { }

void FSpineEditorPlugin::ShutdownModule () { }



