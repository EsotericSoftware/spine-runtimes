declare namespace SDK {
	class ICollisionPoly {
		Reset(): void;
		IsDefault(): boolean;

		GetPoints(): number[];
		SetPoints(arr: number[]): void;
	}
}