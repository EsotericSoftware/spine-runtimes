
/** Provides access to the project's assets.
 * @see {@link https://www.construct.net/make-games/manuals/construct-3/scripting/scripting-reference/interfaces/iassetmanager | IAssetManager documentation } */
declare class IAssetManager
{
    loadImageAsset(iImageInfo: IImageInfo): void;

    /**
	 * @deprecated All supported platforms now support WebM Opus, so this property is always true and is now redundant.
	 */
	readonly isWebMOpusSupported: boolean;

    fetchText(url: string): Promise<string>;
    fetchJson(url: string): Promise<object>;
    fetchBlob(url: string): Promise<Blob>;
    fetchArrayBuffer(url: string): Promise<ArrayBuffer>;
    getProjectFileUrl(url: string): Promise<string>;
    getMediaFileUrl(url: string): Promise<string>;
    readonly mediaFolder: string;
    
    /**
	 * @deprecated All supported platforms now support WebM Opus, so use the Web Audio API decodeAudioData() method instead.
	 */
    decodeWebMOpus(audioContext: AudioContext, arrayBuffer: ArrayBuffer): Promise<AudioBuffer>;

    loadScripts(...urls: string[]): Promise<void>;
    compileWebAssembly(url: string): Promise<WebAssembly.Module>;
    loadStyleSheet(url: string): Promise<void>;
}
