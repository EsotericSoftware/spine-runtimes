
/** SDK base class for an object type.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/addon-sdk-interfaces/isdkobjecttypebase | ISDKObjectTypeBase documentation } */
declare class ISDKObjectTypeBase_<InstanceType extends IInstance> extends IObjectType<InstanceType>
{
    _onCreate(): void;

    getImageInfo(): IImageInfo;

    _loadTextures(renderer: IRenderer): Promise<any>;
    _releaseTextures(renderer: IRenderer): void;
    _onDynamicTextureLoadComplete(): void;
    _preloadTexturesWithInstances(renderer: IRenderer): void;
}

declare var ISDKObjectTypeBase: typeof ISDKObjectTypeBase_;
