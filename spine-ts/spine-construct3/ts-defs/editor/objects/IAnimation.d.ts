declare namespace SDK {
	class IAnimation {
		GetName(): string;
		GetObjectType(): SDK.IObjectType;
		GetFrames(): SDK.IAnimationFrame[];

		AddFrame(blob: Blob, width: number, height: number): Promise<SDK.IAnimationFrame>;

		SetSpeed(s: number): void;
		GetSpeed(): number;
		SetLooping(l: boolean): void;
		IsLooping(): boolean;
		SetPingPong(p: boolean): void;
		IsPingPong(): boolean;
		SetRepeatCount(r: number): void;
		GetRepeatCount(): number;
		SetRepeatTo(f: number): void;
		GetRepeatTo(): number;
		
		Delete(): void;
	}
}