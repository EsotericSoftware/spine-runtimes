
/** SDK base class for a world instance.
 * @see {@link https://www.construct.net/en/make-games/manuals/construct-3/scripting/scripting-reference/addon-sdk-interfaces/isdkworldinstancebase | ISDKWorldInstanceBase documentation } */
declare class ISDKWorldInstanceBase_ extends IWorldInstanceSDKBase
{
    _handleRendererContextLoss(): void;

    _onRendererContextLost(): void;
    _onRendererContextRestored(): void;

    _draw(renderer: IRenderer): void;

    _rendersToOwnZPlane(): boolean;
    _mustPreDraw(): boolean;
}

declare var ISDKWorldInstanceBase: typeof ISDKWorldInstanceBase_;
