declare namespace SDK {
	class IObjectType extends IObjectClass {
		GetImage(): SDK.IAnimationFrame;
		EditImage(): void;

		GetAnimations(): SDK.IAnimation[];
		EditAnimations(): void;

		AddAnimation(animName: string, frameBlob: Blob, frameWidth: number, frameHeight: number): Promise<SDK.IAnimation>;

		CreateWorldInstance(layer: SDK.ILayer): SDK.IWorldInstance;
		GetAllInstances(): SDK.IWorldInstance[];
		
		IsInContainer(): boolean;
		GetContainer(): SDK.IContainer | null;
		CreateContainer(initialObjectTypes: SDK.IObjectType[]): SDK.IContainer;
	}
}