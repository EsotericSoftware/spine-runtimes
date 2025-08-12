
/** Represents the Audio object.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/plugin-interfaces/audio | IAudioObjectType documentation } */
declare class IAudioObjectType<InstType extends IInstance = IInstance> extends IObjectType<InstType>
{
	readonly audioContext: AudioContext;
	readonly destinationNode: AudioDestinationNode;

	isSilent: boolean;
	masterVolume: number;

	stopAll(): void;
}
