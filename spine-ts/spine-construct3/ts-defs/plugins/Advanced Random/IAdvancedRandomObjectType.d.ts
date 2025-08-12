
type AdvancedRandomGradientMode = "rgb" | "float";
type AdvancedRandomProbabilityTableEntry = number | string;

/** Represents the Advanced Random object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/advanced-random | IAdvancedRandomObjectType documentation } */
declare class IAdvancedRandomObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	seed: string;
	octaves: number;

	classic2d(x: number, y: number): number;
	classic3d(x: number, y: number, z: number): number;
	billow2d(x: number, y: number): number;
	billow3d(x: number, y: number, z: number): number;
	ridged2d(x: number, y: number): number;
	ridged3d(x: number, y: number, z: number): number;
	cellular2d(x: number, y: number): number;
	cellular3d(x: number, y: number, z: number): number;
	voronoi2d(x: number, y: number): number;
	voronoi3d(x: number, y: number, z: number): number;

	createGradient(name: string, mode: AdvancedRandomGradientMode): void;
	setCurrentGradient(name: string): void;
	addGradientStop(position: number, value: number): void;
	sampleGradient(name: string | null, value: number): number;

	createProbabilityTable(name: string): void;
	createProbabilityTableFromJSON(name: string, jsonStr: string): void;
	getProbabilityTableAsJSON(): string;
	setCurrentProbabilityTable(name: string): void;
	addProbabilityTableEntry(weight: number, value: AdvancedRandomProbabilityTableEntry): void;
	removeProbabilityTableEntry(weight: number, value: AdvancedRandomProbabilityTableEntry): void;
	sampleProbabilityTable(name?: string): AdvancedRandomProbabilityTableEntry;

	createPermutationTable(length: number, offset: number): void;
	shufflePermutationTable(): void;
	getPermutation(index: number): number;
}
