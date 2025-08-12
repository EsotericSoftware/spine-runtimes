declare namespace SDK {
	class ILayout {
		GetName(): string;
		GetProject(): SDK.IProject;
		GetEventSheet(): SDK.IEventSheet;
		GetAllLayers(): SDK.ILayer[];
	}
}